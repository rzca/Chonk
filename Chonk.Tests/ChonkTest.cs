using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chonk.Tests;

public class Tests
{
    private static readonly string TestResourcesPath =
        Path.Combine(TestContext.CurrentContext.TestDirectory, "TestResources");

    private const string MagnaCartaName = "Magna Carta";
    private const string MarkdownSyntaxDocumentName = "Markdown Syntax Document";
    
    private static readonly string MagnaCarta = File.ReadAllText(Path.Combine(TestResourcesPath, "MagnaCarta.txt"));
    private static readonly string MarkdownSyntaxDocument = File.ReadAllText(Path.Combine(TestResourcesPath, "MarkdownSyntax.md"));

    private static readonly Dictionary<string, string> documents =
        new Dictionary<string, string>() { { MagnaCartaName, MagnaCarta }, { MarkdownSyntaxDocumentName, MarkdownSyntaxDocument } };

    [Test]
    public void Smoketest()
    {
        var document =
            "This is the string that we want to split. We want to try to split it into sentences if possible, but this sentence is long.";

        var chunks = Chonk.Chunk(document, maxChunkSize: 60).ToList();
        
        foreach (var chunk in chunks)
        {
            Console.WriteLine($"{chunk.text} (starts at index {chunk.startingPos})");
        }
        
        Assert.That(chunks,
            Is.EqualTo(new List<TextChunk>()
            {
                new TextChunk("This is the string that we want to split.", 0),
                new TextChunk("We want to try to split it into sentences if possible,", 42),
                new TextChunk("but this sentence is long.", 97)
            }));

        // This is the string that we want to split. (starts at index 0)
        // We want to try to split it into sentences if possible, (starts at index 42)
        // but this sentence is long. (starts at index 97)
    }
    
    [Test]
    public void SmoketestCustomLength()
    {
        var document =
            "This is the string that we want to split. We want to try to split it into sentences if possible, but this sentence is long.";
        
        var chunks = Chonk.Chunk(document, maxChunkSize: 60, lengthFunc: str => str.Length).ToList();
        
        foreach (var chunk in chunks)
        {
            Console.WriteLine($"{chunk.text} (starts at index {chunk.startingPos})");
        }
        
        Assert.That(chunks,
            Is.EqualTo(new List<TextChunk>()
            {
                new TextChunk("This is the string that we want to split.", 0),
                new TextChunk("We want to try to split it into sentences if possible,", 42),
                new TextChunk("but this sentence is long.", 97)
            }));
        
        // This is the string that we want to split. (starts at index 0)
        // We want to try to split it into sentences if possible, (starts at index 42)
        // but this sentence is long. (starts at index 97)
    }
    
    [Test]
    public void WhitespaceIsTrimmed()
    {
        var document =
            "This is the string that we want to split. We want to try to split it into sentences if possible, but this sentence is long.";

        var chunks = Chonk.Chunk(document, maxChunkSize: 50).ToList();

        foreach (var chunk in chunks)
        {
            Assert.That(!chunk.text.StartsWith(" "));
        }
    }


    [TestCase(MagnaCartaName)]
    [TestCase(MarkdownSyntaxDocumentName)]
    public void SplitsTextFast(string documentName)
    {
        const int chunkSize = 500;
        
        var chunked = Chonk.Chunk(documents[documentName], chunkSize).ToList();
        
        var calculatedLength = chunked.Last().startingPos + chunked.Last().text.Length;
        Assert.That(calculatedLength, Is.EqualTo(documents[documentName].Length));


        var startingPos = 0;
        foreach (var chunk in chunked)
        {
            Assert.Multiple(() =>
            {
                Assert.That(chunk.text, Has.Length.GreaterThan(2));
                Assert.That(chunk.text, Has.Length.GreaterThan(chunkSize / 3));
                Assert.That(chunk.text, Is.EqualTo(documents[documentName].Substring(chunk.startingPos, chunk.text.Length)));
                Assert.That(chunk.text, Has.Length.LessThanOrEqualTo(chunkSize));
                Assert.That(chunk.startingPos, Is.GreaterThanOrEqualTo(startingPos)); // show that it is sorted
            });

            startingPos = chunk.startingPos;
        }
    }

    [TestCase(MagnaCartaName)]
    [TestCase(MarkdownSyntaxDocumentName)]
    public void SplitsTextCustomLengthFunc(string documentName)
    {
        const int chunkSize = 400;
        Func<string, int> lengthFunc = str => str.Length / 4;
        
        var chunked = Chonk.Chunk(documents[documentName], chunkSize, lengthFunc: lengthFunc).ToList();

        var calculatedLength = chunked.Last().startingPos + chunked.Last().text.Length;
        Assert.That(calculatedLength, Is.EqualTo(documents[documentName].Length));
        
        var startingPos = 0;
        foreach (var chunk in chunked)
        {
            Assert.Multiple(() =>
            {
                Assert.That(lengthFunc(chunk.text), Is.GreaterThan(2));
                Assert.That(lengthFunc(chunk.text), Is.GreaterThan(chunkSize / 3));
                Assert.That(chunk.text,
                    Is.EqualTo(documents[documentName].Substring(chunk.startingPos, chunk.text.Length)));
                Assert.That(lengthFunc(chunk.text), Is.LessThanOrEqualTo(chunkSize));
                Assert.That(chunk.startingPos, Is.GreaterThanOrEqualTo(startingPos)); // show that it is sorted
            });

            startingPos = chunk.startingPos;
        }
    }
    
    [Test]
    public void SplitsSmallTextFast()
    {
        const string text = "this is some text! It is not a very long text, but it will do for a test. \n\n This is the next paragraph.";
        var chunked = Chonk.Chunk(text, 25).ToList();

        var maxLength = chunked.Select(m => m.text.Length).Max();

        Assert.That(maxLength, Is.LessThanOrEqualTo(25));
        var calculatedLength = chunked.Last().startingPos + chunked.Last().text.Length;
        Assert.That(calculatedLength, Is.EqualTo(text.Length));
    }

    [Test]
    public void SplitsSmallTextCustomLengthFunc()
    {
        var text = "this is some text! It is not a very long text, but it will do for a test. \n\n This is the next paragraph.";
        var chunked = Chonk.Chunk(text, 25, lengthFunc: str => str.Length / 4).ToList();

        var maxLength = chunked.Select(m => m.text.Length).Max();

        Assert.GreaterOrEqual(512, maxLength);
        var calculatedLength = chunked.Last().startingPos + chunked.Last().text.Length;
        Assert.That(calculatedLength, Is.EqualTo(text.Length));
    }

    [Test]
    public void TestIgnoreDelimitersAtEndCustomLengthFunc()
    {
        var text =
            "JOHN, by the grace of God King of England to his archbishops, bishops, abbots, servants, and to all his officials and loyal subjects, Greeting.";

        var mid = Chonk.FindClosestToMiddle(text, ".", str => str.Length);

        Assert.That(mid, Is.Null);
    }
    
    [Test]
    public void TestIgnoreDelimitersAtEndFast()
    {
        var text =
            "JOHN, by the grace of God King of England to his archbishops, bishops, abbots, servants, and to all his officials and loyal subjects, Greeting.";

        var mid = Chonk.FindClosestToMiddle(text, ".");

        Assert.That(mid, Is.Null);
    }
    
    [Test]
    public void DelimiterIsTwoNewlines()
    {
        var text =
            "This is line 1\nthis is line two\n\nthis is line three";

        var chunks = Chonk.Chunk(text, maxChunkSize: 20, delimiters: new[] { "\n\n", "\n", " " }).ToList();

        Assert.That(chunks,
            Is.EqualTo(new List<TextChunk>()
            {
                new TextChunk("This is line 1\n", 0),
                new TextChunk("this is line two\n\n", 15),
                new TextChunk("this is line three", 33)
            }));
    }

    [Test]
    public void FindMidpointCustomLengthFunc()
    {
        var text = "this is some text, It is not a very long text, but it, will do for a test, This is the next, paragraph.";
        var midpointIndex = Chonk.FindClosestToMiddle(text, ",", str => str.Length);
        Assert.That(midpointIndex, Is.EqualTo(54));
    }
    
    [Test]
    public void FindMidpointFast()
    {
        var text = "this is some text, It is not a very long text, but it, will do for a test, This is the next, paragraph.";
        var midpointIndex = Chonk.FindClosestToMiddle(text, ",");
        Assert.That(midpointIndex, Is.EqualTo(54));
    }

    [Test]
    public void FindMidpointIsNullCustomLengthFunc()
    {
        var text = "this is some text, It is not a very long text.";

        var midpointIndex = Chonk.FindClosestToMiddle(text, "-", str => str.Length);

        Assert.That(midpointIndex, Is.Null);
    }
    
    [Test]
    public void FindMidpointIsNullFast()
    {
        var text = "this is some text, It is not a very long text.";

        var midpointIndex = Chonk.FindClosestToMiddle(text, "-");

        Assert.That(midpointIndex, Is.Null);
    }

    [Test]
    public void FindMidpoint()
    {
        var text = "aaaaaaaa-aaaaaaaa-aaaaaaaa-aaaaaaaa-aaaaaaaa-aaaaaaaa-bbbbbbbb-bbbbbbbb-bbbbbbbb-bbbbbbbb-bbbbbbbb-bbbbbbbb";
        var length = text.Length;
        var lengthFunc = (string str) => str.Count(s => s.Equals('a') || s.Equals('-'));

        var mid = Chonk.FindClosestToMiddle(text, "-");
        var leftLength = lengthFunc(text.Substring(0, mid.Value));
        var leftFraction = (double)leftLength / (double)lengthFunc(text);

        Assert.That(leftFraction < .4 || leftFraction > .6);

        var mid2 = Chonk.FindClosestToMiddle(text, "-", lengthFunc);
        var leftLength2 = lengthFunc(text.Substring(0, mid2.Value));
        var leftFraction2 = (double)leftLength2 / (double)lengthFunc(text);

        Assert.That(leftFraction2 > .4 && leftFraction2 < .6);
    }
    
    [Test]
    public void FindApproximateMidpointCustomLengthFunc()
    {
        var text = "aaaaaaaa-aaaaaaaa-aaaaaaaa-aaaaaaaa-bbbbbbbb-bbbbbbbb-bbbbbbbb-bbbbbbbb";
        var length = text.Length;

        var lengthFunc = (string str) => str.Count(s => s.Equals('a') || s.Equals('-'));
        var mid = Chonk.FindApproximateMidpoint(text, str => str.Count(s => s.Equals('a') || s.Equals('-')));

        var leftLength = lengthFunc(text.Substring(0, mid));
        var leftFraction = (double)leftLength / (double)lengthFunc(text);

        Assert.That(leftFraction >= .4 && leftFraction < .6);
    }
    
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(10)]
    [TestCase(20)]
    [TestCase(100)]
    public void VerySmallChunksCustomLength(int maxChunkSize)
    {
        const string text = "shopping shopping shopping. shopping shopping, shopping shopping shopping";

        var tokens = new List<string>() { "shop", "ing", " " };

        var getLengthInTokens = (string str) =>
        {
            var tokenCounts = tokens.Select(token => new { token, occurences = str.AsSpan().IndexesOf(token).Count() })
                .ToList();

            var unmatchedChars = str.Length -
                                 tokenCounts.Select(tokenCount => tokenCount.token.Length * tokenCount.occurences)
                                     .Sum();

            return unmatchedChars + tokenCounts.Select(tokenCount => tokenCount.occurences).Sum();
        };

        var chunks = Chonk.Chunk(text, maxChunkSize: maxChunkSize, lengthFunc: getLengthInTokens);

        foreach (var chunk in chunks)
        {
            Assert.That(getLengthInTokens(chunk.text), Is.LessThanOrEqualTo(maxChunkSize));
        }
    }
    
    
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(10)]
    [TestCase(20)]
    [TestCase(100)]
    public void VerySmallChunks(int maxChunkSize)
    {
        const string text = "shopping shopping shopping. shopping shopping, shopping shopping shopping";

        var chunks = Chonk.Chunk(text, maxChunkSize: maxChunkSize);

        foreach (var chunk in chunks)
        {
            Assert.That(chunk.text.Length, Is.LessThanOrEqualTo(maxChunkSize));
        }
    }
}