[![Nuget](https://img.shields.io/nuget/v/Xperience.Xman)](https://www.nuget.org/packages/Xperience.Xman#versions-body-tab)
[![build](https://github.com/kentico-ericd/xperience-manager/actions/workflows/build.yml/badge.svg)](https://github.com/kentico-ericd/xperience-manager/actions/workflows/build.yml)

# Xperience Manager (xman)

This tool simplifies the process of installing and managing Xperience by Kentico instances by providing step-by-step wizards with default options provided.

<img src="https://raw.githubusercontent.com/kentico-ericd/xperience-manager/master/img/screenshot.png" width="350">

## Installing the tool

Run the following command from a command prompt such as Powershell:

```bash
dotnet tool install Xperience.Xman -g
```

## Updating the tool

Run the following command from a command prompt such as Powershell:

```bash
dotnet tool update xperience.xman -g
```

## Usage

The following commands can be executed using the `xman` tool name:

- `?`, `help`
- `i`, `install`
- `u`, `update`
- `ci store`, `ci restore`

### Installing a project with the wizard

1. Create an empty directory in the location you wish to install Xperience by Kentico
1. In a command prompt, navigate to the empty directory

   ```bash
   cd C:\inetpub\wwwroot\xbk
   ```

1. Run the `install` command which will begin the installation wizard:

   ```bash
   xman install
   ```

### Installing a project with a configuration file

Whenever you install a new project with the wizard, a `xman.json` file is automatically generated. When you run the `install` command, the wizard is skipped if the tool detects a `xman.json` file in the installation directory. You can also manually create the file before installation, e.g.:

```json
{
    "AdminPassword": "test",
    "DatabaseName": "xbk28",
    "ProjectName": "xbk28",
    "ServerName": "my-server",
    "Template": "kentico-xperience-sample-mvc",
    "UseCloud": false,
    "Version": "28.0.0"
}
```

### Updating a project version

Currently, there is a bug with updating the project's database version, so the tool only updates the NuGet packages and builds the project. However, the database update command is provided in the UI for easy copy-pasting.

1. Run the `update` command from the Xperience by Kentico root directory which will begin the update wizard:

   ```bash
   xman update
   ```

### Running Continuous Integration

You can use the `ci` command to serialize the database or restore the CI repository to the database:

- `xman ci store`
- `xman ci restore`
