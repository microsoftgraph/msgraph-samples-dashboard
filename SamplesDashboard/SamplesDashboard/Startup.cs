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
            services.AddHttpClient();

            // Add a GraphQL client
            services
                .AddHttpClient<GraphQLHttpClient>(cli => {
                    // Enable dependency graph info in GraphQL queries
                    // https://docs.github.com/en/graphql/overview/schema-previews#access-to-a-repositories-dependency-graph-preview
                    cli.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.hawkgirl-preview+json")
                    );
                    // Include user agent info
                    cli.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(Configuration.GetValue<string>("Product"),
                                                Configuration.GetValue<string>("ProductVersion"))
                    );
                })
                .AddPolicyHandler(GitHubRetryPolicy.Policy)
                .AddHttpMessageHandler<GitHubAuthHandler>();

            services.AddSingleton<GraphQLHttpClientOptions>(provider => new GraphQLHttpClientOptions
            {
                EndPoint = new Uri("https://api.github.com/graphql")
            });

            services.AddSingleton<IGraphQLWebsocketJsonSerializer, SystemTextJsonSerializer>();

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
