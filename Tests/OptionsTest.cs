// Copyright (c) 2013-2025 Cem Dervis, MIT License.
// https://sharpconfig.org

using NUnit.Framework;
using SharpConfig;

namespace Tests
{
  [TestFixture]
  public sealed class OptionsTest
  {
    [Test]
    public void TestArrayElementSeparator()
    {
      Configuration.ResetOptions();
      Configuration.ArrayElementSeparator = '|';

      var cfg = new Configuration();

      cfg["TestSection"]["Setting"].StringValueArray = new[] { "String1", "String2", "String3" };

      Assert.AreEqual("{String1|String2|String3}", cfg["TestSection"]["Setting"].RawValue);

      Configuration.ResetOptions();
    }
  }
}
