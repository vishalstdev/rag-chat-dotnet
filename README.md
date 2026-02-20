# RAG Chat Application
Principal-level architecture demo using C# + Semantic Kernel + Vector Search

## Features
- Chat with OpenAI GPT-4
- Load documents (PDF, TXT) and split into chunks
- Generate embeddings using OpenAI
- Store vectors in Qdrant for semantic search
- Retrieve relevant context using vector similarity
- Answer questions based on document content

## Architecture

*Note: The following architecture diagrams were created by analyzing the codebase with Claude AI to visualize component relationships and data flow.*

### High-Level Flow
```
┌─────────────┐
│   User      │
│  Document   │
└──────┬──────┘
       │
       ▼
┌──────────────────┐
│ VectorDocumentStore│  ◄─── IDocumentStore
└──────┬──────┬─────┘
       │      │
       ▼      ▼
 ┌─────────┐ ┌──────────────┐
 │Document │ │ EmbeddingService│
 │Chunker  │ │  (OpenAI API)  │
 └────┬────┘ └────────┬───────┘
      │              │
      └──────┬───────┘
             ▼
    ┌─────────────────┐
    │VectorStoreService│
    │   (Qdrant DB)   │
    └─────────────────┘

┌─────────────┐
│    User     │
│   Query     │
└──────┬──────┘
       │
       ▼
┌──────────────────┐
│  RagOrchestrator │  ◄─── IRagOrchestrator
└──────┬──────┬────┘
       │      │
       ▼      ▼
┌──────────────┐  ┌────────────────┐
│VectorDocument│  │ ChatCompletion │
│    Store     │  │   (GPT-4)      │
│(Retrieve top │  │                │
│  3 chunks)   │  │                │
└──────────────┘  └────────────────┘
```

### Component Interaction
```
Program.cs
    │
    ├──► EmbeddingService (OpenAI API wrapper)
    │
    ├──► VectorStoreService (Qdrant client)
    │
    ├──► VectorDocumentStore (implements IDocumentStore)
    │         │
    │         ├──► DocumentChunker
    │         ├──► EmbeddingService
    │         └──► VectorStoreService
    │
    └──► RagOrchestrator (implements IRagOrchestrator)
              │
              ├──► IDocumentStore (for retrieval)
              └──► IChatCompletionService (for GPT-4)
```

### Data Flow: Document Ingestion
```
1. User uploads document.pdf
2. VectorDocumentStore reads file
3. DocumentChunker splits into 500-word chunks with 50-word overlap
4. For each chunk:
   a. EmbeddingService generates 1536-dim vector
   b. VectorStoreService stores in Qdrant with metadata
5. System ready for queries
```

### Data Flow: Question Answering
```
1. User asks "What are the working hours?"
2. RagOrchestrator receives query
3. VectorDocumentStore:
   a. Generates embedding for query
   b. Searches Qdrant (cosine similarity, threshold 0.7)
   c. Returns top 3 relevant chunks
4. RagOrchestrator:
   a. Builds context message with retrieved chunks
   b. Sends to GPT-4 via ChatCompletionService
   c. Returns answer to user
5. Chat history maintained for follow-up questions
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


## Development Notes

This project was developed with assistance from Claude AI (Anthropic) for:
- Architecture diagram generation
- Code review and best practices
- Documentation structure

All core implementation, design decisions, and system architecture are my own work.
