using SpreadsheetEngine;

namespace SpreadsheetEngine.Tests;

public class SyntaxTests
{
    [Theory]
    [InlineData("1")]
    [InlineData("A1")]
    [InlineData("A1 = (not B2)")]
    [InlineData("(A1 + B1) * 2 / 3 - 1")]
    [InlineData("mmax(A1, B1 * 2, 3) > min(C1, 4) + 1")]
    [InlineData("not mmin(1) = max(1, 2)")]
    public void ValidExpressions(string expr) => Assert.Null(Parser.Validate(expr));

    [Theory]
    [InlineData("", 1)]
    [InlineData("   ", 1)]
    [InlineData("(1 + 1", 7)]
    [InlineData("1 + 1)", 6)]
    [InlineData("()", 2)]
    [InlineData("1 < 2 < 3", 7)]
    [InlineData("max(1)", 6)]
    [InlineData("max(1,2,3)", 8)]
    [InlineData("mmax()", 6)]
    [InlineData("mmin(1, 2", 10)]
    [InlineData("max 1, 2", 5)]
    [InlineData("1 = foo", 5)]
    [InlineData("1 % 2", 3)]
    [InlineData("1 +", 4)]
    [InlineData("* 2", 1)]
    [InlineData("1 + * 2", 5)]
    [InlineData("-5", 1)]
    [InlineData("12A", 3)]
    [InlineData("A0", 1)]
    [InlineData("1 + not 0", 5)]
    [InlineData("1 2", 3)]
    [InlineData("1 and 2", 3)]
    public void InvalidExpressions_ReportPosition(string expr, int position)
    {
        var error = Parser.Validate(expr);
        Assert.NotNull(error);
        Assert.Equal(position, error.Position);
        Assert.StartsWith($"Синтаксична помилка (позиція {position})", error.Message);
    }

    [Fact]
    public void Parse_Throws_SyntaxErrorException() =>
        Assert.Throws<SyntaxErrorException>(() => Parser.Parse("mmax(1,"));

    [Fact]
    public void UnknownIdentifier_LengthCoversWholeWord()
    {
        var error = Parser.Validate("1 = hello")!;
        Assert.Equal(5, error.Position);
        Assert.Equal(5, error.Length);
    }

    [Fact]
    public void References_ListsAllCells()
    {
        var ast = Parser.Parse("mmax(A1, B2, 3) + C3 > A1");
        Assert.Equal(["A1", "B2", "C3", "A1"], ast.References().Select(a => a.Name));
    }
}
