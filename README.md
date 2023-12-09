[![Nuget](https://img.shields.io/nuget/v/Xperience.Xman)](https://www.nuget.org/packages/Xperience.Xman#versions-body-tab)
[![build](https://github.com/kentico-ericd/xperience-manager/actions/workflows/build.yml/badge.svg)](https://github.com/kentico-ericd/xperience-manager/actions/workflows/build.yml)

# Xperience Manager (xman)

This tool simplifies the process of installing and managing Xperience by Kentico instances by providing step-by-step wizards with default options provided.

<img src="https://raw.githubusercontent.com/kentico-ericd/xperience-manager/master/img/screenshot.png" width="350" />

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

## Getting started

This tool must be run from a parent directory containing one or more Xperience by Kentico projects, for example the C:\inetpub\wwwroot directory. When you [install](#installing-a-new-project) a new instance, a new profile is created in the `xman.json` file, allowing you to manage multiple installations without changing directory.

## Usage

The following commands can be executed using the `xman` tool name:

- `?`, `help`
- [`i`, `install`](#installing-a-new-project)
- [`u`, `update`](#updating-a-project-version)
- [`ci <store> <restore>`](#running-continuous-integration)
- [`p`, `profile <add> <delete> <switch>`](#managing-profiles)

### Managing profiles

<img src="https://raw.githubusercontent.com/kentico-ericd/xperience-manager/master/img/profiles.png" width="350" />

Certain commands such as `update` are executed against the installation indicated by the current profile. The `profile` command shows you the current profile, and allows you to switch profiles. If you only have one profile, that is automatically selected.

To __switch__ profiles, run the `profile` command from the directory containing the `xman.json` file:

```bash
xman profile
```

You can __add__ or __delete__ profiles using the corresponding commands. This can be useful to register Xperience by Kentico installations that weren't installed using the tool.

```bash
xman p add
xman p delete
```

### Installing a new project

This command installs a new Xperience by Kentico project in a subfolder of the current directory. The name of the profile and subfolder are determined by the __Project name__ entered during installation. The `xman.json` file contains default installation options, which you may edit to speed up the installation of new instances. For example:

```json
"DefaultInstallOptions": {
   "AdminPassword": "mypassword",
   "DatabaseName": "mycompany",
   "ProjectName": "mysite",
   "ServerName": "company-server",
   "Template": "kentico-xperience-sample-mvc",
   "UseCloud": true,
   "Version": null //Version cannot have a default value
},
```

1. Run the `install` command from the directory containing the `xman.json` file, which will begin the installation wizard:

   ```bash
   xman install
   ```

### Updating a project version

Currently, there is a bug with updating the project's database version, so the tool only updates the NuGet packages and builds the project. However, the database update command is provided in the UI for easy copy-pasting.

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
1. Run the `update` command from the directory containing the `xman.json` file, which will begin the update wizard:

   ```bash
   xman update
   ```

### Running Continuous Integration

You can use the `ci` command to serialize the database or restore the CI repository to the database. Your project must have been built at least once to run CI commands.

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
2. Run the desired command to begin the CI process:

   - `xman ci store`
   - `xman ci restore`
