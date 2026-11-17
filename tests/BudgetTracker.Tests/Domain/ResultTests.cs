namespace BudgetTracker.Tests.Domain;

public class ResultTests {
    [Fact]
    public void Success_is_ok(){
        Assert.True(Result<int>.Success(1).IsOk);
    }

    [Fact]
    public void Failure_is_not_ok(){
        Assert.False(Result<int>.Failure("bad").IsOk);
    }

    [Fact]
    public void Map_transforms_a_successful_value(){
        var result = Result<int>.Success(2).Map(x => x * 10);

        var ok = Assert.IsType<Result<int>.Ok>(result);
        Assert.Equal(20, ok.Value);
    }

    [Fact]
    public void Map_keeps_the_errors_of_a_failure(){
        var result = Result<int>.Failure("bad", "worse").Map(x => x.ToString());

        var fail = Assert.IsType<Result<string>.Fail>(result);
        Assert.Equal(new[] { "bad", "worse" }, fail.Errors);
    }

    [Fact]
    public void Match_runs_the_branch_that_fits(){
        var ok = Result<int>.Success(5).Match(v => $"ok {v}", e => $"fail {e.Count}");
        var fail = Result<int>.Failure("a", "b").Match(v => $"ok {v}", e => $"fail {e.Count}");

        Assert.Equal("ok 5", ok);
        Assert.Equal("fail 2", fail);
    }
}
