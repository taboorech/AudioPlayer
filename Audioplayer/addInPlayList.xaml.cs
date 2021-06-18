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
using System.IO;
using System.Xml.Linq;

namespace Audioplayer
{
    /// <summary>
    /// Логика взаимодействия для addInPlayList.xaml
    /// </summary>
    public partial class addInPlayList : Window
    {
        List<CheckBox> selectSongBlocks = new List<CheckBox>();
        List<string> songs = new List<string>();
        List<CheckBox> allCheckBoxes = new List<CheckBox>();        
        public void listWriter(List<CheckBox> allCheckBoxesList, List<CheckBox> checkBoxes, List<string> song)
        {
            for(int i = 0; i < checkBoxes.Count; i++)
            {
                selectSongBlocks.Add(checkBoxes[i]);
            }
            for (int i = 0; i < song.Count; i++)
            {
                songs.Add(song[i]);
            }
            for (int i = 0; i < allCheckBoxesList.Count; i++)
            {
                allCheckBoxes.Add(allCheckBoxesList[i]);
            }
        }        
        public addInPlayList()
        {
            InitializeComponent();
            foreach (XElement playList in XDocument.Load("settings.xml").Element("Settings").Element("Playlists").Elements("playlist"))
            {
                createElementInGrid(playList.Value, pL.RowDefinitions.Count);
            }
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    for (int i = 0; i < (window as MainWindow).selectSongBlocks.Count; i++)
                    {                        
                        (window as MainWindow).selectSongBlocks[i].IsChecked = false;
                    }
                    (window as MainWindow).selectSongBlocks.Clear();
                    (window as MainWindow).addDirectory.Visibility = Visibility.Hidden;
                }
            }
        }

        private void addInPlayListFunc(object sender, RoutedEventArgs e)
        {
            XDocument pL = XDocument.Load(((TextBlock)sender).Text);
            XElement songsInPL = pL.Element("Songs");
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    for (int i = 0; i < (window as MainWindow).selectSongBlocks.Count; i++)
                    {
                        songsInPL.Add(new XElement("song", songs[allCheckBoxes.IndexOf((window as MainWindow).selectSongBlocks[i])]));
                        pL.Save(((TextBlock)sender).Text);                        
                    }                    
                }
            }
            this.Close();
        }
        private void createElementInGrid(string nameOfPlayList, int i)
        {
            Grid playListsGrid = (Grid)FindName("pL");
            RowDefinition row = new RowDefinition();
            row.Height = new GridLength(50, GridUnitType.Pixel);            
            playListsGrid.RowDefinitions.Add(row);
            Grid dirInfo = new Grid();
            Grid.SetRow(dirInfo, i);
            ColumnDefinition dirsPath = new ColumnDefinition();         
            TextBlock nameOfDir = new TextBlock();
            nameOfDir.FontSize = 17;
            nameOfDir.VerticalAlignment = VerticalAlignment.Center;
            nameOfDir.PreviewMouseDown += addInPlayListFunc;
            nameOfDir.Margin = new Thickness(15, 0, 0, 0);
            nameOfDir.Padding = new Thickness(50, 10, 10, 10);
            nameOfDir.Text = nameOfPlayList;
            nameOfDir.Cursor = Cursors.Hand;            
            nameOfDir.MouseEnter += onMouseEnter;
            nameOfDir.MouseLeave += onMouseLeave;
            onMouseEnter(nameOfDir, null);
            onMouseLeave(nameOfDir, null);
            Grid.SetColumn(nameOfDir, 0);
            dirInfo.Children.Add(nameOfDir);            
            playListsGrid.Children.Add(dirInfo);
        }
        private void onMouseEnter(object sender, RoutedEventArgs e)
        {
            ((TextBlock)sender).Background = Brushes.Gray;
            ((TextBlock)sender).Foreground = Brushes.White;
        }

        private void onMouseLeave(object sender, RoutedEventArgs e)
        {
            ((TextBlock)sender).Background = Brushes.White;
            ((TextBlock)sender).Foreground = Brushes.Black;
        }
    }
}
