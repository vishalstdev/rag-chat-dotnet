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
var chat = kernel.GetRequiredService<IChatCompletionService>();

Console.WriteLine("=== RAG Chat with Vector Search ===\n");

// Initialize services
var embeddingService = new EmbeddingService(apiKey);
var vectorStore = new VectorStoreService();
await vectorStore.CreateCollectionIfNotExistsAsync();

var documentStore = new VectorDocumentStore(embeddingService, vectorStore);

// Document loading
Console.Write("Enter document path (or press Enter to skip): ");
var docPath = Console.ReadLine()?.Trim();

bool hasDocuments = false;
if (!string.IsNullOrEmpty(docPath))
{
    try
    {
        hasDocuments = await documentStore.IngestDocumentAsync(docPath);
        if (hasDocuments)
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

// Chat loop with vector search
var history = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();
Console.WriteLine("Type your questions (or 'exit' to quit):\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrEmpty(input)) continue;
    if (input.ToLower() == "exit") break;
    
    try
    {
        string contextMessage = "";
        
        // If we have documents, do vector search
        if (hasDocuments)
        {
            var relevantChunks = await documentStore.RetrieveRelevantContextAsync(input);
            var chunksList = relevantChunks.ToList();
            
            if (chunksList.Count > 0)
            {
                Console.WriteLine($"\n{Symbols.Info} Found {chunksList.Count} relevant chunks");
                var relevantContext = string.Join("\n\n", chunksList);
                contextMessage = $"Answer the question based on this context:\n\n{relevantContext}\n\nQuestion: {input}";
            }
            else
            {
                contextMessage = $"No relevant context found. Answer generally: {input}";
            }
        }
        else
        {
            contextMessage = input;
        }
        
        history.AddUserMessage(contextMessage);
        
        var response = await chat.GetChatMessageContentAsync(history);
        history.AddAssistantMessage(response.Content);
        
        Console.WriteLine($"\nAI: {response.Content}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{Symbols.Failure} Error: {ex.Message}\n");
    }
}

Console.WriteLine("Goodbye!");