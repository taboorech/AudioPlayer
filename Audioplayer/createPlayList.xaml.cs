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
        MainWindow mainWindow = new MainWindow();        
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
            xDoc.Save($"{((TextBox)FindName("nameOfCreateList")).Text}.xml");            
            this.Close();            
        }
    }
}
