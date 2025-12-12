using UglyToad.PdfPig;
using System.Text;

public class DocumentReader
{
    public static string ReadPdf(string filePath)
    {
        var text = new StringBuilder();
        
        using (var document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages())
            {
                text.Append(page.Text);
            }
        }
        
        return text.ToString();
    }
    
    public static string ReadText(string filePath)
    {
        return File.ReadAllText(filePath);
    }
}
