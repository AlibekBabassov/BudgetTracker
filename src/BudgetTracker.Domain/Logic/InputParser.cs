using System.Globalization;

namespace BudgetTracker.Domain.Logic;

public static class InputParser {
    public static bool TryAmount(string? text, out decimal amount){
        amount = 0;

        if(string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = text.Trim().Replace(" ", "").Replace(',', '.');

        return decimal.TryParse(
            normalized,
            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out amount);
    }

    public static bool TryDate(string? text, out DateOnly date) =>
        DateOnly.TryParseExact(text?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    public static bool TryDay(string? text, out int day) =>
        int.TryParse(text?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out day);
}
