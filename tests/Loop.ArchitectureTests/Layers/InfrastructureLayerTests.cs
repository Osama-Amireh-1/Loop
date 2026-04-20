using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using Shouldly;

namespace Loop.ArchitectureTests.Layers;

public class InfrastructureLayerTests : BaseTest
{
    // ──────────────────────────────────────────────────────────────
    //  DbContext
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void DbContext_ShouldResideIn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .Inherit(typeof(DbContext))
            .Should()
            .ResideInNamespaceStartingWith("Loop.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DbContext_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .Inherit(typeof(DbContext))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────
    //  Entity Type Configurations
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void EntityConfigurations_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void EntityConfigurations_ShouldHave_ConfigurationSuffix()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .HaveNameEndingWith("Configuration")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────
    //  Repository Implementations
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void Repositories_ShouldResideIn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespaceStartingWith("Loop.Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
