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
using System.Xml.Linq;

namespace Audioplayer
{
    /// <summary>
    /// Логика взаимодействия для createPlayList.xaml
    /// </summary>
    public partial class createPlayList : Window
    {
        int number = 0;        
        public void writer(int numberOfGrid)
        {
            number = numberOfGrid;
        }
        public createPlayList()
        {
            InitializeComponent();
        }        

        private void createButtonClick(object sender, RoutedEventArgs e)
        {
            XDocument xDoc = new XDocument(new XElement("Songs", ""));
            xDoc.Save($"{nameOfCreateList.Text}.xml");
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    (window as MainWindow).settingsXML.Element("Settings").Element("Playlists").Add(new XElement("playlist", $"{nameOfCreateList.Text}.xml"));
                    (window as MainWindow).settingsXML.Save("settings.xml");
                    (window as MainWindow).pL.Children.Clear();
                    (window as MainWindow).pL.RowDefinitions.Clear();
                    (window as MainWindow).getPlayListsNames();
                }
            }            
            this.Close();            
        }
    }
}
