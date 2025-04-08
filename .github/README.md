# GitHub Actions Workflows for MyBook.Writer

This directory contains GitHub Actions workflows for automating the build, test, and deployment processes for the MyBook.Writer library.

## Available Workflows

### NuGet Package Publishing (`nuget-publish.yml`)

This workflow builds and publishes the MyBook.Writer library as a NuGet package on NuGet.org when a release is created on GitHub.

#### Workflow Triggers

- **Release Published**: Automatically triggered when a new release is published on GitHub.
- **Manual Trigger**: Can be manually triggered from the GitHub Actions tab.

#### Workflow Steps

1. Checkout the repository
2. Setup .NET 8.0
3. Restore dependencies
4. Build the project
5. Run tests
6. Create NuGet package
7. Push package to NuGet.org (only on release events)

## Setting Up NuGet API Key

To use this workflow, you need to add your NuGet API key to your GitHub repository secrets:

1. Go to your GitHub repository
2. Click on "Settings"
3. Select "Secrets and variables" → "Actions"
4. Click "New repository secret"
5. Name: `NUGET_API_KEY`
6. Value: Your NuGet API key from https://www.nuget.org/account/apikeys
7. Click "Add secret"

## Versioning

When creating a new release on GitHub, the version from the release tag will be used for the NuGet package.
Use semantic versioning format (e.g., v1.2.3) for your release tags. 