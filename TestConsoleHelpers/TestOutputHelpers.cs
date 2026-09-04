using ConsoleHelpers;
using Shouldly;

namespace TestConsoleHelpers;

public class TestOutputHelpers
{
    private const int DefaultLength = 80;

    // -------------------------------------------------------------------------
    // BoxedMessage
    // -------------------------------------------------------------------------

    [Fact]
    public void BoxedMessage_StarBorder_TopAndBottomAreFull80Stars()
    {
        var result = OutputHelpers.BoxedMessage("Hello", '*');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[0].ShouldBe(new string('*', DefaultLength));
        lines[2].ShouldBe(new string('*', DefaultLength));
    }

    [Fact]
    public void BoxedMessage_StarBorder_MessageLineStartsAndEndsWithStar()
    {
        var result = OutputHelpers.BoxedMessage("Hello", '*');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[1].ShouldStartWith("*");
        lines[1].ShouldEndWith("*");
    }

    [Fact]
    public void BoxedMessage_StarBorder_MessageLineContainsMessage()
    {
        var result = OutputHelpers.BoxedMessage("Hello", '*');
        result.ShouldContain("Hello");
    }

    [Fact]
    public void BoxedMessage_StarBorder_MessageLineIsFullLineLength()
    {
        var result = OutputHelpers.BoxedMessage("Hello", '*');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[1].Length.ShouldBe(DefaultLength);
    }

    [Fact]
    public void BoxedMessage_NonStarBorder_TopAndBottomAreDashes()
    {
        var result = OutputHelpers.BoxedMessage("Hello", '-');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[0].ShouldBe(new string('-', DefaultLength));
        lines[2].ShouldBe(new string('-', DefaultLength));
    }

    [Fact]
    public void BoxedMessage_NonStarBorder_MessageLineUsePipeIndicators()
    {
        var result = OutputHelpers.BoxedMessage("Hello", '-');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[1].ShouldStartWith("|");
        lines[1].ShouldEndWith("|");
    }

    [Theory]
    [InlineData('*')]
    [InlineData('-')]
    [InlineData('=')]
    public void BoxedMessage_AnyBorder_ProducesThreeNonEmptyLines(char border)
    {
        var result = OutputHelpers.BoxedMessage("Test", border);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines.Length.ShouldBe(3);
    }

    [Fact]
    public void BoxedMessage_CustomLineLength_BordersMatchCustomLength()
    {
        var result = OutputHelpers.BoxedMessage("Hi", '*', lineLength: 40);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[0].Length.ShouldBe(40);
        lines[2].Length.ShouldBe(40);
        lines[1].Length.ShouldBe(40);
    }

    // -------------------------------------------------------------------------
    // BoxedMessageWithTitle
    // -------------------------------------------------------------------------

    [Fact]
    public void BoxedMessageWithTitle_ContainsTitle()
    {
        var result = OutputHelpers.BoxedMessageWithTitle("My Title", "My Message");
        result.ShouldContain("My Title");
    }

    [Fact]
    public void BoxedMessageWithTitle_ContainsMessage()
    {
        var result = OutputHelpers.BoxedMessageWithTitle("My Title", "My Message");
        result.ShouldContain("My Message");
    }

    [Fact]
    public void BoxedMessageWithTitle_ContainsDashSeparators()
    {
        var result = OutputHelpers.BoxedMessageWithTitle("Title", "Message");
        var expectedDashes = $"*{new string('-', DefaultLength - 2)}*";
        result.ShouldContain(expectedDashes);
    }

    [Fact]
    public void BoxedMessageWithTitle_StartsWithStarBorder()
    {
        var result = OutputHelpers.BoxedMessageWithTitle("Title", "Message");
        result.ShouldStartWith(new string('*', DefaultLength));
    }

    [Fact]
    public void BoxedMessageWithTitle_EndsWithStarBorder()
    {
        var result = OutputHelpers.BoxedMessageWithTitle("Title", "Message").TrimEnd();
        result.ShouldEndWith(new string('*', DefaultLength));
    }

    [Fact]
    public void BoxedMessageWithTitle_AllContentLinesAreFullLineLength()
    {
        var result = OutputHelpers.BoxedMessageWithTitle("Title", "Message");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            line.Length.ShouldBe(DefaultLength);
        }
    }

    // -------------------------------------------------------------------------
    // BoxedArrayWithTitle
    // -------------------------------------------------------------------------

    [Fact]
    public void BoxedArrayWithTitle_ContainsTitle()
    {
        var result = OutputHelpers.BoxedArrayWithTitle("Header", ["Item1", "Item2"]);
        result.ShouldContain("Header");
    }

    [Fact]
    public void BoxedArrayWithTitle_ContainsAllItems()
    {
        var items = new[] { "Alpha", "Beta", "Gamma" };
        var result = OutputHelpers.BoxedArrayWithTitle("Header", items);

        foreach (var item in items)
        {
            result.ShouldContain(item);
        }
    }

    [Fact]
    public void BoxedArrayWithTitle_EndsWithStarBorder()
    {
        var result = OutputHelpers.BoxedArrayWithTitle("Header", ["A", "B"]).TrimEnd();
        result.ShouldEndWith(new string('*', DefaultLength));
    }

    [Fact]
    public void BoxedArrayWithTitle_HasDashSeparatorBetweenItems()
    {
        var result = OutputHelpers.BoxedArrayWithTitle("Header", ["Item1", "Item2"]);
        var expectedDashes = $"*{new string('-', DefaultLength - 2)}*";
        result.ShouldContain(expectedDashes);
    }

    [Fact]
    public void BoxedArrayWithTitle_SingleItem_NoDashSeparator()
    {
        var result = OutputHelpers.BoxedArrayWithTitle("Header", ["OnlyItem"]);
        var dashedSeparator = $"*{new string('-', DefaultLength - 2)}*";
        // The title box uses no dashes; only inter-item separators use them
        result.ShouldNotContain(dashedSeparator);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void BoxedArrayWithTitle_CorrectNumberOfItems_AllPresent(int count)
    {
        var items = Enumerable.Range(1, count).Select(i => $"Item {i}").ToArray();
        var result = OutputHelpers.BoxedArrayWithTitle("Header", items);

        foreach (var item in items)
        {
            result.ShouldContain(item);
        }
    }

    // -------------------------------------------------------------------------
    // BoxedList
    // -------------------------------------------------------------------------

    [Fact]
    public void BoxedList_StarBorder_TopAndBottomAreFull80Stars()
    {
        var items = new List<string> { "One", "Two" };
        var result = OutputHelpers.BoxedList(items, '*');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[0].ShouldBe(new string('*', DefaultLength));
        lines.Last().ShouldBe(new string('*', DefaultLength));
    }

    [Fact]
    public void BoxedList_StarBorder_ItemLinesStartAndEndWithStar()
    {
        var items = new List<string> { "Apple", "Banana" };
        var result = OutputHelpers.BoxedList(items, '*');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // lines[0] = top border, lines[1] = Apple, lines[2] = Banana, lines[3] = bottom border
        lines[1].ShouldStartWith("*");
        lines[1].ShouldEndWith("*");
        lines[2].ShouldStartWith("*");
        lines[2].ShouldEndWith("*");
    }

    [Fact]
    public void BoxedList_NonStarBorder_ItemLinesUsePipeIndicators()
    {
        var items = new List<string> { "Apple" };
        var result = OutputHelpers.BoxedList(items, '-');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[1].ShouldStartWith("|");
        lines[1].ShouldEndWith("|");
    }

    [Fact]
    public void BoxedList_ContainsAllItems()
    {
        var items = new List<string> { "Red", "Green", "Blue" };
        var result = OutputHelpers.BoxedList(items, '*');

        foreach (var item in items)
        {
            result.ShouldContain(item);
        }
    }

    [Fact]
    public void BoxedList_ItemLinesAreFullLineLength()
    {
        var items = new List<string> { "Hello" };
        var result = OutputHelpers.BoxedList(items, '*');
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines[1].Length.ShouldBe(DefaultLength);
    }

    [Fact]
    public void BoxedList_CustomLineLength_AllLinesMatchCustomLength()
    {
        var items = new List<string> { "A", "B" };
        var result = OutputHelpers.BoxedList(items, '*', lineLength: 40);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            line.Length.ShouldBe(40);
        }
    }

    [Theory]
    [InlineData('*')]
    [InlineData('-')]
    [InlineData('=')]
    public void BoxedList_AnyBorder_ProducesCorrectLineCount(char border)
    {
        var items = new List<string> { "X", "Y", "Z" };
        var result = OutputHelpers.BoxedList(items, border);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // top border + 3 items + bottom border = 5
        lines.Length.ShouldBe(5);
    }

    // -------------------------------------------------------------------------
    // BoxedListWithTitle
    // -------------------------------------------------------------------------

    [Fact]
    public void BoxedListWithTitle_ContainsTitle()
    {
        var result = OutputHelpers.BoxedListWithTitle("My Title", ["Item1"]);
        result.ShouldContain("My Title");
    }

    [Fact]
    public void BoxedListWithTitle_ContainsAllItems()
    {
        var items = new List<string> { "First", "Second", "Third" };
        var result = OutputHelpers.BoxedListWithTitle("Header", items);

        foreach (var item in items)
        {
            result.ShouldContain(item);
        }
    }

    [Fact]
    public void BoxedListWithTitle_EndsWithStarBorder()
    {
        var result = OutputHelpers.BoxedListWithTitle("Title", ["A", "B"]).TrimEnd();
        result.ShouldEndWith(new string('*', DefaultLength));
    }

    [Fact]
    public void BoxedListWithTitle_HasDashSeparatorBetweenItems()
    {
        var result = OutputHelpers.BoxedListWithTitle("Title", ["Item1", "Item2"]);
        var expectedDashes = $"*{new string('-', DefaultLength - 2)}*";
        result.ShouldContain(expectedDashes);
    }

    [Fact]
    public void BoxedListWithTitle_SingleItem_NoDashSeparator()
    {
        var result = OutputHelpers.BoxedListWithTitle("Title", ["OnlyItem"]);
        var dashedSeparator = $"*{new string('-', DefaultLength - 2)}*";
        result.ShouldNotContain(dashedSeparator);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(6)]
    public void BoxedListWithTitle_VariousItemCounts_AllItemsPresent(int count)
    {
        var items = Enumerable.Range(1, count).Select(i => $"Entry {i}").ToList();
        var result = OutputHelpers.BoxedListWithTitle("Header", items);

        foreach (var item in items)
        {
            result.ShouldContain(item);
        }
    }
}
