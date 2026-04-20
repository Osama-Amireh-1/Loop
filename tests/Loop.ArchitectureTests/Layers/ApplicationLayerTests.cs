using Loop.Application.Abstractions.Messaging;
using MediatR;
using NetArchTest.Rules;
using Shouldly;

namespace Loop.ArchitectureTests.Layers;

public class ApplicationLayerTests : BaseTest
{
    // ──────────────────────────────────────────────────────────────
    //  Commands
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void Commands_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Commands_ShouldHave_CommandSuffix()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────
    //  Queries
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void Queries_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Queries_ShouldHave_QuerySuffix()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────
    //  Command Handlers
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void CommandHandlers_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void CommandHandlers_ShouldResideIn_CommandNamespace()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .ResideInNamespaceContaining("Command")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────
    //  Query Handlers
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void QueryHandlers_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void QueryHandlers_ShouldResideIn_QueryNamespace()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .ResideInNamespaceContaining("Query")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────
    //  Domain Event Handlers
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void DomainEventHandlers_ShouldBe_Sealed()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(INotificationHandler<>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void DomainEventHandlers_ShouldHave_DomainEventHandlerSuffix()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(INotificationHandler<>))
            .Should()
            .HaveNameEndingWith("DomainEventHandler")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────
    //  Interfaces (Ports)
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void Interfaces_InApplicationLayer_ShouldStartWith_I()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
