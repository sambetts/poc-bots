// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BastardBot.Bot.Bots;
using BastardBot.Bot.Dialogs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using BastardBot.Common.DB;
using BastardBot.Common;

namespace BastardBot.Bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddMemoryCache(); 

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Create the bot services(QnA) as a singleton.
            services.AddSingleton<IBotServices, BotServices>();

            // Register LUIS recognizer
            services.AddSingleton<BastardRecognizer>();

            // The MainDialog that will be run by the bot.
            services.AddSingleton<MainBastardDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, BastardBot<MainBastardDialog>>();


            services.AddDbContext<BastardDBContext>(options => options
                .UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                moreOptions => moreOptions.CommandTimeout(120))
            );

            // Bot things
            services.AddBastardServices(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            //app.UseMvc();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {

                // Default
                endpoints.MapControllers();
            });
        }
    }
}
