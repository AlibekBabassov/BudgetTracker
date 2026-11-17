using System.Text.Json;
using System.Text.Json.Serialization;

namespace BudgetTracker.Domain.Logic;

[JsonSerializable(typeof(AppData))]
internal sealed partial class AppJsonContext : JsonSerializerContext {
}

public static class DataSerializer {
    public static string Serialize(AppData data) =>
        JsonSerializer.Serialize(data, AppJsonContext.Default.AppData);

    public static Result<AppData> Deserialize(string? json){
        if(string.IsNullOrWhiteSpace(json))
            return Result<AppData>.Success(AppData.Empty);

        try{
            var data = JsonSerializer.Deserialize(json, AppJsonContext.Default.AppData);
            return data is null
                ? Result<AppData>.Failure("Saved data is empty.")
                : Result<AppData>.Success(Normalize(data));
        }catch(JsonException){
            return Result<AppData>.Failure("Saved data is corrupted.");
        }
    }

    private static AppData Normalize(AppData data) => new(
        (data.Transactions ?? Array.Empty<Transaction>())
            .Where(t => t is not null && t.Category is not null && t.Note is not null)
            .ToList(),
        (data.Limits ?? Array.Empty<BudgetLimit>())
            .Where(l => l is not null && l.Category is not null)
            .ToList(),
        data.Settings is { Theme: not null, Currency: not null } ? data.Settings : AppSettings.Default);
}
