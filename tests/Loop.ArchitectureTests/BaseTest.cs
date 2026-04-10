using System.Reflection;
using Loop.Application.Abstractions.Messaging;
using Loop.Domain.Users;
using Loop.Infrastructure.Database;
using Loop.Web.Api;

namespace Loop.ArchitectureTests;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = typeof(User).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(ICommand).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(LoopContext).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}

