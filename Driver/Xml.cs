using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Driver
{
    public static class Xml
    {
        public static string Serialize<T>(T obj)
        {
            var xml = new XmlSerializer(obj.GetType());

            using var writer = new StringWriter();

            xml.Serialize(writer, obj);

            return writer.ToString();
        }
    }
}
