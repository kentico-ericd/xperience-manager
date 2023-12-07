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

## Getting started

This tool must be run from a parent directory containing one or more Xperience by Kentico projects, for example the C:\inetpub\wwwroot directory. When you [install](#installing-a-project-with-the-wizard) a new instance, a new profile is created in the `xman.config` file, allowing you to manage multiple installations without changing directory.

## Usage

The following commands can be executed using the `xman` tool name:

- `?`, `help`
- `p`, `profile`
- `i`, `install`
- `u`, `update`
- `ci store`, `ci restore`

### Changing profiles

<img src="https://raw.githubusercontent.com/kentico-ericd/xperience-manager/master/img/profiles.png" width="350">

Certain commands such as `update` are executed against the installation indicated by the current profile. The `profile` command shows you the current profile, and allows you to switch profiles. If you only have one profile, that is automatically selected.

1. Run the `profile` command from the directory containing the `xman.json` file:

   ```bash
   xman profile
   ```

### Installing a project with the wizard

1. Run the `install` command from the directory containing the `xman.json` file, which will begin the installation wizard:

   ```bash
   xman install
   ```

### Updating a project version

Currently, there is a bug with updating the project's database version, so the tool only updates the NuGet packages and builds the project. However, the database update command is provided in the UI for easy copy-pasting.

1. (optional) Select a profile with the [`profile`](#changing-profiles) command
1. Run the `update` command from the directory containing the `xman.json` file, which will begin the update wizard:

   ```bash
   xman update
   ```

### Running Continuous Integration

You can use the `ci` command to serialize the database or restore the CI repository to the database.

1. (optional) Select a profile with the [`profile`](#changing-profiles) command
2. Run the desired command to begin the CI process:

   - `xman ci store`
   - `xman ci restore`
