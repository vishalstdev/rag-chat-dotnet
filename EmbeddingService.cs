#pragma warning disable CS0618 // Suppress obsolete warning for now

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
}