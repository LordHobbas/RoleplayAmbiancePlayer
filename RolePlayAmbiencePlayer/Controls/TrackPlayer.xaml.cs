using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for TrackPlayer.xaml
    /// </summary>
    public partial class TrackPlayer : UserControl
    {
        public List<Clip> Queue;
        private List<Clip> _originalQueue;

        public int QueueIndex = -1;

        private bool _loop = true;
        private double _volumeDragStart;
        private bool userIsDraggingSlider = false;
        private bool _mediaPlayerPlaying = false;
        private bool _mediaLoaded = false;
        private bool _muted = false;
        private string[] _allowedFileExtensions = { ".wav", ".flac", ".mp3" };
        private ListViewItem current_ListViewItem;
        private int _errorTimer = 0;
        private bool _showError = false;
        private bool _shuffled = true;

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(TrackPlayer), new PropertyMetadata("Untitled Track"));
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }


        public TrackPlayer()
        {
            InitializeComponent();

            MediaHandler.TrackPlayers.Add(this);
        }

        public bool Load(string path)
        {
            current_ListViewItem = ListView_Playlist.Items[QueueIndex] as ListViewItem;
            if (!TestIfPlayable(path))
            {
                //Show Error message?
                current_ListViewItem.Foreground = Brushes.IndianRed;
                Stop();
                _mediaLoaded = false;
                return false;
            }
            Uri fileUri = new Uri(path);
            //MePlayer.Open(fileUri);
            MediaElement_Player.Source = fileUri;
            if (fileUri.IsFile && Name == "")
                TextBlock_CurrentlyPlaying.Text = System.IO.Path.GetFileName(fileUri.LocalPath);
            else if (Name != "")
                TextBlock_CurrentlyPlaying.Text = Name;

            //if success
            _mediaLoaded = true;
            Slider_Progress.IsEnabled = true;
            return true;
        }

        public bool Load(Clip clip)
        {
            return Load(clip.Path);
        }

        public void Remove()
        {
            ToDefault();
            MediaHandler.RemoveTrackPlayer(this);
        }

        public void UpdateVolume()
        {
            MediaElement_Player.Volume = Slider_Volume.Value * MediaHandler.MainVolume;
        }

        public void Play()
        {
            if (Queue.Count == 0)
                return;
            Button_Pause.Visibility = Visibility.Visible;
            Button_Play.Visibility = Visibility.Collapsed;

            UpdateVolume();
            MediaElement_Player.Play();
            _mediaPlayerPlaying = true;
        }

        public void Pause()
        {
            if (Queue.Count == 0)
                return;

            Button_Pause.Visibility = Visibility.Collapsed;
            Button_Play.Visibility = Visibility.Visible;

            MediaElement_Player.Pause();
            _mediaPlayerPlaying = false;
        }

        private void Stop()
        {
            Button_Pause.Visibility = Visibility.Collapsed;
            Button_Play.Visibility = Visibility.Visible;

            MediaElement_Player.Stop();
            _mediaPlayerPlaying = false;
        }

        private void Unload()
        {
            Stop();
            MediaElement_Player.Source = null;
            _mediaLoaded = false;
            _mediaPlayerPlaying = false;

            Slider_Progress.Value = 0;
            TextBlock_Progress.Text = "-";
            TextBlock_ClipLength.Text = "-";
            TextBlock_CurrentlyPlaying.Text = "No clip playing";
            userIsDraggingSlider = false;
            Slider_Progress.IsEnabled = false;
        }

        private void ToDefault()
        {
            TextBlock_Error.Visibility = Visibility.Collapsed;
            QueueIndex = -1;
            Unload();
            Queue = new List<Clip>();
            _originalQueue = new List<Clip>();
            ListView_Playlist.Items.Clear();
        }

        private void Mute(bool slider = false)
        {
            _muted = true;
            Button_Mute.Visibility = Visibility.Collapsed;
            Button_UnMute.Visibility = Visibility.Visible;
            MediaElement_Player.IsMuted = true;
        }

        private void UnMute()
        {
            _muted = false;
            Button_Mute.Visibility = Visibility.Visible;
            Button_UnMute.Visibility = Visibility.Collapsed;
            MediaElement_Player.IsMuted = false;
            if (MediaElement_Player.Volume < .05)
            {
                MediaElement_Player.Volume = .05;
                Slider_Volume.Value = .05;
            }
        }

        private void EnableLoop()
        {
            Button_LoopOff.Visibility = Visibility.Collapsed;
            Button_LoopOn.Visibility = Visibility.Visible;
            _loop = true;
        }

        private void DisableLoop()
        {
            Button_LoopOff.Visibility = Visibility.Visible;
            Button_LoopOn.Visibility = Visibility.Collapsed;
            _loop = false;
        }

        private void Next()
        {
            if (Queue.Count == 0)
            {
                ToDefault();
                return;
            }

            bool success = false;
            for (int i=0; i < Queue.Count; i++)
            {
                QueueIndex++;
                if (QueueIndex >= Queue.Count)
                {
                    QueueIndex = 0;
                    if (!_loop)
                    {
                        Stop();
                        success = true;
                        break;
                    }
                }
                if (Load(Queue[QueueIndex]))
                {
                    Play();
                    success = true;
                    break;
                }
            }
            if (!success)
                Unload();
        }

        private void Previous()
        {
            if(MediaElement_Player.Position.TotalSeconds < 2 && QueueIndex > 0)
            {
                QueueIndex--;
                Load(Queue[QueueIndex]);
            }
            else
            {
                MediaElement_Player.Position = TimeSpan.FromSeconds(0);
            }
        }

        private void PopulateListView()
        {
            ListView_Playlist.Items.Clear();
            foreach(Clip c in Queue)
            {
                PopulateListView(c);
            }
            ListView_Playlist.SelectedIndex = QueueIndex;
        }

        private void PopulateListView(Clip c)
        {
            ListViewItem lvi = new ListViewItem()
            {
                Content = c.ToString(),
                Tag = c
            };
            lvi.MouseDoubleClick += ListViewItem_Clip_MouseDoubleClick;
            lvi.ContextMenu = ContextMenu_Playlist;
            ListView_Playlist.Items.Add(lvi);
        }

        private void Shuffle()
        {
            if(Queue.Count > 1)
            {
                if(_mediaLoaded)
                {
                    Clip currentlyPlaying = Queue[QueueIndex];
                    Queue.RemoveAt(QueueIndex);
                    Queue.Shuffle();
                    Queue.Insert(0, currentlyPlaying);
                } else
                {
                    Queue.Shuffle();
                }
                QueueIndex = 0;
                PopulateListView();
            }
            _shuffled = true;
        }

        private void UnShuffle()
        {
            if (_mediaPlayerPlaying)
                QueueIndex = _originalQueue.IndexOf(Queue[QueueIndex]);
            Queue = new List<Clip>(_originalQueue);
            PopulateListView();
            _shuffled = false;
        }

        public void UpdateInformation()
        {
            PopulateListView();
            if(QueueIndex != -1)
                TextBlock_CurrentlyPlaying.Text = Queue[QueueIndex].Name;
        }

        private bool TestIfPlayable(string path)
        {
            //Does path exist?
            if (!System.IO.File.Exists(path))
            {
                showError($"Path '{path}' does not exist!");
                return false;
            }

            //Does the file have an extension, and is it allowed?
            if(System.IO.Path.HasExtension(path))
            {
                string fileExt = System.IO.Path.GetExtension(path);
                if(_allowedFileExtensions.Contains(fileExt))
                {
                    return true;
                }else
                {
                    showError($"File extension '{fileExt}' not allowed!");
                    return false;
                }
            }
            else
            {
                showError($"File '{path}' does not have an extension");
                return false;
            }
        }

        private void showError(string error)
        {
            TextBlock_Error.Visibility = Visibility.Visible;
            TextBlock_Error.Text = "Error: " + error;
            _showError = true;
            _errorTimer = 0;
        }

        public void Tick(int interval)
        {
            if (MediaElement_Player.Source != null && MediaElement_Player.NaturalDuration.HasTimeSpan && !userIsDraggingSlider)
            {
                Slider_Progress.Value = MediaElement_Player.Position.TotalSeconds / MediaElement_Player.NaturalDuration.TimeSpan.TotalSeconds;

                TextBlock_Progress.Text = $"{MediaElement_Player.Position.ToString(@"mm\:ss")}";
                TextBlock_ClipLength.Text = $"{MediaElement_Player.NaturalDuration.TimeSpan.ToString(@"mm\:ss")}";
            }
            ListView_Playlist.SelectedIndex = QueueIndex;

            if (_showError)
            {
                Console.WriteLine(_errorTimer);
                if (_errorTimer > 5000)
                {
                    _showError = false;
                    TextBlock_Error.Visibility = Visibility.Collapsed;
                }
                _errorTimer += interval;
            }
        }

        // ######################################### Events ###############################################

        #region Controls
        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Button_Pause_Click(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void Button_Prev_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void Button_Next_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

        private void Button_Mute_Click(object sender, RoutedEventArgs e)
        {
            Mute();
        }

        private void Button_Unmute_Click(object sender, RoutedEventArgs e)
        {
            UnMute();
        }

        private void Button_LoopOff_Click(object sender, RoutedEventArgs e)
        {
            EnableLoop();
        }

        private void Button_LoopOn_Click(object sender, RoutedEventArgs e)
        {
            DisableLoop();
        }

        private void Slider_Volume_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _volumeDragStart = Slider_Volume.Value;
        }

        private void Slider_Volume_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (Slider_Volume.Value == 0)
                Mute();
            else
            {
                if (_muted)
                    UnMute();
                UpdateVolume();
            }
        }

        private void Slider_Progress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if(_mediaLoaded)
            {
                MediaElement_Player.Position = TimeSpan.FromSeconds(Slider_Progress.Value * MediaElement_Player.NaturalDuration.TimeSpan.TotalSeconds);
                userIsDraggingSlider = false;
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Change Volume
            //Slider_Volume.Value += (e.Delta > 0) ? 0.1 : -0.1;
        }

        #endregion

        private void Slider_Progress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {

        }

        private void MePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            //Play next file
            Next();
        }

        private void Button_ShuffleOff_Click(object sender, RoutedEventArgs e)
        {
            Shuffle();
            Button_ShuffleOff.Visibility = Visibility.Collapsed;
            Button_ShuffleOn.Visibility = Visibility.Visible;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string DropType = e.Data.GetFormats()[0];
            Console.WriteLine("Dropped");

            if(DropType == DataFormats.FileDrop)
            {
                Console.WriteLine("Recognized as file drop");
                string file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                string fileExtension = System.IO.Path.GetExtension(file);
                //if(allowedFileExtensions.Contains(fileExtension))
                Load(file);
                //Console.WriteLine($"{fileExtension} not allowed");
            }
            else if(DropType == "playlist" && e.Data.GetData("playlist") is Playlist p)
            {
                Console.WriteLine("Recognized as playlist");
                _originalQueue = new List<Clip>(p.Clips);
                Queue = new List<Clip>(p.Clips);
                _mediaLoaded = false;
                QueueIndex = 0;
                Console.WriteLine(_shuffled);
                if(_shuffled)
                    Queue.Shuffle();

                PopulateListView();
                Load(Queue[0]);
                Play();
            }
            else if(DropType == "clip" && e.Data.GetData("clip") is Clip c)
            {
                Console.WriteLine("Recognized as clip");

                _originalQueue.Add(c);
                Queue.Add(c);
                PopulateListView(c);
                if (Queue.Count == 1)
                {
                    Console.WriteLine("Start playing clip");
                    QueueIndex = 0;
                    Load(Queue[0]);
                    Play();
                    Console.WriteLine("Started playing");
                    Console.WriteLine(MediaElement_Player.Source);
                }
            }

            Grid_DragOverlay.Visibility = Visibility.Hidden;
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                /*string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if(files[0].IndexOf('.') != -1)
                {
                    string fileExtension = files[0].Split('.')[1];
                    if(allowedFileExtensions.Contains(fileExtension))
                        Console.WriteLine("Allowed!");
                    else
                        Console.WriteLine($"Not allowed : {fileExtension}");
                }*/
            }
            Grid_DragOverlay.Visibility = Visibility.Visible;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            Grid_DragOverlay.Visibility = Visibility.Hidden;
        }

        private void ListViewItem_Clip_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            QueueIndex = ListView_Playlist.Items.IndexOf(sender);
            Load(Queue[QueueIndex]);
        }

        private void TrackPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            ToDefault();
        }

        private void Button_ShuffleOn_Click(object sender, RoutedEventArgs e)
        {
            UnShuffle();
            Button_ShuffleOff.Visibility = Visibility.Visible;
            Button_ShuffleOn.Visibility = Visibility.Collapsed;
        }

        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnu = sender as MenuItem;
            if (mnu != null)
            {
                ListViewItem lvi = ((ContextMenu)mnu.Parent).PlacementTarget as ListViewItem;
                if(lvi.Tag is Clip c)
                {
                    //Remove
                    _originalQueue.Remove(c);
                    Queue.Remove(c);
                    ListView_Playlist.Items.Remove(lvi);

                    if (QueueIndex >= Queue.Count)
                        QueueIndex = Queue.Count - 1;

                    if (lvi == current_ListViewItem)
                    {
                        if (QueueIndex != -1)
                        {
                            QueueIndex--;
                            Next();
                        }
                        else
                            Unload();
                    }                            
                }
            }
        }

        private void MenuItem_Delete_Track_Click(object sender, RoutedEventArgs e)
        {
            Remove();
        }

        private void TextBlock_TrackName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = PointToScreen(new Point(2, -1));
            WindowHandler.OpenEditTrackNameWindow(this, p);

            e.Handled = true; //To stop MainWindow from getting activated
        }

        private void MenuItem_Clear_Queue_Click(object sender, RoutedEventArgs e)
        {
            ToDefault();
        }
    }
}
