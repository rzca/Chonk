namespace Chonk;

/// <summary>
/// Contains utility methods for chunking large texts into smaller strings while attempting to preserve meaning.
/// 
/// See https://github.com/rczca/Chonk
/// </summary>
public static class Chonk
{
    private static readonly IReadOnlyList<string> DefaultEnglishDelimiters = new List<string>()
    {
        "\n\n", "\r\n", "\n", ".", "!", "?", ",", " "
    };

    private const double MinFractionOfLengthOfOneSide = .3;
    private static bool IsBalancedEnough(double fraction) => fraction is > MinFractionOfLengthOfOneSide and < 1 - MinFractionOfLengthOfOneSide;

    /// <summary>
    /// Chunks the given text into smaller chunks with the specified size using a custom length function.
    /// </summary>
    /// <param name="text">The text to be chunked.</param>
    /// <param name="maxChunkSize">The maximum of each chunk as determined by the <paramref name="lengthFunc"/>.</param>
    /// <param name="delimiters">A collection of delimiters that will be recursively used to split the text</param>
    /// <param name="lengthFunc">A function that calculates the length of a string (e.g. when tokenizing).</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Chunk"/> objects representing the chunks.</returns>
    public static IEnumerable<TextChunk> Chunk(string text, int maxChunkSize = 512, IReadOnlyList<string>? delimiters = null, Func<string, int>? lengthFunc = null)
    {
        return ChunkInternal(text.AsSpan(), 0, maxChunkSize, delimiters ?? DefaultEnglishDelimiters, lengthFunc);
    }

    internal static IEnumerable<TextChunk> ChunkInternal(ReadOnlySpan<char> text, int startingPos, int chunkSize, IReadOnlyList<string> delimiters, Func<string, int>? lengthFunc = null)
    {
        if (chunkSize < 1)
        {
            throw new ArgumentException($"{nameof(chunkSize)} cannot be less than one");
        }

        var lengthOfText = lengthFunc?.Invoke(text.ToString()) ?? text.Length;

        // base case: the current text is short enough
        if (lengthOfText <= chunkSize)
        {
            return new List<TextChunk>() { new TextChunk(text.ToString(), startingPos) };
        }

        // if we have exhausted all delimiters, do our best to split the string such that we balance the two halves
        // else, we use the head of the delimiters to find the index of its occurence that balances the two halves
        var maybeMidpointIndex = delimiters switch
        {
            [] => FindApproximateMidpoint(text, lengthFunc),
            [var delimiter, ..] => FindClosestToMiddle(text, delimiter, lengthFunc)
        };

        if (maybeMidpointIndex is null)
        {
            return ChunkInternal(text, startingPos, chunkSize, delimiters.Skip(1).ToList(), lengthFunc);
        }
        
        var midpointIndex = maybeMidpointIndex.Value;

        // include the delimiter at the end of the first chunk but not at the beginning of the second
        var left = text[..midpointIndex];
        var right = text[midpointIndex..];
        
        // For convenience, trim whitespace from the beginning of the second string
        var rightTrimmed = right.TrimStart();
        var charactersTrimmed = right.Length - rightTrimmed.Length;

        return ChunkInternal(left, startingPos, chunkSize, delimiters, lengthFunc)
            .Concat(ChunkInternal(rightTrimmed, startingPos + midpointIndex + charactersTrimmed, chunkSize, delimiters, lengthFunc));
    }

    // This method finds the index of the delimiter that best balances the two sides of the string
    internal static int? FindClosestToMiddle(ReadOnlySpan<char> text, string delimiter, Func<string, int>? lengthFunc = null)
    {
        var lengthOfText = lengthFunc?.Invoke(text.ToString()) ?? text.Length;

        // ReadOnlySpan<char> cannot be captured to be used in lambdas, so we have to be imperative here
        var indexesAndFractions = new List<(int index, double leftSideFractionOfLength)>();
        
        foreach (var index in text.IndexesOf(delimiter, StringComparison.Ordinal))
        {
            var length = lengthFunc?.Invoke(text.ToString().Substring(0, index)) ?? index;
            indexesAndFractions.Add((index, length / (double)lengthOfText));
        }

        // filter indexes to ones that balance the two halves enough and then pick the best one
        var eligibleIndexes = indexesAndFractions
            .Where(frac => IsBalancedEnough(frac.leftSideFractionOfLength))
            .OrderBy(x => Math.Abs(x.leftSideFractionOfLength - .5))
            .ToList();

        return eligibleIndexes switch
        {
            [] => null,
            [var head, ..] => head.index + delimiter.Length
        };
    }

    // Use a binary search to find a midpoint that approximately balances the two sides by length when a custom length
    // function is passed
    internal static int FindApproximateMidpoint(ReadOnlySpan<char> text, Func<string, int>? lengthFunc = null)
    {
        if (lengthFunc is null)
        {
            return text.Length / 2;
        }
        
        var textStr = text.ToString();

        var min = 0;
        var max = text.Length;
        while (min < max)
        {
            var mid = (min + max) / 2;

            var leftLength = lengthFunc(textStr.Substring(0, mid));
            var leftFraction = (double)leftLength / (double)lengthFunc(textStr);

            switch (leftFraction)
            {
                case >= MinFractionOfLengthOfOneSide and < 1 -MinFractionOfLengthOfOneSide:
                    return mid;
                case < MinFractionOfLengthOfOneSide:
                    min = mid + 1;
                    break;
                default:
                    max = mid - 1;
                    break;
            }
        }
        
        // In a true binary search we wouldn't get to this point, but it is possible for a valid tokenizing length
        // function to not be monotonically increasing with the length of the input string.
        // Consider a tokenizer with one token for each letter [a-zA-Z] and also the tokens "fight" and "ing".
        // lengthFunc("fighting") < lengthFunc("figh") + lengthFunc("ting")
        
        // in this case, we just return mid
        return (min + max) / 2;
    }
}