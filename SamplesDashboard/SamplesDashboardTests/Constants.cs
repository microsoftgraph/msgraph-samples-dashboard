namespace SamplesDashboardTests
{
    public static class Constants
    {

        public const string GraphQLSamplesResponse = @"{
  ""data"": {
    ""search"": {
      ""nodes"": [
        {
          ""name"": ""powershell-intune-samples""
        },
        {
          ""name"": ""microsoft-graph-comms-samples""
        },
        {
          ""name"": ""msgraph-training-nodeexpressapp""
        },
        {
          ""name"": ""msgraph-training-reactspa""
        },
        {
          ""name"": ""msgraph-training-aspnet-core""
        },
        {
          ""name"": ""msgraph-training-pythondjangoapp""
        },
        {
          ""name"": ""msgraph-training-phpapp""
        },
        {
          ""name"": ""msgraph-training-angularspa""
        },
        {
          ""name"": ""msgraph-training-dotnet-core""
        },
        {
          ""name"": ""msgraph-training-changenotifications""
        }
      ],
      ""pageInfo"": {
        ""endCursor"": ""Y3Vyc29yOjEwMA=="",
        ""hasNextPage"": false
      }
    }
  }
}";

        public const string GraphQLSdksResponse = @"{
  ""data"": {
    ""search"": {
      ""nodes"": [
        {
          ""name"": ""msgraph-sdk-java-core""
        },
        {
          ""name"": ""Intune-PowerShell-SDK-Code-Generator""
        },
        {
          ""name"": ""msgraph-cli-core""
        },
        {
          ""name"": ""msgraph-samples-dashboard""
        },
        {
          ""name"": ""microsoft-graph-devx-api""
        },
        {
          ""name"": ""microsoft-graph-explorer-v4""
        },
        {
          ""name"": ""msgraph-sdk-dotnet""
        },
        {
          ""name"": ""msgraph-sdk-php""
        },
        {
          ""name"": ""msgraph-sdk-dotnet""
        },
        {
          ""name"": ""msgraph-cli""
        }
      ],
      ""pageInfo"": {
        ""endCursor"": ""Y3Vyc29yOjEwMA=="",
        ""hasNextPage"": false
      }
    }
  }
}";

        public const string GetRepoResponse = @"{
    ""data"": {
        ""organization"": {
            ""repository"": {
                ""name"": ""msgraph-training-aspnet-core"",
                ""description"": ""Microsoft Graph Training Module - Build ASP.NET Core apps with Microsoft Graph"",
                ""issues"": {
                    ""totalCount"": 0
                },
                ""pullRequests"": {
                    ""totalCount"": 0
                },
                ""stargazers"": {
                    ""totalCount"": 15
                },
                ""url"": ""https://github.com/microsoftgraph/msgraph-training-aspnet-core"",
                ""forks"": {
                    ""totalCount"": 25
                },
                ""defaultBranchRef"": {
                    ""name"": ""main""
                },
                ""updatedAt"": ""2022-03-06T22:54:02Z"",
                ""vulnerabilityAlerts"": {
                    ""totalCount"": 0,
                    ""edges"": []
                },
                ""dependencyGraphManifests"": {
                    ""nodes"": [
                        {
                            ""filename"": ""demo/GraphTutorial/GraphTutorial.csproj"",
                            ""dependencies"": {
                                ""nodes"": [
                                    {
                                        ""packageManager"": ""NUGET"",
                                        ""packageName"": ""Microsoft.Identity.Web"",
                                        ""requirements"": ""= 1.5.1"",
                                        ""repository"": {
                                            ""name"": ""microsoft-identity-web"",
                                            ""releases"": {
                                                ""nodes"": [
                                                    {
                                                        ""name"": ""1.23.0"",
                                                        ""tagName"": ""1.23.0""
                                                    }
                                                ]
                                            }
                                        }
                                    },
                                    {
                                        ""packageManager"": ""NUGET"",
                                        ""packageName"": ""Microsoft.Identity.Web.MicrosoftGraph"",
                                        ""requirements"": ""= 1.5.1"",
                                        ""repository"": {
                                            ""name"": ""microsoft-identity-web"",
                                            ""releases"": {
                                                ""nodes"": []
                                            }
                                        }
                                    },
                                    {
                                        ""packageManager"": ""NUGET"",
                                        ""packageName"": ""Microsoft.Identity.Web.UI"",
                                        ""requirements"": ""= 1.5.1"",
                                        ""repository"": {
                                            ""name"": ""microsoft-identity-web"",
                                            ""releases"": {
                                                ""nodes"": [
                                                    {
                                                        ""name"": ""1.23.0"",
                                                        ""tagName"": ""1.23.0""
                                                    }
                                                ]
                                            }
                                        }
                                    },
                                    {
                                        ""packageManager"": ""NUGET"",
                                        ""packageName"": ""TimeZoneConverter"",
                                        ""requirements"": ""= 3.3.0"",
                                        ""repository"": {
                                            ""name"": ""TimeZoneConverter"",
                                            ""releases"": {
                                                ""nodes"": [
                                                    {
                                                        ""name"": ""5.0.0"",
                                                        ""tagName"": ""5.0.0""
                                                    }
                                                ]
                                            }
                                        }
                                    }
                                ]
                            }
                        }
                    ]
                }
            }
        }
    }
}";
    }
}
