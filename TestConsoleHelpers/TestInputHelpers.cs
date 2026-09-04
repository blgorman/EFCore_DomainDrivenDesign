using ConsoleHelpers;
using Shouldly;

namespace TestConsoleHelpers;

public class TestInputHelpers : IDisposable
{
    private readonly TextReader _originalIn = Console.In;
    private readonly TextWriter _originalOut = Console.Out;

    public TestInputHelpers()
    {
        // suppress console output during tests
        Console.SetOut(new StringWriter());
    }

    public void Dispose()
    {
        Console.SetIn(_originalIn);
        Console.SetOut(_originalOut);
    }

    private void SetInput(string input) => Console.SetIn(new StringReader(input));

    // -------------------------------------------------------------------------
    // GetInputAsDouble
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("3.14", 3.14)]
    [InlineData("0", 0.0)]
    [InlineData("-1.5", -1.5)]
    public void GetInputAsDouble_ValidInput_ReturnsExpected(string input, double expected)
    {
        SetInput(input);
        InputHelpers.GetInputAsDouble("Enter:").ShouldBe(expected);
    }

    [Fact]
    public void GetInputAsDouble_InvalidThenValid_ReturnsValid()
    {
        SetInput("abc\n42.0");
        InputHelpers.GetInputAsDouble("Enter:").ShouldBe(42.0);
    }

    [Fact]
    public void GetInputAsDouble_BelowMinThenValid_ReturnsValid()
    {
        SetInput("1.0\n5.0");
        InputHelpers.GetInputAsDouble("Enter:", min: 3.0).ShouldBe(5.0);
    }

    [Fact]
    public void GetInputAsDouble_AboveMaxThenValid_ReturnsValid()
    {
        SetInput("100.0\n5.0");
        InputHelpers.GetInputAsDouble("Enter:", max: 10.0).ShouldBe(5.0);
    }

    [Fact]
    public void GetInputAsDouble_AtMinBoundary_ReturnsValue()
    {
        SetInput("3.0");
        InputHelpers.GetInputAsDouble("Enter:", min: 3.0, max: 10.0).ShouldBe(3.0);
    }

    [Fact]
    public void GetInputAsDouble_AtMaxBoundary_ReturnsValue()
    {
        SetInput("10.0");
        InputHelpers.GetInputAsDouble("Enter:", min: 3.0, max: 10.0).ShouldBe(10.0);
    }

    // -------------------------------------------------------------------------
    // GetInputAsInt
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("5", 5)]
    [InlineData("-10", -10)]
    [InlineData("0", 0)]
    public void GetInputAsInt_ValidInput_ReturnsExpected(string input, int expected)
    {
        SetInput(input);
        InputHelpers.GetInputAsInt("Enter:").ShouldBe(expected);
    }

    [Fact]
    public void GetInputAsInt_InvalidThenValid_ReturnsValid()
    {
        SetInput("not-a-number\n42");
        InputHelpers.GetInputAsInt("Enter:").ShouldBe(42);
    }

    [Fact]
    public void GetInputAsInt_BelowMinThenValid_ReturnsValid()
    {
        SetInput("1\n10");
        InputHelpers.GetInputAsInt("Enter:", min: 5).ShouldBe(10);
    }

    [Fact]
    public void GetInputAsInt_AboveMaxThenValid_ReturnsValid()
    {
        SetInput("100\n3");
        InputHelpers.GetInputAsInt("Enter:", max: 10).ShouldBe(3);
    }

    [Fact]
    public void GetInputAsInt_AtMinBoundary_ReturnsMin()
    {
        SetInput("5");
        InputHelpers.GetInputAsInt("Enter:", min: 5, max: 10).ShouldBe(5);
    }

    [Fact]
    public void GetInputAsInt_AtMaxBoundary_ReturnsMax()
    {
        SetInput("10");
        InputHelpers.GetInputAsInt("Enter:", min: 5, max: 10).ShouldBe(10);
    }

    [Fact]
    public void GetInputAsInt_DoubleThenValid_ReturnsValid()
    {
        SetInput("3.7\n7");
        InputHelpers.GetInputAsInt("Enter:").ShouldBe(7);
    }

    // -------------------------------------------------------------------------
    // GetInputAsBool
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("Y")]
    [InlineData("y")]
    [InlineData("Yes")]
    [InlineData("YES")]
    [InlineData("yes")]
    public void GetInputAsBool_YesVariants_ReturnsTrue(string input)
    {
        SetInput(input);
        InputHelpers.GetInputAsBool("Continue?").ShouldBeTrue();
    }

    [Theory]
    [InlineData("N")]
    [InlineData("n")]
    [InlineData("No")]
    [InlineData("NO")]
    [InlineData("no")]
    public void GetInputAsBool_NoVariants_ReturnsFalse(string input)
    {
        SetInput(input);
        InputHelpers.GetInputAsBool("Continue?").ShouldBeFalse();
    }

    [Fact]
    public void GetInputAsBool_InvalidThenYes_ReturnsTrue()
    {
        SetInput("maybe\nY");
        InputHelpers.GetInputAsBool("Continue?").ShouldBeTrue();
    }

    [Fact]
    public void GetInputAsBool_InvalidThenNo_ReturnsFalse()
    {
        SetInput("sure\nN");
        InputHelpers.GetInputAsBool("Continue?").ShouldBeFalse();
    }

    [Fact]
    public void GetInputAsBool_MultipleInvalidThenValid_ReturnsTrue()
    {
        SetInput("x\n1\n?\nY");
        InputHelpers.GetInputAsBool("Continue?").ShouldBeTrue();
    }

    // -------------------------------------------------------------------------
    // GetInputAsString
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("hello world")]
    [InlineData("123")]
    [InlineData("test input")]
    public void GetInputAsString_VariousInputs_ReturnsExpected(string input)
    {
        SetInput(input);
        InputHelpers.GetInputAsString("Enter:").ShouldBe(input);
    }

    [Fact]
    public void GetInputAsString_AllowEmptyTrue_ReturnsEmptyString()
    {
        SetInput("");
        InputHelpers.GetInputAsString("Enter:", allowEmpty: true).ShouldBe(string.Empty);
    }

    [Fact]
    public void GetInputAsString_AllowEmptyFalse_EmptyThenValid_ReturnsValid()
    {
        SetInput("\nactual input");
        InputHelpers.GetInputAsString("Enter:", allowEmpty: false).ShouldBe("actual input");
    }

    [Fact]
    public void GetInputAsString_AllowEmptyFalse_WhitespaceThenValid_ReturnsValid()
    {
        SetInput("   \nreal value");
        InputHelpers.GetInputAsString("Enter:", allowEmpty: false).ShouldBe("real value");
    }
}
