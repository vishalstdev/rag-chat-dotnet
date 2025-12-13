using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Configuration;

// Load config
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var apiKey = config["OpenAI:ApiKey"];
var model = config["OpenAI:Model"];

// Build kernel
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(model, apiKey);
var kernel = builder.Build();

var chat = kernel.GetRequiredService<IChatCompletionService>();
var history = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();

Console.WriteLine("=== RAG Chat Application ===\n");

// Document loading
Console.Write("Enter document path (or press Enter to skip): ");
var docPath = Console.ReadLine()?.Trim();

if (!string.IsNullOrEmpty(docPath) && File.Exists(docPath))
{
    try
    {
        string content;
        
        if (docPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            content = DocumentReader.ReadPdf(docPath);
            Console.WriteLine($"✓ Loaded PDF: {Path.GetFileName(docPath)}");
        }
        else
        {
            content = DocumentReader.ReadText(docPath);
            Console.WriteLine($"✓ Loaded text file: {Path.GetFileName(docPath)}");
        }
        
        // Add document to context
        history.AddSystemMessage($"You are a helpful assistant. Use the following document to answer user questions:\n\n{content}");
        Console.WriteLine($"✓ Document loaded into context ({content.Length} characters)\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error loading document: {ex.Message}\n");
    }
}
else if (!string.IsNullOrEmpty(docPath))
{
    Console.WriteLine($"✗ File not found: {docPath}\n");
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
    
    history.AddUserMessage(input);
    
    try
    {
        var response = await chat.GetChatMessageContentAsync(history);
        history.AddAssistantMessage(response.Content);
        
        Console.WriteLine($"AI: {response.Content}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}\n");
    }
}

Console.WriteLine("Goodbye!");