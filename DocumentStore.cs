public interface IDocumentStore
{
    Task<bool> IngestDocumentAsync(string filePath);
    Task<IEnumerable<string>> RetrieveRelevantContextAsync(string query, int topK = 3);
}

public class VectorDocumentStore : IDocumentStore
{
    private readonly EmbeddingService _embeddingService;
    private readonly VectorStoreService _vectorStore;
    private readonly DocumentChunker _chunker;
    private bool _hasDocuments;

    public VectorDocumentStore(EmbeddingService embeddingService, VectorStoreService vectorStore)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _chunker = new DocumentChunker(chunkSize: 500, overlap: 50);
    }

    public async Task<bool> IngestDocumentAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return false;

        var content = filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? DocumentReader.ReadPdf(filePath)
            : DocumentReader.ReadText(filePath);

        var chunks = _chunker.ChunkText(content);

        for (int i = 0; i < chunks.Count; i++)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(chunks[i]);
            await _vectorStore.StoreChunkAsync($"chunk_{i}", chunks[i], embedding);
        }

        _hasDocuments = true;
        return true;
    }

    public async Task<IEnumerable<string>> RetrieveRelevantContextAsync(string query, int topK = 3)
    {
        if (!_hasDocuments) return Enumerable.Empty<string>();

        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);
        var searchResults = await _vectorStore.SearchAsync(queryEmbedding, limit: topK);
        
        return searchResults.Select(r => r.text);
    }
}
