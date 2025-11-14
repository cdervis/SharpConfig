// Copyright (c) 2013-2025 Cem Dervis, MIT License.
// https://sharpconfig.org

using NUnit.Framework;
using SharpConfig;

namespace Tests
{
    internal sealed class DummyClass
    {
        public int Value;
    }

    internal sealed class DummyTypeStringConverter : ITypeStringConverter
    {
        public Type ConvertibleType => typeof(DummyClass);

        public string ConvertToString(object value)
        {
            return (value as DummyClass)!.Value.ToString();
        }

        public object? TryConvertFromString(string value, Type hint)
        {
            return int.TryParse(value, out int result) ? new DummyClass { Value = result } : null;
        }
    }

    [TestFixture]
    public sealed class InvalidInputTest
    {
        [Test]
        public void ConfigLoading()
        {
            var cfg = new Configuration();

            Assert.Throws<ArgumentNullException>(() => Configuration.LoadFromFile(null));
            Assert.Throws<FileNotFoundException>(() => Configuration.LoadFromFile("doesnotexist.ini"));
            Assert.Throws<ArgumentNullException>(() => Configuration.LoadFromStream(null));
            Assert.Throws<ArgumentNullException>(() => Configuration.LoadFromString(null));
            Assert.Throws<ArgumentNullException>(() => Configuration.LoadFromBinaryFile(null));
            Assert.Throws<ArgumentNullException>(() => Configuration.LoadFromBinaryStream(null));
            Assert.Throws<ArgumentNullException>(() => cfg.SaveToFile(null));
            Assert.Throws<ArgumentNullException>(() => cfg.SaveToStream(null));
            Assert.Throws<ArgumentNullException>(() => cfg.SaveToBinaryFile(null));
            Assert.Throws<ArgumentNullException>(() => cfg.SaveToBinaryStream(null));
        }

        [Test]
        public void EmptyObjects()
        {
            Assert.Throws<ArgumentNullException>(() => new Setting(null));
            Assert.Throws<ArgumentNullException>(() => new Section(null));
        }

        [Test]
        public void Options()
        {
            Assert.Throws<ArgumentNullException>(() => Configuration.CultureInfo = null);
            Assert.Throws<ArgumentException>(() => Configuration.PreferredCommentChar = 'a');
            Assert.Throws<ArgumentException>(() => Configuration.ArrayElementSeparator = '\0');
        }

        [Test]
        public void ConfigSectionOperations()
        {
            var cfg = new Configuration();
            Assert.Throws<ArgumentNullException>(() => cfg.Add((Section)null));

            var section = new Section("SomeSection");
            Assert.DoesNotThrow(() => cfg.Add(section));

            Assert.Throws<ArgumentException>(() => cfg.Add(section));

            Assert.Throws<ArgumentNullException>(() => cfg.Remove((string)null));
            Assert.DoesNotThrow(() => cfg.Remove("SomeSection"));

            Assert.Throws<ArgumentNullException>(() => cfg.RemoveAllNamed((string)null));
            Assert.Throws<ArgumentNullException>(() => cfg.Contains((string)null));
            Assert.Throws<ArgumentNullException>(() => cfg.Contains("A", null));
            Assert.Throws<ArgumentNullException>(() => cfg.Contains(null, "B"));
            Assert.DoesNotThrow(() => cfg.Contains("A", "B"));

            Assert.DoesNotThrow(() => cfg.Add("SomeSection"));

            Assert.DoesNotThrow(() => cfg[0].ToString());
            Assert.Throws<ArgumentOutOfRangeException>(() => cfg[-1].ToString());
        }

        [Test]
        public void Sections()
        {
            var section = new Section("SomeSection");
            section.Add("S");

            Assert.Throws<ArgumentException>(() => Section.FromObject(null, 1));
            Assert.Throws<ArgumentNullException>(() => Section.FromObject("A", null));
            Assert.Throws<ArgumentNullException>(() => section.ToObject(null));
            Assert.Throws<ArgumentNullException>(() => section.GetValuesFrom(null));
            Assert.Throws<ArgumentNullException>(() => section.SetValuesTo(null));
            Assert.Throws<ArgumentNullException>(() => section.Remove((string)null));
            Assert.Throws<ArgumentNullException>(() => section.Remove((Setting)null));
            Assert.Throws<ArgumentNullException>(() => section.Remove(""));
            Assert.Throws<ArgumentNullException>(() => section.RemoveAllNamed(null));
            Assert.Throws<ArgumentNullException>(() => section.Contains((string)null));
            Assert.DoesNotThrow(() => section[0].ToString());
            Assert.Throws<ArgumentOutOfRangeException>(() => section[-1].ToString());
            Assert.Throws<ArgumentOutOfRangeException>(() => section[1].ToString());
        }

        [Test]
        public void TypeStringConverterRegistration()
        {
            Assert.Throws<ArgumentNullException>(() => Configuration.RegisterTypeStringConverter(null));

            var converter = new DummyTypeStringConverter();
            Assert.DoesNotThrow(() => Configuration.RegisterTypeStringConverter(converter));

            Assert.Throws<InvalidOperationException>(() => Configuration.RegisterTypeStringConverter(converter));

            Assert.Throws<ArgumentNullException>(() => Configuration.DeregisterTypeStringConverter(null));
            Assert.DoesNotThrow(() => Configuration.DeregisterTypeStringConverter(typeof(DummyClass)));
            Assert.Throws<InvalidOperationException>(() => Configuration.DeregisterTypeStringConverter(typeof(DummyClass)));

            Assert.Throws<ArgumentNullException>(() => Configuration.FindTypeStringConverter(null));
        }
    }
}