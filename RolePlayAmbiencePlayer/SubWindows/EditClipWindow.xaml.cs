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
    /// Interaction logic for EditClipWindow.xaml
    /// </summary>
    public partial class EditClipWindow : Window
    {
        private bool _saving = false;
        private Clip clip;
        private bool _closing = false;
        private TreeViewItem tvi;
        private bool newItem;

        public EditClipWindow(Clip clip, TreeViewItem treeViewItem, bool newItem = false)
        {
            InitializeComponent();

            this.clip = clip;
            this.tvi = treeViewItem;
            this.newItem = newItem;
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            _saving = true;

            clip.Name = TextBox_Name.Text;
            clip.Path = TextBox_Path.Text;

            //DialogResult = true;
            Console.WriteLine("Closing");

            if(newItem)
            {
                TreeViewItem clip_item = WindowHandler.Main.NewTreeViewClip(clip);
                tvi.Items.Add(clip_item);

                if (tvi.Tag is Playlist p)
                    p.Clips.Add(clip);
                else if (tvi.Tag is MediaFolder f)
                    f.Clips.Add(clip);
            }
            else
            {
                tvi.Header = $"♫ {clip.Name}";
                MediaHandler.UpdateClipsVisuals();
            }
            DialogResult = true;

            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _closing = true;
            WindowHandler.EditClip = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_Name.Text = clip.Name;
            TextBox_Path.Text = clip.Path;
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_Name_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextBox_Path_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
