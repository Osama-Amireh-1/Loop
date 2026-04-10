using System.Reflection;
using Loop.Application;
using Loop.Application.Interfaces;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Loop.Infrastructure;
using Loop.Infrastructure.Repository;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Loop.SharedKernel.UnitOfWork;
using Loop.Web.Api;
using Loop.Web.Api.Extensions;
using Loop.Web.Api.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();

WebApplication app = builder.Build();

app.ApplyMigrations();


    app.UseSwaggerWithUi();


app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

// REMARK: If you want to use Controllers, you'll need this.
app.MapControllers();

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
namespace Loop.Web.Api
{
    public partial class Program;
}


