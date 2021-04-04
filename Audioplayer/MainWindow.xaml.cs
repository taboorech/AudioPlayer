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
using System.Timers;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;

namespace Audioplayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer auSet = new MediaPlayer();
        public bool switcher = false;
        Elements elem = new Elements();
        Time time = new Time();
        findAudio finder = new findAudio();
        DispatcherTimer timer = new DispatcherTimer();
        List<TextBlock> deleteButtons = new List<TextBlock>();
        int count = 0;
        List<string> songs = new List<string>();
        List<Border> playButtons = new List<Border>();
        bool looper = false;
        public MainWindow()
        {
            InitializeComponent();            
            XDocument settings = XDocument.Load("settings.xml");
            List<string> path = new List<string>();
            foreach(XElement dir in settings.Element("Dirs").Elements("dir"))
            {
                path.Add(dir.Value);
            }
            //string path = @"C:\Users\Кисе\Desktop\Музыка";        
            finder.finder(path, songs);
            auSet.Open(new Uri(songs[count]));
            ((TextBlock)FindName("SoundName")).Text = elem.soundName(auSet.Source.ToString());
            Grid playList = (Grid)FindName("playList");
            TextBlock prgressBar = (TextBlock)FindName("progressBar");
            ((Border)FindName("prevSound")).MouseEnter += elem.onMouseEnter;
            ((Border)FindName("play")).MouseEnter += elem.onMouseEnter;
            ((Border)FindName("nextSound")).MouseEnter += elem.onMouseEnter;
            ((Border)FindName("addDirectory")).MouseEnter += elem.onMouseEnter;
            ((Border)FindName("addDirectory")).Background = Brushes.Transparent;
            ((Border)FindName("allMusics")).MouseEnter += elem.onMouseEnter;
            ((Border)FindName("allMusics")).Background = Brushes.Transparent;
            ((Border)FindName("settings")).MouseEnter += elem.onMouseEnter;
            ((Border)FindName("settings")).Background = Brushes.Transparent;
            //((Border)FindName("loopButton")).MouseEnter += elem.onMouseEnter;
            ((Border)FindName("prevSound")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("play")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("nextSound")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("addDirectory")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("allMusics")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("settings")).MouseLeave += elem.onMouseLeave;
            string settingsFile = "settings.xml";            
            List<string> dirsList = new List<string>();
            if (!File.Exists(settingsFile))
            {
                XDocument createXML = new XDocument(new XElement("Dirs", ""));
                createXML.Save("settings.xml");
            }
            XDocument sett = XDocument.Load(settingsFile);
            Grid dirs = (Grid)FindName("Dirs");
            foreach (XElement dir in sett.Element("Dirs").Elements("dir"))
            {
                dirsList.Add(dir.Value);
            }
            for (int i = 0; i < dirsList.Count; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(50, GridUnitType.Pixel);
                dirs.RowDefinitions.Add(row);
                Grid dirInfo = new Grid();
                Grid.SetRow(dirInfo, i);
                ColumnDefinition dirsPath = new ColumnDefinition();
                ColumnDefinition buttonToDel = new ColumnDefinition();
                dirsPath.Width = new GridLength(3, GridUnitType.Star);
                buttonToDel.Width = new GridLength(0.3, GridUnitType.Star);
                dirInfo.ColumnDefinitions.Add(dirsPath);
                dirInfo.ColumnDefinitions.Add(buttonToDel);
                Border borderToDel = new Border();
                borderToDel.Cursor = Cursors.Hand;
                borderToDel.MouseEnter += elem.onMouseEnter;
                borderToDel.MouseLeave += elem.onMouseLeave;                
                TextBlock deleteButton = new TextBlock();
                deleteButton.FontSize = 20;
                deleteButton.HorizontalAlignment = HorizontalAlignment.Center;
                deleteButton.VerticalAlignment = VerticalAlignment.Center;
                deleteButton.Text = "-";
                deleteButtons.Add(deleteButton);
                deleteButton.PreviewMouseDown += deletePath;
                borderToDel.Child = deleteButton;
                elem.onMouseEnter(borderToDel, null);
                elem.onMouseLeave(borderToDel, null);
                Grid.SetColumn(borderToDel, 1);
                TextBlock nameOfDir = new TextBlock();
                nameOfDir.FontSize = 17;
                nameOfDir.VerticalAlignment = VerticalAlignment.Center;
                nameOfDir.Margin = new Thickness(15, 0, 0, 0);
                //nameOfDir.HorizontalAlignment = HorizontalAlignment.Center;
                nameOfDir.Text = dirsList[i];
                Grid.SetColumn(nameOfDir, 0);
                dirInfo.Children.Add(nameOfDir);
                dirInfo.Children.Add(borderToDel);
                dirs.Children.Add(dirInfo);
            }
            //((Border)FindName("loopButton")).MouseLeave += elem.onMouseLeave;
            progressBar.PreviewMouseDown += progressBar_click;
            TextBlock progress = (TextBlock)FindName("progress");            
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += toTimer;
            timer.Start();
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock) FindName("durationTime"));
            createPlayListElement();
        }

        public void progressBar_click(object sender, MouseEventArgs e)
        {
            PreviewMouseMove += progressBar_MouseMove;
            PreviewMouseUp += progressBar_MouseButtonUp;
            double obj = (progressBar).PointToScreen(new Point(0, 0)).X;

            var windowPosition = Mouse.GetPosition(this);
            double screenPosition = this.PointToScreen(windowPosition).X;

            double widthOfElement = (progressBar).ActualWidth;
            if (screenPosition - obj > 0 && screenPosition - obj < widthOfElement)
            {
                auSet.Position = TimeSpan.FromSeconds(auSet.NaturalDuration.TimeSpan.TotalSeconds * (((screenPosition - obj) * 100 / widthOfElement) / 100));
            }
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
        }

        public void progressBar_MouseMove(object sender, MouseEventArgs e)
        {
            auSet.Pause();
            double obj = (progressBar).PointToScreen(new Point(0, 0)).X;

            var windowPosition = Mouse.GetPosition(this);
            double screenPosition = this.PointToScreen(windowPosition).X;

            double widthOfElement = (progressBar).ActualWidth;
            if (screenPosition - obj > 0 && screenPosition - obj < widthOfElement)
            {
                auSet.Position = TimeSpan.FromSeconds(auSet.NaturalDuration.TimeSpan.TotalSeconds * (((screenPosition - obj) * 100 / widthOfElement) / 100));
            }
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
            PreviewMouseUp += progressBar_MouseButtonUp;
        }

        public void progressBar_MouseButtonUp(object sender, MouseEventArgs e)
        {
            PreviewMouseMove -= progressBar_MouseMove;
            if (switcher)
            {
                auSet.Play();
            }
            //time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
        }

        public void toTimer(object sender, EventArgs e)
        {
            if(auSet.NaturalDuration.HasTimeSpan && (auSet.Position.TotalSeconds * 100 / auSet.NaturalDuration.TimeSpan.TotalSeconds) >= 100 && !looper) 
            {
                if (count >= (songs.Count - 1))
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                auSet.Open(new Uri(songs[count]));
                ((TextBlock)FindName("SoundName")).Text = elem.soundName(auSet.Source.ToString());
                progress.Width = 0;
                auSet.Play();
            } else if(auSet.NaturalDuration.HasTimeSpan && (auSet.Position.TotalSeconds * 100 / auSet.NaturalDuration.TimeSpan.TotalSeconds) >= 100 && looper)
            {
                auSet.Position = TimeSpan.FromSeconds(0);
                progress.Width = 0;
            }
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
        }

        public void playPause(object sender, RoutedEventArgs e)
        {
            if (!switcher)
            {
                auSet.Play();
                ((TextBlock)(playButtons[count].Child)).Text = "||";
                switcher = true;
                timer.Start();
            } 
            else
            {
                auSet.Pause();
                ((TextBlock)(playButtons[count].Child)).Text = "▶";
                switcher = false;
                timer.Stop();
            }
            elem.changeButton(switcher, (TextBlock)(((Border)sender).Child));
        }

        private void AddButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                XDocument settings = XDocument.Load("settings.xml");
                XElement dirs = settings.Element("Dirs");
                MessageBox.Show(result.ToString());
                if (result.ToString() == "OK")
                {
                    dirs.Add(new XElement("dir", dialog.SelectedPath, new XAttribute("id", deleteButtons.Count)));
                }
                settings.Save("settings.xml");
                //MessageBox.Show(dialog.SelectedPath);
            }
        }

        private void volumeBar_click(object sender, RoutedEventArgs e)
        {
            PreviewMouseMove += volumeBar_MouseMove;
            PreviewMouseUp += volumeBar_MouseUp;
            double obj = ((TextBlock)FindName("volumeBar")).PointToScreen(new Point(0, 0)).X;

            var windowPosition = Mouse.GetPosition(this);
            double screenPosition = this.PointToScreen(windowPosition).X;

            double widthOfElement = ((TextBlock)FindName("volumeBar")).ActualWidth;
            if ((screenPosition - obj >= 0) && (screenPosition - obj <= widthOfElement))
            {
                ((TextBlock)FindName("volume")).Width = screenPosition - obj;
                auSet.Volume = ((((screenPosition - obj) * 100) / widthOfElement) / 100);             
            }            
        }

        private void volumeBar_MouseMove(object sender, MouseEventArgs e)
        {
            double obj = ((TextBlock)FindName("volumeBar")).PointToScreen(new Point(0, 0)).X;

            var windowPosition = Mouse.GetPosition(this);
            double screenPosition = this.PointToScreen(windowPosition).X;

            double widthOfElement = ((TextBlock)FindName("volumeBar")).ActualWidth;
            if ((screenPosition - obj >= 0) && (screenPosition - obj <= widthOfElement))
            {
                ((TextBlock)FindName("volume")).Width = screenPosition - obj;
                auSet.Volume = ((((screenPosition - obj) * 100) / widthOfElement) / 100);
            }
        }

        private void volumeBar_MouseUp(object sender, MouseEventArgs e)
        {
            PreviewMouseMove -= volumeBar_MouseMove;
            double obj = ((TextBlock)FindName("volumeBar")).PointToScreen(new Point(0, 0)).X;

            var windowPosition = Mouse.GetPosition(this);
            double screenPosition = this.PointToScreen(windowPosition).X;

            double widthOfElement = ((TextBlock)FindName("volumeBar")).ActualWidth;
            if ((screenPosition - obj >= 0) && (screenPosition - obj <= widthOfElement))
            {
                ((TextBlock)FindName("volume")).Width = screenPosition - obj;
                auSet.Volume = ((((screenPosition - obj) * 100) / widthOfElement) / 100);
            }
        }

        private void deletePath(object sender, RoutedEventArgs e)
        {
            XDocument settings = XDocument.Load("settings.xml");
            foreach (XElement dir in settings.Element("Dirs").Elements("dir"))
            {
                if (dir.Attribute("id").Value == deleteButtons.IndexOf((TextBlock)sender).ToString())
                {
                    dir.Remove();
                }
            }
            settings.Save("settings.xml");
            //deleteButtons.IndexOf((Border)sender);
        }

        public void createPlayListElement()
        {
            for (int i = 0; i < songs.Count; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(50, GridUnitType.Pixel);
                playList.RowDefinitions.Add(row);
                Grid song = new Grid();
                ColumnDefinition name = new ColumnDefinition();
                GridLength nameWidth = new GridLength(2, GridUnitType.Star);
                name.Width = nameWidth;
                // Song Name
                TextBlock songName = new TextBlock();
                songName.Text = elem.soundName(songs[i]);
                songName.FontSize = 17;
                songName.Margin = new Thickness(15, 0, 0, 0);
                songName.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumn(songName, 0);
                song.Children.Add(songName);
                // Play Button
                ColumnDefinition play = new ColumnDefinition();
                GridLength playWidth = new GridLength(0.2, GridUnitType.Star);
                play.Width = playWidth;
                Border playButton = new Border();
                playButton.BorderThickness = new Thickness(0);
                TextBlock playButtonBlock = new TextBlock();
                //playButton.Background = Brushes.Gray;
                playButton.MouseEnter += elem.onMouseEnter;                
                playButton.MouseLeave += elem.onMouseLeave;
                playButton.Child = playButtonBlock;
                playButtonBlock.Text = "▶";
                playButtonBlock.FontSize = 17;
                playButtonBlock.VerticalAlignment = VerticalAlignment.Center;
                playButtonBlock.HorizontalAlignment = HorizontalAlignment.Center;
                playButton.PreviewMouseDown += playButton_click;                
                playButton.Cursor = Cursors.Hand;
                elem.onMouseEnter(playButton, null);
                elem.onMouseLeave(playButton, null);
                playButtons.Add(playButton);
                Grid.SetColumn(playButton, 1);
                song.Children.Add(playButton);
                ColumnDefinition add = new ColumnDefinition();
                add.Width = playWidth;
                // addButton
                Button addButton = new Button();
                addButton.BorderThickness = new Thickness(0);
                addButton.Content = "+";
                addButton.FontSize = 17;
                Grid.SetColumn(addButton, 2);
                song.Children.Add(addButton);
                //ColumnDefinition blank = new ColumnDefinition();              
                ColumnDefinition time = new ColumnDefinition();
                time.Width = playWidth;
                TextBlock durationTimeBlock = new TextBlock();
                durationTimeBlock.FontSize = 17;
                Grid.SetColumn(durationTimeBlock, 3);
                song.Children.Add(durationTimeBlock);
                //
                song.ColumnDefinitions.Add(name);
                song.ColumnDefinitions.Add(play);
                song.ColumnDefinitions.Add(add);
                //song.ColumnDefinitions.Add(blank);
                song.ColumnDefinitions.Add(time);
                Grid.SetRow(song, i);
                playList.Children.Add(song);

            }
        }

        public void playButton_click(object sender, RoutedEventArgs e)
        {
            if(((TextBlock)((Border)sender).Child).Text != "||" && count != playButtons.IndexOf((Border)sender))
            {
                switcher = false;
                count = playButtons.IndexOf((Border)sender);
                auSet.Open(new Uri(songs[count]));
                progress.Width = 0;
                ((TextBlock)FindName("SoundName")).Text = elem.soundName(auSet.Source.ToString());
                time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
            }
            for (int i = 0; i < playButtons.Count; i++)
            {
                ((TextBlock)(playButtons[i]).Child).Text = "▶";
            }
            ((TextBlock)((Border)sender).Child).Text = "||";            
            
            playPause((Border)FindName("play"), null);
                        
            /*if (switcher)
            {
                auSet.Play();
            }*/
        }

        public void loopButton_click(object sender, RoutedEventArgs e)
        {
            if (!looper)
            {
                looper = true;
                ((Border)sender).Background = Brushes.Gray;
                ((TextBlock)(((Border)sender).Child)).Foreground = Brushes.White;
            }
            else
            {
                looper = false;
                ((Border)sender).Background = Brushes.White;
                ((TextBlock)(((Border)sender).Child)).Foreground = Brushes.Black;
            }
        }

        public void changeSounds(object sender, RoutedEventArgs e)
        {
            if(((Border)sender).Name == "prevSound")
            {
                if (count == 0)
                {
                    count = songs.Count - 1;
                }
                else
                {
                    count--;
                }
            } 
            else
            {
                if (count == (songs.Count - 1))
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
            }
            auSet.Open(new Uri(songs[count]));
            ((TextBlock)FindName("SoundName")).Text = elem.soundName(auSet.Source.ToString());
            progress.Width = 0;
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
            if(switcher)
            {
                auSet.Play();
            }
        }
        public void changeWindow(object sender, RoutedEventArgs e)
        {            
            switch (((Border)sender).Name)
            {
                case "allMusics":
                    ((ScrollViewer)FindName("toSounds")).Visibility = Visibility.Visible;
                    ((ScrollViewer)FindName("toDirs")).Visibility = Visibility.Hidden;
                    ((Border)FindName("addDirectory")).Visibility = Visibility.Hidden;
                    break;
                case "settings":
                    //Window1 wd = new Window1();
                    //this.Close();                    
                    //wd.ShowDialog();
                    ((ScrollViewer)FindName("toSounds")).Visibility = Visibility.Hidden;
                    ((ScrollViewer)FindName("toDirs")).Visibility = Visibility.Visible;
                    ((Border)FindName("addDirectory")).Visibility = Visibility.Visible;
                    //Panel.SetZIndex(((ScrollViewer)FindName("toDirs")), 5);
                    break;
            }
        }
    }
    
    class Elements
    {
        public string soundName(string fullName)
        {
            string[] soundName = fullName.Split('/', '\\');
            return soundName[soundName.Length - 1];
            //elem.Text = soundName[soundName.Length - 1];
        }

        public void changeButton(bool switcher, TextBlock elem)
        {
            if(!switcher)
            {
                elem.Text = "Play";
            } 
            else
            {
                elem.Text = "Pause";
            }
        }

        public void onMouseEnter(object sender, MouseEventArgs e)
        {
            if(((Border)sender).Name == "prevSound" || ((Border)sender).Name == "play" || ((Border)sender).Name == "nextSound")
            {
                ((Border)sender).Background = Brushes.White;
                ((TextBlock)(((Border)sender).Child)).Foreground = Brushes.Black;
            } else
            {                
                ((Border)sender).Background = Brushes.Gray;
                ((TextBlock)(((Border)sender).Child)).Foreground = Brushes.White;
            }
        }

        public void onMouseLeave(object sender, MouseEventArgs e)
        {
            if (((Border)sender).Name == "prevSound" || ((Border)sender).Name == "play" || ((Border)sender).Name == "nextSound")
            {
                ((Border)sender).Background = Brushes.Gray;
                ((TextBlock)(((Border)sender).Child)).Foreground = Brushes.White;
            }
            else
            {
                ((Border)sender).Background = Brushes.Transparent;
                ((TextBlock)(((Border)sender).Child)).Foreground = Brushes.Black;
            }
        }
    }

    class Time
    {   
        public void timeToSlider(MediaPlayer audioSettings, TextBlock progressBar, TextBlock progress, TextBlock current, TextBlock duration)
        {
            if (audioSettings.NaturalDuration.HasTimeSpan)
            {
                progress.Width = progressBar.ActualWidth * ((audioSettings.Position.TotalSeconds * 100 / audioSettings.NaturalDuration.TimeSpan.TotalSeconds) / 100);
            }
            string currentSeconds = "";
            string durationSeconds = "";
            if(Math.Floor(audioSettings.Position.TotalSeconds % 60) < 10)
            {
                currentSeconds = $"0{Math.Floor(audioSettings.Position.TotalSeconds % 60)}";
            } else
            {
                currentSeconds = $"{Math.Floor(audioSettings.Position.TotalSeconds % 60)}";
            }
            current.Text = $"{Math.Floor(audioSettings.Position.TotalSeconds / 60)}:{currentSeconds}";
            if (audioSettings.NaturalDuration.HasTimeSpan)
            {
                if (Math.Floor(audioSettings.NaturalDuration.TimeSpan.TotalSeconds % 60) < 10)
                {
                    durationSeconds = $"0{Math.Floor(audioSettings.NaturalDuration.TimeSpan.TotalSeconds % 60)}";
                } else
                {
                    durationSeconds = $"{Math.Floor(audioSettings.NaturalDuration.TimeSpan.TotalSeconds % 60)}";
                }
                duration.Text = $"{Math.Floor(audioSettings.NaturalDuration.TimeSpan.TotalSeconds / 60)}:{durationSeconds}";
            }
        }
    }

    class findAudio
    {
        public void finder(List<string> path, List<string> songs)
        {
            Regex rg = new Regex("(.mp3)|(.wav)", RegexOptions.IgnoreCase);
            string[] files = new string[4096];
            string[] dirs = new string[4096];
            List<string> dirList = new List<string>();
            for (int i = 0; i < path.Count; i++)
            {
                if (Directory.Exists(path[i]))
                {
                    files = Directory.GetFiles(path[i]);
                    dirs = Directory.GetDirectories(path[i]);
                }

                foreach (string file in files)
                {
                    if (rg.Match(file).Length > 0)
                    {
                        songs.Add(file);
                    }
                }

                foreach (string dir in dirs)
                {
                    dirList.Add(dir);
                    finder(dirList, songs);
                }
            }
        }
    }
}
