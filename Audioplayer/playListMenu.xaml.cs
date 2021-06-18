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

namespace Audioplayer
{
    /// <summary>
    /// Логика взаимодействия для playListMenu.xaml
    /// </summary>
    public partial class playListMenu : Window
    {        
        Elements elem = new Elements();
        public playListMenu()
        {
            InitializeComponent();
            addMusicButton.Background = Brushes.Transparent;
            addMusicButton.MouseEnter += elem.onMouseEnter;
            addMusicButton.MouseLeave += elem.onMouseLeave;
        }        

        private void addMusicButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).DownloadMusic(youtubeMusicPath.Text);
                }
            }            
            Close();
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(playlistGeneralMenu))
                {
                    (window as playlistGeneralMenu).Close();
                }
            }
        }

        private void textBoxClick(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).Text = null;
            ((TextBox)sender).Opacity = 1;
        }
    }
}
