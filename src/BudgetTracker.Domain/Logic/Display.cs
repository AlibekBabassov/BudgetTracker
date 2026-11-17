using System.Globalization;

namespace BudgetTracker.Domain.Logic;

public static class Display {
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string Money(decimal amount, string symbol) =>
        $"{amount.ToString("N2", Inv)} {symbol}";

    public static string Month(DateOnly date) => date.ToString("MMMM yyyy", Inv);

    public static string ShortMonth(DateOnly date) => date.ToString("MMM", Inv);

    public static string Date(DateOnly date) => date.ToString("dd MMM yyyy", Inv);

    public static string InputDate(DateOnly date) => date.ToString("yyyy-MM-dd", Inv);
}
