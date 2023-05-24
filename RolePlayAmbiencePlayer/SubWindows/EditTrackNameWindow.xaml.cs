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
using System.Windows.Shapes;

namespace RolePlayAmbiencePlayer
{
    /// <summary>
    /// Interaction logic for EditTrackNameWindow.xaml
    /// </summary>
    public partial class EditTrackNameWindow : Window
    {
        private TrackPlayer trackPlayer;
        private bool _closing = false;

        public EditTrackNameWindow(TrackPlayer trackPlayer)
        {
            InitializeComponent();
            this.trackPlayer = trackPlayer;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void TextBox_TrackName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                Cancel();
            }
            else if(e.Key == Key.Enter)
            {
                Save();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_TrackName.Text = trackPlayer.Header;
        }

        private void Save()
        {
            trackPlayer.Header = TextBox_TrackName.Text;
            if(!_closing)
                Close();
        }

        private void Cancel()
        {
            if(!_closing)
                Close();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            if (!_closing)
                Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _closing = true;
            WindowHandler.EditTrackName = null;
        }
    }
}
