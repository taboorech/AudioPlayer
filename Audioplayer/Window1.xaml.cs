using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Windows.Media;

namespace Audioplayer
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        List<TextBlock> deleteButtons = new List<TextBlock>();
        public Window1()
        {
            InitializeComponent();
            string settingsFile = "settings.xml";            
            List<string> dirsList = new List<string>();            
            if (!File.Exists(settingsFile))
            {
                XDocument settings = new XDocument(new XElement("Dirs", ""));
                settings.Save("settings.xml");
            }
            XDocument sett = XDocument.Load(settingsFile);
            Grid dirs = (Grid)FindName("Dirs");
            foreach(XElement dir in sett.Element("Dirs").Elements("dir"))
            {
                dirsList.Add(dir.Value);
            }
            for(int i = 0; i < dirsList.Count; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(50, GridUnitType.Pixel);
                dirs.RowDefinitions.Add(row);
                Grid dirInfo = new Grid();
                Grid.SetRow(dirInfo, i);
                ColumnDefinition path = new ColumnDefinition();
                ColumnDefinition buttonToDel = new ColumnDefinition();
                path.Width = new GridLength(3, GridUnitType.Star);
                buttonToDel.Width = new GridLength(1, GridUnitType.Star);
                dirInfo.ColumnDefinitions.Add(path);
                dirInfo.ColumnDefinitions.Add(buttonToDel);
                Border borderToDel = new Border();
                borderToDel.MouseEnter += mouseEnter;
                TextBlock deleteButton = new TextBlock();
                deleteButton.FontSize = 17;
                deleteButton.Text = "-";
                deleteButtons.Add(deleteButton);
                deleteButton.PreviewMouseDown += deletePath;
                borderToDel.Child = deleteButton;
                Grid.SetColumn(borderToDel, 1);
                TextBlock nameOfDir = new TextBlock();
                nameOfDir.FontSize = 17;
                nameOfDir.Text = dirsList[i];
                Grid.SetColumn(nameOfDir, 0);
                dirInfo.Children.Add(nameOfDir);
                dirInfo.Children.Add(borderToDel);
                dirs.Children.Add(dirInfo);
            }
        }

        private void AddButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                XDocument settings = XDocument.Load("settings.xml");
                XElement dirs = settings.Element("Dirs");
                dirs.Add(new XElement("dir", dialog.SelectedPath, new XAttribute("id", deleteButtons.Count)));
                settings.Save("settings.xml");
                //MessageBox.Show(dialog.SelectedPath);
            }
        }

        private void deletePath(object sender, RoutedEventArgs e)
        {
            XDocument settings = XDocument.Load("settings.xml");
            foreach (XElement dir in settings.Element("Dirs").Elements("dir"))
            {
                if(dir.Attribute("id").Value == deleteButtons.IndexOf((TextBlock)sender).ToString())
                {
                    dir.Remove();
                }
            }
            settings.Save("settings.xml");
            //deleteButtons.IndexOf((Border)sender);
        }

        private void mouseEnter(object sender, MouseEventArgs e)
        {
            ((Border)sender).Background = Brushes.Gray;
            ((TextBlock)(((Border)sender).Child)).Foreground = Brushes.White;       
        }
    }
}
