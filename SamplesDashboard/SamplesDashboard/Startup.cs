// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http.Headers;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Identity.Web;
using SamplesDashboard.MessageHandlers;
using SamplesDashboard.Policies;
using SamplesDashboard.Services;

namespace SamplesDashboard
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
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration);

            services.AddControllersWithViews();

            services.AddMemoryCache();
            services.AddHttpClient("Default", cli => {
                // Include user agent info
                cli.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue(Configuration.GetValue<string>("Product"),
                                            Configuration.GetValue<string>("ProductVersion"))
                );
                cli.Timeout = TimeSpan.FromSeconds(500);
            });

            // Add a GraphQL client
            services
                .AddHttpClient<GraphQLHttpClient>(cli => {
                    // Include user agent info
                    cli.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(Configuration.GetValue<string>("Product"),
                                                Configuration.GetValue<string>("ProductVersion"))
                    );
                    cli.Timeout = TimeSpan.FromSeconds(500);
                })
                .AddPolicyHandler(GitHubRetryPolicy.Policy)
                .AddHttpMessageHandler<GitHubAuthHandler>();

            services.AddSingleton<GraphQLHttpClientOptions>(provider => new GraphQLHttpClientOptions
            {
                EndPoint = new Uri(Constants.GitHubGraphQLEndpoint)
            });

            services.AddSingleton<IGraphQLWebsocketJsonSerializer, SystemTextJsonSerializer>();

            services.AddSingleton<CacheService>();
            services.AddSingleton<RepositoriesService>();
            services.AddSingleton<GitHubAuthService>();
            services.AddSingleton<ManifestFromFileService>();
            services.AddSingleton<MicrosoftOpenSourceService>();
            services.AddSingleton<NuGetService>();
            services.AddSingleton<NpmService>();
            services.AddSingleton<CocoaPodsService>();
            services.AddSingleton<MavenService>();
            services.AddHostedService<RepositoriesHostedService>();
            services.AddScoped<GitHubAuthHandler>();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Repositories API", Version = "v1"});
            });
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Repositories API V1");
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
