---
title: Home
---

<div class="main-title-container">
  <img src="assets/logo.png" alt="logo" />
  <h1>SharpConfig</h1>
</div>

SharpConfig is an easy to use cfg/ini configuration library for .NET.

You can use SharpConfig to read, modify and save configuration files and streams, in either text or binary format.

<div class="badges">
    <a href="https://www.nuget.org/packages/sharpconfig"><img src="https://img.shields.io/nuget/v/sharpconfig"/></a>
    <a href="https://www.nuget.org/stats/packages/sharpconfig?groupby=Version"><img src="https://img.shields.io/nuget/dt/sharpconfig"/></a>
</div>

## Installation

Choose one of:

- .NET CLI: `> dotnet add package sharpconfig`
- NuGet Package Manager: `> NuGet\Install-Package sharpconfig`
- [Latest Source Code (.zip)](https://github.com/cdervis/SharpConfig/archive/refs/tags/v3.2.9.1.zip)

## Examples

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

string someString = section["SomeString"].StringValue;
int someInteger = section["SomeInteger"].IntValue;
float someFloat = section["SomeFloat"].FloatValue;
bool someBool = section["SomeBoolean"].BoolValue;
int[] someIntArray = section["SomeArray"].IntValueArray;
string[] someStringArray = section["SomeArray"].StringValueArray;
DayOfWeek day = section["Day"].GetValue<DayOfWeek>();

// Entire user-defined objects can be created from sections and vice versa.
var person = config["Person"].ToObject<Person>();
// ...
```

### Loading

```csharp linenums="1"
var cfg1 = Configuration.LoadFromFile("myConfig.cfg");        // Load from a text-based file.
var cfg2 = Configuration.LoadFromStream(myStream);            // Load from a text-based stream.
var cfg3 = Configuration.LoadFromString(myString);            // Load from text (source code).
var cfg4 = Configuration.LoadFromBinaryFile("myConfig.cfg");  // Load from a binary file.
var cfg5 = Configuration.LoadFromBinaryStream(myStream);      // Load from a binary stream.
```

### Iterating

```csharp linenums="1"
// A configuration conforms to IEnumerable and therefore supports normal iteration.

foreach (var section in myConfig)
{
    foreach (var setting in section)
    {
        // ...
    }
}
```

### Creating a configuration in memory

```csharp linenums="1"
// Create the configuration.
var myConfig = new Configuration();

// Set some values.
// This will automatically create the sections and settings.
myConfig["Video"]["Width"].IntValue = 1920;
myConfig["Video"]["Height"].IntValue = 1080;

// Set an array value.
myConfig["Video"]["Formats"].StringValueArray = new[] { "RGB32", "RGBA32" };

// Get the values just to test.
int width = myConfig["Video"]["Width"].IntValue;
int height = myConfig["Video"]["Height"].IntValue;
string[] formats = myConfig["Video"]["Formats"].StringValueArray;

// ...
```

### Saving

```csharp linenums="1"
myConfig.SaveToFile("myConfig.cfg");       // Save to a text-based file.
myConfig.SaveToStream(myStream);           // Save to a text-based stream.
myConfig.SaveToBinaryFile("myConfig.cfg"); // Save to a binary file.
myConfig.SaveToBinaryStream(myStream);     // Save to a binary stream.
```
    
## Options

Sometimes a project has special configuration files or other needs, for example ignoring all comments in a file.

To allow for greater flexibility, SharpConfig's behavior is
modifiable using **static properties** of the `Configuration` class.

The following properties are available:

| Option | Type | Description |
|--------|------|-------------|
| **`CultureInfo`** | `CultureInfo` | Gets or sets the `CultureInfo` that is used for value conversion in SharpConfig. <br/> The default value is `CultureInfo.InvariantCulture`. |
| **`ValidCommentChars`** | `#!csharp char[]` | Gets the array that contains all valid comment delimiting characters. <br/> The default value is `{ '#', ';' }` |
| **`PreferredCommentChar`** | `#!csharp char` | Gets or sets the preferred comment char when saving configurations. <br/> The default value is `'#'`. |
| **`ArrayElementSeparator`** | `#!csharp char` | Gets or sets the array element separator character for settings. <br/> The default value is `','`. <br/> Remember that after you change this value while `Setting` instances exist, to expect their `ArraySize` and other array-related values to return different values. |
| **`IgnoreInlineComments`** | `#!csharp bool` | Gets or sets a value indicating whether inline comments should be ignored **when parsing a configuration**. |
| **`IgnorePreComments`** | `#!csharp bool` | Gets or sets a value indicating whether pre-comments should be ignored **when parsing a configuration**. |
| **`SpaceBetweenEquals`** | `#!csharp bool` | Gets or sets a value indicating whether space between equals should be added **when saving a configuration**. |
| **`OutputRawStringLiterals`** | `#!csharp bool` | Gets or sets a value indicating whether string values are written without quotes, but including everything in between. <br/> For example, a setting `MySetting=" Example value"` would be written to a file as `MySetting= Example value`.

## Ignoring properties, fields and types

Suppose you have the following class:

```csharp linenums="1"
class SomeClass
{
    public string Name { get; set; }
    public int Age { get; set; }

    [SharpConfig.Ignore]
    public int SomeInt { get; set; }
}
```

SharpConfig will now ignore the `SomeInt` property when creating sections from objects of type `SomeClass` and vice versa.
Now suppose you have a type in your project that should always be ignored.
You would have to mark every property that returns this type with a `#!csharp [SharpConfig.Ignore]` attribute.
An easier solution is to just apply the `#!csharp [SharpConfig.Ignore]` attribute to the type.

```csharp title="Example" linenums="1"
[SharpConfig.Ignore]
class ThisTypeShouldAlwaysBeIgnored
{
    // ...
}
```

**instead of:**

```csharp title="Redundant attributes" linenums="1"
[SharpConfig.Ignore]
class SomeClass
{
  [SharpConfig.Ignore]
  public ThisTypeShouldAlwaysBeIgnored T1 { get; set; }

  [SharpConfig.Ignore]
  public ThisTypeShouldAlwaysBeIgnored T2 { get; set; }

  [SharpConfig.Ignore]
  public ThisTypeShouldAlwaysBeIgnored T3 { get; set; }
}
```

!!! info
    This ignoring mechanism works the same way for public fields.

## Adding custom object converters

There may be cases where you want to implement conversion rules for a custom type, with
specific requirements. This is easy and involves two steps, which are illustrated
using the `Person` example:

```csharp linenums="1"
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

Step 1: Create a custom converter class that derives from `SharpConfig.TypeStringConverter<T>`.

```csharp title="Example" linenums="1"
using SharpConfig;

public class PersonStringConverter : TypeStringConverter<Person>
{
    // This method is responsible for converting a Person object to a string.
    public override string ConvertToString(object value)
    {
      var person = (Person)value;
      return string.Format("[{0};{1}]", person.Name, person.Age);
    }

    // This method is responsible for converting a string to a Person object.
    public override object ConvertFromString(string value, Type hint)
    {
      var split = value.Trim('[', ']').Split(';');

      var person = new Person();
      person.Name = split[0];
      person.Age = int.Parse(split[1]);

      return person;
    }
}
```

Step 2: Register the `PersonStringConverter` (anywhere you like):

```csharp linenums="1"
using SharpConfig;

Configuration.RegisterTypeStringConverter(new PersonStringConverter());
```

That's it!

Whenever a `Person` object is used on a `Setting` (via `#!csharp GetValue()` and `#!csharp SetValue()`),
your converter is selected to take care of the conversion.
This also automatically works with `#!csharp SetValue()` for arrays and `#!csharp GetValueArray()`.
