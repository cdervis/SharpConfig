// Copyright (c) 2013-2026 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SharpConfig
{
  internal static class ConfigurationWriter
  {
    internal static string WriteToString(Configuration cfg)
    {
      using (var writer = new StringWriter())
      {
        WriteToTextWriter(cfg, writer);
        return writer.ToString();
      }
    }

    internal static void WriteToTextWriter(Configuration cfg, TextWriter writer)
    {
      if (cfg == null)
      {
        throw new ArgumentNullException(nameof(cfg));
      }

      if (writer == null)
      {
        throw new ArgumentNullException(nameof(writer));
      }

      bool isFirstSection = true;

      void WriteSection(Section section)
      {
        if (!isFirstSection)
        {
          writer.WriteLine();
        }

        if (!isFirstSection && section.PreComment != null)
        {
          writer.WriteLine();
        }

        if (section.Name != Section.DefaultSectionName)
        {
          writer.WriteLine(section.ToString());
        }

        foreach (var setting in section)
        {
          writer.WriteLine(setting.ToString());
        }

        if (section.Name != Section.DefaultSectionName || section.SettingCount > 0)
        {
          isFirstSection = false;
        }
      }

      var defaultSection = cfg.DefaultSection;

      if (defaultSection.SettingCount > 0)
      {
        WriteSection(defaultSection);
      }

      foreach (var section in cfg.Where(section => section != defaultSection))
      {
        WriteSection(section);
      }
    }

    internal static void WriteToStreamTextual(Configuration cfg, Stream stream, Encoding? encoding)
    {
      if (cfg == null)
      {
        throw new ArgumentNullException(nameof(cfg));
      }

      if (stream == null)
      {
        throw new ArgumentNullException(nameof(stream));
      }

      encoding ??= Encoding.UTF8;
      using (var writer = new StreamWriter(stream, encoding, bufferSize: 1024, leaveOpen: true))
      {
        WriteToTextWriter(cfg, writer);
        writer.Flush();
      }
    }

    internal static void WriteToStreamBinary(Configuration cfg, Stream stream, BinaryWriter? writer)
    {
      if (cfg == null)
      {
        throw new ArgumentNullException(nameof(cfg));
      }

      if (stream == null)
      {
        throw new ArgumentNullException(nameof(stream));
      }

      if (writer == null)
      {
        writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
      }

      writer.Write(cfg.SectionCount);

      foreach (var section in cfg)
      {
        writer.Write(section.Name);
        writer.Write(section.SettingCount);

        WriteCommentsBinary(writer, section);

        // Write the section's settings.
        foreach (var setting in section)
        {
          writer.Write(setting.Name);
          writer.Write(setting.RawValue);

          WriteCommentsBinary(writer, setting);
        }
      }

      writer.Flush();
    }

    private static void WriteCommentsBinary(BinaryWriter writer, ConfigurationElement element)
    {
      writer.Write(element.Comment != null);
      if (element.Comment != null)
      {
        // SharpConfig <3.0 wrote the comment char here.
        // We'll just write a single char for backwards-compatibility.
        writer.Write(' ');
        writer.Write(element.Comment);
      }

      writer.Write(element.PreComment != null);
      if (element.PreComment != null)
      {
        // Same as with inline comments above.
        writer.Write(' ');
        writer.Write(element.PreComment);
      }
    }
  }
}