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
- [`m`, `macros`](#re-signing-macros)
- [`ci <store> <restore>`](#running-continuous-integration)
- [`cd <store> <restore> <config>`](#running-continuous-deployment)
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

### Re-signing macros

See [our documentation](https://docs.xperience.io/xp/developers-and-admins/configuration/macro-expressions/macro-signatures) for more information about macro signatures and the available options.

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
1. Run the `macros` command from the directory containing the `xman.json` file, which will begin the macro wizard:

   ```bash
   xman macros
   ```

### Running Continuous Integration

You can use the `ci` command to serialize the database or restore the CI repository to the database. Your project must have been built at least once to run CI commands.

1. (optional) Select a profile with the [`profile`](#managing-profiles) command
2. Run the desired command to begin the CI process:

   - `xman ci store`
   - `xman ci restore`

### Running Continuous Deployment

This tool can help you manage a local [Continuous Deployment](https://docs.xperience.io/xp/developers-and-admins/ci-cd/continuous-deployment) environment. For example, if you are self-hosting your website and you have __DEV__ and __PROD__ Xperience by Kentico instances, the tool simplifies the process of migrating database changes from lower environments to production.

The CD configuration files and repositories are stored in a subdirectory from where the tool is run from, by default in `/ContinuousDeployment`. You can customize the path by changing the __CDRootPath__ property in `xman.json`:

```json
{
    "CDRootPath": "C:\\XperienceCDFiles",
    ...
}
```

The [configuration file](https://docs.xperience.io/xp/developers-and-admins/ci-cd/exclude-objects-from-ci-cd) is automatically created when you run the `cd` command and can be manually edited to fine-tune the CD process. You can also run the `config` command to edit the configuration file using a wizard. For example, you may want to change the [__RestoreMode__](https://docs.xperience.io/xp/developers-and-admins/ci-cd/exclude-objects-from-ci-cd#ExcludeobjectsfromCI/CD-CDrestoremode) before restoring CD data to the database.

1. Select a profile with the [`profile`](#managing-profiles) command. This determines which configuration file is modified
1. Run the `config` command from the directory containing the `xman.json` file, which will begin the configuration wizard:

   ```bash
   xman cd config
   ```

When you are finished development and wish to serialize the CD data to the filesystem, use the `store` command:

1. Select a profile with the [`profile`](#managing-profiles) command. This determines which project's database is serialized
1. Run the `store` command from the directory containing the `xman.json` file:

   ```bash
   xman p # switch to DEV profile
   xman cd store # serialize DEV database
   ```

To migrate the changes from development to production, run the `restore` command:

1. Select a profile with the [`profile`](#managing-profiles) command. This determines which project's database is updated
1. Run the `restore` command from the directory containing the `xman.json` file. The tool will display a list of profiles to choose as the __source__ for the restore process (in this example, the DEV profile):

   ```bash
   xman p # switch to PROD profile
   xman cd restore # restore DEV CD files to PROD database
   ```