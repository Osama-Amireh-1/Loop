using Loop.SharedKernel;
using NetArchTest.Rules;
using Shouldly;

namespace Loop.ArchitectureTests.Layers;

public class DomainLayerTests : BaseTest
{
    // ──────────────────────────────────────────────────────────────
    //  Entities & Aggregate Roots
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void Entities_ShouldHave_PrivateParameterlessConstructor()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        List<Type> failingTypes = [];

        foreach (Type entityType in entityTypes)
        {
            bool hasPrivateParameterlessCtor = entityType
                .GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Any(c => c.GetParameters().Length == 0);

            if (!hasPrivateParameterlessCtor)
            {
                failingTypes.Add(entityType);
            }
        }

        failingTypes.ShouldBeEmpty(
            $"The following entities lack a private parameterless constructor: {string.Join(", ", failingTypes.Select(t => t.Name))}");
    }

    [Fact]
    public void Entities_ShouldNotHave_PublicConstructors()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        List<Type> failingTypes = [];

        foreach (Type entityType in entityTypes)
        {
            bool hasPublicCtor = entityType
                .GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Length > 0;

            if (hasPublicCtor)
            {
                failingTypes.Add(entityType);
            }
        }

        failingTypes.ShouldBeEmpty(
            $"The following entities have public constructors (use static factory methods instead): {string.Join(", ", failingTypes.Select(t => t.Name))}");
    }

    [Fact]
    public void Entities_ShouldHave_PrivateSetters()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        List<string> violations = [];

        foreach (Type entityType in entityTypes)
        {
            var publicSetters = entityType.GetProperties(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.DeclaredOnly)
                .Where(p => p.SetMethod is not null && p.SetMethod.IsPublic);

            foreach (var prop in publicSetters)
            {
                violations.Add($"{entityType.Name}.{prop.Name}");
            }
        }

        violations.ShouldBeEmpty(
            $"The following entity properties have public setters: {string.Join(", ", violations)}");
    }

    // ──────────────────────────────────────────────────────────────
    //  Domain Events
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void DomainEvents_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DomainEvents_ShouldHave_DomainEventSuffix()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DomainEvents_ShouldResideIn_DomainNamespace()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .ResideInNamespaceStartingWith("Loop.Domain")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────
    //  Value Objects
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void ValueObjects_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(ValueObject))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
