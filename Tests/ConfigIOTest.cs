// Copyright (c) 2013-2026 Cem Dervis, MIT License.
// https://sharpconfig.org

using System.IO;
using System.Text;
using SharpConfig;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using FileAssert = NUnit.Framework.Legacy.FileAssert;

namespace Tests
{
  [TestFixture]
  public sealed class ConfigIoTest
  {
    private static Configuration CreateExampleConfig()
    {
      var cfg = new Configuration();
      cfg["TestSection"]["IntSetting1"].IntValue = 100;
      cfg["TestSection"]["IntSetting2"].IntValue = 200;
      cfg["TestSection"]["StringSetting1"].StringValue = "Test";
      return cfg;
    }

    private static void ValidateExampleConfig(Configuration cfg)
    {
      Assert.AreEqual(cfg["TestSection"]["IntSetting1"].IntValue, 100);
      Assert.AreEqual(cfg["TestSection"]["IntSetting2"].IntValue, 200);
      Assert.AreEqual(cfg["TestSection"]["StringSetting1"].StringValue, "Test");
    }

    [Test]
    public void WriteAndReadConfig_File()
    {
      var cfg = CreateExampleConfig();

      var filename = Path.GetTempFileName();

      cfg.SaveToFile(filename);
      FileAssert.Exists(filename, "Failed to create the test config file.");

      cfg = Configuration.LoadFromFile(filename);
      File.Delete(filename);

      ValidateExampleConfig(cfg);
    }

    [Test]
    public void WriteAndReadConfig_Stream()
    {
      var cfg = CreateExampleConfig();

      var stream = new MemoryStream();
      cfg.SaveToStream(stream);

      stream.Position = 0;

      cfg = Configuration.LoadFromStream(stream);
      stream.Dispose();

      ValidateExampleConfig(cfg);
    }

    [Test]
    public void WriteAndReadConfig_BinaryFile()
    {
      var cfg = CreateExampleConfig();

      var filename = Path.GetTempFileName();

      cfg.SaveToBinaryFile(filename);
      FileAssert.Exists(filename, "Failed to create the test config file.");

      cfg = Configuration.LoadFromBinaryFile(filename);
      File.Delete(filename);

      ValidateExampleConfig(cfg);
    }

    [Test]
    public void WriteAndReadConfig_BinaryStream()
    {
      var cfg = CreateExampleConfig();

      var stream = new MemoryStream();
      cfg.SaveToBinaryStream(stream);

      stream.Position = 0;

      cfg = Configuration.LoadFromBinaryStream(stream);
      stream.Dispose();

      ValidateExampleConfig(cfg);
    }

    [Test]
    public void LoadFromStream_LeavesProvidedStreamOpen()
    {
      var bytes = Encoding.UTF8.GetBytes("[TestSection]\nIntSetting1=100\nIntSetting2=200\nStringSetting1=Test");
      var stream = new MemoryStream(bytes);

      var cfg = Configuration.LoadFromStream(stream);

      ValidateExampleConfig(cfg);
      Assert.DoesNotThrow(() => stream.Position = 0);
      Assert.IsTrue(stream.CanRead);
    }

    [Test]
    public void SaveToStream_LeavesProvidedStreamOpen()
    {
      var cfg = CreateExampleConfig();
      var stream = new MemoryStream();

      cfg.SaveToStream(stream);

      Assert.DoesNotThrow(() => stream.Position = 0);
      Assert.IsTrue(stream.CanRead);
    }

    [Test]
    public void LoadFromBinaryStream_LeavesProvidedStreamOpen()
    {
      var cfg = CreateExampleConfig();
      var stream = new MemoryStream();
      cfg.SaveToBinaryStream(stream);
      stream.Position = 0;

      var loaded = Configuration.LoadFromBinaryStream(stream);

      ValidateExampleConfig(loaded);
      Assert.DoesNotThrow(() => stream.Position = 0);
      Assert.IsTrue(stream.CanRead);
    }

    [Test]
    public void SaveToBinaryStream_DefaultWriterLeavesStreamOpen()
    {
      var cfg = CreateExampleConfig();
      var stream = new MemoryStream();

      cfg.SaveToBinaryStream(stream);

      Assert.DoesNotThrow(() => stream.Position = 0);
      Assert.IsTrue(stream.CanRead);
    }

    [Test]
    public void SaveToBinaryStream_CallerProvidedWriterIsLeftOpen()
    {
      var cfg = CreateExampleConfig();
      var stream = new MemoryStream();
      var writer = new BinaryWriter(stream);

      cfg.SaveToBinaryStream(stream, writer);

      Assert.DoesNotThrow(() => writer.Write(1));
      Assert.DoesNotThrow(() => stream.WriteByte(1));
    }

    [Test]
    public void LoadFromBinaryStream_CallerProvidedReaderIsLeftOpen()
    {
      var cfg = CreateExampleConfig();
      var stream = new MemoryStream();
      cfg.SaveToBinaryStream(stream);
      stream.Position = 0;
      var reader = new BinaryReader(stream);

      var loaded = Configuration.LoadFromBinaryStream(stream, reader);

      ValidateExampleConfig(loaded);
      Assert.DoesNotThrow(() => reader.BaseStream.Position = 0);
      Assert.IsTrue(reader.BaseStream.CanRead);
    }
  }
}
