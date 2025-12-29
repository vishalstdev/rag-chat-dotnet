using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var apiKey = config["OpenAI:ApiKey"];
var model = config["OpenAI:Model"];

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(model, apiKey);
var kernel = builder.Build();

Console.WriteLine("=== RAG Chat with Vector Search ===\n");

// Initialize services
var embeddingService = new EmbeddingService(apiKey);
var vectorStore = new VectorStoreService();
await vectorStore.CreateCollectionIfNotExistsAsync();

var documentStore = new VectorDocumentStore(embeddingService, vectorStore);
var ragOrchestrator = new RagOrchestrator(kernel.GetRequiredService<IChatCompletionService>(), documentStore);

// Document loading
Console.Write("Enter document path (or press Enter to skip): ");
var docPath = Console.ReadLine()?.Trim();

if (!string.IsNullOrEmpty(docPath))
{
    try
    {
        var success = await documentStore.IngestDocumentAsync(docPath);
        if (success)
        {
            var fileType = docPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? "PDF" : "text file";
            Console.WriteLine($"{Symbols.Success} Loaded {fileType}: {Path.GetFileName(docPath)}");
            Console.WriteLine($"{Symbols.Success} Document processed and stored in vector DB\n");
        }
        else
        {
            Console.WriteLine($"{Symbols.Failure} File not found: {docPath}\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{Symbols.Failure} Error: {ex.Message}\n");
    }
}
else
{
    Console.WriteLine("No document loaded. Chatting without context.\n");
}

// Chat loop
Console.WriteLine("Type your questions (or 'exit' to quit):\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrEmpty(input)) continue;
    if (input.ToLower() == "exit") break;
    
    try
    {
        var response = await ragOrchestrator.AskAsync(input);
        Console.WriteLine($"\nAI: {response}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{Symbols.Failure} Error: {ex.Message}\n");
    }
}

Console.WriteLine("Goodbye!");