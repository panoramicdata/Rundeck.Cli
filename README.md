[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

[![Codacy Badge](https://app.codacy.com/project/badge/grade/Rundeck.Cli)](https://app.codacy.com/gh/panoramicdata/Rundeck.Cli/dashboard)

# Rundeck.Cli

Nuget package for dotnet new Rundeck

## Build instructions

To build, from the root directory (and already having installed the latest version of nuget), type:
> nuget pack .\Rundeck.Cli.nuspec

## Upload instructions
You can then upload it to [https://www.nuget.org/packages/manage/upload](https://www.nuget.org/packages/manage/upload)

## Installation instructions
To install the template, use:

> dotnet new -i Rundeck.Cli

To create a new project using the template, use:

> dotnet new Rundeck

or

> dotnet new Rundeck --name MyProject.MyNameSpace

or

> dotnet new Rundeck --name MyProject.MyNameSpace --url=RunDeckUrl --username=RundeckUsername --password=RundeckPassword