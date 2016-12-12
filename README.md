# AnyBar .NET Core client

[![Latest version](https://img.shields.io/nuget/v/AnyBar.Client.svg)](https://www.nuget.org/packages/AnyBar.Client) 
[![License MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/jenyayel/anybar-client/master/LICENSE.md) 

Client library for controlling  [AnyBar](https://github.com/tonsky/AnyBar).

## Build Status

| Build server | Platform     | Build status                                                                                                                                                                 | Tests              |
|--------------|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------|
| AppVeyor     | Windows      | [![Build status](https://img.shields.io/appveyor/ci/jenyayel/anybar-client/master.svg)](https://ci.appveyor.com/project/jenyayel/anybar-client/branch/master)                |                    |
| Travis       | Linux / OS X | [![Build status](https://img.shields.io/travis/jenyayel/anybar-client/master.svg)](https://travis-ci.org/jenyayel/anybar-client)                                             |                    |


## Install

AnyBar client is available as a NuGet package. 
You can install it by listing it in your `project.json` or `.csproj` files or by using the [NuGet CLI](https://docs.nuget.org/ndocs/guides/install-nuget):
```
Install-Package AnyBar.Client
```

## Usage

The API usage:
```
var client = new AnyBarClient("localhost", 1738);
client.Change(AnyBarImage.Red);
```

The list of possible colors defined in type `AnyBar.AnyBarImage`:

```
public enum AnyBarImage
{
    White,
    Red,
    Orange,
    Yellow,
    Green,
    Cyan,
    Blue,
    Purple,
    Black,
    Question,
    Exclamation
}
```


## Build from source

Clone the repo and run `./build.ps1` or `./build.sh`. Build tasks defined in `build.cake` 
file which uses [Cake](http://cakebuild.net/).
