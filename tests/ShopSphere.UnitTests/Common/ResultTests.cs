using ShopSphere.Domain.Common;

namespace ShopSphere.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_should_have_success_state()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_should_have_failure_state_and_error()
    {
        var error = new Error("Test.Code", "Something went wrong.");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Match_should_execute_success_branch()
    {
        var result = Result.Success();

        var value = result.Match(
            onSuccess: () => "success",
            onFailure: _ => "failure");

        value.Should().Be("success");
    }

    [Fact]
    public void Match_should_execute_failure_branch()
    {
        var error = new Error("Test.Code", "Something went wrong.");
        var result = Result.Failure(error);

        var value = result.Match(
            onSuccess: () => "success",
            onFailure: e => e.Code);

        value.Should().Be("Test.Code");
    }

    [Fact]
    public void Generic_success_should_contain_value()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Generic_failure_should_not_contain_value()
    {
        var error = new Error("Test.Code", "Something went wrong.");

        var result = Result<int>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().Be(default);
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Generic_implicit_value_conversion_should_create_success()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Generic_implicit_error_conversion_should_create_failure()
    {
        var error = new Error("Test.Code", "Something went wrong.");

        Result<int> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Value.Should().Be(default);
    }

    [Fact]
    public void Generic_match_should_execute_success_branch()
    {
        var result = Result<int>.Success(42);

        var value = result.Match(
            onSuccess: number => number * 2,
            onFailure: _ => 0);

        value.Should().Be(84);
    }

    [Fact]
    public void Generic_match_should_execute_failure_branch()
    {
        var error = new Error("Test.Code", "Something went wrong.");
        var result = Result<int>.Failure(error);

        var value = result.Match(
            onSuccess: _ => "success",
            onFailure: e => e.Code);

        value.Should().Be("Test.Code");
    }

    [Fact]
    public void Result_should_use_value_equality()
    {
        Result.Success().Should().Be(Result.Success());
        Result.Failure(new Error("Test.Code", "Message"))
            .Should().Be(Result.Failure(new Error("Test.Code", "Message")));
    }

    [Fact]
    public void Generic_result_should_use_value_equality()
    {
        Result<int>.Success(42).Should().Be(Result<int>.Success(42));
        Result<int>.Failure(new Error("Test.Code", "Message"))
            .Should().Be(Result<int>.Failure(new Error("Test.Code", "Message")));
    }

    [Fact]
    public void Generic_match_should_pass_success_value_to_handler()
    {
        var result = Result<string>.Success("value");

        var value = result.Match(success => success, _ => "failure");

        value.Should().Be("value");
    }
}