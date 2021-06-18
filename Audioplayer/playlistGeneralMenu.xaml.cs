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
    /// Логика взаимодействия для playlistGeneralMenu.xaml
    /// </summary>
    public partial class playlistGeneralMenu : Window
    {        
        Elements elem = new Elements();
        public playlistGeneralMenu()
        {
            InitializeComponent();
            addSongs.MouseEnter += elem.onMouseEnter;
            addSongs.MouseLeave += elem.onMouseLeave;
            addSongs.Background = Brushes.White;
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    addSongs.MouseDown += (window as MainWindow).playListMenuButtonClick;
                }
            }            
            deletePlaylist.MouseEnter += elem.onMouseEnter;
            deletePlaylist.MouseLeave += elem.onMouseLeave;
            deletePlaylist.Background = Brushes.White;
        }       
    }
}
