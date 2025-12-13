# RAG Chat Application
Principal-level architecture demo using C# + Semantic Kernel

## Goal
Multi-tenant document chat system with enterprise patterns

## Tech Stack
- .NET 8 + Semantic Kernel
- Azure OpenAI (planned)
- Vector DB (planned)


## Setup
1. Copy `appsettings.example.json` to `appsettings.json`
2. Add your OpenAI API key in `appsettings.json`
3. Run: `dotnet run`


## Usage
```bash
dotnet run
# Enter document path or press Enter to skip
# Ask questions about the document
```

## Tech Stack
- .NET 8 + C#
- Semantic Kernel
- OpenAI GPT-4
- PdfPig for PDF parsing

## Architecture
- Simple RAG implementation using system message context
- Next: Vector embeddings for semantic search