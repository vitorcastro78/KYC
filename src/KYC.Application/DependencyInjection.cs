using System.Reflection;
using FluentValidation;
using KYC.Application.Behaviors;
using KYC.Application.Cases;
using KYC.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KYC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddScoped<SarSubmissionProcessor>();
        services.AddValidatorsFromAssemblyContaining<StartKycCaseCommandValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton<DueDiligenceLevelEvaluator>();
        services.AddSingleton<PolicyComplianceValidator>();
        services.AddSingleton<SarEligibilityEvaluator>();
        return services;
    }
}
