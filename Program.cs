// See https://aka.ms/new-console-template for more information
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();
// You'll add OpenAI key later - for now just structure
var kernel = builder.Build();

Console.WriteLine("RAG Chat App - Hello World");
Console.WriteLine("Semantic Kernel initialized successfully!");

