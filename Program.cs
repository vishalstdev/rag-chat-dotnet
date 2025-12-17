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
var chunker = new DocumentChunker(chunkSize: 500, overlap: 50);

await vectorStore.CreateCollectionIfNotExistsAsync();

// Document loading
Console.Write("Enter document path (or press Enter to skip): ");
var docPath = Console.ReadLine()?.Trim();

List<string> documentChunks = new();

if (!string.IsNullOrEmpty(docPath) && File.Exists(docPath))
{
    try
    {
        string content;
        
        if (docPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            content = DocumentReader.ReadPdf(docPath);
            Console.WriteLine($"{Symbols.Success} Loaded PDF: {Path.GetFileName(docPath)}");
        }
        else
        {
            content = DocumentReader.ReadText(docPath);
            Console.WriteLine($"{Symbols.Success} Loaded text file: {Path.GetFileName(docPath)}");
        }
        
        // Chunk the document
        documentChunks = chunker.ChunkText(content);
        Console.WriteLine($"{Symbols.Success} Document split into {documentChunks.Count} chunks");
        
        // Generate embeddings and store in vector DB
        Console.WriteLine($"{Symbols.Info} Generating embeddings and storing in vector DB...");
        
        for (int i = 0; i < documentChunks.Count; i++)
        {
            var embedding = await embeddingService.GenerateEmbeddingAsync(documentChunks[i]);
            await vectorStore.StoreChunkAsync($"chunk_{i}", documentChunks[i], embedding);
            
            if ((i + 1) % 10 == 0)
            {
                Console.WriteLine($"  Processed {i + 1}/{documentChunks.Count} chunks...");
            }
        }
        
        Console.WriteLine($"{Symbols.Success} Vector database ready with {documentChunks.Count} chunks\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{Symbols.Failure} Error: {ex.Message}\n");
    }
}
else if (!string.IsNullOrEmpty(docPath))
{
    Console.WriteLine($"{Symbols.Failure} File not found: {docPath}\n");
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
        
        // If we have document chunks, do vector search
        if (documentChunks.Count > 0)
        {
            var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(input);
            var searchResults = await vectorStore.SearchAsync(queryEmbedding, limit: 3);
            
            if (searchResults.Count > 0)
            {
                Console.WriteLine($"\n{Symbols.Info} Found {searchResults.Count} relevant chunks (scores: {string.Join(", ", searchResults.Select(r => $"{r.score:F2}"))})");
                
                var relevantContext = string.Join("\n\n", searchResults.Select(r => r.text));
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