// Copyright (c) 2013-2026 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;
using System.Linq;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using CollectionAssert = NUnit.Framework.Legacy.CollectionAssert;
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

            Assert.AreEqual(2, section.GetSettingsNamed("Setting1").Count());
            Assert.AreEqual(1, section.GetSettingsNamed("Setting1", StringComparison.Ordinal).Count());
        }

        [Test]
        public void GetSectionsNamed()
        {
            var cfg = new Configuration();
            cfg.Add(new Section("Section1"));
            cfg.Add(new Section("section1"));
            cfg.Add(new Section("Section2"));

            Assert.AreEqual(2, cfg.GetSectionsNamed("Section1").Count());
            Assert.AreEqual(1, cfg.GetSectionsNamed("Section1", StringComparison.Ordinal).Count());
        }

        [Test]
        public void CachedObjectMappingPreservesBehavior()
        {
            var source = new TestObject
            {
                IntField = 10,
                StringProp = "Hello"
            };

            for (int i = 0; i < 3; i++)
            {
                var section = Section.FromObject("TestSection", source);
                Assert.AreEqual(10, section["IntField"].IntValue);
                Assert.AreEqual("Hello", section["StringProp"].StringValue);
            }

            var targetSection = new Section("TestSection");
            targetSection["IntField"].IntValue = 10;
            targetSection["StringProp"].StringValue = "Hello";

            for (int i = 0; i < 3; i++)
            {
                var obj = targetSection.ToObject<TestObject>();
                Assert.AreEqual(10, obj.IntField);
                Assert.AreEqual("Hello", obj.StringProp);
            }
        }

        [Test]
        public void ReflectionMappingPreservesBehaviorAcrossRepeatedCalls()
        {
            var source = new RichMappingObject
            {
                Number = 42,
                Name = "Source",
                MultilineProp = "Line1\nLine2",
                MultilineField = "Field1\nField2",
                IntArrayProp = new[] { 1, 2, 3 },
                StringArrayField = new[] { "A", "B" },
                IgnoredProp = "Ignored source property",
                IgnoredByTypeProp = new IgnoredMappedType { Value = "Ignored source type" }
            };

            for (int i = 0; i < 3; i++)
            {
                var section = Section.FromObject("TestSection", source);

                Assert.AreEqual(7, section.SettingCount);
                Assert.AreEqual(42, section["Number"].IntValue);
                Assert.AreEqual("Source", section["Name"].StringValue);
                Assert.AreEqual("[[\nLine1\nLine2\n]]", section["MultilineProp"].RawValue);
                Assert.AreEqual("[[\nField1\nField2\n]]", section["MultilineField"].RawValue);
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, section["IntArrayProp"].IntValueArray);
                CollectionAssert.AreEqual(new[] { "A", "B" }, section["StringArrayField"].StringValueArray);
                Assert.AreEqual(7, section["ReadonlyField"].IntValue);
                Assert.IsFalse(section.Contains("IgnoredProp"));
                Assert.IsFalse(section.Contains("IgnoredByTypeProp"));
            }

            var targetSection = new Section("Target");
            targetSection["Number"].IntValue = 42;
            targetSection["Name"].StringValue = "Target";
            targetSection["MultilineProp"].RawValue = "[[\nLine1\nLine2\n]]";
            targetSection["MultilineField"].RawValue = "[[\nField1\nField2\n]]";
            targetSection["IntArrayProp"].RawValue = "{1,2,3}";
            targetSection["StringArrayField"].RawValue = "{A,B}";
            targetSection["ReadonlyField"].IntValue = 999;
            targetSection["IgnoredProp"].StringValue = "Should stay untouched";
            targetSection["IgnoredByTypeProp"].StringValue = "Should also stay untouched";

            for (int i = 0; i < 3; i++)
            {
                var generic = targetSection.ToObject<RichMappingObject>();
                AssertRichMappingObject(
                    generic,
                    expectedName: "Target",
                    expectedIgnoredProp: "ignored-prop-default",
                    expectedIgnoredTypeValue: "ignored-type-default");

                var nonGeneric = (RichMappingObject)targetSection.ToObject(typeof(RichMappingObject));
                AssertRichMappingObject(
                    nonGeneric,
                    expectedName: "Target",
                    expectedIgnoredProp: "ignored-prop-default",
                    expectedIgnoredTypeValue: "ignored-type-default");

                var existing = new RichMappingObject
                {
                    Number = -1,
                    Name = "Before",
                    MultilineProp = "Before",
                    MultilineField = "Before",
                    IntArrayProp = new[] { 9 },
                    StringArrayField = new[] { "Z" },
                    IgnoredProp = "Keep ignored property",
                    IgnoredByTypeProp = new IgnoredMappedType { Value = "Keep ignored type" }
                };

                targetSection.SetValuesTo(existing);

                AssertRichMappingObject(
                    existing,
                    expectedName: "Target",
                    expectedIgnoredProp: "Keep ignored property",
                    expectedIgnoredTypeValue: "Keep ignored type");
            }

            var updateSection = new Section("Update");
            updateSection.Add("Number");
            updateSection.Add("Name");
            updateSection.Add("MultilineProp");
            updateSection.Add("MultilineField");
            updateSection.Add("IntArrayProp");
            updateSection.Add("StringArrayField");
            updateSection.Add("ReadonlyField");
            updateSection.Add("IgnoredProp").RawValue = "keep-ignored-property";
            updateSection.Add("IgnoredByTypeProp").RawValue = "keep-ignored-type";

            for (int i = 0; i < 3; i++)
            {
                updateSection.GetValuesFrom(source);

                Assert.AreEqual(42, updateSection["Number"].IntValue);
                Assert.AreEqual("Source", updateSection["Name"].StringValue);
                Assert.AreEqual("[[\nLine1\nLine2\n]]", updateSection["MultilineProp"].RawValue);
                Assert.AreEqual("[[\nField1\nField2\n]]", updateSection["MultilineField"].RawValue);
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, updateSection["IntArrayProp"].IntValueArray);
                CollectionAssert.AreEqual(new[] { "A", "B" }, updateSection["StringArrayField"].StringValueArray);
                Assert.AreEqual(7, updateSection["ReadonlyField"].IntValue);
                Assert.AreEqual("keep-ignored-property", updateSection["IgnoredProp"].StringValue);
                Assert.AreEqual("keep-ignored-type", updateSection["IgnoredByTypeProp"].StringValue);
            }
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
            public string StringProp { get; set; } = string.Empty;

            [SharpConfig.Ignore]
            public int IgnoredField;

            [SharpConfig.Ignore]
            public string IgnoredProp { get; set; } = string.Empty;
        }

        [SharpConfig.Ignore]
        private sealed class IgnoredMappedType
        {
            public string Value { get; set; } = string.Empty;
        }

        private sealed class RichMappingObject
        {
            public int Number { get; set; }
            public string Name = string.Empty;

            [Multiline]
            public string MultilineProp { get; set; } = string.Empty;

            [Multiline]
            public string MultilineField = string.Empty;

            public int[] IntArrayProp { get; set; } = Array.Empty<int>();
            public string[] StringArrayField = Array.Empty<string>();

#pragma warning disable CS0649
            public readonly int ReadonlyField = 7;
#pragma warning restore CS0649

            [SharpConfig.Ignore]
            public string IgnoredProp { get; set; } = "ignored-prop-default";

            public IgnoredMappedType IgnoredByTypeProp { get; set; } =
                new IgnoredMappedType { Value = "ignored-type-default" };
        }

        private static void AssertRichMappingObject(
            RichMappingObject obj,
            string expectedName,
            string expectedIgnoredProp,
            string expectedIgnoredTypeValue)
        {
            Assert.AreEqual(42, obj.Number);
            Assert.AreEqual(expectedName, obj.Name);
            Assert.AreEqual("Line1\nLine2", obj.MultilineProp);
            Assert.AreEqual("Field1\nField2", obj.MultilineField);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, obj.IntArrayProp);
            CollectionAssert.AreEqual(new[] { "A", "B" }, obj.StringArrayField);
            Assert.AreEqual(7, obj.ReadonlyField);
            Assert.AreEqual(expectedIgnoredProp, obj.IgnoredProp);
            Assert.AreEqual(expectedIgnoredTypeValue, obj.IgnoredByTypeProp.Value);
        }
    }
}
