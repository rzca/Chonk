using System;
using System.Collections.Generic;
using System.Linq;

namespace Chonk.Tests;

public class SpanExtensionsTest
{
    [TestCase(0, 3)]
    [TestCase(3, 3)]
    [TestCase(4, 7)]
    public void IndexOfTest(int startIndex, int expectedResponse)
    {
        var document = "ooo.ooo.ooo";
        
        var index = document.AsSpan().IndexOf(".", startIndex: 0, StringComparison.Ordinal);
        Assert.That(index, Is.EqualTo(3));
        
        var indexWithStartIndex3 = document.AsSpan().IndexOf(".", startIndex: 3, StringComparison.Ordinal);
        Assert.That(indexWithStartIndex3, Is.EqualTo(3));
        
        var indexWithStartIndex4 = document.AsSpan().IndexOf(".", startIndex: 4, StringComparison.Ordinal);
        Assert.That(indexWithStartIndex4, Is.EqualTo(7));
    }

    [Test]
    public void IndexOfThrowsOnStartIndexOutOfBounds()
    {
        var document = "ooo.ooo.ooo";

        Assert.Throws<IndexOutOfRangeException>(() => document.AsSpan().IndexOf(".", 11));
        Assert.Throws<IndexOutOfRangeException>(() => document.AsSpan().IndexOf(".", -1));
    }
    
    [TestCase(".", new int[] { 3, 7 })]
    [TestCase("o", new int[] { 0, 1, 2, 4, 5, 6, 8, 9, 10})]
    [TestCase("x", new int[] {}) ]
    public void IndexesOfTest(string delimiter, IEnumerable<int> expectedIndexes)
    {
        var document =
            "ooo.ooo.ooo";
        
        var indexes = document.AsSpan().IndexesOf(delimiter, StringComparison.Ordinal).ToList();
        
        Assert.That(indexes, Is.EqualTo(expectedIndexes));
    }
    
    [Test]
    public void DelimiterIsAString()
    {
        var document = "ooo..ooo..ooo";

        var indexes = document.AsSpan().IndexesOf("..");
        Assert.That(indexes, Is.EqualTo(new int[] { 3, 8 }) );
    }
}