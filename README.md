# msgraph-samples-dashboard

## Get started with Samples Dashboard

You'll need to do this to run Samples Dashboard locally.

1. `git clone https://github.com/microsoftgraph/msgraph-samples-dashboard.git`

1. Install [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0) and [Node.js](https://nodejs.org/).node

1. Open a terminal, go to the ClientApp folder, and run `npm install` to install the web client dependencies.

1. Register an application in Azure Active Directory with the following properties
    - Single-tenant
    - A single-page application redirect URI with the value `https://localhost:5001`
    - In the **Expose an API** section, set:
        - **Application ID URI** to `api://<APPLICATION_ID>`, where `<APPLICATION_ID>` is the app ID of the registration
        - **Scopes defined by this API** - `api://cbd8a550-241d-466a-bf5e-20485800e81e/Repos.Read`, Admins and users can consent
        - **Authorized client applications**: Add the app ID of this registration

1.

1. Run the app.

## YAML Schema

If you do not have the YAML header in README, you can add metadata in devx.yml in the root of the repo. Here's the 3 relevant bits of information.

```yml
---
languages:
- python
- html
extensions:
    services:
    - Security
dependencyFile: demo/GraphTutorial/app/build.gradle
noDependencies: true
---
```
