using SpreadsheetEngine;
using static SpreadsheetEngine.Tests.Helpers;

namespace SpreadsheetEngine.Tests;

public class EvaluationTests
{
    [Theory]
    [InlineData("0", "0")]
    [InlineData("42", "42")]
    [InlineData("12345678901234567890123456789", "12345678901234567890123456789")]
    [InlineData("true", "ІСТИНА")]
    [InlineData("false", "ХИБНІСТЬ")]
    [InlineData("((7))", "7")]
    public void Constants(string expr, string expected) => Assert.Equal(expected, Eval(expr));

    [Theory]
    [InlineData("2 + 3", "5")]
    [InlineData("10 - 15", "-5")]
    [InlineData("6 * 7", "42")]
    [InlineData("20 / 6", "3")]
    [InlineData("0 - 7 / 2", "-3")]
    [InlineData("99999999999999999999 * 99999999999999999999", "9999999999999999999800000000000000000001")]
    [InlineData("100000000000000000000 / 4", "25000000000000000000")]
    [InlineData("true + true", "2")]
    public void Arithmetic(string expr, string expected) => Assert.Equal(expected, Eval(expr));

    [Theory]
    [InlineData("max(3, 7)", "7")]
    [InlineData("min(3, 7)", "3")]
    [InlineData("max(min(10, 4), 2)", "4")]
    [InlineData("mmax(5)", "5")]
    [InlineData("mmax(1, 9, 4, 7)", "9")]
    [InlineData("mmin(8, 3, 6)", "3")]
    [InlineData("mmin(10 - 20, 0, 5)", "-10")]
    [InlineData("mmax(2 * 3, max(4, 5), 1)", "6")]
    [InlineData("mmax(99999999999999999999, 1, 2)", "99999999999999999999")]
    public void Functions(string expr, string expected) => Assert.Equal(expected, Eval(expr));

    [Theory]
    [InlineData("5 = 5", "ІСТИНА")]
    [InlineData("5 = 6", "ХИБНІСТЬ")]
    [InlineData("3 < 5", "ІСТИНА")]
    [InlineData("5 < 5", "ХИБНІСТЬ")]
    [InlineData("5 > 3", "ІСТИНА")]
    [InlineData("2 + 2 = 4", "ІСТИНА")]
    [InlineData("10 / 3 > 3", "ХИБНІСТЬ")]
    [InlineData("true = (2 > 1)", "ІСТИНА")]
    public void Comparisons(string expr, string expected) => Assert.Equal(expected, Eval(expr));

    [Theory]
    [InlineData("not true", "ХИБНІСТЬ")]
    [InlineData("not 0", "ІСТИНА")]
    [InlineData("not 5", "ХИБНІСТЬ")]
    [InlineData("not not 5", "ІСТИНА")]
    [InlineData("not 2 + 2 = 5", "ІСТИНА")]
    public void Not(string expr, string expected) => Assert.Equal(expected, Eval(expr));

    [Theory]
    [InlineData("2 + 3 * 4", "14")]
    [InlineData("(2 + 3) * 4", "20")]
    [InlineData("20 - 6 - 4", "10")]
    [InlineData("100 / 10 / 5", "2")]
    [InlineData("8 - 2 * 3 + 1", "3")]
    [InlineData("1 + 2 > 2", "ІСТИНА")]
    [InlineData("not 1 > 2", "ІСТИНА")]
    public void Precedence(string expr, string expected) => Assert.Equal(expected, Eval(expr));

    [Theory]
    [InlineData("  mmax ( 1 ,   2 , 3 )   =   3  ")]
    [InlineData("NOT FALSE = (MMIN(4, 2) * 2 = 4)")]
    [InlineData("mіn(4, 9) = 4")]
    public void WhitespaceCaseAndCyrillic(string expr) => Assert.Equal("ІСТИНА", Eval(expr));

    [Fact]
    public void DivisionByZero_IsEvaluationError()
    {
        var s = new Spreadsheet(1, 2);
        s.SetExpression("A1", "5 / B1");
        Assert.Equal("Ділення на нуль", s["A1"].Error);
        Assert.Null(s["A1"].SyntaxError);
    }
}
