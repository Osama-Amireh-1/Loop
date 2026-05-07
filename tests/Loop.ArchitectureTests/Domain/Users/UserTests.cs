using Loop.Domain.Common;
using Loop.Domain.Users;
using Loop.SharedKernel;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Users;

public class UserTests
{
    [Fact]
    public void Create_ShouldInitializeUserAndPointsBalance()
    {
        var tierId = Guid.NewGuid();

        var user = CreateUser(tierId: tierId);

        user.FirstName.ShouldBe("John");
        user.LastName.ShouldBe("Doe");
        user.TierId.ShouldBe(tierId);
        user.PointsBalance.TotalPoints.ShouldBe(0);
        user.PointsBalance.LifetimePoints.ShouldBe(0);
        user.PointsBalance.UserId.ShouldBe(user.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreditPoints_ShouldFail_WhenAmountIsNotPositive(int amount)
    {
        var user = CreateUser();

        var result = user.CreditPoints(amount);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.InvalidCreditAmount");
        user.PointsBalance.TotalPoints.ShouldBe(0);
    }

    [Fact]
    public void CreditPoints_ShouldIncreaseTotalAndLifetimePoints_WhenAmountIsPositive()
    {
        var user = CreateUser();

        var result = user.CreditPoints(25);

        result.IsSuccess.ShouldBeTrue();
        user.PointsBalance.TotalPoints.ShouldBe(25);
        user.PointsBalance.LifetimePoints.ShouldBe(25);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DebitPoints_ShouldFail_WhenAmountIsNotPositive(int amount)
    {
        var user = CreateUser();

        var result = user.DebitPoints(amount);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.InvalidDebitAmount");
    }

    [Fact]
    public void DebitPoints_ShouldThrowDomainException_WhenBalanceIsInsufficient()
    {
        var user = CreateUser();
        _ = user.CreditPoints(10);

        Should.Throw<DomainException>(() => user.DebitPoints(11));
    }

    [Fact]
    public void DebitPoints_ShouldDecreaseTotalPoints_WhenBalanceIsSufficient()
    {
        var user = CreateUser();
        _ = user.CreditPoints(40);

        var result = user.DebitPoints(15);

        result.IsSuccess.ShouldBeTrue();
        user.PointsBalance.TotalPoints.ShouldBe(25);
        user.PointsBalance.LifetimePoints.ShouldBe(40);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateProfileWithoutChangingPhone_WhenPhoneIsNotProvided()
    {
        var user = CreateUser();
        var originalPhone = user.Phone;

        user.UpdateProfile("Jane", "Smith", "https://example.com/profile.jpg");

        user.FirstName.ShouldBe("Jane");
        user.LastName.ShouldBe("Smith");
        user.ProfileImageUrl.ShouldBe("https://example.com/profile.jpg");
        user.Phone.ShouldBe(originalPhone);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdatePhone_WhenPhoneIsProvided()
    {
        var user = CreateUser();
        var newPhone = Phone.Create("9876543").Value;

        user.UpdateProfile("Jane", "Smith", null, newPhone);

        user.FirstName.ShouldBe("Jane");
        user.LastName.ShouldBe("Smith");
        user.Phone.Value.ShouldBe("9876543");
    }

    [Fact]
    public void ChangePasswordHash_ShouldSetNewHash()
    {
        var user = CreateUser();

        user.ChangePasswordHash("new-hash");

        user.PasswordHash.ShouldBe("new-hash");
    }

    [Fact]
    public void UpgradeTier_ShouldSetNewTierId()
    {
        var user = CreateUser();
        var newTierId = Guid.NewGuid();

        user.UpgradeTier(newTierId);

        user.TierId.ShouldBe(newTierId);
    }

    [Fact]
    public void HasEnoughPoints_ShouldReflectCurrentBalance()
    {
        var user = CreateUser();
        _ = user.CreditPoints(30);

        user.HasEnoughPoints(30).ShouldBeTrue();
        user.HasEnoughPoints(31).ShouldBeFalse();
    }

    private static User CreateUser(
        string firstName = "John",
        string lastName = "Doe",
        string phoneNumber = "1234567",
        string emailAddress = "john.doe@example.com",
        Guid? tierId = null) =>
        User.Create(
            firstName,
            lastName,
            Phone.Create(phoneNumber).Value,
            Email.Create(emailAddress).Value,
            "password-hash",
            Gender.Male,
            tierId ?? Guid.NewGuid());
}
