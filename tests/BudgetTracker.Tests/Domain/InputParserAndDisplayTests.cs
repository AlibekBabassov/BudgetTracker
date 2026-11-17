namespace BudgetTracker.Tests.Domain;

public class InputParserAndDisplayTests {
    [Theory]
    [InlineData("12.5", "12.5")]
    [InlineData("12,5", "12.5")]
    [InlineData(" 7 ", "7")]
    [InlineData("1 000", "1000")]
    [InlineData("0.99", "0.99")]
    public void Amounts_are_parsed_with_dot_or_comma(string text, string expected){
        Assert.True(InputParser.TryAmount(text, out var amount));
        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), amount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.2.3")]
    [InlineData("1e5")]
    public void Bad_amounts_are_rejected_without_throwing(string? text){
        Assert.False(InputParser.TryAmount(text, out _));
    }

    [Fact]
    public void Dates_must_look_like_year_month_day(){
        Assert.True(InputParser.TryDate("2026-03-15", out var date));
        Assert.Equal(new DateOnly(2026, 3, 15), date);

        Assert.False(InputParser.TryDate("15/03/2026", out _));
        Assert.False(InputParser.TryDate("2026-02-30", out _));
        Assert.False(InputParser.TryDate(null, out _));
    }

    [Theory]
    [InlineData("5", 5)]
    [InlineData(" 31 ", 31)]
    public void Days_are_whole_numbers(string text, int expected){
        Assert.True(InputParser.TryDay(text, out var day));
        Assert.Equal(expected, day);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("x")]
    public void Bad_days_are_rejected(string text){
        Assert.False(InputParser.TryDay(text, out _));
    }

    [Fact]
    public void Money_has_thousand_separators_two_decimals_and_a_symbol(){
        Assert.Equal("1,234.50 ₸", Display.Money(1234.5m, "₸"));
        Assert.Equal("-20.00 $", Display.Money(-20m, "$"));
    }

    [Fact]
    public void Dates_and_months_are_formatted_the_same_everywhere(){
        Assert.Equal("March 2026", Display.Month(new DateOnly(2026, 3, 1)));
        Assert.Equal("Mar", Display.ShortMonth(new DateOnly(2026, 3, 1)));
        Assert.Equal("05 Mar 2026", Display.Date(new DateOnly(2026, 3, 5)));
        Assert.Equal("2026-03-05", Display.InputDate(new DateOnly(2026, 3, 5)));
    }
}
