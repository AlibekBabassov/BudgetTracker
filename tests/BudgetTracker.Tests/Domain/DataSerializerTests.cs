namespace BudgetTracker.Tests.Domain;

public class DataSerializerTests {
    private static AppData Sample() => new(
        new[] { new Transaction(Guid.NewGuid(), TransactionType.Income, "Salary", 1000m, new DateOnly(2026, 3, 1), "March") },
        new[] { new BudgetLimit("Food", 300m) },
        new AppSettings("dark", "$"));

    [Fact]
    public void Serialize_then_deserialize_gives_the_same_data(){
        var data = Sample();

        var result = DataSerializer.Deserialize(DataSerializer.Serialize(data));

        var ok = Assert.IsType<Result<AppData>.Ok>(result);
        Assert.Equal(data.Transactions, ok.Value.Transactions);
        Assert.Equal(data.Limits, ok.Value.Limits);
        Assert.Equal(data.Settings, ok.Value.Settings);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_text_means_a_fresh_start(string? json){
        var result = DataSerializer.Deserialize(json);

        var ok = Assert.IsType<Result<AppData>.Ok>(result);
        Assert.Empty(ok.Value.Transactions);
        Assert.Equal(AppSettings.Default, ok.Value.Settings);
    }

    [Theory]
    [InlineData("{not json")]
    [InlineData("[]")]
    [InlineData("12")]
    public void Corrupted_text_is_a_failure_not_an_exception(string json){
        Assert.IsType<Result<AppData>.Fail>(DataSerializer.Deserialize(json));
    }

    [Fact]
    public void The_json_null_literal_is_a_failure(){
        Assert.IsType<Result<AppData>.Fail>(DataSerializer.Deserialize("null"));
    }

    [Fact]
    public void An_empty_object_is_filled_with_defaults(){
        var result = DataSerializer.Deserialize("{}");

        var ok = Assert.IsType<Result<AppData>.Ok>(result);
        Assert.Empty(ok.Value.Transactions);
        Assert.Empty(ok.Value.Limits);
        Assert.Equal(AppSettings.Light, ok.Value.Settings.Theme);
    }
}
