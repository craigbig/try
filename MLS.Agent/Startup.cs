﻿using System;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Pocket;
using Recipes;
using WorkspaceServer;
using static Pocket.Logger<MLS.Agent.Startup>;

namespace MLS.Agent
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Environment = env;

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            Configuration = configurationBuilder.Build();
        }

        protected IConfigurationRoot Configuration { get; }

        protected IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(options =>
                    {
                        options.Filters.Add(new ExceptionFilter());
                        options.Filters.Add(new BadRequestOnInvalidModelFilter());
                    })
                    .AddJsonOptions(o =>
                    {
                        o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                        o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    });

            services.AddSingleton(Configuration);

            services.TryAddSingleton<WorkspaceServerRegistry>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env,
            IServiceProvider serviceProvider)
        {
            Log.Info("Agent version {orchestrator_version} starting in environment {environment}",
                     AssemblyVersionSensor.Version().AssemblyInformationalVersion,
                     Environment.EnvironmentName);

            app.UseDefaultFiles()
               .UseStaticFiles()
               .UseMvc();

            serviceProvider
                .GetRequiredService<WorkspaceServerRegistry>()
                .StartAllServers()
                .DontAwait();
        }
    }
}
