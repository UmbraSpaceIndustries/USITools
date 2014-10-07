using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace USITools.Tests.Unit
{
    [TestClass]
    public class when_reading_a_byte_stream
    {
        [TestMethod]
        public void Should_be_able_to_compress_a_byte_stream()
        {
            var stream = GetSampleByteArray();

        }

        private byte[] GetSampleByteArray()
        {
            var b = new byte[4096];
            var r = new Random();
            for (int i = 0; i < 4096; i++)
            {
                var element = (byte)r.Next(0, 255);
                b[i] = element;
            }
            return b;
        }
    }
}
    