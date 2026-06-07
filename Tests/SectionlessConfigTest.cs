// Copyright (c) 2013-2026 Cem Dervis, MIT License.
// https://sharpconfig.org

using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using SharpConfig;

namespace Tests
{
  [TestFixture]
  public sealed class SectionlessConfigTest
  {
    [Test]
    public void SectionlessFileKeepsSettings()
    {
      var cfg = Configuration.LoadFromString("Setting1 = Value1\nSetting2 = Value2");

      Assert.AreEqual(2, cfg.DefaultSection.SettingCount);
      Assert.AreEqual("Value1", cfg.DefaultSection["Setting1"].StringValue);
      Assert.AreEqual("Value2", cfg.DefaultSection["Setting2"].StringValue);

      // Round-trips without gaining a section header.
      var saved = cfg.SaveToString();
      Assert.IsFalse(saved.Contains('['));
      Assert.AreEqual(2, Configuration.LoadFromString(saved).DefaultSection.SettingCount);
    }
  }
}
