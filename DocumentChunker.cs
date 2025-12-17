public class DocumentChunker
{
    private readonly int _chunkSize;
    private readonly int _overlap;
    
    public DocumentChunker(int chunkSize = 500, int overlap = 50)
    {
        _chunkSize = chunkSize;
        _overlap = overlap;
    }
    
    public List<string> ChunkText(string text)
    {
        var chunks = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i += (_chunkSize - _overlap))
        {
            var chunk = string.Join(" ", words.Skip(i).Take(_chunkSize));
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }
        }
        
        return chunks;
    }
}
