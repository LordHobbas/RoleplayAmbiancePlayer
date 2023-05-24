using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeViewItem = System.Windows.Controls.TreeViewItem;
using Point = System.Windows.Point;
using Mouse = System.Windows.Input.Mouse;

namespace RolePlayAmbiencePlayer
{
    public static class MediaHandler
    {
        public static List<TrackPlayer> TrackPlayers = new List<TrackPlayer>();
        public static double VolumeDragStart;
        public static bool UserIsDraggingSlider = false;
        public static double MainVolume = 1;

        public static List<Playlist> Playlists;
        public static List<MediaFolder> Folders;

        private static Random rng = new Random();
        private static int interval = 100; //milliseconds

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void SortPlaylists()
        {
            Playlists = Playlists.OrderBy(p => p.Name).ToList();
            Folders = Folders.OrderBy(p => p.Name).ToList();
        }

        public async static void SetUpBackgroundTask()
        {
            while(true)
            {
                WindowHandler.Main.Dispatcher.Invoke(() =>
                {
                    Tick();
                });

                await Task.Delay(interval);
            }
        }

        public static void UpdateMainVolume()
        {
            foreach(TrackPlayer tp in TrackPlayers)
                tp.UpdateVolume();
        }

        public static void RemoveTrackPlayer(TrackPlayer tp)
        {
            TrackPlayers.Remove(tp);
            WindowHandler.Main.RemoveTrack(tp);
        }

        public static void PlayAll()
        {
            foreach (TrackPlayer tp in TrackPlayers)
                tp.Play();
        }

        public static void PauseAll()
        {
            foreach (TrackPlayer tp in TrackPlayers)
                tp.Pause();
        }

        public static void UpdateClipsVisuals()
        {
            foreach (TrackPlayer tp in TrackPlayers)
                tp.UpdateInformation();
        }

        private static void Tick()
        {
            foreach (TrackPlayer tp in TrackPlayers)
            {
                tp.Tick(interval);
            }
        }
    }
}
