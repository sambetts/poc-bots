using BastardBot.Common;
using BastardBot.Common.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BastardBot.Tests
{
    class Program
    {

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World! This is a console app for testing whatever isn't working. Shouldn't be run normally.");
            IConfiguration config = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", false, true)
              .AddJsonFile("appsettings.Development.json", true, true)
              .Build();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            var serviceProvider = new ServiceCollection()
                .AddDbContext<BastardDBContext>(options => options
                    .UseSqlServer(config.GetConnectionString("DefaultConnection"),
                    moreOptions => moreOptions.CommandTimeout(120))
                )
                .AddBastardServices(config)
                .BuildServiceProvider();

            using (DIBastardBrain trainingModel = new DIBastardBrain(serviceProvider))
            {
                await trainingModel.InitDatabaseAndModel();
                await trainingModel.TrainAndPublishNewModel();
            }
            
        }
    }
}
