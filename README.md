<div class="title-block" style="text-align: center;" align="center">

# SharpConfig

<p><img title="Polly logo" src="docs/assets/logo.svg" width="130" height="130"></p>

**Easy to use cfg / ini configuration library for .NET.**

You can use SharpConfig to read, modify and save configuration files and streams, in either text or binary format.

Minimalistic, fully portable ([.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0#select-net-standard-version)), zero package dependencies.

[![Website](https://img.shields.io/badge/Homepage-blue)](https://sharpconfig.org)
[![NuGet Version](https://img.shields.io/nuget/v/sharpconfig)](https://www.nuget.org/packages/sharpconfig) 
[![NuGet Downloads](https://img.shields.io/nuget/dt/sharpconfig)](https://www.nuget.org/packages/sharpconfig)
[![.NET](https://github.com/cdervis/SharpConfig/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cdervis/SharpConfig/actions/workflows/dotnet.yml)

</div>

## Install

- .NET CLI: `> dotnet add package sharpconfig`
- Package Manager: `> NuGet\Install-Package sharpconfig`
- [Download latest](https://github.com/cdervis/SharpConfig/archive/refs/tags/v3.2.9.1.zip)

## Example

```ini
# An example configuration:

[General]
# a comment
SomeString = Hello World!
SomeInteger = 10 # an inline comment
SomeFloat = 20.05
SomeBoolean = true
SomeArray = { 1, 2, 3 }
Day = Monday

[Person]
Name = Peter
Age = 50
```

To read these values, your C# code would look like:

```csharp
var config = Configuration.LoadFromFile("sample.cfg");
var section = config["General"];

string    someString      = section["SomeString"].StringValue;
int       someInteger     = section["SomeInteger"].IntValue;
float     someFloat       = section["SomeFloat"].FloatValue;
bool      someBool        = section["SomeBoolean"].BoolValue;
int[]     someIntArray    = section["SomeArray"].IntValueArray;
string[]  someStringArray = section["SomeArray"].StringValueArray;
DayOfWeek day             = section["Day"].GetValue<DayOfWeek>();

// Entire user-defined objects can be created from sections and vice versa.
var person = config["Person"].ToObject<Person>();
// ...
```

## Documentation

The full documentation is available on the [website](https://sharpconfig.org).

