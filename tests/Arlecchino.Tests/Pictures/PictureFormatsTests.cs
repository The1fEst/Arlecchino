using System.Collections.Generic;
using System.Linq;
using Arlecchino.Pictures;
using Xunit;

namespace Arlecchino.Tests.Pictures;

public sealed class PictureFormatsTests
{
    /// <summary>
    /// A picture is recognized by what is in it rather than by what it is called: a file opened from a
    /// panel is as likely to be named wrongly as rightly.
    /// </summary>
    /// <param name="name">What the format that should claim the head is called.</param>
    /// <param name="head">The head of a file.</param>
    [Theory]
    [MemberData(nameof(Heads))]
    public void EachFormatClaimsItsOwnHead(string name, byte[] head)
    {
        Assert.Equal(name, PictureFormats.For(head)?.Name);
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { 1, 2, 3 })]
    [InlineData(new byte[] { 0x1F, 0x8B, 8, 0, 0, 0, 0, 0, 0, 3, 1, 2, 3, 4, 5, 6, 7, 8 })]
    public void WhatNoFormatClaimsIsNotRead(byte[] bytes)
    {
        Assert.Null(PictureFormats.For(bytes));
        Assert.Null(PictureFormats.Read(bytes));
    }

    /// <summary>Every format is offered a file, so no two of them may answer to the same name.</summary>
    [Fact]
    public void TheFormatsAreNamedApart()
    {
        var names = PictureFormats.All.Select(static format => format.Name).ToList();

        Assert.Equal(names.Count, names.Distinct().Count());
    }

    public static IEnumerable<object[]> Heads() =>
    [
        ["png", new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13 }],
        ["jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xDB, 0, 67, 0 }],
        [
            "bmp",
            new byte[]
            {
                (byte)'B', (byte)'M', 0, 0, 0, 0, 0, 0, 0, 0, 54, 0, 0, 0,
                40, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0,
            },
        ],
        ["qoi", new byte[] { (byte)'q', (byte)'o', (byte)'i', (byte)'f', 0, 0, 0, 1, 0, 0, 0, 1, 4, 0 }],
        ["pnm", "P6\n1 1"u8.ToArray()],
        ["tga", new byte[] { 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 24, 0 }],
    ];
}
