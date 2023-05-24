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
    public class Playlist
    {
        public string Name;
        public List<Clip> Clips;

        public Playlist()
        {
            Clips = new List<Clip>();
        }

        public void SortClips()
        {
            Clips = Clips.OrderBy(c => c.Name).ToList();
        }

        public XElement ToXElement()
        {
            XElement xe = new XElement("Playlist",
                new XAttribute("name", Name));

            foreach (Clip c in Clips)
                xe.Add(c.ToXElement());

            return xe;
        }
    }
}
