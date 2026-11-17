namespace BudgetTracker.Web.Services;

public interface IStorage {
    Task<string?> GetAsync(string key);

    Task SetAsync(string key, string value);

    Task RemoveAsync(string key);
}
