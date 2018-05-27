using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace MyCodeCampTests
{
    internal class CustomConverter : JsonConverter<Column>
    {
        private readonly Type[] _types;

        public CustomConverter()
        {
           // _types = types;
        }

        public override void WriteJson(JsonWriter writer, Column value, JsonSerializer serializer)
        {
            JObject.Parse($"{{{value.Name}:'{value.Type}'}}").WriteTo(writer);
//            writer.WritePropertyName(value.Name);
//            writer.WriteValue(value.Type);

        }



        public override Column ReadJson(JsonReader reader, Type objectType, Column existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var s = (string)reader.Value;

            return new Column();
        }


    }
    [XmlRoot(ElementName = "resultTable")]
    public class ResultTable
    {
        public object[] metadata { get; set; }
            
            
        [XmlArray(ElementName = "columns")]
        [XmlArrayItem(ElementName = "column")]
        [JsonProperty(ItemConverterType = typeof(CustomConverter))]
        public Column[] Columns { get; set; }

        [XmlArrayItem("row")]
        public string[] Rows { get; set; }
    }

    [XmlRoot(ElementName = "columns")]
    public class Column
    {
        private Dictionary<string, string> _columnsDict;

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
            
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

    }

    public class Class1
    {
        private readonly ITestOutputHelper _output;

        public Class1(ITestOutputHelper output)
        {
            _output = output;
        }

        public static string ToXml(object o)
        {
            var xser = new XmlSerializer(o.GetType());
            using(var stream = new MemoryStream())
            {
                xser.Serialize(stream, o);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static ResultTable FromXml(string s)
        {
            var xser = new XmlSerializer(typeof(ResultTable));
            using (var stream = new StringReader(s))
            {
                var o = xser.Deserialize(stream) as ResultTable;
                return o;
            }
        }


        [Fact]
        public void TestXml()
        {
            var o = new ResultTable();
            o.metadata = Array.Empty<string>();
            o.Columns= new Column[]
            {
                new Column(){Name = "n1", Type = "t1"},
                new Column(){Name = "n2", Type = "t2"},
            };

            o.Rows = new string[2] {"sdfsdfsd","fdfdfsfsd"};
            var s = ToXml(o);
            var o1 = FromXml(s);
            //_output.WriteLine(s);

            Assert.Equal(s, ToXml(o1));

            var  j1 = JsonConvert.SerializeObject(o);

           // var jo = JsonConvert.DeserializeObject<ResultTable>(j1);

            _output.WriteLine(j1);
        }

        [Fact]
        public void T1()
        {
            var d = new Dictionary<string, string>()
            {
                {"ff","fsdf"},
                {"ff1","fsdf"},
                {"ff2","fsdf"},
            };

            var s = JsonConvert.SerializeObject(d);
            _output.WriteLine(s);
        }
    }
}