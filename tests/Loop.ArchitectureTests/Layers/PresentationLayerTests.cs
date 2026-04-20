using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;
using Shouldly;

namespace Loop.ArchitectureTests.Layers;

public class PresentationLayerTests : BaseTest
{
    // ──────────────────────────────────────────────────────────────
    //  Controllers
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void Controllers_ShouldHave_ControllerSuffix()
    {
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Controllers_ShouldInherit_ControllerBase()
    {
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .Inherit(typeof(ControllerBase))
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Controllers_ShouldResideIn_ControllersNamespace()
    {
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .Should()
            .ResideInNamespaceEndingWith("Controllers")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Controllers_ShouldHave_ApiControllerAttribute()
    {
        IEnumerable<Type> controllerTypes = Types.InAssembly(PresentationAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .GetTypes();

        List<Type> failingTypes = [];

        foreach (Type controllerType in controllerTypes)
        {
            bool hasApiControllerAttribute = controllerType
                .GetCustomAttributes(typeof(ApiControllerAttribute), true)
                .Length > 0;

            if (!hasApiControllerAttribute)
            {
                failingTypes.Add(controllerType);
            }
        }

        failingTypes.ShouldBeEmpty(
            $"The following controllers are missing [ApiController] attribute: {string.Join(", ", failingTypes.Select(t => t.Name))}");
    }

    [Fact]
    public void Controllers_ShouldHave_RouteAttribute()
    {
        IEnumerable<Type> controllerTypes = Types.InAssembly(PresentationAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .GetTypes();

        List<Type> failingTypes = [];

        foreach (Type controllerType in controllerTypes)
        {
            bool hasRouteAttribute = controllerType
                .GetCustomAttributes(typeof(RouteAttribute), true)
                .Length > 0;

            if (!hasRouteAttribute)
            {
                failingTypes.Add(controllerType);
            }
        }

        failingTypes.ShouldBeEmpty(
            $"The following controllers are missing [Route] attribute: {string.Join(", ", failingTypes.Select(t => t.Name))}");
    }

    // ──────────────────────────────────────────────────────────────
    //  Presentation should not contain domain logic
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public void PresentationLayer_ShouldNotReference_DomainEntities_Directly()
    {
        // Presentation should talk to the Application layer via MediatR commands/queries,
        // not reference domain entities directly (except for routing through shared DTOs).
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .ShouldNot()
            .HaveDependencyOn("Loop.Domain.Users")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Controllers_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssembly(PresentationAssembly)
            .That()
            .Inherit(typeof(ControllerBase))
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
