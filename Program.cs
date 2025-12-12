
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

Console.WriteLine("RAG Chat App - Simple Chat Mode");
Console.WriteLine("Type 'exit' to quit\n");

var history = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();

// Test document reading
Console.WriteLine("Testing document reader...");
var testDoc = DocumentReader.ReadText("TestDoc.txt");
Console.WriteLine($"Read document: {testDoc}\n");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    
    if (input?.ToLower() == "exit") break;
    
    history.AddUserMessage(input);
    
    var response = await chat.GetChatMessageContentAsync(history);
    history.AddAssistantMessage(response.Content);
    
    Console.WriteLine($"AI: {response.Content}\n");
}


