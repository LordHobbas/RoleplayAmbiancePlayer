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
    /// Interaction logic for EditFolderWindow.xaml
    /// </summary>
    public partial class EditFolderWindow : Window
    {
        private MediaFolder folder;

        public EditFolderWindow(MediaFolder folder)
        {
            InitializeComponent();

            this.folder = folder;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            folder.Name = TextBox_Name.Text;
            DialogResult = true;
            Close();
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowHandler.EditFolder = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_Name.Text = folder.Name;
        }

        private void TextBox_Name_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
