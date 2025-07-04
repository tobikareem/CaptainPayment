using CaptainPayment.Core.Config;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Stripe.Services.Customers;
using CaptainPayment.Stripe.Services.Payments;
using CaptainPayment.Stripe.Services.Products;
using CaptainPayment.Stripe.Services.Subscriptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace CaptainPayment.Extensions;

public static class ServiceCollection
{
    public static IServiceCollection AddPaymentLibrary(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaymentSettings>(configuration.GetSection(PaymentSettings.SectionName));

        services.AddScoped<ISubscriptionService, StripeSubscriptionService>();
        services.AddScoped<IPaymentProvider, StripeSubscriptionService>();

        return services;
    }

    public static IServiceCollection AddStripePayments(this IServiceCollection services, Action<StripeSettings> configureOptions)
    {
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        services.Configure(configureOptions);

        return services.AddStripePayments();
    }

    public static IServiceCollection AddStripePayments(this IServiceCollection services, IConfiguration configuration, string sectionName = "Stripe")
    {
        services.Configure<StripeSettings>(configuration.GetSection(sectionName));

        return services.AddStripePayments();
    }

    private static IServiceCollection AddStripePayments(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<ISubscriptionService, StripeSubscriptionService>();
        services.AddScoped<IPaymentProvider, StripeSubscriptionService>();
        services.AddScoped<ICustomerService, StripeCustomerService>();
        services.AddScoped<IProductService, StripeProductService>();
        services.AddScoped<IPaymentMethodService, StripePaymentMethodService>();
        services.AddScoped<ISetupIntentService, StripeSetupIntentService>();

        return services;
    }


    //public static IServiceCollection AddStripePayments(this IServiceCollection services, Action<StripeSettings>? configureStripe = null)
    //{
    //    if (configureStripe != null)
    //    {
    //        services.Configure<PaymentSettings>(settings =>
    //        {
    //            configureStripe(settings.Stripe);
    //        });
    //    }

    //    services.AddScoped<ISubscriptionService, StripeSubscriptionService>();
    //    services.AddScoped<IPaymentProvider, StripeSubscriptionService>();

    //    return services;
    //}
}
