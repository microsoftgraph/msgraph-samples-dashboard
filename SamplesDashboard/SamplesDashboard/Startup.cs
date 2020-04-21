// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using GraphQL.Client.Http;
using SamplesDashboard.Services;
using SamplesDashboard.HostedServices;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using SamplesDashboard.DataFiles;
using SamplesDashboard.Models;

namespace SamplesDashboard
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Environment = environment;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration["DefaultConnection"]));

            services.AddDefaultIdentity<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            services.AddAuthentication()
                .AddIdentityServerJwt()
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = Configuration["AuthenticationConfig:Microsoft:ClientId"];
                    microsoftOptions.ClientSecret = Configuration["AuthenticationConfig:Microsoft:ClientSecret"];
                    microsoftOptions.AuthorizationEndpoint = Configuration["AuthenticationConfig:Microsoft:Instance"] + Configuration["AuthenticationConfig:Microsoft:TenantId"] + "/oauth2/v2.0/authorize";
                    microsoftOptions.TokenEndpoint = Configuration["AuthenticationConfig:Microsoft:Instance"] + Configuration["AuthenticationConfig:Microsoft:TenantId"] + "/oauth2/v2.0/token";
                    microsoftOptions.Scope.Add("email");
                    microsoftOptions.Scope.Add("profile");
                    microsoftOptions.Scope.Add("openid");
                });

            services.AddControllersWithViews();
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddHttpClient<GraphQLHttpClient>(c =>
            {
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Configuration.GetValue<string>("GithubAuthenticationToken"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.antiope-preview+json"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.hawkgirl-preview+json"));
                c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Configuration.GetValue<string>("product"), Configuration.GetValue<string>("product_version")));
            })
                .AddPolicyHandler(Policies.GithubRetryPolicy);

            services.AddHttpClient("github", c =>
            {
                c.BaseAddress = new Uri("https://api.github.com/");
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Configuration.GetValue<string>("GithubAuthenticationToken"));
                // Github API versioning
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                // Github requires a user-agent
                c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Configuration.GetValue<string>("product"), Configuration.GetValue<string>("product_version")));

            });

            services.AddSingleton<GraphQLHttpClientOptions>(provider => new GraphQLHttpClientOptions()
            {
                EndPoint = new Uri("https://api.github.com/graphql"),
            });

            services.AddSingleton<RepositoriesService>();
            services.AddSingleton<NugetService>();
            services.AddSingleton<NpmService>();
            services.AddSingleton<AzureSdkService>();
            services.AddHostedService<RepositoryHostedService>();

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
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseCors(); 
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
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
