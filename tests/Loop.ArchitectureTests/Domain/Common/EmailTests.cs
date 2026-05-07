using Loop.Domain.Common;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Common;

public class EmailTests
{
    [Fact]
    public void Create_ShouldFail_WhenEmailIsEmpty()
    {
        var result = Email.Create(" ");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Email.Empty");
    }

    [Fact]
    public void Create_ShouldFail_WhenEmailIsInvalid()
    {
        var result = Email.Create("not-an-email");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Email.Invalid");
    }

    [Fact]
    public void Create_ShouldNormalizeEmail_WhenEmailIsValid()
    {
        var result = Email.Create("  test.user@example.com ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("TEST.USER@EXAMPLE.COM");
    }
}
