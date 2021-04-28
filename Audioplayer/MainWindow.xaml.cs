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
        List<Border> deleteButtons = new List<Border>();
        int count = 0;        
        List<string> songs = new List<string>();
        List<Border> playButtons = new List<Border>();
        List<string> path = new List<string>();
        List<CheckBox> selectSongBlocks = new List<CheckBox>();
        List<CheckBox> checkBoxes = new List<CheckBox>();
        List<TextBlock> songsNames = new List<TextBlock>();
        //List<Grid> songGrid = new List<Grid>();
        List<Border> addButtons = new List<Border>();
        List<string> dirsList = new List<string>();
        List<string> listOfSongs = new List<string>();
        string[] xmlPlayLists = new string[4096];
        bool playListSwitch = false;
        bool looper = false;
        string nameOfPlayList = "";
        int playListCount = 0;
        TextBlock barObject = null;
        XDocument settingsXML = null;
        int page = 0;
        public MainWindow()
        {
            InitializeComponent(); 
            if(!File.Exists("settings.xml"))
            {
                XDocument createXML = new XDocument(new XElement("Settings", new XElement("Dirs", ""), new XElement("LastSong", new XAttribute("playList", "none"), 0), new XElement("Volume", "1"), new XElement("page", 0)));
                createXML.Save("settings.xml");
            }
            settingsXML = XDocument.Load("settings.xml");
            page = Int32.Parse(settingsXML.Element("Settings").Element("Page").Value);            
            changeWindow(null, null);
            auSet.Volume = Double.Parse(settingsXML.Element("Settings").Element("Volume").Value);
            this.Loaded += onWindowLoad;
            foreach (XElement dir in settingsXML.Element("Settings").Element("Dirs").Elements("dir"))
            {
                path.Add(dir.Value);
            }
            finder.finder(path, songs);
            count = Int32.Parse(settingsXML.Element("Settings").Element("LastSong").Value);
            if (settingsXML.Element("Settings").Element("LastSong").Attribute("playList").Value != "none")
            {
                playListSwitch = true;
                nameOfPlayList = $"{settingsXML.Element("Settings").Element("LastSong").Attribute("playList").Value}";
                XDocument playListXML = XDocument.Load($"{nameOfPlayList}");
                foreach (XElement song in playListXML.Element("Songs").Elements("song"))
                {
                    listOfSongs.Add(song.Value);
                }                
                playListCount = Int32.Parse(settingsXML.Element("Settings").Element("LastSong").Value);                
                count = songs.IndexOf(listOfSongs[Int32.Parse(settingsXML.Element("Settings").Element("LastSong").Value)]);
            }                        
            //path.Clear();            
            if (path.Count > 0)
            {
                auSet.Open(new Uri(songs[count]));
                SoundName.Text = elem.soundName(auSet.Source.ToString());
            }
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
            ((Border)FindName("playListsButton")).MouseEnter += elem.onMouseEnter;
            ((Border)FindName("settings")).Background = Brushes.Transparent;
            ((Border)FindName("playListsButton")).Background = Brushes.Transparent;
            ((Border)FindName("prevSound")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("play")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("nextSound")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("addDirectory")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("allMusics")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("settings")).MouseLeave += elem.onMouseLeave;
            ((Border)FindName("playListsButton")).MouseLeave += elem.onMouseLeave;                                  
            
            ///////////////////////////////           
            /////////////////////////////            
            Grid dirs = (Grid)FindName("Dirs");
            foreach (XElement dir in settingsXML.Element("Settings").Element("Dirs").Elements("dir"))
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
                deleteButtons.Add(borderToDel);
                borderToDel.PreviewMouseDown += deletePath;
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
            progressBar.PreviewMouseDown += progressBar_click;
            TextBlock progress = (TextBlock)FindName("progress");            
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += toTimer;
            timer.Start();
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock) FindName("durationTime"));
            createPlayListElement();
            getPlayListsNames();            
        }

        private void getPlayListsNames()
        {
            xmlPlayLists = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml");
            for (int i = 0; i < xmlPlayLists.Length; i++)
            {
                //XDocument xpl = XDocument.Load(xmlPlayLists[i]);
                string[] nameOfPlayList = xmlPlayLists[i].Split('\\', '/');
                if (nameOfPlayList[nameOfPlayList.Length - 1] != "settings.xml")
                {
                    createElementInGrid(nameOfPlayList[nameOfPlayList.Length - 1], i);
                }
            }
        }        
        
        private void onWindowLoad(object sender, RoutedEventArgs e)
        {
            volume.Width = auSet.Volume * volumeBar.ActualWidth;
        }
        private void progressBar_click(object sender, MouseEventArgs e)
        {
            PreviewMouseMove += progressBar_MouseMove;
            PreviewMouseUp += progressBar_MouseButtonUp;
            barObject = (TextBlock)sender;            
            double obj = ((TextBlock)sender).PointToScreen(new Point(0, 0)).X;

            var windowPosition = Mouse.GetPosition(this);
            double screenPosition = this.PointToScreen(windowPosition).X;

            double widthOfElement = ((TextBlock)sender).ActualWidth;
            if (screenPosition - obj >= 0 && screenPosition - obj <= widthOfElement)
            {
                if (barObject.Name != "volumeBar")
                {
                    auSet.Position = TimeSpan.FromSeconds(auSet.NaturalDuration.TimeSpan.TotalSeconds * (((screenPosition - obj) * 100 / widthOfElement) / 100));
                    time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
                } else
                {
                    ((TextBlock)FindName("volume")).Width = screenPosition - obj;
                    auSet.Volume = ((((screenPosition - obj) * 100) / widthOfElement) / 100);
                }
            }
        }

        private void progressBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (barObject.Name != "volumeBar")
            {
                auSet.Pause();
            }
            double obj = (barObject).PointToScreen(new Point(0, 0)).X;

            var windowPosition = Mouse.GetPosition(this);
            double screenPosition = this.PointToScreen(windowPosition).X;

            double widthOfElement = (barObject).ActualWidth;
            if (screenPosition - obj >= 0 && screenPosition - obj <= widthOfElement)
            {
                if (barObject.Name != "volumeBar")
                {
                    auSet.Position = TimeSpan.FromSeconds(auSet.NaturalDuration.TimeSpan.TotalSeconds * (((screenPosition - obj) * 100 / widthOfElement) / 100));
                    time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
                } else
                {
                    volume.Width = screenPosition - obj;
                    auSet.Volume = ((((screenPosition - obj) * 100) / widthOfElement) / 100);
                }
            }
            //time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
            PreviewMouseUp += progressBar_MouseButtonUp;
        }

        private void progressBar_MouseButtonUp(object sender, MouseEventArgs e)
        {
            PreviewMouseMove -= progressBar_MouseMove;
            if (switcher && barObject.Name != "volumeBar")
            {
                auSet.Play();
            }
            if(barObject.Name == "volumeBar")
            {                
                settingsXML.Element("Settings").Element("Volume").Value = auSet.Volume.ToString();
                settingsXML.Save("settings.xml");
            }            
            PreviewMouseUp -= progressBar_MouseButtonUp;
            //time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
        }        

        private void createElementInGrid(string nameOfPlayList, int i)
        {
            Grid playListsGrid = (Grid)FindName("pL");
            RowDefinition row = new RowDefinition();
            row.Height = new GridLength(50, GridUnitType.Pixel);
            playListsGrid.RowDefinitions.Add(row);
            Grid dirInfo = new Grid();
            Grid.SetRow(dirInfo, i);
            //ColumnDefinition dirsPath = new ColumnDefinition();
            Border toNameOfDir = new Border();
            TextBlock nameOfDir = new TextBlock();
            nameOfDir.FontSize = 17;
            nameOfDir.VerticalAlignment = VerticalAlignment.Center;
            toNameOfDir.PreviewMouseDown += changeWindow;
            toNameOfDir.Margin = new Thickness(15, 0, 0, 0);
            toNameOfDir.Padding = new Thickness(30, 10, 10, 10);
            nameOfDir.Text = nameOfPlayList;
            toNameOfDir.Cursor = Cursors.Hand;
            toNameOfDir.Child = nameOfDir;
            toNameOfDir.MouseEnter += elem.onMouseEnter;
            toNameOfDir.MouseLeave += elem.onMouseLeave;
            elem.onMouseEnter(toNameOfDir, null);
            elem.onMouseLeave(toNameOfDir, null);
            Grid.SetColumn(toNameOfDir, 0);
            dirInfo.Children.Add(toNameOfDir);
            playListsGrid.Children.Add(dirInfo);
            //MessageBox.Show(nameOfPlayList);
        }

        private void toTimer(object sender, EventArgs e)
        {
            //MessageBox.Show(((auSet.Position.TotalSeconds * 100 / auSet.NaturalDuration.TimeSpan.TotalSeconds) >= 100).ToString());
            if(auSet.NaturalDuration.HasTimeSpan && (auSet.Position.TotalSeconds == auSet.NaturalDuration.TimeSpan.TotalSeconds) && !looper) 
            {
                if (playListSwitch)
                {
                    if (playListCount == listOfSongs.Count - 1)
                    {
                        playListCount = 0;
                        count = songs.IndexOf(listOfSongs[playListCount]);
                    }
                    else
                    {
                        playListCount++;
                        count = songs.IndexOf(listOfSongs[playListCount]);
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
                for (int i = 0; i < playButtons.Count; i++)
                {
                    ((TextBlock)(playButtons[i]).Child).Text = "▶";
                }
                if (playListSwitch)
                {
                    ((TextBlock)(playButtons[count + songs.Count].Child)).Text = "❚❚";
                }
                else
                {
                    ((TextBlock)(playButtons[count].Child)).Text = "❚❚";
                }
                progress.Width = 0;
                settingsXML.Element("Settings").Element("LastSong").Value = count.ToString();
                settingsXML.Save("settings.xml");
                auSet.Play();    
            } else if(auSet.NaturalDuration.HasTimeSpan && (auSet.Position.TotalSeconds == auSet.NaturalDuration.TimeSpan.TotalSeconds) && looper)
            {
                auSet.Position = TimeSpan.FromSeconds(0);
                progress.Width = 0;
            }
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
        }
        
        private void playPause(object sender, RoutedEventArgs e)
        {
            if (!switcher)
            {
                auSet.Play();
                if (playListSwitch)
                {
                    ((TextBlock)(playButtons[playListCount + songs.Count]).Child).Text = "❚❚";
                }
                else
                {
                    ((TextBlock)(playButtons[count]).Child).Text = "❚❚";
                }                            
                switcher = true;
                timer.Start();
            } 
            else
            {
                auSet.Pause();
                if (playListSwitch)
                {
                    ((TextBlock)(playButtons[playListCount + songs.Count]).Child).Text = "▶";
                }
                else
                {
                    ((TextBlock)(playButtons[count]).Child).Text = "▶";
                }                                
                switcher = false;
                timer.Stop();                
            }
            elem.changeButton(switcher, (TextBlock)(((Border)sender).Child));
            if (!playListSwitch)
            {
                settingsXML.Element("Settings").Element("LastSong").Value = count.ToString();
            }else
            {
                settingsXML.Element("Settings").Element("LastSong").Value = playListCount.ToString();
            }
            settingsXML.Save("settings.xml");
        }

        private void AddButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (((ScrollViewer)FindName("toDirs")).Visibility == Visibility.Visible)
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();                    
                    Grid dirs = (Grid)FindName("Dirs");
                    if (result.ToString() == "OK")
                    {
                        songs.Clear();
                        path.Add(dialog.SelectedPath);
                        finder.finder(path, songs);
                        dirsList.Add(dialog.SelectedPath);
                        playList.Children.Clear();
                        playList.RowDefinitions.Clear();
                        //
                        addButtons.Clear();
                        checkBoxes.Clear();
                        playButtons.Clear();
                        //
                        createPlayListElement();
                        settingsXML.Element("Settings").Element("Dirs").Add(new XElement("dir", dialog.SelectedPath, new XAttribute("id", deleteButtons.Count)));
                        //path.Add(dialog.SelectedPath);
                        RowDefinition row = new RowDefinition();
                        row.Height = new GridLength(50, GridUnitType.Pixel);
                        dirs.RowDefinitions.Add(row);
                        Grid dirInfo = new Grid();
                        Grid.SetRow(dirInfo, dirs.RowDefinitions.Count - 1);
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
                        deleteButtons.Add(borderToDel);
                        borderToDel.PreviewMouseDown += deletePath;
                        borderToDel.Child = deleteButton;
                        elem.onMouseEnter(borderToDel, null);
                        elem.onMouseLeave(borderToDel, null);
                        Grid.SetColumn(borderToDel, 1);
                        TextBlock nameOfDir = new TextBlock();
                        nameOfDir.FontSize = 17;
                        nameOfDir.VerticalAlignment = VerticalAlignment.Center;
                        nameOfDir.Margin = new Thickness(15, 0, 0, 0);
                        //nameOfDir.HorizontalAlignment = HorizontalAlignment.Center;
                        nameOfDir.Text = dialog.SelectedPath;
                        //selectPath.Add(dialog.SelectedPath);
                        Grid.SetColumn(nameOfDir, 0);
                        dirInfo.Children.Add(nameOfDir);
                        dirInfo.Children.Add(borderToDel);
                        dirs.Children.Add(dirInfo);
                        //finder.finder(path, songs);
                        //createPlayListElement();
                    }
                    settingsXML.Save("settings.xml");
                    //MessageBox.Show(dialog.SelectedPath);
                }
            } else if(((ScrollViewer)FindName("toSounds")).Visibility == Visibility.Visible)
            {
                addInPlayList addInPlayListWindow = new addInPlayList();                
                if (addButtons.Contains((Border)sender))
                {
                    selectSongBlocks.Add(checkBoxes[addButtons.IndexOf((Border)sender)]);
                }
                //playListSwitch = false;
                //songs = allSongsFromDirs;
                addInPlayListWindow.listWriter(checkBoxes, selectSongBlocks, songs);
                addInPlayListWindow.Top = ((Border)sender).PointToScreen(new Point(0, 0)).Y + ((Border)sender).ActualHeight;
                addInPlayListWindow.Left = ((Border)sender).PointToScreen(new Point(0, 0)).X - (addInPlayListWindow.Width / 2);
                selectSongBlocks.Clear();
                addInPlayListWindow.ShowDialog();               
                /*if (((ScrollViewer)((StackPanel)FindName("playLists")).Parent).Visibility == Visibility.Visible)
                {
                    ((ScrollViewer)((StackPanel)FindName("playLists")).Parent).Visibility = Visibility.Hidden;
                }
                else
                {
                    ((ScrollViewer)((StackPanel)FindName("playLists")).Parent).Visibility = Visibility.Visible;
                }*/
            } else
            {
                createPlayList createPlayListWindow = new createPlayList();                
                createPlayListWindow.Top = ((Border)sender).PointToScreen(new Point(0, 0)).Y + ((Border)sender).ActualHeight;
                createPlayListWindow.Left = ((Border)sender).PointToScreen(new Point(0, 0)).X - (createPlayListWindow.Width / 2);
                createPlayListWindow.writer(((Grid)FindName("pL")).RowDefinitions.Count);
                createPlayListWindow.Owner = this;
                createPlayListWindow.ShowDialog();
                nameOfPlayList = "";
                pL.Children.Clear();
                pL.RowDefinitions.Clear();
                getPlayListsNames();
                
                /*if (((StackPanel)FindName("createForm")).Visibility == Visibility.Visible)
                {
                    ((StackPanel)FindName("createForm")).Visibility = Visibility.Hidden;
                }
                else
                {
                    ((StackPanel)FindName("createForm")).Visibility = Visibility.Visible;
                }*/
            }
        }

        private void deletePath(object sender, RoutedEventArgs e)
        {               
            foreach (XElement dir in settingsXML.Element("Settings").Element("Dirs").Elements("dir"))
            {                
                if (dir.Value == dirsList[deleteButtons.IndexOf((Border)sender)])
                {
                    dir.Remove();
                    for (int i = 0; i < Dirs.Children.Count; i++)
                    {
                        if (i == Grid.GetRow(Dirs.Children[i]) && i == deleteButtons.IndexOf((Border)sender))
                        {
                            Dirs.Children.Remove(Dirs.Children[i]);
                        }
                    }
                    for (int i = 0; i < Dirs.RowDefinitions.Count; i++)
                    {
                        if (i == deleteButtons.IndexOf((Border)sender))
                        {
                            Dirs.RowDefinitions.RemoveAt(i);
                        }
                    }
                }
            }
            path.Remove(dirsList[deleteButtons.IndexOf((Border)sender)]);
            songs.Clear();
            playList.RowDefinitions.Clear();
            playList.Children.Clear();
            finder.finder(path, songs);
            dirsList.RemoveAt(deleteButtons.IndexOf((Border)sender));
            deleteButtons.Remove((Border)sender);
            //MessageBox.Show(path.Count.ToString());
            createPlayListElement();
            settingsXML.Save("settings.xml");
            auSet.Pause();
            if(count >= songs.Count && path.Count > 0)
            {
                count = 0;
                auSet.Open(new Uri(songs[count]));
                SoundName.Text = elem.soundName(songs[count]);
                if(switcher)
                {
                    auSet.Play();
                }
            }
            //deleteButtons.IndexOf((Border)sender);
        }

        public void createPlayListElement()
        {
            if (playList.Children.Count == 0)
            {
                for (int i = 0; i < songs.Count; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(50, GridUnitType.Pixel);
                    playList.RowDefinitions.Add(row);
                    Grid song = new Grid();                    
                    GridLength playWidth = new GridLength(0.2, GridUnitType.Star);
                    ColumnDefinition toCheck = new ColumnDefinition();
                    toCheck.Width = playWidth;
                    CheckBox selectSong = new CheckBox { IsChecked = false, IsThreeState = false, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(15, 0, 0, 0) };
                    checkBoxes.Add(selectSong);
                    //TextBlock selectSong = new TextBlock();
                    //selectSong.FontSize = 17;
                    selectSong.Click += checkBoxClick;
                    Grid.SetColumn(selectSong, 0);
                    song.Children.Add(selectSong);
                    ColumnDefinition name = new ColumnDefinition();
                    GridLength nameWidth = new GridLength(2, GridUnitType.Star);
                    name.Width = nameWidth;
                    // Song Name
                    TextBlock songName = new TextBlock();
                    songsNames.Add(songName);
                    songName.Text = elem.soundName(songs[i]);
                    songName.FontSize = 17;
                    songName.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(songName, 1);
                    song.Children.Add(songName);
                    // Play Button
                    ColumnDefinition play = new ColumnDefinition();
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
                    Grid.SetColumn(playButton, 2);
                    song.Children.Add(playButton);
                    ColumnDefinition add = new ColumnDefinition();
                    add.Width = playWidth;
                    Border addButton = new Border();
                    addButton.MouseEnter += elem.onMouseEnter;
                    addButton.MouseLeave += elem.onMouseLeave;
                    playButton.BorderThickness = new Thickness(0);
                    TextBlock addButtonBlock = new TextBlock();
                    addButton.BorderThickness = new Thickness(0);
                    addButtonBlock.VerticalAlignment = VerticalAlignment.Center;
                    addButtonBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    addButton.Cursor = Cursors.Hand;
                    addButtonBlock.Text = "+";
                    addButtonBlock.FontSize = 17;
                    addButton.Child = addButtonBlock;
                    addButton.PreviewMouseDown += AddButtonMouseDown;
                    addButtons.Add(addButton);
                    elem.onMouseEnter(addButton, null);
                    elem.onMouseLeave(addButton, null);
                    Grid.SetColumn(addButton, 3);
                    song.Children.Add(addButton);
                    //ColumnDefinition blank = new ColumnDefinition();              

                    //
                    song.ColumnDefinitions.Add(toCheck);
                    song.ColumnDefinitions.Add(name);
                    song.ColumnDefinitions.Add(play);
                    song.ColumnDefinitions.Add(add);
                    //song.ColumnDefinitions.Add(blank);                
                    Grid.SetRow(song, i);
                    playList.Children.Add(song);
                }
            }
            if(playListSwitch)
            {
                //songsInPlayListGrid.Children.Clear();                
                for (int i = 0; i < listOfSongs.Count; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(50, GridUnitType.Pixel);
                    songsInPlayListGrid.RowDefinitions.Add(row);
                    Grid song = new Grid();
                    //songGrid.Add(song);
                    GridLength playWidth = new GridLength(0.2, GridUnitType.Star);
                    ColumnDefinition toCheck = new ColumnDefinition();
                    toCheck.Width = playWidth;
                    //TextBlock selectSong = new TextBlock();
                    //selectSong.FontSize = 17;
                    ColumnDefinition name = new ColumnDefinition();
                    GridLength nameWidth = new GridLength(2, GridUnitType.Star);
                    name.Width = nameWidth;
                    // Song Name
                    TextBlock songName = new TextBlock();
                    songsNames.Add(songName);
                    songName.Text = elem.soundName(listOfSongs[i]);
                    songName.FontSize = 17;
                    songName.VerticalAlignment = VerticalAlignment.Center;
                    Grid.SetColumn(songName, 1);
                    song.Children.Add(songName);
                    // Play Button
                    ColumnDefinition play = new ColumnDefinition();
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
                    Grid.SetColumn(playButton, 2);
                    song.Children.Add(playButton);
                    ColumnDefinition add = new ColumnDefinition();
                    add.Width = playWidth;
                    Border addButton = new Border();
                    addButton.MouseEnter += elem.onMouseEnter;
                    addButton.MouseLeave += elem.onMouseLeave;
                    playButton.BorderThickness = new Thickness(0);
                    TextBlock addButtonBlock = new TextBlock();
                    addButton.BorderThickness = new Thickness(0);
                    addButtonBlock.VerticalAlignment = VerticalAlignment.Center;
                    addButtonBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    addButton.Cursor = Cursors.Hand;
                    addButtonBlock.Text = "-";
                    addButtonBlock.FontSize = 17;
                    addButton.Child = addButtonBlock;
                    addButton.PreviewMouseDown += AddButtonMouseDown;                    
                    elem.onMouseEnter(addButton, null);
                    elem.onMouseLeave(addButton, null);
                    Grid.SetColumn(addButton, 3);
                    song.Children.Add(addButton);
                    //ColumnDefinition blank = new ColumnDefinition();              

                    //
                    song.ColumnDefinitions.Add(toCheck);
                    song.ColumnDefinitions.Add(name);
                    song.ColumnDefinitions.Add(play);
                    song.ColumnDefinitions.Add(add);
                    //song.ColumnDefinitions.Add(blank);                
                    Grid.SetRow(song, i);
                    songsInPlayListGrid.Children.Add(song);
                }
                playListSwitch = false;
            }
        }
        
        private void playButton_click(object sender, RoutedEventArgs e)
        {            
            if (((TextBlock)((Border)sender).Child).Text != "❚❚")
            {                
                if(playButtons.IndexOf((Border)sender) < songs.Count)
                {
                    playListSwitch = false;
                } else
                {
                    playListSwitch = true;
                }
                if (!playListSwitch && count != playButtons.IndexOf((Border)sender))
                {
                    switcher = false;
                    playListCount = -1;
                    count = playButtons.IndexOf((Border)sender);
                    settingsXML.Element("Settings").Element("LastSong").Attribute("playList").Value = "none";
                    settingsXML.Save("settings.xml");
                    auSet.Open(new Uri(songs[count]));
                    progress.Width = 0;
                    SoundName.Text = elem.soundName(auSet.Source.ToString());
                    time.timeToSlider(auSet, progressBar, progress, currentTime, durationTime);
                }
                else if(playListSwitch && playListCount != playButtons.IndexOf((Border)sender) - songs.Count)
                {                    
                    switcher = false;
                    playListCount = playButtons.IndexOf((Border)sender) - songs.Count;
                    count = songs.IndexOf(listOfSongs[playButtons.IndexOf((Border)sender) - songs.Count]);                    
                    settingsXML.Element("Settings").Element("LastSong").Attribute("playList").Value = nameOfPlayList;
                    settingsXML.Save("settings.xml");
                    auSet.Open(new Uri(songs[count]));
                    progress.Width = 0;
                    SoundName.Text = elem.soundName(auSet.Source.ToString());
                    time.timeToSlider(auSet, progressBar, progress, currentTime, durationTime);
                }                
            }
            for (int i = 0; i < playButtons.Count; i++)
            {
                ((TextBlock)(playButtons[i]).Child).Text = "▶";
            }
            //((TextBlock)(playButtons[count].Child)).Text = "❚❚";
            ((TextBlock)((Border)sender).Child).Text = "❚❚";

            playPause(play, null);                                    
        }
        
        private void checkBoxClick(object sender, RoutedEventArgs e)
        {
            if (!selectSongBlocks.Contains((CheckBox)sender))
            {
                selectSongBlocks.Add((CheckBox)sender);
            }
            else
            {
                selectSongBlocks.Remove((CheckBox)sender);
            }
            //MessageBox.Show(songs[selectSongBlocks.IndexOf((CheckBox)sender)]);
        }

        private void loopButton_click(object sender, RoutedEventArgs e)
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
        
        private void changeSounds(object sender, RoutedEventArgs e)
        {
            if(((Border)sender).Name == "prevSound")
            {
                if (playListSwitch)
                {
                    if (playListCount == 0)
                    {
                        playListCount = listOfSongs.Count - 1;
                        count = songs.IndexOf(listOfSongs[playListCount]);
                    }
                    else
                    {
                        playListCount--;
                        count = songs.IndexOf(listOfSongs[playListCount]);
                    }
                } else
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
            } 
            else
            {
                if (playListSwitch)
                {
                    if(playListCount == listOfSongs.Count - 1) 
                    {
                        playListCount = 0;
                        count = songs.IndexOf(listOfSongs[playListCount]);
                    }
                    else
                    {
                        playListCount++;
                        count = songs.IndexOf(listOfSongs[playListCount]);
                    }
                } else
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
            }
            for (int i = 0; i < playButtons.Count; i++)
            {
                ((TextBlock)(playButtons[i]).Child).Text = "▶";
            }
            if(switcher)
            {
                ((TextBlock)(playButtons[count]).Child).Text = "❚❚";
            }            
            auSet.Open(new Uri(songs[count]));
            ((TextBlock)FindName("SoundName")).Text = elem.soundName(auSet.Source.ToString());
            progress.Width = 0;
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
            if(switcher)
            {
                auSet.Play();
            }
            settingsXML.Element("Settings").Element("LastSong").Value = count.ToString();
            settingsXML.Save("settings.xml");
        }

        private void changeWindow(object sender, RoutedEventArgs e)
        {            
            if (sender != null)
            {                
                switch (((Border)sender).Name)
                {
                    case "allMusics":
                        page = 0;
                        break;
                    case "settings":
                        page = 1;
                        break;
                    case "playListsButton":
                        page = 2;
                        break;
                    case "":
                        page = 3;
                        break;
                }
            }

            switch(page)
            {
                case 0:
                    toSounds.Visibility = Visibility.Visible;
                    toDirs.Visibility = Visibility.Hidden;
                    toPlayLists.Visibility = Visibility.Hidden;
                    playListInterface.Visibility = Visibility.Hidden;
                    songsInPlayList.Visibility = Visibility.Hidden;
                    addDirectory.Visibility = Visibility.Visible;
                    break;
                case 1:
                    toSounds.Visibility = Visibility.Hidden;
                    toDirs.Visibility = Visibility.Visible;
                    toPlayLists.Visibility = Visibility.Hidden;
                    playListInterface.Visibility = Visibility.Hidden;
                    songsInPlayList.Visibility = Visibility.Hidden;
                    addDirectory.Visibility = Visibility.Visible;
                    break;
                case 2:
                    toSounds.Visibility = Visibility.Hidden;
                    toDirs.Visibility = Visibility.Hidden;
                    toPlayLists.Visibility = Visibility.Visible;
                    playListInterface.Visibility = Visibility.Hidden;
                    songsInPlayList.Visibility = Visibility.Hidden;
                    addDirectory.Visibility = Visibility.Visible;
                    break;
                case 3:                    
                    toSounds.Visibility = Visibility.Hidden;
                    toDirs.Visibility = Visibility.Hidden;
                    toPlayLists.Visibility = Visibility.Hidden;
                    playListInterface.Visibility = Visibility.Visible;
                    songsInPlayList.Visibility = Visibility.Visible;
                    if (((TextBlock)((Border)sender).Child).Text != nameOfPlayList)
                    {
                        songsInPlayListGrid.Children.Clear();
                        songsInPlayListGrid.RowDefinitions.Clear();
                        playListSwitch = true;
                        XDocument playListXML = XDocument.Load($"{((TextBlock)((Border)sender).Child).Text}");
                        foreach (XElement song in playListXML.Element("Songs").Elements("song"))
                        {
                            listOfSongs.Add(song.Value);
                        }
                        nameOfPlayList = ((TextBlock)((Border)sender).Child).Text;
                        createPlayListElement();
                    }
                    playListName.Text = nameOfPlayList;
                    addDirectory.Visibility = Visibility.Hidden;
                    break;
            }
            if(page == 3)
            {
                page = 2;
            }
            settingsXML.Element("Settings").Element("Page").Value = page.ToString();
            settingsXML.Save("settings.xml");
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
    
}
