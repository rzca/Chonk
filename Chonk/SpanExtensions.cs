namespace Chonk;

internal static class SpanExtensions
{
    internal static IEnumerable<int> IndexesOf(this ReadOnlySpan<char> text, string delimiter)
    {
        return text.IndexesOf(delimiter, StringComparison.Ordinal);
    }
    
    internal static IEnumerable<int> IndexesOf(this ReadOnlySpan<char> text, string delimiter, StringComparison comparisonType)
    {
        var indexesOfDelimiter = new List<int>();
        int index = -1;

        while ((index = text.IndexOf(delimiter, index + 1, comparisonType)) != -1)
        {
            indexesOfDelimiter.Add(index);
        }

        return indexesOfDelimiter;
    }

    internal static int IndexOf(this ReadOnlySpan<char> text, string delimiter, int startIndex)
    {
        return text.IndexOf(delimiter, startIndex, StringComparison.CurrentCulture);
    }

    internal static int IndexOf(this ReadOnlySpan<char> text, string delimiter, int startIndex,
        StringComparison comparisonType)
    {
        if (startIndex < 0 || startIndex > text.Length + 1)
        {
            throw new IndexOutOfRangeException("startIndex out of range");
        }

        var indexWithoutAddingStartIndex = text.Slice(startIndex).IndexOf(delimiter.AsSpan(), comparisonType);

        return indexWithoutAddingStartIndex == -1 ? -1 : indexWithoutAddingStartIndex + startIndex;
    }
}