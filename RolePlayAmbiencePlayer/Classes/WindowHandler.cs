using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading.Tasks;

using TreeViewItem = System.Windows.Controls.TreeViewItem;
using Point = System.Windows.Point;
using Mouse = System.Windows.Input.Mouse;

namespace RolePlayAmbiencePlayer
{
    public static class WindowHandler
    {
        public static MainWindow Main;
        public static EditClipWindow EditClip;
        public static EditTrackNameWindow EditTrackName;
        public static EditFolderWindow EditFolder;
        public static EditPlaylistWindow EditPlaylist;

        private static Dispatcher Dispatcher;

        public static void SetMainWindow(MainWindow mw)
        {
            Main = mw;
            Dispatcher = mw.Dispatcher;
        }

        public static void CloseSubWindows()
        {
            EditClip?.Close();
            EditTrackName?.Close();
            EditFolder?.Close();
            EditPlaylist?.Close();
        }

        public static void OpenEditClipWindow(Clip clip, TreeViewItem tvi, Point point)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CloseSubWindows();

                EditClip = new EditClipWindow(clip, tvi)
                {
                    Left = point.X,
                    Top = point.Y,
                    Owner = Main,
                    Title = $"Editing {clip.Name}"
                };

                if (EditClip.ShowDialog() == true)
                {
                    Main.ShowUnsavedChanges();
                }
            }));
        }

        public static void OpenNewClipWindow(Clip clip, TreeViewItem tvi, Point point)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                CloseSubWindows();

                EditClip = new EditClipWindow(clip, tvi, true)
                {
                    Left = point.X,
                    Top = point.Y,
                    Owner = Main,
                    Title = "New Clip"
                };

                if(EditClip.ShowDialog() == true)
                {
                    Main.ShowUnsavedChanges();
                }
            }));
            
        }

        public static void OpenEditPlaylistWindow(Playlist playlist, TreeViewItem tvi, Point position)
        {
            CloseSubWindows();

            EditPlaylist = new EditPlaylistWindow(playlist)
            {
                Left = position.X,
                Top = position.Y,
                Owner = Main,
                Title = $"Editing {playlist.Name}"
            };

            if (EditPlaylist.ShowDialog() == true)
            {
                MediaHandler.SortPlaylists();
                Main.Populate_TreeView();
                Main.ShowUnsavedChanges();
            }
        }

        public static void OpenNewPlaylistWindow(Point position)
        {
            Playlist playlist = new Playlist();

            CloseSubWindows();

            EditPlaylist = new EditPlaylistWindow(playlist)
            {
                Left = position.X,
                Top = position.Y,
                Owner = Main,
                Title = "Enter name of new playlist"
            };

            if(EditPlaylist.ShowDialog() == true)
            {
                MediaHandler.Playlists.Add(playlist);
                MediaHandler.SortPlaylists();
                Main.Populate_TreeView();
                Main.ShowUnsavedChanges();
            }
        }

        public static void OpenEditTrackNameWindow(TrackPlayer trackPlayer, Point point)
        {
            CloseSubWindows();

            EditTrackName?.Close();
            EditTrackName = new EditTrackNameWindow(trackPlayer)
            {
                Left = point.X,
                Top = point.Y,
                Owner = Main
            };

            EditTrackName.Show();
        }

        public static void OpenNewFolderWindow(Point positon)
        {
            MediaFolder folder = new MediaFolder();

            CloseSubWindows();

            EditFolder = new EditFolderWindow(folder)
            {
                Left = positon.X,
                Top = positon.Y,
                Owner = Main,
                Title = "New Folder"
            };

            if(EditFolder.ShowDialog() == true)
            {
                MediaHandler.Folders.Add(folder);
                MediaHandler.SortPlaylists();
                Main.Populate_TreeView();
                Main.ShowUnsavedChanges();
            }
        }

        public static void OpenEditFolderWindow(MediaFolder folder, Point positon)
        {
            CloseSubWindows();

            EditFolder = new EditFolderWindow(folder)
            {
                Left = positon.X,
                Top = positon.Y,
                Owner = Main,
                Title = $"Editing {folder.Name}"
            };

            if (EditFolder.ShowDialog() == true)
            {
                MediaHandler.SortPlaylists();
                Main.Populate_TreeView();
                Main.ShowUnsavedChanges();
            }
        }

        public static void ActivateSubWindow()
        {
            EditClip?.Activate();
            EditTrackName?.Activate();
            EditFolder?.Activate();
            EditPlaylist?.Activate();
        }
    }
}
