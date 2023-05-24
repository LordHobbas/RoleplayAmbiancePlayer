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
    public class MediaFolder
    {
        public List<Clip> Clips;
        public string Name;

        public MediaFolder()
        {
            Name = "Untitled";
            Clips = new List<Clip>();
        }

        public override string ToString()
        {
            return Name;
        }

        public void SortClips()
        {
            Clips = Clips.OrderBy(c => c.Name).ToList();
        }

        public XElement ToXElement()
        {
            XElement xe = new XElement("Folder",
                new XAttribute("name", Name));

            foreach (Clip c in Clips)
                xe.Add(c.ToXElement());

            return xe;
        }
    }
}
