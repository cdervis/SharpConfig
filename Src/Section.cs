// Copyright (c) 2013-2026 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpConfig
{
  /// <summary>
  /// Represents a group of <see cref="Setting"/> objects.
  /// </summary>
  public sealed class Section : ConfigurationElement, IEnumerable<Setting>
  {
    /// <summary>
    /// The name of the default, hidden section.
    /// </summary>
    public const string DefaultSectionName = "$SharpConfigDefaultSection";

    private sealed class PropertyMappingMetadata
    {
      public PropertyMappingMetadata(PropertyInfo property)
      {
        Property = property;
        CanRead = property.CanRead;
        CanWrite = property.CanWrite;
        Ignore = HasIgnoreAttribute(property) || HasIgnoreAttribute(property.PropertyType);
        IsMultiline = HasMultilineAttribute(property);
      }

      public PropertyInfo Property { get; }
      public bool CanRead { get; }
      public bool CanWrite { get; }
      public bool Ignore { get; }
      public bool IsMultiline { get; }
    }

    private sealed class FieldMappingMetadata
    {
      public FieldMappingMetadata(FieldInfo field)
      {
        Field = field;
        IsInitOnly = field.IsInitOnly;
        Ignore = HasIgnoreAttribute(field) || HasIgnoreAttribute(field.FieldType);
        IsMultiline = HasMultilineAttribute(field);
      }

      public FieldInfo Field { get; }
      public bool IsInitOnly { get; }
      public bool Ignore { get; }
      public bool IsMultiline { get; }
    }

    private sealed class TypeMappingMetadata
    {
      public TypeMappingMetadata(Type type)
      {
        Properties = Array.ConvertAll(
            type.GetProperties(BindingFlags.Instance | BindingFlags.Public),
            property => new PropertyMappingMetadata(property));

        Fields = Array.ConvertAll(
            type.GetFields(BindingFlags.Instance | BindingFlags.Public),
            field => new FieldMappingMetadata(field));
      }

      public PropertyMappingMetadata[] Properties { get; }
      public FieldMappingMetadata[] Fields { get; }
    }

    private static readonly object s_mappingMetadataLock = new object();
    
    private static readonly Dictionary<Type, TypeMappingMetadata> s_mappingMetadata = new();

    private readonly List<Setting> _settings;
    /// <summary>
    /// Initializes a new instance of the <see cref="Section"/> class.
    /// </summary>
    ///
    /// <param name="name">The name of the section.</param>
    public Section(string name) : base(name)
    {
      _settings = new List<Setting>();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Section"/> class that is
    /// based on an existing object.
    /// Important: the section is built only from the public getter properties
    /// and fields of its type.
    /// When this method is called, all of those properties will be called
    /// and fields accessed once to obtain their values.
    /// Properties and fields that are marked with the <see cref="IgnoreAttribute"/> attribute
    /// or are of a type that is marked with that attribute, are ignored.
    /// </summary>
    /// <param name="name">The name of the section.</param>
    /// <param name="obj"></param>
    /// <returns>The newly created section.</returns>
    ///
    /// <exception cref="ArgumentException">When <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">When <paramref name="obj"/> is null.</exception>
    public static Section FromObject(string name, object obj)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentException("The section name must not be null or empty.", nameof(name));
      }

      if (obj == null)
      {
        throw new ArgumentNullException(nameof(obj));
      }

      var section = new Section(name);
      var type = obj.GetType();

      var metadata = GetTypeMappingMetadata(type);

      foreach (var prop in metadata.Properties)
      {
        if (!prop.CanRead || prop.Ignore)
        {
          // Skip this property, as it can't be read from.
          continue;
        }

        object? value = prop.Property.GetValue(obj, null);
        var setting = new Setting(prop.Property.Name);

        if (prop.IsMultiline)
        {
          setting.RawValue = $"[[\n{value}\n]]";
        }
        else
        {
          setting.SetValue(value);
        }

        section.Add(setting);
      }

      // Repeat for each public field.
      foreach (var field in metadata.Fields)
      {
        if (field.Ignore)
        {
          // Skip this field.
          continue;
        }

        object? value = field.Field.GetValue(obj);
        var setting = new Setting(field.Field.Name);

        if (field.IsMultiline)
        {
          setting.RawValue = $"[[\n{value}\n]]";
        }
        else
        {
          setting.SetValue(value);
        }

        section.Add(setting);
      }

      return section;
    }

    /// <summary>
    /// Creates an object of a specific type, and maps the settings
    /// in this section to the public properties and writable fields of the object.
    /// Properties and fields that are marked with the <see cref="IgnoreAttribute"/> attribute
    /// or are of a type that is marked with that attribute, are ignored.
    /// </summary>
    ///
    /// <typeparam name="T">
    /// The type of object to create.
    /// Note: the type must be default-constructible, meaning it has a public default constructor.
    /// </typeparam>
    ///
    /// <returns>The created object.</returns>
    ///
    /// <remarks>
    /// The specified type must have a public default constructor
    /// in order to be created.
    /// </remarks>
    public T ToObject<T>()
        where T : new()
    {
      var obj = Activator.CreateInstance<T>()!;
      SetValuesTo(obj);

      return obj;
    }

    /// <summary>
    /// Creates an object of a specific type, and maps the settings
    /// in this section to the public properties and writable fields of the object.
    /// Properties and fields that are marked with the <see cref="IgnoreAttribute"/> attribute
    /// or are of a type that is marked with that attribute, are ignored.
    /// </summary>
    ///
    /// <param name="type">
    /// The type of object to create.
    /// Note: the type must be default-constructible, meaning it has a public default constructor.
    /// </param>
    ///
    /// <returns>The created object.</returns>
    ///
    /// <remarks>
    /// The specified type must have a public default constructor
    /// in order to be created.
    /// </remarks>
    ///
    /// <exception cref="ArgumentNullException">When <paramref name="type"/> is null.</exception>
    public object ToObject(Type type)
    {
      if (type == null)
      {
        throw new ArgumentNullException(nameof(type));
      }

      var obj = Activator.CreateInstance(type)!;
      SetValuesTo(obj!);

      return obj;
    }

    /// <summary>
    /// Assigns the values of an object's public properties and fields to the corresponding
    /// <b>already existing</b> settings in this section.
    /// Properties and fields that are marked with the <see cref="IgnoreAttribute"/> attribute
    /// or are of a type that is marked with that attribute, are ignored.
    /// </summary>
    ///
    /// <param name="obj">The object from which the values are obtained.</param>
    ///
    /// <exception cref="ArgumentNullException">When <paramref name="obj"/> is null.</exception>
    public void GetValuesFrom(object obj)
    {
      if (obj == null)
      {
        throw new ArgumentNullException(nameof(obj));
      }

      var metadata = GetTypeMappingMetadata(obj.GetType());

      // Scan the type's properties.
      foreach (var prop in metadata.Properties)
      {
        if (!prop.CanRead)
        {
          continue;
        }

        SetSettingValueFromProperty(prop, obj);
      }

      // Scan the type's fields.
      foreach (var field in metadata.Fields)
      {
        SetSettingValueFromField(field, obj);
      }
    }

    private void SetSettingValueFromProperty(PropertyMappingMetadata property, object instance)
    {
      if (property.Ignore)
      {
        return;
      }

      Setting? setting = FindSetting(property.Property.Name);
      if (setting == null)
      {
        return;
      }

      object? value = property.Property.GetValue(instance, null);

      if (property.IsMultiline)
      {
        setting.RawValue = $"[[\n{value}\n]]";
      }
      else
      {
        setting.SetValue(value);
      }
    }

    private void SetSettingValueFromField(FieldMappingMetadata field, object instance)
    {
      if (field.Ignore)
      {
        return;
      }

      Setting? setting = FindSetting(field.Field.Name);
      if (setting == null)
      {
        return;
      }

      object? value = field.Field.GetValue(instance);

      if (field.IsMultiline)
      {
        setting.RawValue = $"[[\n{value}\n]]";
      }
      else
      {
        setting.SetValue(value);
      }
    }

    /// <summary>
    /// Assigns the values of this section to an object's public properties and fields.
    /// Properties and fields that are marked with the <see cref="IgnoreAttribute"/> attribute
    /// or are of a type that is marked with that attribute, are ignored.
    /// </summary>
    ///
    /// <param name="obj">The object that is modified based on the section.</param>
    ///
    /// <exception cref="ArgumentNullException">When <paramref name="obj"/> is null.</exception>
    public void SetValuesTo(object obj)
    {
      if (obj == null)
      {
        throw new ArgumentNullException(nameof(obj));
      }

      var metadata = GetTypeMappingMetadata(obj.GetType());

      // Scan the type's properties.
      foreach (var prop in metadata.Properties)
      {
        if (!prop.CanWrite || prop.Ignore)
        {
          continue;
        }

        var setting = FindSetting(prop.Property.Name);

        if (setting == null)
        {
          continue;
        }

        var propElementType = prop.Property.PropertyType.GetElementType();
        var value = prop.Property.PropertyType.IsArray ? setting.GetValueArray(propElementType!)
                                                       : setting.GetValue(prop.Property.PropertyType);

        if (prop.Property.PropertyType.IsArray)
        {
          var settingArray = value as Array;
          var propArray = prop.Property.GetValue(obj, null) as Array;

          if (settingArray != null && (propArray == null || propArray.Length != settingArray.Length))
          {
            // (Re)create the property's array.
            propArray = Array.CreateInstance(propElementType!, length: settingArray.Length);
          }

          for (int i = 0; i < settingArray!.Length; i++)
          {
            propArray?.SetValue(settingArray.GetValue(i), i);
          }

          prop.Property.SetValue(obj, propArray, null);
        }
        else
        {
          prop.Property.SetValue(obj, value, null);
        }
      }

      // Scan the type's fields.
      foreach (var field in metadata.Fields)
      {
        // Skip readonly fields.
        if (field.IsInitOnly || field.Ignore)
        {
          continue;
        }

        var setting = FindSetting(field.Field.Name);

        if (setting == null)
        {
          continue;
        }

        var fieldElementType = field.Field.FieldType.GetElementType();
        var value = field.Field.FieldType.IsArray ? setting.GetValueArray(fieldElementType!)
                                                  : setting.GetValue(field.Field.FieldType);

        if (field.Field.FieldType.IsArray)
        {
          var settingArray = value as Array;
          var fieldArray = field.Field.GetValue(obj) as Array;

          if (settingArray != null && (fieldArray == null || fieldArray.Length != settingArray.Length))
          {
            // (Re)create the field's array.
            fieldArray = Array.CreateInstance(fieldElementType!, settingArray.Length);
          }

          for (int i = 0; i < settingArray!.Length; i++)
          {
            fieldArray!.SetValue(settingArray.GetValue(i), i);
          }

          field.Field.SetValue(obj, fieldArray);
        }
        else
        {
          field.Field.SetValue(obj, value);
        }
      }
    }

    /// <summary>
    /// Gets an enumerator that iterates through the section.
    /// </summary>
    public IEnumerator<Setting> GetEnumerator() => _settings.GetEnumerator();

    /// <summary>
    /// Gets an enumerator that iterates through the section.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Adds a setting to the section.
    /// </summary>
    /// <param name="setting">The setting to add.</param>
    ///
    /// <exception cref="ArgumentNullException">When <paramref name="setting"/> is null.</exception>
    /// <exception cref="ArgumentException">When the specified setting already exists in the
    /// section.</exception>
    public void Add(Setting setting)
    {
      if (setting == null)
      {
        throw new ArgumentNullException(nameof(setting));
      }

      if (Contains(setting))
      {
        throw new ArgumentException("The specified setting already exists in the section.");
      }

      _settings.Add(setting);
    }

    /// <summary>
    /// Adds a setting with a specific name and empty value to the section.
    /// </summary>
    /// <param name="settingName">The name of the setting to add.</param>
    /// <returns>The added setting.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="settingName"/> is null or
    /// empty.</exception>
    public Setting Add(string settingName) => Add(settingName, string.Empty);

    /// <summary>
    /// Adds a setting with a specific name and value to the section.
    /// </summary>
    /// <param name="settingName">The name of the setting to add.</param>
    /// <param name="settingValue">The initial value of the setting to add.</param>
    /// <returns>The added setting.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="settingName"/> is null or
    /// empty.</exception>
    public Setting Add(string settingName, object settingValue)
    {
      var setting = new Setting(settingName, settingValue);
      Add(setting);

      return setting;
    }

    /// <summary>
    /// Removes a setting from the section by its name.
    /// If there are multiple settings with the same name, only the first setting is removed.
    /// To remove all settings that have the name name, use the RemoveAllNamed() method instead.
    /// </summary>
    /// <param name="settingName">The case-sensitive name of the setting to remove.</param>
    /// <returns>True if a setting with the specified name was removed; false otherwise.</returns>
    ///
    /// <exception cref="ArgumentNullException">When <paramref name="settingName"/> is null or
    /// empty.</exception>
    public bool Remove(string settingName)
    {
      if (string.IsNullOrEmpty(settingName))
      {
        throw new ArgumentNullException(nameof(settingName));
      }

      var setting = FindSetting(settingName);
      return setting != null && Remove(setting);
    }

    /// <summary>
    /// Removes a setting from the section.
    /// </summary>
    /// <param name="setting">The setting to remove.</param>
    /// <returns>True if the setting was removed; false otherwise.</returns>
    public bool Remove(Setting setting)
    {
      if (setting == null)
      {
        throw new ArgumentNullException(nameof(setting));
      }

      return _settings.Remove(setting);
    }

    /// <summary>
    /// Removes all settings that have a specific name.
    /// </summary>
    /// <param name="settingName">The case-sensitive name of the settings to remove.</param>
    ///
    /// <exception cref="ArgumentNullException">When <paramref name="settingName"/> is null or
    /// empty.</exception>
    public void RemoveAllNamed(string settingName)
    {
      if (string.IsNullOrEmpty(settingName))
      {
        throw new ArgumentNullException(nameof(settingName));
      }

      while (Remove(settingName))
      {
        // Nothing to do.
      }
    }

    /// <summary>
    /// Clears the section of all settings.
    /// </summary>
    public void Clear() => _settings.Clear();

    /// <summary>
    /// Determines whether a specified setting is contained in the section.
    /// </summary>
    /// <param name="setting">The setting to check for containment.</param>
    /// <returns>True if the setting is contained in the section; false otherwise.</returns>
    public bool Contains(Setting setting) => _settings.Contains(setting);

    /// <summary>
    /// Determines whether a specifically named setting is contained in the section.
    /// </summary>
    /// <param name="settingName">The case-sensitive name of the setting.</param>
    /// <returns>True if the setting is contained in the section; false otherwise.</returns>
    ///
    /// <exception cref="ArgumentNullException">When <paramref name="settingName"/> is null or
    /// empty.</exception>
    public bool Contains(string settingName)
    {
      if (string.IsNullOrEmpty(settingName))
      {
        throw new ArgumentNullException(nameof(settingName));
      }

      return FindSetting(settingName) != null;
    }

    /// <summary>
    /// Gets the number of settings that are in the section.
    /// </summary>
    public int SettingCount => _settings.Count;

    /// <summary>
    /// Gets or sets a setting by index.
    /// </summary>
    /// <param name="index">The index of the setting in the section.</param>
    ///
    /// <returns>
    /// The setting at the specified index.
    /// Note: no setting is created when using this accessor.
    /// </returns>
    ///
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is out of
    /// range.</exception>
    public Setting this[int index]
    {
      get
      {
        if (index < 0 || index >= _settings.Count)
        {
          throw new ArgumentOutOfRangeException(nameof(index));
        }

        return _settings[index];
      }
    }

    /// <summary>
    /// Gets or sets a setting by its name.
    /// If there are multiple settings with the same name, the first setting is returned.
    /// If you want to obtain all settings that have the same name, use the GetSettingsNamed() method instead.
    /// </summary>
    ///
    /// <param name="name">The case-sensitive name of the setting.</param>
    ///
    /// <returns>
    /// The setting if found, otherwise a new setting with
    /// the specified name is created, added to the section and returned.
    /// </returns>
    public Setting this[string name]
    {
      get
      {
        var setting = FindSetting(name);

        if (setting == null)
        {
          setting = new Setting(name);
          _settings.Add(setting);
        }

        return setting;
      }
    }

    /// <summary>
    /// Gets all settings that have a specific name.
    /// </summary>
    /// <param name="name">The case-sensitive name of the settings.</param>
    /// <returns>
    /// The found settings.
    /// </returns>
    public IEnumerable<Setting> GetSettingsNamed(string name)
    {
      return _settings.Where(
          setting => string.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    // Finds a setting by its name.
    private Setting? FindSetting(string name)
    {
      return _settings.FirstOrDefault(
          setting => string.Equals(setting.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static TypeMappingMetadata GetTypeMappingMetadata(Type type)
    {
      lock (s_mappingMetadataLock)
      {
        if (!s_mappingMetadata.TryGetValue(type, out var metadata))
        {
          metadata = new TypeMappingMetadata(type);
          s_mappingMetadata.Add(type, metadata);
        }

        return metadata;
      }
    }

    private static bool HasIgnoreAttribute(MemberInfo member)
      => member.GetCustomAttributes(typeof(IgnoreAttribute), false).Length > 0;

    private static bool HasIgnoreAttribute(Type type)
      => type.GetCustomAttributes(typeof(IgnoreAttribute), false).Length > 0;

    private static bool HasMultilineAttribute(MemberInfo member)
      => member.GetCustomAttributes(typeof(MultilineAttribute), false).Length > 0;

    /// <summary>
    /// Gets the element's expression as a string.
    /// An example for a section would be "[Section]".
    /// </summary>
    /// <returns>The element's expression as a string.</returns>
    protected override string GetStringExpression() => $"[{Name}]";
  }
}
