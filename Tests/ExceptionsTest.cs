// Copyright (c) 2013-2025 Cem Dervis, MIT License.
// https://sharpconfig.org

using System;
using NUnit.Framework;

namespace SharpConfig
{
    [TestFixture]
    public sealed class ExceptionsTest
    {
        [Test]
        public void ParserException()
        {
            var ex = new ParserException("Some Message", 123);
            Assert.AreEqual("Line 123: Some Message", ex.Message);
            Assert.AreEqual(123, ex.Line);
        }
    }
}