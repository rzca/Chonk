using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Benchmarks;
using Microsoft.CodeAnalysis;
using Microsoft.SemanticKernel.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chonk.Benchmark;

[Config(typeof(AntiVirusFriendlyConfig))]
[ShortRunJob]
public class Benchmarks
{
    private static readonly string testResources =
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    private string MagnaCarta;

    private readonly Consumer Consumer = new Consumer();

    [GlobalSetup]
    public void Setup()
    {
        this.MagnaCarta = File.ReadAllText(Path.Combine(testResources, "TestResources", "MagnaCarta.txt"));
    }

    [Benchmark]
    public void MsChunkMagnaCarta()
    {
        // tokens is currently defined as len / 4
        TextChunker.SplitPlainTextLines(this.MagnaCarta, 400);
    }
    
    [Benchmark]
    public void ChonkMagnaCarta()
    {
        Chonk.Chunk(this.MagnaCarta, 100).Consume(this.Consumer);
    }

    [Benchmark]
    public void ChonkMagnaCartaCustomLengthFunc()
    {
        Chonk.Chunk(this.MagnaCarta, 400, lengthFunc: str => str.Length / 4).Consume(this.Consumer);
    }
}