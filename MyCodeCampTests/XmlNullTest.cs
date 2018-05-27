using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace MyCodeCampTests
{
    public class XmlTest
    {
        public decimal? NullDecimal { get; set; }
    }

    public class XmlNullTest
    {
        private readonly ITestOutputHelper _o;

        public XmlNullTest(ITestOutputHelper o)
        {
            _o = o;
        }

        [Fact]
        public void MakeXml()
        {
            var nd = new XmlTest()
            {
                NullDecimal = 1.2m
            };
            var str = Class1.ToXml(nd);
            _o.WriteLine(str);
        }

        [Fact]
        public void GetObject()
        {
            var o = FromXml<XmlTest>(XmlString);
           _o.WriteLine($"{o.NullDecimal}");
        }


        public static T FromXml<T>(string s) where T : class
        {
            using (var reader = new StringReader(s))
            {
                return 
                    new XmlSerializer(typeof(T))
                        .Deserialize(reader) as T;
            }
        }

        private const string XmlString = @"<?xml version=""1.0""?>
<XmlTest>
</XmlTest>";
    }
}