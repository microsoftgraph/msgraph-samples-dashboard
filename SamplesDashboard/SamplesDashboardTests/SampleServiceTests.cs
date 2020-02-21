// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using GraphQL.Client.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SamplesDashboard.Services;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace SamplesDashboardTests
{
    public class SampleServiceTests
    {
        private readonly SampleService _serviceProvider;
        public IConfiguration Configuration { get; }
        public SampleServiceTests()
        {
            var services = new ServiceCollection();
            services.AddHttpClient<GraphQLHttpClient>(c =>
            {
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Configuration.GetValue<string>("auth_token"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.antiope-preview+json"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.hawkgirl-preview+json"));
                c.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Configuration.GetValue<string>("product"), Configuration.GetValue<string>("product_version")));
            });
            services.AddSingleton(provider => new GraphQLHttpClientOptions()
            {
                EndPoint = new Uri("https://api.github.com/graphql"),
            });
            services.AddSingleton<SampleService>();
            var serviceProvider = services.BuildServiceProvider();

            _serviceProvider = serviceProvider.GetService<SampleService>();
        }

        [Fact]
        public async Task ShouldGetSampleLanguagesAsync()
        {
            //Arrange
            var sampleName = "powershell-intune-samples";

            //Act
            var languages = await _serviceProvider.GetLanguages(sampleName);

            //Assert
            Assert.NotNull(languages);
            Assert.Equal("powershell", languages.First());
        }

        [Fact]
        public async Task ShouldGetSampleFeaturesAsync()
        {
            //Arrange
            var sampleName = "powershell-intune-samples";

            //Act
            var services = await _serviceProvider.GetFeatures(sampleName);

            //Assert
            Assert.NotNull(services);
            Assert.Equal("Intune", services.First());
        }

        [Fact]
        public async Task ShouldGetNullSampleLanguageAsync()
        {
            //Arrange
            var sampleName = "msgraph-training-aspnetmvcapp";

            //Act
            var languages = await _serviceProvider.GetLanguages(sampleName);

            //Assert
            Assert.Empty(languages);
        }

        [Fact]
        public async Task ShouldGetNullSampleFeaturesAsync()
        {
            //Arrange
            var sampleName = "msgraph-training-aspnetmvcapp";

            //Act
            var services = await _serviceProvider.GetFeatures(sampleName);

            //Assert
            Assert.Empty(services);
        }
    }
}
