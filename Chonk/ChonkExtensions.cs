namespace Chonk;

public static class ChonkExtensions
{
    private static readonly IReadOnlyList<string> EnglishProseDelimiters = new List<string>()
    {
        "\n\n", "\r\n", "\n", ".", "!", ",", " "
    };
    
    private static readonly IReadOnlyList<string> EnglishWithLineBreaksDelimiters = new List<string>()
    {
        "\n\n", "\r\n", ".", "!", ",", " "
    };

    // thanks to https://github.com/microsoft/semantic-kernel
    private static readonly IReadOnlyList<string> MarkdownDelimiters = new List<string>()
    {
        ".", "?!", ";", ":", ",", ")]}", " ", "-", "\n\r"
    };

    public static IEnumerable<TextChunk> ChunkEnglishProse(this string text, int maxChunkSize = 512,
        Func<string, int>? lengthFunc = null)
    {
        return Chonk.Chunk(text, maxChunkSize, EnglishProseDelimiters, lengthFunc);
    }
    
    public static IEnumerable<TextChunk> ChunkEnglishWithWrappedLines(this string text, int maxChunkSize = 512,
        Func<string, int>? lengthFunc = null)
    {
        return Chonk.Chunk(text, maxChunkSize, EnglishWithLineBreaksDelimiters, lengthFunc);
    }

    public static IEnumerable<TextChunk> ChunkMarkdownText(this string text, int maxChunkSize = 512,
        Func<string, int>? lengthFunc = null)
    {
        return Chonk.Chunk(text, maxChunkSize, MarkdownDelimiters, lengthFunc);
    }
    

}