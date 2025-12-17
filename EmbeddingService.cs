using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class EmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embeddingService;
    
    public EmbeddingService(string apiKey, string model = "text-embedding-3-small")
    {
        _embeddingService = new OpenAITextEmbeddingGenerationService(model, apiKey);
    }
    
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
    {
        return await _embeddingService.GenerateEmbeddingAsync(text);
    }
    
    public async Task<List<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(List<string> texts)
    {
        var embeddings = new List<ReadOnlyMemory<float>>();
        foreach (var text in texts)
        {
            embeddings.Add(await GenerateEmbeddingAsync(text));
        }
        return embeddings;
    }
}
