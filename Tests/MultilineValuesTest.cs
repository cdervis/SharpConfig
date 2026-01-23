// Copyright (c) 2013-2026 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using SharpConfig;

namespace Tests
{
  [TestFixture]
  public sealed class MultilineValuesTest
  {
    [Test]
    public void BasicMultilineValue()
    {
      const string cfgStr = "[Section]\nSetting=[[\nLine1\nLine2\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      Assert.AreEqual("[[\nLine1\nLine2\n]]", setting.RawValue);
      Assert.AreEqual("Line1\nLine2", setting.StringValue);
    }

    [Test]
    public void EmptyMultilineValue()
    {
      const string cfgStr = "[Section]\nSetting=[[\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      Assert.AreEqual("[[\n]]", setting.RawValue);
      Assert.AreEqual(string.Empty, setting.StringValue);
    }

    [Test]
    public void SingleLineMultilineValue()
    {
      const string cfgStr = "[Section]\nSetting=[[Content]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      Assert.AreEqual("[[Content]]", setting.RawValue);
      Assert.AreEqual("Content", setting.StringValue);
    }

    [Test]
    public void MultipleBlankLinesInMultiline()
    {
      const string cfgStr = "[Section]\nSetting=[[\n\n\n\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      Assert.AreEqual("[[\n\n\n\n]]", setting.RawValue);
      Assert.AreEqual("\n\n", setting.StringValue);
    }

    [Test]
    public void QuotesInMultilineValue()
    {
      const string cfgStr = "[Section]\nSetting=[[\n\"Line1\"\n'Line2'\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      // Verbatim content, but StringValue property normally trims wrapping quotes.
      // However, we changed it to NOT trim if it's multiline.
      Assert.AreEqual("\"Line1\"\n'Line2'", setting.StringValue);
    }

    [Test]
    public void CommentCharsInMultilineValue()
    {
      const string cfgStr = "[Section]\nSetting=[[\n# Line1\n; Line2\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      Assert.AreEqual("# Line1\n; Line2", setting.StringValue);
    }

    [Test]
    public void EqualsSignsInMultilineValue()
    {
      const string cfgStr = "[Section]\nSetting=[[\nKey1=Value1\nKey2=Value2\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      Assert.AreEqual("Key1=Value1\nKey2=Value2", setting.StringValue);
    }

    [Test]
    public void BracketsInMultilineValue()
    {
      const string cfgStr = "[Section]\nSetting=[[\n[Section]\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      Assert.AreEqual("[Section]", setting.StringValue);
    }

    [Test]
    public void EscapeSequencesInMultilineValue()
    {
      const string cfgStr = "[Section]\nSetting=[[\n\\n\\t\\\\\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      // SharpConfig doesn't unescape within [[ ]], it's verbatim.
      Assert.AreEqual("\\n\\t\\\\", setting.StringValue);
    }

    [Test]
    public void IndentationPreservation()
    {
      const string cfgStr = "[Section]\nSetting=[[\n  Line1\n    Line2\n]]";
      var cfg = Configuration.LoadFromString(cfgStr);
      var setting = cfg["Section"]["Setting"];

      Assert.AreEqual("  Line1\n    Line2", setting.StringValue);
    }

    [Test]
    public void MissingClosingDelimiter()
    {
      const string cfgStr = "[Section]\nSetting=[[\nLine1\nLine2";
      Assert.Throws<ParserException>(() => Configuration.LoadFromString(cfgStr));
    }

    [Test]
    public void RoundTripTest()
    {
      var cfg = new Configuration();
      cfg["Section"]["Setting"].RawValue = "[[\nLine1\nLine2\n]]";

      var saved = cfg.SaveToString();
      var reloaded = Configuration.LoadFromString(saved);

      Assert.AreEqual(cfg["Section"]["Setting"].RawValue, reloaded["Section"]["Setting"].RawValue);
      Assert.AreEqual(cfg["Section"]["Setting"].StringValue, reloaded["Section"]["Setting"].StringValue);
    }

    [Test]
    public void MultilineAttributeTest()
    {
      var obj = new MultilineTestObject { MultilineValue = "Line1\nLine2" };
      var section = Section.FromObject("Section", obj);

      // We expect the RawValue to be wrapped in [[ ]]
      Assert.AreEqual("[[\nLine1\nLine2\n]]", section["MultilineValue"].RawValue);

      // Verify it can be read back
      var back = section.ToObject<MultilineTestObject>();
      Assert.AreEqual(obj.MultilineValue, back.MultilineValue);
    }

    private class MultilineTestObject
    {
      [Multiline]
      public string MultilineValue { get; set; } = string.Empty;
    }

    [Test]
    public void VerifyTestFile()
    {
      string content = File.ReadAllText("MultilineValuesTest.txt");

      string filename = Path.GetTempFileName();
      File.WriteAllText(filename, content);

      try
      {
        var cfg = Configuration.LoadFromFile(filename);

        var mv1 = cfg["Section"]["MultilineValue1"];
        Assert.IsTrue(mv1.RawValue.StartsWith("[["));
        Assert.IsTrue(mv1.RawValue.EndsWith("]]"));

        var lines = mv1.StringValue.Split('\n');
        Assert.AreEqual("Line1", lines[0]);
        Assert.AreEqual("Line2", lines[1]);
        Assert.AreEqual("  Line3 (indented)", lines[2]);
        Assert.AreEqual("\"Line4 with quote at start", lines[3]);
        Assert.AreEqual("Line5 with quote at end\"", lines[4]);

        var regex = cfg["Section"]["RegexPattern"].StringValue;
        Assert.IsTrue(regex.Contains("IP\\s*Address(?!s)"));
        Assert.IsTrue(regex.Contains("(?<Time>\\d{4}-\\d{2}-\\d{2}\\s+\\d{2}:\\d{2}:\\d{2}\\s+UTC)"));

        var lines1 = cfg["Section"]["MultilineValue1"].RawValue.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual("[[", lines1[0]);
        Assert.AreEqual("Line1", lines1[1]);
        Assert.AreEqual("Line2", lines1[2]);
        Assert.AreEqual("  Line3 (indented)", lines1[3]);
        Assert.AreEqual("\"Line4 with quote at start", lines1[4]);
        Assert.AreEqual("Line5 with quote at end\"", lines1[5]);
        Assert.AreEqual("Line6 with \" in the middle", lines1[6]);
        Assert.AreEqual("Line7 with \"\" in the middle", lines1[7]);
        Assert.AreEqual("\"\"", lines1[8]);
        Assert.AreEqual("]]", lines1[9]);

        Assert.AreEqual("SomeValue", cfg["Section"]["SomeSetting"].StringValue);
      }
      finally
      {
        File.Delete(filename);
      }
    }
  }
}
