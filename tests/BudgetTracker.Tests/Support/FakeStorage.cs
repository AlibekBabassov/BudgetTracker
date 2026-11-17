using BudgetTracker.Web.Services;

namespace BudgetTracker.Tests.Support;

public sealed class FakeStorage : IStorage {
    public Dictionary<string, string> Data { get; } = new();

    public bool FailReads { get; set; }

    public bool FailWrites { get; set; }

    public Task<string?> GetAsync(string key){
        if(FailReads)
            throw new InvalidOperationException("Simulated read failure.");

        return Task.FromResult<string?>(Data.TryGetValue(key, out var value) ? value : null);
    }

    public Task SetAsync(string key, string value){
        if(FailWrites)
            throw new InvalidOperationException("Simulated write failure (storage full).");

        Data[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key){
        Data.Remove(key);
        return Task.CompletedTask;
    }
}
