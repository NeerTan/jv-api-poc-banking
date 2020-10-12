using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Driver
{
    public static class DIExt
    {
        public static void AddAuthenticatedSoapClient<TService, TImpl>(this IServiceCollection svc)
            where TService : class
            where TImpl : ClientBase<TService>, TService, new()
        {
            svc.AddSingleton<TImpl>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<WorkdayCredentialsOptions>>().Value;

                var client = new TImpl();
                client.Endpoint.EndpointBehaviors.Add(new AuthenticationBehavior(opts));

                return client;
            });
            svc.AddSingleton<TService>(sp => sp.GetRequiredService<TImpl>());
        }
    }
}
