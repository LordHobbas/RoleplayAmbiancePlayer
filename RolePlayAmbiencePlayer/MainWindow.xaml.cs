using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RolePlayAmbiencePlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Dragging variables
        private string _draggedIdentifier = null;
        private object _draggedObject = null;
        private bool _preDraggingFromTreeView = false;
        private Point _draggingStartPoint = new Point();
        private UIElement _draggedUIElement = null;
        private Window _dragdropWindow;

        public MainWindow()
        {
            InitializeComponent();
            WindowHandler.SetMainWindow(this);
        }

        public TreeViewItem NewTreeViewPlaylist(Playlist playlist)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = $"Ξ {playlist.Name}",
                Tag = playlist,
                AllowDrop = true
            };
            item.PreviewMouseDown += TreeViewItem_Playlist_PreviewMouseDown;
            item.GiveFeedback += TreeViewItem_GiveFeedback;
            item.PreviewDrop += TreeViewItem_Playlist_PreviewDrop;
            item.DragOver += TreeViewItem_Collection_DragOver;
            item.ContextMenu = ContextMenu_TreeViewItem_Playlist;
            return item;
        }

        public TreeViewItem NewTreeViewClip(Clip clip)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = $"♫ {clip.Name}",
                Tag = clip
            };
            item.GiveFeedback += TreeViewItem_GiveFeedback;
            item.PreviewMouseDown += TreeViewItem_Clip_PreviewMouseDown;
            item.MouseDoubleClick += TreeViewItem_Clip_MouseDoubleClick;
            item.ContextMenu = ContextMenu_TreeViewItem_Clip;
            return item;
        }

        public TreeViewItem NewTreeViewFolder(MediaFolder folder)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = $"🗀 {folder.Name}",
                Tag = folder
            };

            item.ContextMenu = ContextMenu_TreeViewItem_Folder;
            item.PreviewDrop += TreeViewItem_Folder_PreviewDrop;
            item.DragOver += TreeViewItem_Collection_DragOver;

            //Loop through all the folders under this folder.
            //Add all the folders as children, and check if they have children, and repeat
            //When done with the subfolders, add playlists
            //Add clips

            return item;
        }

        public void Populate_TreeView()
        {
            if (!ConfigHandler.TracklistsLoaded)
                return;

            TreeView_Clips.Items.Clear();
            TreeViewItem treeViewPlaylistsHeader = new TreeViewItem()
            {
                Header = "--- Playlists ---",
                FontSize = 14,
                Foreground = Brushes.DarkGreen
            };
            treeViewPlaylistsHeader.Selected += TreeViewItem_Header_Selected;
            TreeView_Clips.Items.Add(treeViewPlaylistsHeader);

            foreach (Playlist playlist in MediaHandler.Playlists)
            {
                TreeViewItem playlist_item = NewTreeViewPlaylist(playlist);

                foreach (Clip clip in playlist.Clips)
                    playlist_item.Items.Add(NewTreeViewClip(clip));

                TreeView_Clips.Items.Add(playlist_item);
            }
            //TreeView_Clips.Items.Add(new Separator());
            TreeViewItem treeViewFoldersHeader = new TreeViewItem()
            {
                Header = "--- Folders ---",
                FontSize = 14,
                Foreground = Brushes.DarkGreen,
                Margin = new Thickness(0, 5, 0, 0)
            };
            treeViewFoldersHeader.Selected += TreeViewItem_Header_Selected;
            TreeView_Clips.Items.Add(treeViewFoldersHeader);

            foreach (MediaFolder folder in MediaHandler.Folders)
            {
                TreeViewItem folder_item = NewTreeViewFolder(folder);

                foreach (Clip clip in folder.Clips)
                    folder_item.Items.Add(NewTreeViewClip(clip));

                TreeView_Clips.Items.Add(folder_item);
            }

            TextBlock_SavedChanges.Visibility = Visibility.Collapsed;
        }

        private void AddTrack(string name)
        {
            TrackPlayer tp = new TrackPlayer();
            tp.Margin = new Thickness(0, 0, 0, 5);

            StackPanel_TrackPlayers.Children.Add(tp);
        }

        public void RemoveTrack(TrackPlayer tp)
        {
            StackPanel_TrackPlayers.Children.Remove(tp);
        }

        private void StartDraggingFromTreeView()
        {
            _preDraggingFromTreeView = false;
            _draggingStartPoint = new Point();
            
            //Stuff

            if(_draggedUIElement == null)
            {
                Console.WriteLine("ERROR!!!");
                return;
            }
            CreateDragDropWindow();

            var _data = new DataObject(_draggedIdentifier, _draggedObject);
            DragDrop.DoDragDrop(_draggedUIElement, _data, DragDropEffects.Move);

            if (this._dragdropWindow != null)
            {
                this._dragdropWindow.Close();
                this._dragdropWindow = null;
            }
        }

        private void CreateDragDropWindow()
        {
            if(_draggedIdentifier == "playlist" || _draggedIdentifier == "clip")
            {
                TextBlock tb = new TextBlock
                {
                    Text = (_draggedUIElement as TreeViewItem).Header.ToString(),
                    Foreground = Brushes.LimeGreen,
                    Background = Brushes.Black,
                    FontFamily = new FontFamily("roboto"),
                    Opacity = 0.5,
                    IsHitTestVisible = false,
                    AllowDrop = false
                };

                Point mousePos = GetAbsoluteMousePos();

                _dragdropWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    AllowDrop = false,
                    Background = null,
                    IsHitTestVisible = false,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Topmost = true,
                    ShowInTaskbar = false,
                    Content = tb,
                    Left = mousePos.X,
                    Top = mousePos.Y,
                };

                _dragdropWindow.Show();
            }
        }

        Point GetAbsoluteMousePos(double offsetX = 0, double offsetY = 0)
        {
            Win32Point w32p = new Win32Point();
            GetCursorPos(ref w32p);
            return new Point(w32p.X + offsetX, w32p.Y + offsetY);
        }


        //Don't know exactly what this does, best to not mess with. Stolen from https://stackoverflow.com/questions/3129443/wpf-4-drag-and-drop-with-visual-element-as-cursor
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        private void stopPreDrag()
        {
            _draggingStartPoint = new Point();
            _draggedUIElement = null;
            _preDraggingFromTreeView = false;
            _draggedIdentifier = null;
            _draggedObject = null;
        }

        private void treeViewItemDrop(object sender, DragEventArgs e)
        {
            TreeViewItem tvi = sender as TreeViewItem;

            //if (!e.Data.GetFormats().Contains(DataFormats.FileDrop))
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            string fileName = System.IO.Path.GetFileName(filePath);
            if (fileName.Contains("."))
                fileName = fileName.Remove(fileName.LastIndexOf("."));

            Clip c = new Clip()
            {
                Name = fileName,
                Path = filePath
            };

            WindowHandler.OpenNewClipWindow(c, tvi, GetAbsoluteMousePos(-52, -50));
        }

        public void ShowUnsavedChanges()
        {
            TextBlock_SavedChanges.Visibility = Visibility.Visible;
        }

        // ######################################### Events ###############################################

        private void MenuItem_AddTrack_Click(object sender, RoutedEventArgs e)
        {
            AddTrack("Untitled Track");
        }

        private void MenuItem_New_Folder_Click(object sender, RoutedEventArgs e)
        {
            WindowHandler.OpenNewFolderWindow(GetAbsoluteMousePos(0, 20));
        }

        private void MenuItem_New_Playlist_Click(object sender, RoutedEventArgs e)
        {
            WindowHandler.OpenNewPlaylistWindow(GetAbsoluteMousePos(0, 20));
        }

        private void MenuItem_PauseAll_Click(object sender, RoutedEventArgs e)
        {
            MediaHandler.PauseAll();
        }

        private void MenuItem_PlayAll_Click(object sender, RoutedEventArgs e)
        {
            MediaHandler.PlayAll();
        }

        private void MenuItem_Playlist_Edit_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TreeViewItem;
            Playlist p = item.Tag as Playlist;

            WindowHandler.OpenEditPlaylistWindow(p, item, GetAbsoluteMousePos(-125, -50));
        }

        private void MenuItem_Playlist_Delete_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem playlist_item = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TreeViewItem;

            MediaHandler.Playlists.Remove(playlist_item.Tag as Playlist);
            TreeView_Clips.Items.Remove(playlist_item);
        }

        private void MenuItem_Clip_Delete_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem clip_item = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TreeViewItem;
            TreeViewItem parent_item = clip_item.Parent as TreeViewItem;

            Console.WriteLine(parent_item.Tag.GetType());

            if (parent_item.Tag is Playlist p)
                p.Clips.Remove((Clip)clip_item.Tag);
            else if (parent_item.Tag is MediaFolder f)
                f.Clips.Remove((Clip)clip_item.Tag);

            parent_item.Items.Remove(clip_item);
        }

        private void MenuItem_Clip_Edit_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TreeViewItem;
            Clip c = item.Tag as Clip;

            WindowHandler.OpenEditClipWindow(c, item, GetAbsoluteMousePos(-125, -50));
        }

        private void MenuItem_Clip_Add_To_Queue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Folder_Edit_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TreeViewItem;
            MediaFolder f = item.Tag as MediaFolder;

            WindowHandler.OpenEditFolderWindow(f, GetAbsoluteMousePos(-125, 50));
        }

        private void MenuItem_Folder_Delete_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = ((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TreeViewItem;
            MediaFolder f = item.Tag as MediaFolder;

            MediaHandler.Folders.Remove(f);
            TreeView_Clips.Items.Remove(item);
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            ConfigHandler.SaveClipCollection();
            Console.WriteLine("Pressed Save Clip");
            TextBlock_SavedChanges.Visibility = Visibility.Collapsed;
        }

        private void Slider_Volume_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            MediaHandler.MainVolume = Slider_Volume.Value;
            MediaHandler.UpdateMainVolume();
        }

        private void Slider_Volume_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            MediaHandler.VolumeDragStart = Slider_Volume.Value;
        }


        private void TreeView_Clips_Drop(object sender, DragEventArgs e)
        {
            Console.WriteLine("Dropped TreeView");
        }

        private void TreeView_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TreeView_Clips.SelectedItem is TreeViewItem ctvi)
                ctvi.IsSelected = false;
        }

        private void TreeViewItem_Clip_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TreeViewItem item && item == sender && item.Tag is Clip c)
            {
                Point mousePos = GetAbsoluteMousePos(-52, -50);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    WindowHandler.OpenEditClipWindow(c, item, mousePos);
                }));

            }
        }

        private void TreeViewItem_Clip_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TreeViewItem item && item == sender)
            {
                _draggedUIElement = item;
                _preDraggingFromTreeView = true;
                _draggedObject = item.Tag;
                _draggingStartPoint = GetAbsoluteMousePos();
                _draggedIdentifier = "clip";
            }
            else
            {

            }
        }

        private void TreeViewItem_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            Point mousePos = GetAbsoluteMousePos();

            this._dragdropWindow.Left = mousePos.X - _dragdropWindow.ActualWidth / 2;
            this._dragdropWindow.Top = mousePos.Y - _dragdropWindow.ActualHeight / 2 + 17;


            Mouse.SetCursor(Cursors.Hand);
            e.Handled = true;
            //base.OnGiveFeedback(e);
        }

        private void TreeViewItem_Header_Selected(object sender, RoutedEventArgs e)
        {
            if (TreeView_Clips.SelectedItem is TreeViewItem tvi)
                tvi.IsSelected = false;
        }

        private void TreeViewItem_Playlist_PreviewDrop(object sender, DragEventArgs e)
        {
            treeViewItemDrop(sender, e);
        }
        private void TreeViewItem_Folder_PreviewDrop(object sender, DragEventArgs e)
        {
            treeViewItemDrop(sender, e);
        }

        private void TreeViewItem_Playlist_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TreeViewItem item && item == sender)
            {
                _draggedUIElement = item;
                _preDraggingFromTreeView = true;
                _draggedObject = item.Tag;
                _draggingStartPoint = GetAbsoluteMousePos();
                _draggedIdentifier = "playlist";
            }
        }

        private void TreeViewItem_Playlist_ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (sender as ContextMenu).PlacementTarget as TreeViewItem;
            tvi.IsSelected = true;
        }

        private void TreeViewItem_Clip_ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (sender as ContextMenu).PlacementTarget as TreeViewItem;
            tvi.IsSelected = true;
        }

        private void TreeViewItem_Clip_ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (sender as ContextMenu).PlacementTarget as TreeViewItem;
            tvi.IsSelected = false;
        }

        private void TreeViewItem_Playlist_ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = (sender as ContextMenu).PlacementTarget as TreeViewItem;
            tvi.IsSelected = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowHandler.CloseSubWindows();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigHandler.LoadClipCollection();
            Populate_TreeView();

            MediaHandler.SetUpBackgroundTask();
            ConfigHandler.SaveAsJSON();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Released && _preDraggingFromTreeView)
                stopPreDrag();

            if (_preDraggingFromTreeView)
            {
                Point currentPos = GetAbsoluteMousePos();

                if (Math.Abs(currentPos.X - _draggingStartPoint.X) > 10 || Math.Abs(currentPos.Y - _draggingStartPoint.Y) > 10)
                    StartDraggingFromTreeView();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_preDraggingFromTreeView)
                stopPreDrag();
        }

        private void TreeViewItem_Collection_DragOver(object sender, DragEventArgs e)
        {
            (sender as TreeViewItem).IsSelected = true;
        }

        private void StackPanel_TrackPlayers_DragOver(object sender, DragEventArgs e)
        {
            if (TreeView_Clips.SelectedItem != null)
                (TreeView_Clips.SelectedItem as TreeViewItem).IsSelected = false;
        }
    }
}
