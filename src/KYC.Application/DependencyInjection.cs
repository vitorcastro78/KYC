using System.Reflection;
using FluentValidation;
using KYC.Application.Behaviors;
using KYC.Application.Cases;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KYC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssemblyContaining<StartKycCaseCommandValidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
