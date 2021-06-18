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
using System.Media;
using YoutubeExplode;
using YoutubeExplode.Converter;
using VideoLibrary;
using MediaToolkit.Model;
using MediaToolkit;

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
        public List<CheckBox> selectSongBlocks = new List<CheckBox>();
        List<CheckBox> checkBoxes = new List<CheckBox>();
        List<TextBlock> songsNames = new List<TextBlock>();
        List<Border> playlists = new List<Border>();
        List<TextBlock> textBlocksToPlayBlock = new List<TextBlock>();
        List<Border> addButtons = new List<Border>();
        List<string> dirsList = new List<string>();
        List<string> listOfSongs = new List<string>();
        List<string> forCreateGrid = new List<string>();
        bool changePlayList = false;        
        bool playListSwitch = false;
        int looper = 0;
        string nameOfPlayList = "";
        int playListCount = 0;
        TextBlock barObject = null;
        public XDocument settingsXML = null;
        int page = 0;
        public MainWindow()
        {
            InitializeComponent();                
            // Creates a XML file of settings if his not exist
            if(!File.Exists("settings.xml"))
            {
                XDocument createXML = new XDocument(new XElement("Settings", new XElement("Dirs", ""), new XElement("LastSong", new XAttribute("playList", "none"), new XAttribute("position", 0), 0), new XElement("Volume", "1"), new XElement("loopMode", 0), new XElement("Page", 0), new XElement("Playlists", "")));
                createXML.Save("settings.xml");
            }
            //
            // Create directory with download musics
            if(!Directory.Exists("Download"))
            {
                Directory.CreateDirectory("Download");
            }
            path.Add($"{Environment.CurrentDirectory}\\Download\\");
            //
            // Gets info about last song
            settingsXML = XDocument.Load("settings.xml");
            page = Int32.Parse(settingsXML.Element("Settings").Element("Page").Value);            
            changeWindow(null, null);
            auSet.Volume = Double.Parse(settingsXML.Element("Settings").Element("Volume").Value);
            looper = Int32.Parse(settingsXML.Element("Settings").Element("loopMode").Value);
            loopButton.SelectedIndex = looper;            
            this.Loaded += onWindowLoad;
            this.Closed += onWindowClose;            
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
                    forCreateGrid.Add(song.Value);
                }                
                playListCount = Int32.Parse(settingsXML.Element("Settings").Element("LastSong").Value);                
                count = playListCount;
            }                   
            if (path.Count > 0)
            {
                if (playListSwitch)
                {
                    auSet.Open(new Uri(listOfSongs[count]));                    
                }
                else
                {
                    auSet.Open(new Uri(songs[count]));
                }
                SoundName.Text = elem.soundName(auSet.Source.ToString());
            }
            //
            // Add animation on buttons
            prevSound.MouseEnter += elem.onMouseEnter;
            play.MouseEnter += elem.onMouseEnter;
            nextSound.MouseEnter += elem.onMouseEnter;
            addDirectory.MouseEnter += elem.onMouseEnter;
            playListStart.MouseEnter += elem.onMouseEnter;
            elem.onMouseEnter(playListStart, null);
            addDirectory.Background = Brushes.Transparent;
            allMusics.MouseEnter += elem.onMouseEnter;
            allMusics.Background = Brushes.Transparent;
            settings.MouseEnter += elem.onMouseEnter;
            playListsButton.MouseEnter += elem.onMouseEnter;
            settings.Background = Brushes.Transparent;
            playListsButton.Background = Brushes.Transparent;
            prevSound.MouseLeave += elem.onMouseLeave;
            play.MouseLeave += elem.onMouseLeave;
            nextSound.MouseLeave += elem.onMouseLeave;
            playListStart.MouseLeave += elem.onMouseLeave;
            elem.onMouseLeave(playListStart, null);
            addDirectory.MouseLeave += elem.onMouseLeave;
            allMusics.MouseLeave += elem.onMouseLeave;
            settings.MouseLeave += elem.onMouseLeave;
            playListsButton.MouseLeave += elem.onMouseLeave;
            progressBar.PreviewMouseDown += progressBar_click;
            playListMenu.Background = Brushes.Gray;
            playListMenu.MouseEnter += elem.onMouseEnter;
            playListMenu.MouseLeave += elem.onMouseLeave;
            DownloadSongOnGeneral.MouseEnter += elem.onMouseEnter;
            DownloadSongOnGeneral.MouseLeave += elem.onMouseLeave;
            DownloadSongOnGeneral.Background = Brushes.Transparent;
            loopButton.SelectionChanged += loopMenu;
            //                        
            // Gets info about attached files and creates on settings page
            foreach (XElement dir in settingsXML.Element("Settings").Element("Dirs").Elements("dir"))
            {
                dirsList.Add(dir.Value);
            }
            for (int i = 0; i < dirsList.Count; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(50, GridUnitType.Pixel);
                Dirs.RowDefinitions.Add(row);
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
                deleteButton.Text = "×";                
                deleteButton.FontWeight = FontWeights.Bold;
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
                Dirs.Children.Add(dirInfo);
            }            
            //            
            // Timer settings
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += toTimer;
            timer.Start();
            //
            // Creates elements in default playlist, in list of playList
            time.timeToSlider(auSet, progressBar, progress, currentTime, durationTime);
            createPlayListElement();
            getPlayListsNames();
            //
            if (nameOfPlayList != "") {
                textBlocksToPlayBlock[playListCount + songs.Count].Text = "[Play]";
            } else
            {
                textBlocksToPlayBlock[count].Text = "[Play]";
            }
            PreviewKeyDown += keyDown;            
        }       

        // Gets play lists
        public void getPlayListsNames()
        {            
            foreach(XElement playlist in settingsXML.Element("Settings").Element("Playlists").Elements("playlist"))
            {                
                createElementInGrid(playlist.Value, pL.RowDefinitions.Count);                
            }            
        }
        //        

        public void playListMenuButtonClick(object sender, RoutedEventArgs e)
        {            
            switch (((Border)sender).Name) {
                case "playListMenu":
                    playlistGeneralMenu playlistGeneralMenu = new playlistGeneralMenu();
                    playlistGeneralMenu.Top = ((Border)sender).PointToScreen(new Point(0, 0)).Y + ((Border)sender).ActualHeight;
                    playlistGeneralMenu.Left = ((Border)sender).PointToScreen(new Point(0, 0)).X - (playlistGeneralMenu.Width / 2);                    
                    playlistGeneralMenu.ShowDialog();
                    break;
                case "addSongs":
                case "DownloadSongOnGeneral":
                    playListMenu playListMenuWindow = new playListMenu();
                    playListMenuWindow.Top = ((Border)sender).PointToScreen(new Point(0, 0)).Y + ((Border)sender).ActualHeight;
                    playListMenuWindow.Left = ((Border)sender).PointToScreen(new Point(0, 0)).X - (playListMenuWindow.Width / 2);                    
                    playListMenuWindow.ShowDialog();
                    break;
            }
        }

        // Settings for main window
        private void onWindowClose(object sender, EventArgs e)
        {
            auSet.Close();
            if (!playListSwitch)
            {
                settingsXML.Element("Settings").Element("LastSong").Value = count.ToString();
            }
            else
            {
                settingsXML.Element("Settings").Element("LastSong").Value = playListCount.ToString();
            }
            settingsXML.Element("Settings").Element("LastSong").Attribute("position").Value = auSet.Position.TotalSeconds.ToString();
            settingsXML.Element("Settings").Element("loopMode").Value = looper.ToString();
            settingsXML.Element("Settings").Element("Page").Value = page.ToString();            
            settingsXML.Save("settings.xml");
        }

        private void onWindowLoad(object sender, RoutedEventArgs e)
        {            
            volume.Width = auSet.Volume * volumeBar.ActualWidth;
            auSet.Position = TimeSpan.FromSeconds(Double.Parse(settingsXML.Element("Settings").Element("LastSong").Attribute("position").Value));
            if (auSet.NaturalDuration.HasTimeSpan)
            {
                progress.Width = progressBar.ActualWidth * ((auSet.Position.TotalSeconds * 100 / auSet.NaturalDuration.TimeSpan.TotalSeconds) / 100);
            }
        }      
        //

        // Download musics from youtube
        public async void DownloadMusic(string videoPath)
        {
            var source = @"Download\";
            var youtube = YouTube.Default;
            var vid = await Task.Run(() => youtube.GetVideo(videoPath));
            await Task.Run(() => File.WriteAllBytes(source + vid.FullName, vid.GetBytes()));

            var inputFile = new MediaFile { Filename = source + vid.FullName };
            string[] nameOfOutputFile = vid.FullName.Split('.');            
            var outputFile = new MediaFile { Filename = $"{source + nameOfOutputFile[0]}.mp3" };
            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                engine.Convert(inputFile, outputFile);
            }
            await Task.Run(() => File.Delete($"Download\\{vid.FullName}"));
            if(page == 2)
            {                
                XDocument playListToDownload = XDocument.Load($"{playListName.Text}");
                playListToDownload.Element("Songs").Add(new XElement("song", $"{Environment.CurrentDirectory}\\Download\\{nameOfOutputFile[0]}.mp3"));
                playListToDownload.Save($"{playListName.Text}");                
                for (int i = 0; i < playlists.Count; i++)
                {
                    if (((TextBlock)playlists[i].Child).Text == $"{playListName.Text}")
                    {
                        await Task.Run(() => changeWindow(playlists[i], null));
                    }
                }
            }
        }        
        //

        // Drag and drop (progress, volume)
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
                    time.timeToSlider(auSet, progressBar, progress, currentTime, durationTime);
                } else
                {
                    volume.Width = screenPosition - obj;
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

        //

        private void startPlayList(object sender, RoutedEventArgs e)
        {
            count = 0;
            playListCount = 0;
            playListSwitch = true;
            if(nameOfPlayList != playListName.Text)
            {
                listOfSongs.Clear();
                nameOfPlayList = playListName.Text;
                XDocument playListXML = XDocument.Load($"{nameOfPlayList}");
                foreach (XElement song in playListXML.Element("Songs").Elements("song"))
                {
                    listOfSongs.Add(song.Value);                    
                }
            }            
            auSet.Open(new Uri(listOfSongs[count]));
            SoundName.Text = elem.soundName(auSet.Source.ToString());
            for (int i = 0; i < playButtons.Count; i++)
            {
                ((TextBlock)(playButtons[i].Child)).Text = "▶";
            }
            ((TextBlock)playButtons[songs.Count].Child).Text = "❚❚";
            for (int i = 0; i < textBlocksToPlayBlock.Count; i++)
            {
                textBlocksToPlayBlock[i].Text = "";
            }
            textBlocksToPlayBlock[songs.Count].Text = "[Play]";
            if (!switcher)
            {
                playPause(play, null);
            }
            else
            {
                auSet.Play();
            }
        }

        // Creates elements in grid of playlists
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
            playlists.Add(toNameOfDir);
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
        //

        // Creates elements in default playlist and another play list
        public void createPlayListElement()
        {
            if (playList.Children.Count == 0)
            {
                for (int i = 0; i < songs.Count; i++)
                {
                    // Creates row in Grid and set her settings
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(50, GridUnitType.Pixel);
                    playList.RowDefinitions.Add(row);
                    //
                    // 
                    Grid song = new Grid();
                    GridLength playWidth = new GridLength(0.2, GridUnitType.Star);
                    // Creates checkBoxes in default playlist
                    ColumnDefinition toCheck = new ColumnDefinition();
                    toCheck.Width = playWidth;
                    CheckBox selectSong = new CheckBox { IsChecked = false, IsThreeState = false, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(15, 0, 0, 0) };
                    checkBoxes.Add(selectSong);
                    selectSong.Click += checkBoxClick;
                    Grid.SetColumn(selectSong, 0);
                    //
                    // Creates column with name
                    ColumnDefinition name = new ColumnDefinition();
                    GridLength nameWidth = new GridLength(2, GridUnitType.Star);
                    name.Width = nameWidth;
                    StackPanel toName = new StackPanel() { Orientation = Orientation.Horizontal };
                    Grid.SetColumn(toName, 1);
                    // Creates blocks for selected song which play now
                    TextBlock blockToPlay = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.Red };
                    textBlocksToPlayBlock.Add(blockToPlay);
                    blockToPlay.FontSize = 17;
                    toName.Children.Add(blockToPlay);
                    // 
                    // Creates block to song name
                    TextBlock songName = new TextBlock();
                    songsNames.Add(songName);
                    songName.Text = elem.soundName(songs[i]);
                    songName.FontSize = 17;
                    songName.VerticalAlignment = VerticalAlignment.Center;
                    toName.Children.Add(songName);
                    //

                    // Creates play buttons
                    ColumnDefinition play = new ColumnDefinition();
                    play.Width = playWidth;
                    Border playButton = new Border();
                    playButton.BorderThickness = new Thickness(0);
                    TextBlock playButtonBlock = new TextBlock();
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
                    //
                    // Creates buttons which add song in playlists
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
                    //
                    // Add all button in created grid
                    song.ColumnDefinitions.Add(toCheck);
                    song.ColumnDefinitions.Add(name);
                    song.ColumnDefinitions.Add(play);
                    song.ColumnDefinitions.Add(add);
                    song.Children.Add(selectSong);
                    song.Children.Add(toName);
                    song.Children.Add(playButton);
                    song.Children.Add(addButton);
                    Grid.SetRow(song, i);
                    //
                    // Add created grid in default grid
                    playList.Children.Add(song);
                }
            }
            if (playListSwitch)
            {
                for (int i = 0; i < forCreateGrid.Count; i++)
                {
                    // Creates row in Grid and set her settings
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(50, GridUnitType.Pixel);
                    songsInPlayListGrid.RowDefinitions.Add(row);
                    //
                    Grid song = new Grid();
                    GridLength playWidth = new GridLength(0.2, GridUnitType.Star);
                    // Creates column without any value
                    ColumnDefinition toCheck = new ColumnDefinition();
                    toCheck.Width = playWidth;
                    // Creates column with name
                    ColumnDefinition name = new ColumnDefinition();
                    GridLength nameWidth = new GridLength(2, GridUnitType.Star);
                    name.Width = nameWidth;
                    StackPanel toName = new StackPanel() { Orientation = Orientation.Horizontal };
                    Grid.SetColumn(toName, 1);
                    // Creates blocks for selected song which play now
                    TextBlock blockToPlay = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.Red};
                    textBlocksToPlayBlock.Add(blockToPlay);
                    blockToPlay.FontSize = 17;
                    toName.Children.Add(blockToPlay);
                    //
                    // Creates block to song name
                    TextBlock songName = new TextBlock();
                    songsNames.Add(songName);
                    songName.Text = elem.soundName(forCreateGrid[i]);
                    songName.FontSize = 17;
                    songName.VerticalAlignment = VerticalAlignment.Center;
                    toName.Children.Add(songName);
                    //
                    // Creates play buttons
                    ColumnDefinition play = new ColumnDefinition();
                    play.Width = playWidth;
                    Border playButton = new Border();
                    playButton.BorderThickness = new Thickness(0);
                    TextBlock playButtonBlock = new TextBlock();
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
                    //
                    // Creates buttons which delete song from playlist
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
                    addButtonBlock.Text = "×";
                    addButtonBlock.FontSize = 17;
                    addButtonBlock.FontWeight = FontWeights.Bold;
                    addButton.Child = addButtonBlock;
                    addButton.PreviewMouseDown += deleteSongFromPlayList;
                    addButtons.Add(addButton);
                    elem.onMouseEnter(addButton, null);
                    elem.onMouseLeave(addButton, null);
                    Grid.SetColumn(addButton, 3);
                    //
                    // Add all button in created grid
                    song.ColumnDefinitions.Add(toCheck);
                    song.ColumnDefinitions.Add(name);
                    song.ColumnDefinitions.Add(play);
                    song.ColumnDefinitions.Add(add);
                    song.Children.Add(toName);
                    song.Children.Add(playButton);
                    song.Children.Add(addButton);
                    Grid.SetRow(song, i);
                    //
                    // Add created grid in grid of playlist
                    songsInPlayListGrid.Children.Add(song);
                }
                //playListSwitch = false;
            }
        }
        //

        // Settings to timer (change music, change time)
        private void toTimer(object sender, EventArgs e)
        {
            if (auSet.NaturalDuration.HasTimeSpan && (auSet.Position.TotalSeconds == auSet.NaturalDuration.TimeSpan.TotalSeconds))
            {
                switch (looper)
                {
                    case 0:
                        if (playListSwitch)
                        {
                            if (playListCount < listOfSongs.Count - 1)
                            {
                                playListCount++;
                                auSet.Open(new Uri(listOfSongs[playListCount]));
                                auSet.Play();
                                //count = playListCount;
                            }
                            else
                            {
                                playPause(play, null);
                                playListCount = -1;                                
                                //MessageBox.Show($"{playListCount}, {playListSwitch}");
                                //playPause(play, null);
                            }
                        }
                        else
                        {
                            if (count < (songs.Count - 1))
                            {
                                count++;
                                auSet.Open(new Uri(songs[count]));
                                auSet.Play();
                            }
                            else
                            {
                                playPause(play, null);
                                count = -1;
                            }
                        }                        
                        break;
                    case 1:
                        if (playListSwitch)
                        {
                            if (playListCount == listOfSongs.Count - 1)
                            {
                                playListCount = 0;
                                //count = playListCount;
                            }
                            else
                            {
                                playListCount++;
                                //count = playListCount;
                            }
                            auSet.Open(new Uri(listOfSongs[playListCount]));
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
                            auSet.Open(new Uri(songs[count]));
                        }
                        progress.Width = 0;
                        auSet.Play();
                        break;
                    case 2:
                        auSet.Position = TimeSpan.FromSeconds(0);
                        progress.Width = 0;
                        auSet.Play();
                        break;
                }                             
                SoundName.Text = elem.soundName(auSet.Source.ToString());
                for (int i = 0; i < playButtons.Count; i++)
                {
                    ((TextBlock)(playButtons[i]).Child).Text = "▶";
                }
                for (int i = 0; i < textBlocksToPlayBlock.Count; i++)
                {
                    textBlocksToPlayBlock[i].Text = "";
                }
                if (playListSwitch && nameOfPlayList == playListName.Text)
                {
                    ((TextBlock)(playButtons[playListCount + songs.Count].Child)).Text = "❚❚";
                    textBlocksToPlayBlock[playListCount + songs.Count].Text = "[Play]";
                }
                else if(!playListSwitch)
                {
                    ((TextBlock)(playButtons[count].Child)).Text = "❚❚";
                    textBlocksToPlayBlock[count].Text = "[Play]";
                }                               
            }
            time.timeToSlider(auSet, progressBar, progress, currentTime, durationTime);
        }
        //

        private void playPause(object sender, RoutedEventArgs e)
        {            
            if (!switcher)
            {                
                auSet.Play();
                if (playListSwitch && nameOfPlayList == playListName.Text)
                {
                    ((TextBlock)(playButtons[playListCount + songs.Count]).Child).Text = "❚❚";
                    textBlocksToPlayBlock[playListCount + songs.Count].Text = "[Play]";
                }
                else if(!playListSwitch)
                {
                    ((TextBlock)(playButtons[count]).Child).Text = "❚❚";
                    textBlocksToPlayBlock[count].Text = "[Play]";
                }                            
                switcher = true;
                timer.Start();
            } 
            else
            {                
                auSet.Pause();
                if (playListSwitch && nameOfPlayList == playListName.Text)
                {
                    ((TextBlock)(playButtons[playListCount + songs.Count]).Child).Text = "▶";                    
                }
                else if(!playListSwitch)
                {
                    ((TextBlock)(playButtons[count]).Child).Text = "▶";                    
                }                                
                switcher = false;
                timer.Stop();
                time.timeToSlider(auSet, progressBar, progress, currentTime, durationTime);                
            }
            elem.changeButton(switcher, (TextBlock)(((Border)sender).Child));            
        }
        // Add in play list, add directory, create play list
        private void AddButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (toDirs.Visibility == Visibility.Visible)
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
            } else if(toSounds.Visibility == Visibility.Visible)
            {
                addInPlayList addInPlayListWindow = new addInPlayList();                
                if (addButtons.Contains((Border)sender))
                {
                    selectSongBlocks.Add(checkBoxes[addButtons.IndexOf((Border)sender)]);
                }
                addInPlayListWindow.listWriter(checkBoxes, selectSongBlocks, songs);                
                addInPlayListWindow.Top = ((Border)sender).PointToScreen(new Point(0, 0)).Y + ((Border)sender).ActualHeight;
                addInPlayListWindow.Left = ((Border)sender).PointToScreen(new Point(0, 0)).X - (addInPlayListWindow.Width / 2);
                //selectSongBlocks.Clear();
                addInPlayListWindow.ShowDialog();              
            } else
            {
                createPlayList createPlayListWindow = new createPlayList();                
                createPlayListWindow.Top = ((Border)sender).PointToScreen(new Point(0, 0)).Y + ((Border)sender).ActualHeight;
                createPlayListWindow.Left = ((Border)sender).PointToScreen(new Point(0, 0)).X - (createPlayListWindow.Width / 2);
                createPlayListWindow.writer(pL.RowDefinitions.Count);
                createPlayListWindow.Owner = this;
                createPlayListWindow.ShowDialog();
                //nameOfPlayList = "";                           
            }
        }
        //
        // Delete directories from where gets songs
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
        //
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
                    for (int i = 0; i < textBlocksToPlayBlock.Count; i++)
                    {
                        textBlocksToPlayBlock[i].Text = "";
                    }
                }
                else if(playListSwitch && playListCount != playButtons.IndexOf((Border)sender) - songs.Count)
                {                    
                    switcher = false;
                    if(changePlayList)
                    {
                        listOfSongs.Clear();
                        XDocument playListXML = XDocument.Load($"{playListName.Text}");
                        foreach (XElement song in playListXML.Element("Songs").Elements("song"))
                        {
                            listOfSongs.Add(song.Value);
                        }
                        changePlayList = false;                        
                    }
                    nameOfPlayList = playListName.Text;
                    playListCount = playButtons.IndexOf((Border)sender) - songs.Count;
                    count = playButtons.IndexOf((Border)sender) - songs.Count;
                    settingsXML.Element("Settings").Element("LastSong").Attribute("playList").Value = nameOfPlayList;
                    settingsXML.Save("settings.xml");
                    auSet.Open(new Uri(listOfSongs[count]));
                    progress.Width = 0;
                    SoundName.Text = elem.soundName(auSet.Source.ToString());
                    time.timeToSlider(auSet, progressBar, progress, currentTime, durationTime);
                    for (int i = 0; i < textBlocksToPlayBlock.Count; i++)
                    {
                        textBlocksToPlayBlock[i].Text = "";
                    }
                }                
            }
            for (int i = 0; i < playButtons.Count; i++)
            {
                ((TextBlock)(playButtons[i]).Child).Text = "▶";                
            }            
            //((TextBlock)(playButtons[count].Child)).Text = "❚❚";
            ((TextBlock)((Border)sender).Child).Text = "❚❚";
            if (playListSwitch)
            {
                textBlocksToPlayBlock[playListCount + songs.Count].Text = "[Play]";
            } else
            {
                textBlocksToPlayBlock[count].Text = "[Play]";
            }
            playPause(play, null);                                    
        }
        // Check boxes click on default play list
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
            if(selectSongBlocks.Count == 0)
            {
                addDirectory.Visibility = Visibility.Hidden;
            } else
            {
                addDirectory.Visibility = Visibility.Visible;
            }
            //MessageBox.Show(songs[selectSongBlocks.IndexOf((CheckBox)sender)]);
        }
        //
        // To loop a song
        private void loopMenu(object sender, RoutedEventArgs e)
        {
            looper = ((ComboBox)sender).SelectedIndex;                        
            //settingsXML.Element("Settings").Element("loopMode").Value = looper.ToString();
        }
        //
        // Buttons to change songs (next song / previous song)
        private void changeSounds(object sender, RoutedEventArgs e)
        {
            if(((Border)sender).Name == "prevSound")
            {
                if (playListSwitch)
                {
                    if (playListCount == 0)
                    {
                        playListCount = listOfSongs.Count - 1;
                        count = playListCount;
                    }
                    else
                    {
                        playListCount--;
                        count = playListCount;
                    }
                    auSet.Open(new Uri(listOfSongs[count]));
                } else if(!playListSwitch)
                {
                    if (count == 0)
                    {
                        count = songs.Count - 1;
                    }
                    else
                    {
                        count--;
                    }
                    auSet.Open(new Uri(songs[count]));
                }
            } 
            else
            {
                if (playListSwitch)
                {
                    if(playListCount == listOfSongs.Count - 1) 
                    {
                        playListCount = 0;
                        count = playListCount;
                    }
                    else
                    {
                        playListCount++;
                        count = playListCount;
                    }
                    auSet.Open(new Uri(listOfSongs[count]));
                } else if(!playListSwitch)
                {
                    if (count == (songs.Count - 1))
                    {
                        count = 0;
                    }
                    else
                    {
                        count++;
                    }
                    auSet.Open(new Uri(songs[count]));
                }
            }
            for (int i = 0; i < playButtons.Count; i++)
            {
                ((TextBlock)(playButtons[i]).Child).Text = "▶";
            }
            for (int i = 0; i < textBlocksToPlayBlock.Count; i++)
            {
                textBlocksToPlayBlock[i].Text = "";
            }            
            if (!playListSwitch)
            {
                if (switcher)
                {
                    ((TextBlock)(playButtons[count]).Child).Text = "❚❚";
                }
                textBlocksToPlayBlock[count].Text = "[Play]";
            }
            else if(playListSwitch && nameOfPlayList == playListName.Text)
            {
                if (switcher)
                {
                    ((TextBlock)(playButtons[playListCount + songs.Count]).Child).Text = "❚❚";
                }                
                textBlocksToPlayBlock[playListCount + songs.Count].Text = "[Play]";
            }                                  
            SoundName.Text = elem.soundName(auSet.Source.ToString());
            progress.Width = 0;
            time.timeToSlider(auSet, progressBar, progress, (TextBlock)FindName("currentTime"), (TextBlock)FindName("durationTime"));
            if(switcher)
            {
                auSet.Play();
            }            
        }
        //
        // Change pages in programm (settings / all Musics / play lists)
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
                    addDirectory.Visibility = Visibility.Hidden;
                    DownloadSongOnGeneral.Visibility = Visibility.Visible;
                    playListSwitch = false;                    
                    break;
                case 1:
                    toSounds.Visibility = Visibility.Hidden;
                    toDirs.Visibility = Visibility.Visible;
                    toPlayLists.Visibility = Visibility.Hidden;
                    playListInterface.Visibility = Visibility.Hidden;
                    songsInPlayList.Visibility = Visibility.Hidden;
                    addDirectory.Visibility = Visibility.Visible;
                    DownloadSongOnGeneral.Visibility = Visibility.Hidden;
                    break;
                case 2:
                    toSounds.Visibility = Visibility.Hidden;
                    toDirs.Visibility = Visibility.Hidden;
                    toPlayLists.Visibility = Visibility.Visible;
                    playListInterface.Visibility = Visibility.Hidden;
                    songsInPlayList.Visibility = Visibility.Hidden;
                    addDirectory.Visibility = Visibility.Visible;
                    DownloadSongOnGeneral.Visibility = Visibility.Hidden;
                    break;
                case 3:                    
                    toSounds.Visibility = Visibility.Hidden;
                    toDirs.Visibility = Visibility.Hidden;
                    toPlayLists.Visibility = Visibility.Hidden;
                    playListInterface.Visibility = Visibility.Visible;                    
                    songsInPlayList.Visibility = Visibility.Visible;
                    DownloadSongOnGeneral.Visibility = Visibility.Hidden;
                    forCreateGrid.Clear();                    
                    while (playButtons.Count > songs.Count)
                    {
                        playButtons.RemoveAt(playButtons.Count - 1);
                    }
                    while (addButtons.Count > songs.Count)
                    {
                        addButtons.RemoveAt(addButtons.Count - 1);
                    }
                    while (textBlocksToPlayBlock.Count > songs.Count)
                    {
                        textBlocksToPlayBlock.RemoveAt(textBlocksToPlayBlock.Count - 1);
                    }
                    songsInPlayListGrid.Children.Clear();
                    songsInPlayListGrid.RowDefinitions.Clear();                                         
                    playListSwitch = true;
                    XDocument playListXML = XDocument.Load($"{((TextBlock)((Border)sender).Child).Text}");
                    foreach (XElement song in playListXML.Element("Songs").Elements("song"))
                    {
                        forCreateGrid.Add(song.Value);
                    }                                            
                    createPlayListElement();
                    playListName.Text = ((TextBlock)((Border)sender).Child).Text;                    
                    if (playListSwitch)
                    {
                        if (nameOfPlayList == playListName.Text)
                        {
                            textBlocksToPlayBlock[playListCount + songs.Count].Text = "[Play]";
                            if (switcher)
                            {
                                ((TextBlock)(playButtons[playListCount + songs.Count]).Child).Text = "❚❚";
                            }
                        }
                    }
                    else if (!playListSwitch)
                    {
                        textBlocksToPlayBlock[count].Text = "[Play]";
                        if (switcher)
                        {
                            ((TextBlock)(playButtons[count]).Child).Text = "❚❚";
                        }
                    }                    
                    addDirectory.Visibility = Visibility.Hidden;
                    changePlayList = true;
                    break;
            }
            if(page == 3)
            {
                page = 2;
            }            
        }
        //        
        private void deleteSongFromPlayList(object sender, RoutedEventArgs e)
        {
            // if delete element = play song
            if(playListCount == (addButtons.IndexOf((Border)sender) - songs.Count) && nameOfPlayList == playListName.Text)
            {
                if (playListCount == listOfSongs.Count - 1)
                {
                    playListCount = 0;
                }
                else
                {
                    playListCount++;
                }
                count = playListCount;
                auSet.Open(new Uri(listOfSongs[count]));
                SoundName.Text = elem.soundName(auSet.Source.ToString());
                if (!changePlayList)
                {
                    ((TextBlock)playButtons[addButtons.IndexOf((Border)sender) - songs.Count].Child).Text = "❚❚";
                }
                textBlocksToPlayBlock[addButtons.IndexOf((Border)sender) - songs.Count].Text = "[Play]";
                if (switcher)
                {
                    auSet.Play();
                }
                settingsXML.Element("Settings").Element("LastSong").Value = playListCount.ToString();
                listOfSongs.RemoveAt(addButtons.IndexOf((Border)sender) - songs.Count);
            }
            //
            // Delete elements from grid
            songsInPlayListGrid.RowDefinitions.RemoveAt(addButtons.IndexOf((Border)sender) - songs.Count);
            for (int i = 0; i < songsInPlayListGrid.Children.Count; i++)
            {
                if (Grid.GetRow(songsInPlayListGrid.Children[i]) > addButtons.IndexOf((Border)sender) - songs.Count)
                {
                    Grid.SetRow(songsInPlayListGrid.Children[i], Grid.GetRow(songsInPlayListGrid.Children[i]) - 1);
                }
                if (Grid.GetRow(songsInPlayListGrid.Children[i]) == addButtons.IndexOf((Border)sender) - songs.Count)
                {
                    songsInPlayListGrid.Children.RemoveAt(i);
                }                
            }
            //                 
            // Delete notes from xml file
            XDocument playListXML = XDocument.Load($"{playListName.Text}");
            foreach(XElement song in playListXML.Element("Songs").Elements("song"))
            {                
                if (song.Value == forCreateGrid[addButtons.IndexOf((Border)sender) - songs.Count])
                {                    
                    song.Remove();
                }
            }
            playListXML.Save($"{playListName.Text}");
            //
            // Remove elements from lists                        
            addButtons.Remove((Border)sender);
            //
        }

        // Binding func on key press
        private void keyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space || e.Key == Key.MediaPlayPause)
            {
                playPause(play, null);
            }

            if(e.Key == Key.MediaNextTrack)
            {
                changeSounds(nextSound, null);
            }

            if(e.Key == Key.MediaPreviousTrack)
            {
                changeSounds(prevSound, null);
            }
        }
        //
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
            if(((Border)sender).Name == "prevSound" || ((Border)sender).Name == "play" || ((Border)sender).Name == "nextSound" || ((Border)sender).Name == "playListStart" || ((Border)sender).Name == "deletePlayList" || ((Border)sender).Name == "playListMenu")
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
            if (((Border)sender).Name == "prevSound" || ((Border)sender).Name == "play" || ((Border)sender).Name == "nextSound" || ((Border)sender).Name == "playListStart" || ((Border)sender).Name == "deletePlayList" || ((Border)sender).Name == "playListMenu")
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
