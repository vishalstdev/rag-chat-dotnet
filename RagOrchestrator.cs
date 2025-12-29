using Microsoft.SemanticKernel.ChatCompletion;

public interface IRagOrchestrator
{
    Task<string> AskAsync(string query);
}

public class RagOrchestrator : IRagOrchestrator
{
    private readonly IChatCompletionService _chat;
    private readonly IDocumentStore _documentStore;
    private readonly ChatHistory _history = new();

    public RagOrchestrator(IChatCompletionService chat, IDocumentStore documentStore)
    {
        _chat = chat;
        _documentStore = documentStore;
    }

    public async Task<string> AskAsync(string query)
    {
        var contextMessage = await BuildContextMessageAsync(query);
        
        _history.AddUserMessage(contextMessage);
        var response = await _chat.GetChatMessageContentAsync(_history);
        _history.AddAssistantMessage(response.Content);
        
        return response.Content;
    }

    private async Task<string> BuildContextMessageAsync(string query)
    {
        var relevantChunks = await _documentStore.RetrieveRelevantContextAsync(query);
        var chunksList = relevantChunks.ToList();
        
        if (chunksList.Count > 0)
        {
            Console.WriteLine($"\n{Symbols.Info} Found {chunksList.Count} relevant chunks");
            var relevantContext = string.Join("\n\n", chunksList);
            return $"Answer the question based on this context:\n\n{relevantContext}\n\nQuestion: {query}";
        }
        
        return query;
    }
}
