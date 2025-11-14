// Copyright (c) 2013-2025 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;

namespace SharpConfig
{
  /// <summary>
  /// Represents an attribute that tells SharpConfig to
  /// treat the subject as a multiline value.
  /// When the value of this property is written, it produces a setting in the form of:
  ///   Setting=[[Value]]
  /// 
  /// Where '[[' and ']]' act as delimiters for the value.
  /// This allows a value to span across multiple lines.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public sealed class MultilineAttribute : Attribute
  {
  }
}
