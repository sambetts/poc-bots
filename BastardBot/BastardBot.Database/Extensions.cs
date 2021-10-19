using BastardBot.Common.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BastardBot.Common
{
    public static class Extensions
    {
        public static IServiceCollection AddBastardServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<BastardDBContext>(options => options
                .UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                moreOptions => moreOptions.CommandTimeout(120))
            );

            services.AddBastardBrain();

            var settings = new SystemSettings(configuration);
            services.AddSingleton(settings);

            return services;
        }

        public static IServiceCollection AddBastardBrain(this IServiceCollection services)
        {
            services.AddSingleton<DIBastardBrain>();
            return services;
        }
    }
}
