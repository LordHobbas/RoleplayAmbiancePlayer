using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text.Json;

namespace RolePlayAmbiencePlayer
{
    public static class ConfigHandler
    {
        public static bool TracklistsLoaded;
        private class JSonHandler {
            //As of the current version of System.Text.Json, only properties are exported to JSon. Fields are exported if [JsonInclude] is used above the field.
            public List<Playlist> playlists { get; set; }
            public List<MediaFolder> mediaFolders { get; set; }
            public JSonHandler()
            {
                playlists = MediaHandler.Playlists;
                mediaFolders = MediaHandler.Folders;
            }
        }

        public static void Initialize()
        {

        }

        private static void SaveDoc()
        {

        }

        public static void SaveAsJSON()
        {
            foreach(Playlist p in MediaHandler.Playlists)
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    IncludeFields = true
                };
                JSonHandler jsh = new JSonHandler();
                string jsonString = JsonSerializer.Serialize(jsh, options);

                File.WriteAllText(@".\ClipCollection.json", jsonString);
            }
        }

        public static void LoadSettingsXML()
        {

        }

        public static void LoadClipCollection()
        {
            TracklistsLoaded = false;
            if (!File.Exists(@".\ClipCollection.xml"))
            {
                Console.WriteLine("Uh oh, file '.\\ClipCollection.xml' does not exist!");
                return;
            }
            XDocument xDoc = XDocument.Load(@".\ClipCollection.xml");
            MediaHandler.Folders = new List<MediaFolder>();
            MediaHandler.Playlists = new List<Playlist>();

            foreach (XElement xPlaylist in xDoc.Root.Elements("Playlist"))
            {
                List<Clip> clips = new List<Clip>();
                foreach (XElement xClip in xPlaylist.Elements("Clip"))
                {
                    Clip clip = new Clip()
                    {
                        Name = xClip.Attribute("name").Value,
                        Path = xClip.Attribute("path").Value
                    };

                    clips.Add(clip);
                }

                Playlist playlist = new Playlist
                {
                    Name = xPlaylist.Attribute("name").Value,
                    Clips = clips
                };

                MediaHandler.Playlists.Add(playlist);
            }

            foreach (XElement xFolder in xDoc.Root.Elements("Folder"))
            {
                List<Clip> clips = new List<Clip>();
                foreach(XElement xClip in xFolder.Elements("Clip"))
                {
                    Clip clip = new Clip()
                    {
                        Name = xClip.Attribute("name").Value,
                        Path = xClip.Attribute("path").Value
                    };
                    clips.Add(clip);
                }

                MediaFolder folder = new MediaFolder
                {
                    Name = xFolder.Attribute("name").Value,
                    Clips = clips
                };

                MediaHandler.Folders.Add(folder);
            }

            MediaHandler.SortPlaylists();
            TracklistsLoaded = true;
        }

        public static void SaveClipCollection()
        {
            XDocument xdoc = new XDocument();
            XElement root = new XElement("RolePlayAmbience");
            xdoc.Add(root);

            foreach (Playlist p in MediaHandler.Playlists)
                root.Add(p.ToXElement());

            foreach (MediaFolder f in MediaHandler.Folders)
                root.Add(f.ToXElement());

            xdoc.Save(@".\ClipCollection.xml");
        }
        public static void LoadPresetsXML()
        {
            if (!File.Exists(@".\Presets.xml"))
            {
                return;
            }
            XDocument xDoc = XDocument.Load(@".\Presets.xml");


        }
    }
}
