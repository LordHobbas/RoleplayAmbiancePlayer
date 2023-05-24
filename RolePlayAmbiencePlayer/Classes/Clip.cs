using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XElement = System.Xml.Linq.XElement;
using XAttribute = System.Xml.Linq.XAttribute;
using System.Text.Json.Serialization;

namespace RolePlayAmbiencePlayer
{
    public class Clip
    {
        public string Name;
        public string Path;

        public Clip()
        {

        }

        public override string ToString()
        {
            return Name;
        }

        public XElement ToXElement()
        {
            return new XElement("Clip",
                new XAttribute("name", Name),
                new XAttribute("path", Path)
                );
        }
    }
}
