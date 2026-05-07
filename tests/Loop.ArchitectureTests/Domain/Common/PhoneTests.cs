using Loop.Domain.Common;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Common;

public class PhoneTests
{
    [Fact]
    public void Create_ShouldFail_WhenPhoneIsEmpty()
    {
        var result = Phone.Create(" ");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Phone.Empty");
    }

    [Fact]
    public void Create_ShouldFail_WhenPhoneLengthIsOutOfRange()
    {
        var result = Phone.Create("123456");

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Phone.InvalidLength");
    }

    [Fact]
    public void Create_ShouldTrimPhone_WhenPhoneIsValid()
    {
        var result = Phone.Create(" 1234567 ");

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe("1234567");
    }
}
