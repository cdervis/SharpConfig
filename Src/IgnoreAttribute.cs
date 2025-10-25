// Copyright (c) 2013-2025 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;

namespace SharpConfig
{
  /// <summary>
  /// Represents an attribute that tells SharpConfig to
  /// ignore the subject this attribute is applied to.
  /// For example, if this attribute is applied to a property
  /// of a type, that property will be ignored when creating
  /// sections from objects and vice versa.
  /// </summary>
  [AttributeUsage(AttributeTargets.All)]
  public sealed class IgnoreAttribute : Attribute
  {
  }
}
