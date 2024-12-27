# Personal Sandbox for Azure features

## Development Environment

This repository has a configuration for VS Code Dev Container to quickly start up with the expected SDKs and utilities.

Follow a few steps below, then you'll be ready to play with Azure features.

1. Install _Dev Container_ plugin to your VS Code
1. From the VS Code's Command Palette, select _Dev Containers: Open Folder in Container..._
1. On the folder selection dialog, select the root directory of your local repository
1. Wait until all the set-up commands complete

## Configurations

You need to set up some environment variables to configure the sample Azure Functions Service in this repository.

Having `local.settings.json` file under the repository root directory allows you to keep the configurations in your local. Don't forget to set up the file with a protective access level.

```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AZURE_STORAGE_ACCOUNT_NAME": "<your Storage account name>",
    "AZURE_STORAGE_ACCOUNT_KEY": "<the primary key for your Storage account>",
    "AZURE_COSMOS_ACCOUNT": "<URI to your Cosmos Database account>",
    "AZURE_COSMOS_PRIMARY_KEY": "<the primary key for your Cosmos Database account>"
  }
}
```
