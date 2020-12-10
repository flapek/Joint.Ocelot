using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Joint.Ocelot.Options;
using Joint.Ocelot.Validators;
using Joint.Builders;
using Ocelot.Provider.Consul;

namespace Joint.Ocelot
{
    public static class Extension
    {
        private const string SectionRoutesName = "anonymousRoutes";
        private const string SectionOcelotName = "ocelot";

        public static IJointBuilder AddOcelot(this IJointBuilder builder, string sectionName = SectionRoutesName)
        {
            var optionsRoutes = builder.GetOptions<AnonymousRoutesOptions>(sectionName);
            var optionsOcelot = builder.GetOptions<OcelotOptions>(SectionOcelotName);
            builder.Services.AddSingleton(optionsRoutes);
            builder.Services.AddSingleton(optionsOcelot);
            builder.Services.AddSingleton<IAnonymousRouteValidator, AnonymousRouteValidator>();
            if (optionsOcelot.Consul)
                builder.Services.AddOcelot().AddConsul();
            else
                builder.Services.AddOcelot();

            return builder;
        }

        public static IApplicationBuilder UseApiGatewayOcelot(this IApplicationBuilder app)
        {
            var optionsOcelot = app.ApplicationServices.GetService<OcelotOptions>();
            if (optionsOcelot.WebSockets)
                app.UseWebSockets();

            return app.UseOcelot(GetOcelotConfiguration()).GetAwaiter().GetResult();
        }

        private static OcelotPipelineConfiguration GetOcelotConfiguration() => new OcelotPipelineConfiguration
        {
            AuthenticationMiddleware = async (context, next) =>
            {
                if (!context.Items.DownstreamRoute().IsAuthenticated)
                {
                    await next.Invoke();
                    return;
                }

                if (context.RequestServices.GetRequiredService<IAnonymousRouteValidator>()
                    .HasAccess(context.Request.Path))
                {
                    await next.Invoke();
                    return;
                }

                var authenticateResult = await context.AuthenticateAsync();
                if (authenticateResult.Succeeded)
                {
                    context.User = authenticateResult.Principal;
                    await next.Invoke();
                    return;
                }

                context.Items.SetError(new UnauthenticatedError("Unauthenticated"));
            }
        };
    }
}
