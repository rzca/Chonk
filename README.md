## Chonk
Chonk is a .NET library that makes it easy to split large texts into chunks that try to maintain semantic meaning. This functionality it often used as a preprocessing step when generating vector embeddings of text documents.

### Getting started
Chonk is available for download as a [NuGet package](https://www.nuget.org/packages/Chonk). [![NuGet Status](http://img.shields.io/nuget/v/Chonk.svg?style=flat)](https://www.nuget.org/packages/Chonk/)

```
dotnet add package Chonk
```

The `Chonk` static class contains utility methods for chunking text into `IEnumerable<Chunk>`s.

```C#
var document =
    "This is the string that we want to split. We want to try to split it into sentences if possible, but this sentence is long.";

var chunks = Chonk.Chunk(document, maxChunkSize: 50).ToList();
foreach (var chunk in chunks)
{
    Console.WriteLine($"{chunk.text} (starts at index {chunk.startingPos})");
}

// This is the string that we want to split. (starts at index 0)
// We want to try to split  (starts at index 42)
// it into sentences if possible, (starts at index 66)
// but this sentence is long. (starts at index 97)
```

### Features
Chonk supports:
- splitting text using a custom list of delimiters
- static extension methods for splitting English and Markdown text with sane delimiters
- splitting text using a user-provided custom length function (e.g. a function which tokenizes a string and returns the count of the tokens)
- associating chunks with the index of the section of the text they came from

Chonk does not yet support:
- splitting tokenized documents instead of strings
- creating chunks that overlap each other
- splitting streams

## Documentation
The `Chonk` static class contains utility methods for chunking text into `IEnumerable<Chunk>`s.

You can pass a custom `Func<string, int>` function for measuring the length of a string:
```C#
var document =
    "This is the string that we want to split. We want to try to split it into sentences if possible, but this sentence is long.";

var chunks = Chonk.Chunk(document, maxChunkSize: 30, lengthFunc: str => str.Length / 4).ToList();

foreach (var chunk in chunksCustomLength)
{
    Console.WriteLine($"{chunk.text} (starts at index {chunk.startingPos})");
}

// This is the string that we want to split. (starts at index 0)
// We want to try to split it into sentences if possible, but this sentence is long. (starts at index 42)
```

Chonk is inspired by Langchain's [RecursiveTextSplitter](https://api.python.langchain.com/en/latest/text_splitter/langchain.text_splitter.RecursiveCharacterTextSplitter.html) and Microsoft Semantic Kernel's [TextChunker](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/SemanticKernel/Text/TextChunker.cs).

### Algorithm
Chonk uses a recursive splitting function to split text documents by an ordered list of delimiters.
- (Base case) If the length of the text (as measured with string.Length or a user-provided custom function) is less than or equal to the maxChunkSize, the text is returned.
- if there are no delimiters in the list, it naively splits the text in half, calls itself on each half and returns the concatenated results.
- If the first delimiter is not present in the text, it calls itself on the text with the rest of the list of delimiters.
- If the first delimiter is present in the text, it splits the text into two sub-texts, calls itself on each of them and returns the concatenated results.

### Guarantees
- The length of each chunk will be less than or equal to the maxChunkSize
  - When a custom length function is used, the length will be measured using the custom function
  - When no custom lengthFunc is used, the length is measured by `Func<string, int> lengthFunc = (text) => text.Length`


All string comparisons (such as when finding delimiters in the text) is done using [StringComparison.Ordinal](https://learn.microsoft.com/en-us/dotnet/api/system.stringcomparison?view=net-6.0#system-stringcomparison-ordinal)

## Benchmarks
Chonk includes code for benchmarking using [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet).

```
dotnet run --project Chonk.Benchmark -c Release
```