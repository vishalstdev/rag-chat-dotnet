using Qdrant.Client;
using Qdrant.Client.Grpc;

public class VectorStoreService
{
    private readonly QdrantClient _client;
    private readonly string _collectionName;
    
    public VectorStoreService(string qdrantEndpoint = "localhost", 
                             int qdrantPort = 6334,
                             string collectionName = "documents")
    {
        _client = new QdrantClient(qdrantEndpoint, qdrantPort);
        _collectionName = collectionName;
    }
    
    public async Task CreateCollectionIfNotExistsAsync()
    {
        try
        {
            await _client.GetCollectionInfoAsync(_collectionName);
            Console.WriteLine($"Collection '{_collectionName}' already exists");
        }
        catch
        {
            await _client.CreateCollectionAsync(_collectionName, new VectorParams
            {
                Size = 1536, // OpenAI embedding size
                Distance = Distance.Cosine
            });
            Console.WriteLine($"Created collection '{_collectionName}'");
        }
    }
    
    public async Task StoreChunkAsync(string chunkId, string text, ReadOnlyMemory<float> embedding)
    {
        var points = new List<PointStruct>
        {
            new PointStruct
            {
                Id = (ulong)chunkId.GetHashCode(),
                Vectors = embedding.ToArray(),
                Payload =
                {
                    ["text"] = text,
                    ["id"] = chunkId
                }
            }
        };
        
        await _client.UpsertAsync(_collectionName, points);
    }
    
    public async Task<List<(string text, double score)>> SearchAsync(
        ReadOnlyMemory<float> queryEmbedding, 
        int limit = 3)
    {
        var searchResult = await _client.SearchAsync(
            _collectionName,
            queryEmbedding.ToArray(),
            limit: (ulong)limit,
            scoreThreshold: 0.7f
        );
        
        var matches = new List<(string text, double score)>();
        
        foreach (var result in searchResult)
        {
            var text = result.Payload["text"].StringValue;
            matches.Add((text, result.Score));
        }
        
        return matches;
    }
}