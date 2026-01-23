// Copyright (c) 2013-2026 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;
using NUnit.Framework;
using SharpConfig;

namespace Tests
{
    [TestFixture]
    public sealed class AdditionalTests
    {
        [Test]
        public void SectionFromObject()
        {
            var obj = new TestObject
            {
                IntField = 10,
                StringProp = "Hello",
                IgnoredField = 20,
                IgnoredProp = "World"
            };

            var section = Section.FromObject("TestSection", obj);

            Assert.AreEqual(2, section.SettingCount);
            Assert.AreEqual(10, section["IntField"].IntValue);
            Assert.AreEqual("Hello", section["StringProp"].StringValue);
            Assert.IsFalse(section.Contains("IgnoredField"));
            Assert.IsFalse(section.Contains("IgnoredProp"));
        }

        [Test]
        public void SectionToObjectNonGeneric()
        {
            var section = new Section("TestSection");
            section["IntField"].IntValue = 10;
            section["StringProp"].StringValue = "Hello";

            var obj = (TestObject)section.ToObject(typeof(TestObject));

            Assert.AreEqual(10, obj.IntField);
            Assert.AreEqual("Hello", obj.StringProp);
        }

        [Test]
        public void NullableSupport()
        {
            var section = new Section("TestSection");
            section["IntVal"].RawValue = "10";
            section["NullIntVal"].RawValue = "";

            Assert.AreEqual(10, section["IntVal"].GetValue<int?>());
            Assert.IsNull(section["NullIntVal"].GetValue<int?>());

            section["IntVal"].SetValue((int?)20);
            Assert.AreEqual("20", section["IntVal"].RawValue);

            section["IntVal"].SetValue((int?)null);
            Assert.AreEqual("", section["IntVal"].RawValue);
        }

        [Test]
        public void CharValueArray()
        {
            var section = new Section("TestSection");
            var chars = new[] { 'a', 'b', 'c' };
            section["Chars"].CharValueArray = chars;

            Assert.AreEqual(chars, section["Chars"].CharValueArray);
        }

        [Test]
        public void SettingIsEmpty()
        {
            var setting = new Setting("Test");
            Assert.IsTrue(setting.IsEmpty);

            setting.RawValue = "Value";
            Assert.IsFalse(setting.IsEmpty);
        }

        [Test]
        public void ArrayElementSeparatorChange()
        {
            var setting = new Setting("Test", "{1;2;3}");
            
            var oldSeparator = Configuration.ArrayElementSeparator;
            Configuration.ArrayElementSeparator = ';';
            try
            {
                Assert.IsTrue(setting.IsArray);
                Assert.AreEqual(3, setting.ArraySize);
                Assert.AreEqual(new[] { 1, 2, 3 }, setting.IntValueArray);
            }
            finally
            {
                Configuration.ArrayElementSeparator = oldSeparator;
            }
        }

        [Test]
        public void JaggedArraysNotSupported()
        {
            var setting = new Setting("Test");
            var jagged = new int[][] { new int[] { 1 } };
            
            Assert.Throws<ArgumentException>(() => setting.SetValue(jagged));
            
            setting.RawValue = "{{1}}";
            Assert.Throws<ArgumentException>(() => setting.GetValueArray(typeof(int[])));
            Assert.Throws<ArgumentException>(() => setting.GetValueArray<int[]>());
        }

        [Test]
        public void GetSettingsNamed()
        {
            var section = new Section("Test");
            section.Add(new Setting("Setting1"));
            section.Add(new Setting("setting1"));
            section.Add(new Setting("Setting2"));

            var settings = section.GetSettingsNamed("Setting1");
            int count = 0;
            foreach (var s in settings) count++;
            
            Assert.AreEqual(2, count);
        }

        [Test]
        public void GetSectionsNamed()
        {
            var cfg = new Configuration();
            cfg.Add(new Section("Section1"));
            cfg.Add(new Section("section1"));
            cfg.Add(new Section("Section2"));

            var sections = cfg.GetSectionsNamed("Section1");
            int count = 0;
            foreach (var s in sections) count++;

            Assert.AreEqual(2, count);
        }

        [Test]
        public void OutputRawStringValues()
        {
            var oldRaw = Configuration.OutputRawStringValues;
            Configuration.OutputRawStringValues = true;
            try
            {
                var setting = new Setting("Test", "\"RawValue\"");
                Assert.AreEqual("\"RawValue\"", setting.StringValue);
                
                setting.StringValue = "NewValue";
                Assert.AreEqual("NewValue", setting.RawValue);
            }
            finally
            {
                Configuration.OutputRawStringValues = oldRaw;
            }
        }

        [Test]
        public void SpaceBetweenEquals()
        {
            var oldSpace = Configuration.SpaceBetweenEquals;
            Configuration.SpaceBetweenEquals = true;
            try
            {
                var setting = new Setting("Name", "Value");
                Assert.AreEqual("Name = Value", setting.ToString());
                
                Configuration.SpaceBetweenEquals = false;
                Assert.AreEqual("Name=Value", setting.ToString());
            }
            finally
            {
                Configuration.SpaceBetweenEquals = oldSpace;
            }
        }

        [Test]
        public void ResetOptions()
        {
            Configuration.PreferredCommentChar = ';';
            Configuration.ArrayElementSeparator = '|';
            Configuration.OutputRawStringValues = true;
            Configuration.IgnoreInlineComments = true;
            Configuration.IgnorePreComments = true;
            Configuration.SpaceBetweenEquals = true;

            Configuration.ResetOptions();

            Assert.AreEqual('#', Configuration.PreferredCommentChar);
            Assert.AreEqual(',', Configuration.ArrayElementSeparator);
            Assert.IsFalse(Configuration.OutputRawStringValues);
            Assert.IsFalse(Configuration.IgnoreInlineComments);
            Assert.IsFalse(Configuration.IgnorePreComments);
            Assert.IsFalse(Configuration.SpaceBetweenEquals);
        }

        private class TestObject
        {
            public int IntField;
            public string StringProp { get; set; }

            [SharpConfig.Ignore]
            public int IgnoredField;

            [SharpConfig.Ignore]
            public string IgnoredProp { get; set; }
        }
    }
}
