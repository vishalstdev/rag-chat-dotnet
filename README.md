# RAG Chat Application
Principal-level architecture demo using C# + Semantic Kernel + Vector Search

## Features
- ✅ Chat with OpenAI GPT-4
- ✅ Load documents (PDF, TXT) and split into chunks
- ✅ Generate embeddings using OpenAI
- ✅ Store vectors in Qdrant for semantic search
- ✅ Retrieve relevant context using vector similarity
- ✅ Answer questions based on document content

## Architecture
```
User Document → Chunking → Embeddings (OpenAI) → Qdrant Vector DB
User Question → Embedding → Vector Search → Top 3 Chunks → GPT-4 → Answer
```

## Prerequisites
- .NET 8 SDK
- Docker (for Qdrant)
- OpenAI API key

## Setup

### 1. Start Qdrant Vector Database
```bash
docker run -p 6333:6333 -p 6334:6334 \
    -v $(pwd)/qdrant_storage:/qdrant/storage:z \
    qdrant/qdrant
```

### 2. Configure OpenAI
```bash
cp appsettings.example.json appsettings.json
# Edit appsettings.json and add your OpenAI API key
```

### 3. Run Application
```bash
dotnet run
```

## Usage
1. Enter path to PDF or text file (or press Enter to skip)
2. App will chunk document, generate embeddings, and store in Qdrant
3. Ask questions - app searches for relevant chunks and answers based on context
4. Type 'exit' to quit

## Example
```
Enter document path: company-policy.txt
✓ Loaded text file: company-policy.txt
✓ Document split into 5 chunks
→ Generating embeddings and storing in vector DB...
✓ Vector database ready with 5 chunks

You: What are the core working hours?
→ Found 2 relevant chunks (scores: 0.89, 0.76)
AI: The core working hours are 10 AM - 3 PM in the employee's timezone.
```

## Tech Stack
- .NET 8 + C#
- Semantic Kernel
- OpenAI (GPT-4 + Embeddings)
- Qdrant Vector Database
- PdfPig for PDF parsing

## Next Steps
- [ ] Multi-document support
- [ ] Persistent storage of embeddings
- [ ] Web API layer
- [ ] Better chunking strategies (semantic chunking)
- [ ] Caching for repeated queries