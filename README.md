# msgraph-samples-dashboard

## Get started with Samples Dashboard

You'll need to do this to run Samples Dashboard locally.

1. `git clone https://github.com/microsoftgraph/msgraph-samples-dashboard.git`

2. Install [AspNetCore 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0).

3. Install [Visual Studio 2019](https://visualstudio.microsoft.com/vs).

4. Generate a [github token](https://help.github.com/en/github/authenticating-to-github/creating-a-personal-access-token-for-the-command-line) and set it as an authentication token in appsettings.json.

5. Open a terminal, go to the ClientApp folder, and run `npm install` to install the web client dependencies.

6. Run the app.

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
---
```
