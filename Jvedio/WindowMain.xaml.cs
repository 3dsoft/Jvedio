﻿using DynamicData;
using Jvedio.ViewModel;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Jvedio.StaticVariable;
using static Jvedio.StaticClass;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace Jvedio
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Window
    {
        public const string UpdateUrl = "http://hitchao.gitee.io/jvedioupdate/Version";
        public const string UpdateExeVersionUrl = "http://hitchao.gitee.io/jvedioupdate/update";
        public const string UpdateExeUrl = "http://hitchao.gitee.io/jvedioupdate/JvedioUpdate.exe";
        public const string NoticeUrl = "https://hitchao.gitee.io/jvediowebpage/notice";

        public DispatcherTimer CheckurlTimer = new DispatcherTimer();
        public int CheckurlInterval = 10;//每5分钟检测一次网址

        public bool Resizing = false;
        public DispatcherTimer ResizingTimer = new DispatcherTimer();

        public Point WindowPoint = new Point(100, 100);
        public Size WindowSize = new Size(1000, 600);
        public JvedioWindowState WinState = JvedioWindowState.Normal;
        public DispatcherTimer ImageSlideTimer;

        public List<Actress> SelectedActress = new List<Actress>();

        public bool IsMouseDown = false;
        public Point MosueDownPoint;

        public bool CanRateChange = false;
        public bool IsToUpdate = false;

        public CancellationTokenSource RefreshScanCTS;
        public CancellationToken RefreshScanCT;


        public Settings WindowSet = null;
        public VieModel_Main vieModel;
        public WindowSearch windowSearch = null;

        private HwndSource _hwndSource;

        public DetailMovie CurrentLabelMovie;


        DispatcherTimer FlowTimer = new DispatcherTimer();
        public bool IsFlowing = false;


        DispatcherTimer FlipoverTimer = new DispatcherTimer();

        


        public Main()
        {
            InitializeComponent();

            SettingsContextMenu.Placement = PlacementMode.Mouse;

            this.Cursor = Cursors.Wait;


            ImageSlideTimer = new DispatcherTimer();
            ImageSlideTimer.Interval = TimeSpan.FromMilliseconds(200);
            ImageSlideTimer.Tick += new EventHandler(ImageSlideTimer_Tick);


            ProgressBar.Visibility = Visibility.Hidden;
            FilterGrid.Visibility = Visibility.Collapsed;
            WinState = 0;

            RefreshSideRB();
            AdjustWindow();

            Properties.Settings.Default.Selected_Background = "#FF8000";
            Properties.Settings.Default.Selected_BorderBrush = "#FF8000";
            //Properties.Settings.Default.DisplayNumber = 5;

            #region "改变窗体大小"
            //https://www.cnblogs.com/yang-fei/p/4737308.html

            if (resizeGrid != null)
            {
                foreach (UIElement element in resizeGrid.Children)
                {
                    Rectangle resizeRectangle = element as Rectangle;
                    if (resizeRectangle != null)
                    {
                        resizeRectangle.PreviewMouseDown += ResizeRectangle_PreviewMouseDown;
                        resizeRectangle.MouseMove += ResizeRectangle_MouseMove;
                    }
                }
            }
            PreviewMouseMove+= OnPreviewMouseMove;
            #endregion
        }

        #region "改变窗体大小"
        private void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WinState == JvedioWindowState.Maximized || WinState == JvedioWindowState.FullScreen) return;
            Rectangle rectangle = sender as Rectangle;

            if (rectangle != null)
            {
                switch (rectangle.Name)
                {
                    case "TopRectangle":
                        Cursor = Cursors.SizeNS;
                        ResizeWindow(ResizeDirection.Top);
                        break;
                    case "Bottom":
                        Cursor = Cursors.SizeNS;
                        ResizeWindow(ResizeDirection.Bottom);
                        break;
                    case "LeftRectangle":
                        Cursor = Cursors.SizeWE;
                        ResizeWindow(ResizeDirection.Left);
                        break;
                    case "Right":
                        Cursor = Cursors.SizeWE;
                        ResizeWindow(ResizeDirection.Right);
                        break;
                    case "TopLeft":
                        Cursor = Cursors.SizeNWSE;
                        ResizeWindow(ResizeDirection.TopLeft);
                        break;
                    case "TopRight":
                        Cursor = Cursors.SizeNESW;
                        ResizeWindow(ResizeDirection.TopRight);
                        break;
                    case "BottomLeft":
                        Cursor = Cursors.SizeNESW;
                        ResizeWindow(ResizeDirection.BottomLeft);
                        break;
                    case "BottomRight":
                        Cursor = Cursors.SizeNWSE;
                        ResizeWindow(ResizeDirection.BottomRight);
                        break;
                    default:
                        break;
                }
            }
        }


        protected void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                Cursor = Cursors.Arrow;
        }

        private void ResizeRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (WinState == JvedioWindowState.Maximized || WinState == JvedioWindowState.FullScreen) return;
            Rectangle rectangle = sender as Rectangle;

            if (rectangle != null)
            {
                switch (rectangle.Name)
                {
                    case "TopRectangle":
                        Cursor = Cursors.SizeNS;
                        break;
                    case "Bottom":
                        Cursor = Cursors.SizeNS;
                        break;
                    case "LeftRectangle":
                        Cursor = Cursors.SizeWE;
                        break;
                    case "Right":
                        Cursor = Cursors.SizeWE;
                        break;
                    case "TopLeft":
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case "TopRight":
                        Cursor = Cursors.SizeNESW;
                        break;
                    case "BottomLeft":
                        Cursor = Cursors.SizeNESW;
                        break;
                    case "BottomRight":
                        Cursor = Cursors.SizeNWSE;
                        break;
                    default:
                        break;
                }
            }
        }

        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += MainWindow_SourceInitialized;
            base.OnInitialized(e);
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        #endregion


        #region "热键"



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);


            //热键
            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            //注册热键

            uint modifier = Properties.Settings.Default.HotKey_Modifiers;
            uint vk = Properties.Settings.Default.HotKey_VK;

            if (Properties.Settings.Default.HotKey_Enable &&  modifier != 0 && vk!=0)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);//取消之前的热键
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk);
                if (!success) { MessageBox.Show("热键冲突！", "热键冲突"); }
            }

        }




        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == Properties.Settings.Default.HotKey_VK)
                            {
                                HideAllWindow();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void HideAllWindow()
        {

            if (IsHide)
            {
                foreach (Window window in App.Current.Windows)
                {
                    if (OpeningWindows.Contains(window.GetType().ToString()))
                    {
                        window.Visibility = Visibility.Visible;
                    }
                }
                IsHide = false;
            }
            else
            {
                OpeningWindows.Clear();
                foreach (Window window in App.Current.Windows)
                {
                        window.Visibility = Visibility.Hidden;
                        OpeningWindows.Add(window.GetType().ToString());
                }
                IsHide = true;

                //隐藏图标
                //notifyIcon.Visible = false;
                NotifyIcon.Visibility = Visibility.Collapsed;
            }


        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            //取消热键
            Console.WriteLine("UnregisterHotKey");
            base.OnClosed(e);
        }


        #endregion


        private void ImageSlideTimer_Tick(object sender, EventArgs e)
        {
            Loadslide();
            ImageSlideTimer.Stop();
        }
        public void InitMovie()
        {
            vieModel = new VieModel_Main();
            if (Properties.Settings.Default.RandomDisplay)
            {
                vieModel.RandomDisplay();
            }
            else
            {
                vieModel.Reset();
                AllRB.IsChecked = true;
            }
            //AsyncLoadImage();
            this.DataContext = vieModel;
            vieModel.CurrentMovieListHideOrChanged += (s, ev) => { StopDownLoad(); };
            vieModel.CurrentMovieListChangedCompleted += (s, ev) => {
                //Console.WriteLine("加载完成");
                //if (vieModel.CurrentMovieList.Count == 0 && vieModel.AllVedioCount > 0)
                //    HandyControl.Controls.Growl.Info("无视频，请右键切换显示模式");
            };
            vieModel.FlipOverCompleted += (s, ev) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    //ScrollViewer.ScrollToTop();
                });
            };
            FlowTimer.Interval = TimeSpan.FromMilliseconds(1);
            FlowTimer.Tick += new EventHandler(FlowTimer_Tick);


            FlipoverTimer.Interval = TimeSpan.FromMilliseconds(1);
            FlipoverTimer.Tick += new EventHandler(FlipoverTimer_Tick);
            

            CheckurlTimer.Interval = TimeSpan.FromMinutes(CheckurlInterval);
            CheckurlTimer.Tick += new EventHandler(CheckurlTimer_Tick);

            ResizingTimer.Interval = TimeSpan.FromSeconds(0.5);
            ResizingTimer.Tick += new EventHandler(ResizingTimer_Tick);




        }


        public async Task<bool> InitActor()
        {
            vieModel.GetActorList();
            await Task.Delay(1);
            return true;
        }



        private const int WM_HOTKEY = 0x312; //窗口消息-热键
        private const int WM_CREATE = 0x1; //窗口消息-创建
        private const int WM_DESTROY = 0x2; //窗口消息-销毁
        private const int Space = 0x3572; //热键ID



        #region "右键命令"
        public  void DeleteIDCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Console.WriteLine("DeleteIDCommandBinding_Executed");
        }

        #endregion

        public void BeginCheckurlThread()
        {
            Thread threadObject = new Thread(CheckUrl);
            threadObject.Start();
        }



        private void CheckurlTimer_Tick(object sender, EventArgs e)
        {
            BeginCheckurlThread();
        }

        

        private void ResizingTimer_Tick(object sender, EventArgs e)
        {
            Resizing = false;
            ResizingTimer.Stop();
        }
        private void FlowTimer_Tick(object sender, EventArgs e)
        {
            vieModel.FlowNum++;
            vieModel.Flow();
            FlowTimer.Stop();
        }

        private void FlipoverTimer_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("翻页");
            vieModel.FlipOver();
            ScrollViewer.ScrollToTop();
            FlipoverTimer.Stop();
        }




        public void Notify_Close(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Collapsed;
            this.Close();
        }

        public void Notify_Show(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Collapsed;
            this.Show();
            this.Opacity = 1;
            this.WindowState = WindowState.Normal;
        }


        void CheckUpdate()
        {
            Task.Run(async () =>
            {
                string content = ""; int statusCode;
                try
                {
                    (content, statusCode) = await Net.Http(UpdateUrl, Proxy: null);
                }
                catch (TimeoutException ex) { Logger.LogN($"URL={UpdateUrl},Message-{ex.Message}"); }

                if (content != "")
                {
                    //检查更新
                    this.Dispatcher.Invoke((Action)delegate ()
                    {
                        string remote = content.Split('\n')[0];
                        string updateContent = content.Replace(remote + "\n","");
                        string local = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                        using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "OldVersion"))
                        {
                            sw.WriteLine(local + "\n");
                            sw.WriteLine(updateContent);
                        }

                        LocalVersionTextBlock.Text = $"当前版本：{local}";
                        RemoteVersionTextBlock.Text = $"最新版本：{remote}";
                        UpdateContentTextBox.Text = updateContent;

                        if (local.CompareTo(remote) < 0) UpdateGrid.Visibility = Visibility.Visible;
                    });
                }
            });
        }


        void ShowNotice()
        {
            Task.Run(async () =>
            {
                string notices = "";
                string path = AppDomain.CurrentDomain.BaseDirectory + "Notice.txt";
                if (File.Exists(path))
                {
                    StreamReader sr = new StreamReader(path);
                    notices = sr.ReadToEnd();
                    sr.Close();
                }
                string content = ""; int statusCode = 404;
                try
                {
                    (content, statusCode) = await Net.Http(NoticeUrl, Proxy: null);
                }
                catch (TimeoutException ex) { Logger.LogN($"URL={NoticeUrl},Message-{ex.Message}"); }
                if (content != "")
                {
                    if (content != notices)
                    {
                        StreamWriter sw = new StreamWriter(path, false);
                        sw.Write(content);
                        sw.Close();
                        this.Dispatcher.Invoke((Action)delegate ()
                        {
                            NoticeTextBlock.Text = content;
                            NoticeGrid.Visibility = Visibility.Visible;
                        });
                    }

                }
            });
        }



        public DownLoader DownLoader;

        public void StartDownload(List<Movie> movieslist)
        {
            List<Movie> movies = new List<Movie>();
            List<Movie> moviesFC2 = new List<Movie>();
            if (movieslist != null)
            {
                foreach (var item in movieslist)
                {
                    if (item.title == "" | item.smallimageurl == "" | item.bigimageurl == "" | item.sourceurl == "" | item.smallimage == null | item.bigimage == null)
                        if (item.id.IndexOf("FC2") >= 0) { moviesFC2.Add(item); } else { movies.Add(item); }
                }
            }

            //添加到下载列表
            DownLoader?.CancelDownload();
            DownLoader = new DownLoader(movies, moviesFC2);
            DownLoader.StartThread();
            double totalcount = moviesFC2.Count + movies.Count;
            Console.WriteLine(totalcount);
            if (totalcount == 0) return;
            //UI更新
            DownLoader.InfoUpdate += (s, e) =>
            {
                InfoUpdateEventArgs eventArgs = e as InfoUpdateEventArgs;
                    try
                    {
                        try { Refresh(eventArgs.Movie.id, eventArgs, totalcount); }
                        catch (TaskCanceledException ex) { Logger.LogE(ex); }
                    }
                    catch (Exception ex1)
                    {
                        Console.WriteLine(ex1.StackTrace);
                        Console.WriteLine(ex1.Message);
                    }
            };


        }

        public async void RefreshCurrentPage(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info( "停止当前下载后再试","Main");
                return;
            }

            //刷新文件夹
            this.Cursor = Cursors.Wait;

            if (vieModel.IsScanning)
            {
                vieModel.IsScanning = false;
                RefreshScanCTS?.Cancel();
            }
            else
            {
              if(Properties.Settings.Default.ScanWhenRefresh)  await ScanWhenRefresh();
            }
            CancelSelect();
            vieModel.Refresh();
            this.Cursor = Cursors.Arrow;
        }

        public async Task<bool> ScanWhenRefresh()
        {
            vieModel.IsScanning = true;
            RefreshScanCTS = new CancellationTokenSource();
            RefreshScanCTS.Token.Register(() => { Console.WriteLine("取消任务"); this.Cursor = Cursors.Arrow; });
            RefreshScanCT = RefreshScanCTS.Token;
            await Task.Run(() =>
            {
                List<string> filepaths = Scan.ScanPaths(ReadScanPathFromConfig(Properties.Settings.Default.DataBasePath.Split('\\').Last().Split('.').First()), RefreshScanCT);
                double num = Scan.DistinctMovieAndInsert(filepaths, RefreshScanCT);
                vieModel.IsScanning = false;

                if (Properties.Settings.Default.AutoDeleteNotExistMovie)
                {
                    //删除不存在影片
                    var movies = DataBase.SelectMoviesBySql("select * from movie");
                    movies.ForEach(movie =>
                    {
                        if (!File.Exists(movie.filepath))
                        {
                            DataBase.DelInfoByType("movie", "id", movie.id);
                        }
                    });

                }

                this.Dispatcher.BeginInvoke(new Action(() => {
                    vieModel.Reset();
                    if (num > 0) HandyControl.Controls.Growl.Info($"扫描并导入 {num} 个视频，详细请看 log\\scanlog\\ 文件夹当天日志文件","Main");
                }), System.Windows.Threading.DispatcherPriority.Render);


            }, RefreshScanCTS.Token);

            return true;
        }


        public void CancelSelect()
        {
            Properties.Settings.Default.EditMode = false; vieModel.SelectedMovie.Clear(); SetSelected();
        }

        public void SelectAll(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (Properties.Settings.Default.EditMode) { CancelSelect(); return; }
            Properties.Settings.Default.EditMode = true;
            foreach (var item in vieModel.CurrentMovieList)
            {
                if (!vieModel.SelectedMovie.Contains(item))
                {
                    vieModel.SelectedMovie.Add(item);

                }
            }
            SetSelected();


        }

        public async void ScrollToEnd(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                while (ScrollViewer.VerticalOffset < ScrollViewer.ScrollableHeight)
                {
                    this.Dispatcher.Invoke((Action)delegate
                    {
                        ScrollViewer.ScrollToBottom();
                    });
                    Task.Delay(100).Wait();
                }

            });
        }


        public  void Refresh(string ID, InfoUpdateEventArgs eventArgs, double totalcount)
        {
            Dispatcher.Invoke((Action) async delegate ()
            {
                ProgressBar.Value = ProgressBar.Maximum * (eventArgs.progress / totalcount); ProgressBar.Visibility = Visibility.Visible;
                if (ProgressBar.Value == ProgressBar.Maximum)
                {
                    DownLoader.State = DownLoadState.Completed; ProgressBar.Visibility = Visibility.Hidden;
                    Console.WriteLine("下载已完成");

                }
                if (DownLoader.State == DownLoadState.Completed | DownLoader.State == DownLoadState.Fail) ProgressBar.Visibility = Visibility.Hidden;

                RefreshMovieByID(eventArgs.Movie.id);
            });
        }

        public void RefreshMovieByID(string ID)
        {
            Movie movie = DataBase.SelectMovieByID(ID);
            if (Properties.Settings.Default.ShowImageMode == "预览图")
            {

            }
            else
            {
                movie.smallimage = StaticClass.GetBitmapImage(movie.id, "SmallPic");
                movie.bigimage = StaticClass.GetBitmapImage(movie.id, "BigPic");
            }
            int idx1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == ID).First());
            int idx2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == ID).First());
            try
            {
                vieModel.CurrentMovieList[idx1] = null;
                vieModel.CurrentMovieList[idx1] = movie;
                vieModel.MovieList[idx2] = null;
                vieModel.MovieList[idx2] = movie;
            }
            catch (ArgumentNullException ex) { }
        }


        public void OpenSubSuctionVedio(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel stackPanel = button.Parent as StackPanel;
            TextBlock textBlock = stackPanel.Children.OfType<TextBlock>().Last();
            string filepath = textBlock.Text;
            PlayVedioWithPlayer(filepath, "");
        }



        private static void OnCreated(object obj, FileSystemEventArgs e)
        {
            //导入数据库

            if (Scan.IsProperMovie(e.FullPath))
            {
                FileInfo fileinfo = new FileInfo(e.FullPath);

                //获取创建日期
                string createDate = "";
                try { createDate = fileinfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"); }
                catch { }
                if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Movie movie = new Movie()
                {
                    filepath = e.FullPath,
                    id = Identify.GetFanhao(fileinfo.Name),
                    filesize = fileinfo.Length,
                    vediotype = (int)Identify.GetVedioType(Identify.GetFanhao(fileinfo.Name)),
                    otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    scandate = createDate 
            };
                if (!string.IsNullOrEmpty(movie.id) & movie.vediotype > 0) { DataBase.InsertScanMovie(movie); }
                Console.WriteLine($"成功导入{e.FullPath}");
            }




        }

        private static void OnDeleted(object obj, FileSystemEventArgs e)
        {
            if (Properties.Settings.Default.ListenAllDir & Properties.Settings.Default.DelFromDBIfDel)
            {
                DataBase.DelInfoByType("movie", "filepath", e.FullPath);
            }
            Console.WriteLine("成功删除" + e.FullPath);
        }



        public FileSystemWatcher[] fileSystemWatcher;
        public string failwatcherMessage = "";

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void AddListen()
        {
            string[] drives = Environment.GetLogicalDrives();
            fileSystemWatcher = new FileSystemWatcher[drives.Count()];
            for (int i = 0; i < drives.Count(); i++)
            {
                try
                {

                    if (drives[i] == @"C:\") { continue; }
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = drives[i];
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Filter = "*.*";
                    watcher.Created += OnCreated;
                    watcher.Deleted += OnDeleted;
                    watcher.EnableRaisingEvents = true;
                    fileSystemWatcher[i] = watcher;

                }
                catch
                {
                    failwatcherMessage += drives[i] + ",";
                    continue;
                }
            }

            if (failwatcherMessage != "")
                HandyControl.Controls.Growl.Info( $"监听{failwatcherMessage}不成功","Main");
        }

        public async void CheckUrl()
        {
            Console.WriteLine("开始检测");
            vieModel.CheckingUrl = true;
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            //获取网址集合

            List<string> urlList = new List<string>();
            urlList.Add(Properties.Settings.Default.Bus);
            urlList.Add(Properties.Settings.Default.BusEurope);
            urlList.Add(Properties.Settings.Default.Library);
            urlList.Add(Properties.Settings.Default.DB);
            urlList.Add(Properties.Settings.Default.Fc2Club);
            urlList.Add(Properties.Settings.Default.Jav321);
            urlList.Add(Properties.Settings.Default.DMM);

            List<bool> enableList = new List<bool>();
            enableList.Add(Properties.Settings.Default.EnableBus);
            enableList.Add(Properties.Settings.Default.EnableBusEu);
            enableList.Add(Properties.Settings.Default.EnableLibrary);
            enableList.Add(Properties.Settings.Default.EnableDB);
            enableList.Add(Properties.Settings.Default.EnableFC2);
            enableList.Add(Properties.Settings.Default.Enable321);
            enableList.Add(Properties.Settings.Default.EnableDMM);

            for (int i = 0; i < urlList.Count; i++)
            {
                bool enable = enableList[i];
                string url = urlList[i];
                if (enable)
                {
                    bool CanConnect = false; bool enablecookie = false; string cookie = "";
                    if (url == Properties.Settings.Default.DB)
                    {
                        enablecookie = true;
                        cookie = Properties.Settings.Default.DBCookie;
                    }
                    try
                    {
                        CanConnect = await Net.TestUrl(url, enablecookie, cookie, "DB");
                    }
                    catch (TimeoutException ex) { Logger.LogN($"URL={url},Message-{ex.Message}"); }

                    if (CanConnect) { if (!result.ContainsKey(url)) result.Add(url, true); } else { if (!result.ContainsKey(url)) result.Add(url, false); }
                }
                else
                   if (!result.ContainsKey(url)) result.Add(url, false);
            }

            try
            {
                this.Dispatcher.Invoke((Action)delegate ()
                {
                    try
                    {

                        if (result[Properties.Settings.Default.Bus]) { BusStatus.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.DB]) { DBStatus.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.Library]) { LibraryStatus.Fill = Brushes.Green; }
                    //if (result[Properties.Settings.Default.Fc2Club]) { FC2Status.Fill = Brushes.Green; }
                    if (result[Properties.Settings.Default.BusEurope]) { BusEuropeStatus.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.Jav321]) { Jav321Status.Fill = Brushes.Green; }
                        if (result[Properties.Settings.Default.DMM]) { DMMStatus.Fill = Brushes.Green; }

                    }
                    catch (KeyNotFoundException ex) { Console.WriteLine(ex.Message); }


                });
            }
            catch (TaskCanceledException ex) { Console.WriteLine(ex.Message); }

            bool IsAllConnect = true;
            bool IsOneConnect = false;
            for (int i = 0; i < enableList.Count; i++)
            {
                if (enableList[i])
                {
                    if (result.ContainsKey(urlList[i]))
                    {
                        if (!result[urlList[i]])
                            IsAllConnect = false;
                        else
                            IsOneConnect = true;
                    }
                }
            }

            this.Dispatcher.Invoke((Action)delegate ()
            {

                if (IsAllConnect)
                    AllStatus.Background = Brushes.Green;
                else if (!IsAllConnect & !IsOneConnect)
                    AllStatus.Background = Brushes.Red;
                else if (IsOneConnect & !IsAllConnect)
                    AllStatus.Background = Brushes.Yellow;

            });
            vieModel.CheckingUrl = false;
        }


        public void AdjustWindow()
        {
            SetWindowProperty();


            HideMargin();

            SideGridColumn.Width = new GridLength(Properties.Settings.Default.SideGridWidth);

            if (Properties.Settings.Default.ShowImageMode == "列表模式")
            {
                MovieMainGrid.Visibility = Visibility.Hidden;
                DetailGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MovieMainGrid.Visibility = Visibility.Visible;
                DetailGrid.Visibility = Visibility.Hidden;
            }

        }

        private void SetWindowProperty()
        {
            //读取窗体设置
            WindowConfig cj = new WindowConfig(this.GetType().Name);
            WindowProperty windowProperty = cj.Read();
            Rect rect = new Rect() { Location = windowProperty.Location, Size = windowProperty.Size };
            WinState = windowProperty.WinState;
            //读到属性值
            if (WinState == JvedioWindowState.FullScreen)
            {
                this.WindowState = WindowState.Maximized;
            }
            else if (WinState == JvedioWindowState.None)
            {
                WinState = 0;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                this.Left = rect.X >= 0 ? rect.X : 0;
                this.Top = rect.Y >= 0 ? rect.Y : 0;
                this.Height = rect.Height > 100 ? rect.Height : 100;
                this.Width = rect.Width > 100 ? rect.Width : 100;
                if (this.Width == SystemParameters.WorkArea.Width | this.Height == SystemParameters.WorkArea.Height) { WinState = JvedioWindowState.Maximized; }
            }
        }




        private void Window_Closed(object sender, EventArgs e)
        {
            if (!IsToUpdate && Properties.Settings.Default.CloseToTaskBar && this.IsVisible == true)
            {
                NotifyIcon.Visibility = Visibility.Visible;
                this.Hide();
                WindowSet?.Hide();
            }
            else
            {
                StopDownLoad();
                SaveRecentWatched();
                ProgressBar.Visibility = Visibility.Hidden;
                WindowTools windowTools = null;
                foreach (Window item in App.Current.Windows)
                {
                    if (item.GetType().Name == "WindowTools") windowTools = item as WindowTools;
                }

                if (windowTools?.IsVisible == true)
                {
                }
                else
                {
                    System.Windows.Application.Current.Shutdown();
                }


            }
        }

        public async void FadeOut()
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                double opacity = this.Opacity;
                await Task.Run(() =>
                {
                    while (opacity > 0.1)
                    {
                        this.Dispatcher.Invoke((Action)delegate { this.Opacity -= 0.05; opacity = this.Opacity; });
                        Task.Delay(1).Wait();
                    }
                });
                this.Opacity = 0;
            }
            this.Close();
        }


        public void CloseWindow(object sender, MouseButtonEventArgs e)
        {
            FadeOut();
        }

        public async void MinWindow(object sender, MouseButtonEventArgs e)
        {
            if (Properties.Settings.Default.EnableWindowFade)
            {
                double opacity = this.Opacity;
                await Task.Run(() =>
                {
                    while (opacity > 0.2)
                    {
                        this.Dispatcher.Invoke((Action)delegate { this.Opacity -= 0.1; opacity = this.Opacity; });
                        Task.Delay(20).Wait();
                    }
                });
            }

            this.WindowState = WindowState.Minimized;
            this.Opacity = 1;

        }


        public void MaxWindow(object sender, MouseButtonEventArgs e)
        {
            Resizing = true;
            if (WinState == 0)
            {
                //最大化
                WinState = JvedioWindowState.Maximized;
                WindowPoint = new Point(this.Left, this.Top);
                WindowSize = new Size(this.Width, this.Height);
                this.Height = SystemParameters.WorkArea.Height;
                this.Width = SystemParameters.WorkArea.Width;
                this.Left = SystemParameters.WorkArea.Left;
                this.Top = SystemParameters.WorkArea.Top;

            }
            else
            {
                WinState = JvedioWindowState.Normal;
                this.Left = WindowPoint.X;
                this.Top = WindowPoint.Y;
                this.Width = WindowSize.Width;
                this.Height = WindowSize.Height;
            }
            this.WindowState = WindowState.Normal;
            this.OnLocationChanged(EventArgs.Empty);
            HideMargin();
        }

        private void HideMargin()
        {
            if (WinState == JvedioWindowState.Normal)
            {
                MainGrid.Margin = new Thickness(10);
                MainBorder.Margin = new Thickness(5);
                Grid.Margin=new Thickness(5);
                this.ResizeMode = ResizeMode.CanResize;
            }
            else if (WinState == JvedioWindowState.Maximized || this.WindowState == WindowState.Maximized)
            {
                MainGrid.Margin = new Thickness(0);
                MainBorder.Margin = new Thickness(0);
                Grid.Margin = new Thickness(0);
                this.ResizeMode = ResizeMode.NoResize;
            }
            ResizingTimer.Start();
        }

        public void FullScreen(object sender, MouseButtonEventArgs e)
        {
            if (WinState == JvedioWindowState.FullScreen)
            {
                WinState = JvedioWindowState.Normal;
                this.WindowState = WindowState.Normal;
                this.Left = WindowPoint.X;
                this.Top = WindowPoint.Y;
                this.Width = WindowSize.Width;
                this.Height = WindowSize.Height;
            }
            else if (WinState == JvedioWindowState.Normal)
            {
                WinState = JvedioWindowState.FullScreen;
                WindowPoint = new Point(this.Left, this.Top);
                WindowSize = new Size(this.Width, this.Height);
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                WinState = JvedioWindowState.FullScreen;
                this.WindowState = WindowState.Maximized;
            }
            this.OnLocationChanged(EventArgs.Empty);
            this.OnStateChanged(EventArgs.Empty);
            HideMargin();
        }


        private void MoveWindow(object sender, MouseEventArgs e)
        {
            AllSearchPopup.IsOpen = false;


            //移动窗口
            if (e.LeftButton == MouseButtonState.Pressed && WinState == JvedioWindowState.Normal)
            {
                this.DragMove();
            }
        }

        WindowTools WindowTools;

        private void OpenTools(object sender, RoutedEventArgs e)
        {
            //SettingsPopup.IsOpen = false;
            if (WindowTools != null) { WindowTools.Close(); }
            WindowTools = new WindowTools();
            WindowTools.Show();

        }


        private void OpenDataBase(object sender, RoutedEventArgs e)
        {
            Window_DBManagement window_DBManagement = Jvedio.GetWindow.Get("Window_DBManagement") as Window_DBManagement;
            if (window_DBManagement == null) window_DBManagement = new Window_DBManagement();


            window_DBManagement.Show();
            

        }


        private void OpenUrl(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            Process.Start(hyperlink.NavigateUri.ToString());
        }

        private void OpenFeedBack(object sender, RoutedEventArgs e)
        {
            Process.Start("https://docs.qq.com/form/page/DRkFITmFxUmt3ZnpQ");
        }

        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.kancloud.cn/hitchao/jvedio/content/Jvedio.md");
        }

        private void OpenThanks(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.kancloud.cn/hitchao/jvedio/1921337");
        }

        private void OpenJvedioWebPage(object sender, RoutedEventArgs e)
        {
            Process.Start("https://hitchao.gitee.io/jvediowebpage/");
        }


        private void HideGrid(object sender, MouseButtonEventArgs e)
        {
            Grid grid = ((Border)sender).Parent as Grid;
            grid.Visibility = Visibility.Hidden;

        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            AboutGrid.Visibility = Visibility.Visible;
            VersionTextBlock.Text = $"版本：{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}";
        }

        private void ShowThanks(object sender, RoutedEventArgs e)
        {
            ThanksGrid.Visibility = Visibility.Visible;
        }

        private void ShowUpdate(object sender, MouseButtonEventArgs e)
        {
            CheckUpdate();
            UpdateGrid.Visibility = Visibility.Visible;
        }





        private void OpenSet_MouseDown(object sender, RoutedEventArgs e)
        {
            //SettingsPopup.IsOpen = false;
            if (WindowSet != null) { WindowSet.Close(); }
            WindowSet = new Settings();
            WindowSet.Show();

        }



        public void SearchContent(object sender, MouseButtonEventArgs e)
        {
            Grid grid = ((Canvas)(sender)).Parent as Grid;
            TextBox SearchTextBox = grid.Children.OfType<TextBox>().First() as TextBox;
            if (grid.Name == "AllSearchGrid") { vieModel.SearchAll = true; } else { vieModel.SearchAll = false; }
            vieModel.Search = SearchTextBox.Text;
        }





        private void SetSearchValue(object sender, MouseButtonEventArgs e)
        {
            AllSearchTextBox.Text = ((TextBlock)sender).Text;
            AllSearchTextBox.Select(AllSearchTextBox.Text.Length, 0);
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            Grid grid = tb.Parent as Grid;
            if (grid.Name == "AllSearchGrid") { vieModel.SearchAll = true; } else { vieModel.SearchAll = false; }


            //if ( vieModel.SearchAll) AllSearchPopup.IsOpen = true;
            //if (SearchCandidate != null & !vieModel.SearchAll) SearchCandidate.Visibility = Visibility.Visible;

            //动画
            DoubleAnimation doubleAnimation = new DoubleAnimation(200, 300, new Duration(TimeSpan.FromMilliseconds(200)));
            AllSearchGrid.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimation);
            AllSearchBorder.BorderBrush =new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["ForegroundSearch"].ToString()));

            
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //if ( vieModel.SearchAll) AllSearchPopup.IsOpen = false;
            //if (SearchCandidate != null & !vieModel.SearchAll) SearchCandidate.Visibility = Visibility.Hidden;
            AllSearchPopup.IsOpen = false;

            DoubleAnimation doubleAnimation = new DoubleAnimation(300, 200, new Duration(TimeSpan.FromMilliseconds(200)));
            AllSearchGrid.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimation);
            AllSearchBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString()));

        }


        public bool CanSearch = false;

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Properties.Settings.Default.SearchImmediately & AllSearchTextBox.Text != "") return;
            TextBox SearchTextBox = sender as TextBox;
            Grid grid = SearchTextBox.Parent as Grid;
            string searchtext = SearchTextBox.Text;
            //文字改变 n 秒后才执行搜索
            AllSearchPopup.IsOpen = true;
            vieModel?.GetSearchCandidate(searchtext);

            if (grid.Name == "AllSearchGrid") { vieModel.SearchAll = true; } else { vieModel.SearchAll = false; }
            vieModel.Search = searchtext;
            
        }


        private int SearchSelectIdex=-1;

        private void SearchTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox SearchTextBox = sender as TextBox;
                if (SearchSelectIdex != -1)
                {
                    ContentPresenter c = (ContentPresenter)SearchItemsControl.ItemContainerGenerator.ContainerFromItem(SearchItemsControl.Items[SearchSelectIdex]);
                    StackPanel stackPanel = FindElementByName<StackPanel>(c, "SearchStackPanel");
                    if (stackPanel != null)
                    {
                        TextBlock textBlock = stackPanel.Children[0] as TextBlock;
                        SearchTextBox.Text= textBlock.Text;
                        SearchTextBox.Select(textBlock.Text.Length, 0);
                        SearchSelectIdex = -1;
                    }
                }
                else
                {
                    
                    Grid grid = SearchTextBox.Parent as Grid;
                    if (SearchTextBox != null)
                    {
                        string searchtext = SearchTextBox.Text;
                        vieModel.Search = searchtext;
                    }
                }
                AllSearchPopup.IsOpen = false;


            }else if (e.Key == Key.Down)
            {
                int count = vieModel.CurrentSearchCandidate.Count;

                SearchSelectIdex += 1;
                if (SearchSelectIdex >= count) SearchSelectIdex = 0;
                SetSearchSelect();

            }
            else if (e.Key == Key.Up)
            {
                int count = vieModel.CurrentSearchCandidate.Count;
                SearchSelectIdex -= 1;
                if (SearchSelectIdex <0) SearchSelectIdex = count-1;
                SetSearchSelect();


            }else if (e.Key == Key.Escape)
            {
                AllSearchPopup.IsOpen = false;
            }else if (e.Key == Key.Delete)
            {
                AllSearchTextBox.Text = "";
            }
        }

        private void SetSearchSelect()
        {
            for (int i = 0; i < SearchItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)SearchItemsControl.ItemContainerGenerator.ContainerFromItem(SearchItemsControl.Items[i]);
                StackPanel stackPanel = FindElementByName<StackPanel>(c, "SearchStackPanel");
                if (stackPanel != null)
                {
                    TextBlock textBlock = stackPanel.Children[0] as TextBlock;
                    if(i==SearchSelectIdex)
                        textBlock.Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
                    else
                        textBlock.Background =new SolidColorBrush(Colors.Transparent);
                }
            }

        }


        private void ShowMovieGrid(object sender, RoutedEventArgs e)
        {
            Grid_GAL.Visibility = Visibility.Hidden;
            Grid_Movie.Visibility = Visibility.Visible;
            ActorInfoGrid.Visibility = Visibility.Collapsed;

            BeginScanStackPanel.Visibility = Visibility.Hidden;
            
        }


        public  void ShowGenreGrid(object sender, RoutedEventArgs e)
        {
                Grid_Movie.Visibility = Visibility.Hidden;
                Grid_GAL.Visibility = Visibility.Visible;
                Grid_Genre.Visibility = Visibility.Visible;
                Grid_Actor.Visibility = Visibility.Hidden;
                Grid_Label.Visibility = Visibility.Hidden;
            this.vieModel.ClickGridType = 0;
            ActorToolsStackPanel.Visibility = Visibility.Hidden;
        }

        private void ShowActorGrid(object sender, RoutedEventArgs e)
        {
            Grid_Movie.Visibility = Visibility.Hidden;
            Grid_GAL.Visibility = Visibility.Visible;
            Grid_Genre.Visibility = Visibility.Hidden;
            Grid_Actor.Visibility = Visibility.Visible;
            Grid_Label.Visibility = Visibility.Hidden;
            this.vieModel.ClickGridType = 1;
            ActorToolsStackPanel.Visibility = Visibility.Visible;
        }

        private void ShowLabelGrid(object sender, RoutedEventArgs e)
        {
            Grid_Movie.Visibility = Visibility.Hidden;
            Grid_GAL.Visibility = Visibility.Visible;
            Grid_Genre.Visibility = Visibility.Hidden;
            Grid_Actor.Visibility = Visibility.Hidden;
            Grid_Label.Visibility = Visibility.Visible;
            this.vieModel.ClickGridType = 2;
            ActorToolsStackPanel.Visibility = Visibility.Hidden;
        }

        private void ShowLabelEditGrid(object sender, RoutedEventArgs e)
        {
            //LabelEditGrid.Visibility = Visibility.Visible;
        }

        public void Tag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            string tag = label.Content.ToString();
            vieModel = new VieModel_Main();
            vieModel.GetMoviebyTag(tag);
            this.DataContext = vieModel;
            vieModel.FlipOver();
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        public void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string type = sender.GetType().ToString();
            string tag = "";
            if(type== "System.Windows.Controls.Label")
            {
                Label Tag = (Label)sender;
                tag = Tag.Content.ToString();
                Match match = Regex.Match(tag, @"\( \d+ \)");
                if (match != null && match.Value!="")
                {
                    tag = tag.Replace(match.Value, "");
                }
            }
            else if(type== "HandyControl.Controls.Tag")
            {
                HandyControl.Controls.Tag Tag = (HandyControl.Controls.Tag)sender;
                tag = Tag.Content.ToString();
            }
            vieModel = new VieModel_Main();
            vieModel.GetMoviebyLabel(tag);
            this.DataContext = vieModel;
            vieModel.FlipOver();
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        public void Studio_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            string genre = label.Content.ToString();
            vieModel = new VieModel_Main();
            vieModel.GetMoviebyStudio(genre);
            this.DataContext = vieModel;
            vieModel.FlipOver();
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        public void Director_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            string genre = label.Content.ToString();
            vieModel = new VieModel_Main();
            vieModel.GetMoviebyDirector(genre);
            this.DataContext = vieModel;
            vieModel.FlipOver();
            ShowMovieGrid(sender, new RoutedEventArgs());
        }

        public void Genre_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            string genre = label.Content.ToString().Split('(')[0];
            vieModel = new VieModel_Main();
            vieModel.GetMoviebyGenre(genre);
            this.DataContext = vieModel;
            vieModel.FlipOver();
            ShowMovieGrid(sender, new RoutedEventArgs());
            vieModel.TextType = genre;
        }

        public void ShowActorMovieFromDetailWindow(Actress actress)
        {
            vieModel = new VieModel_Main();
            vieModel.GetMoviebyActress(actress);
            actress = DataBase.SelectInfoFromActress(actress);
            actress.smallimage = StaticClass.GetBitmapImage(actress.name, "Actresses");//不加载图片能节约 1s
            vieModel.Actress = actress;
            this.DataContext = vieModel;
            vieModel.FlipOver();
            ShowMovieGrid(this, new RoutedEventArgs());
            ActorInfoGrid.Visibility = Visibility.Visible;
        }

        public void ActorCheckBox_Click(object sender, RoutedEventArgs e)
        {
            SelectedActress.Clear();
            ActorSetSelected();
        }

        public void ActorSetSelected()
        {
            for (int i = 0; i < ActorItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ActorItemsControl.ItemContainerGenerator.ContainerFromItem(ActorItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "ActorWrapPanel");
                if (wrapPanel != null)
                {
                    Border border = wrapPanel.Children[0] as Border;
                    TextBox textBox = c.ContentTemplate.FindName("ActorNameTextBox", c) as TextBox;
                    if (textBox != null)
                    {
                        border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                        foreach (Actress actress in SelectedActress)
                        {
                            if (actress.name == textBox.Text.Split('(')[0])
                            {
                                border.Background = Brushes.LightGreen; break;
                            }
                        }
                    }
                }
            }

        }


        public void BorderMouseEnter(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode)
            {
                Border border = sender as Border;
                border.BorderBrush = (SolidColorBrush)Application.Current.Resources["Selected_BorderBrush"];
            }

        }

        public void BorderMouseLeave(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode)
            {
                Border border = sender as Border;
                border.BorderBrush = Brushes.Transparent;
            }
        }



        public void ActorBorderMouseEnter(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.ActorEditMode)
            {
                Border border = sender as Border;
                border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundTitle"];
            }

        }

        public void ActorBorderMouseLeave(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.ActorEditMode)
            {
                Border border = sender as Border;
                border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
            }
        }

        public void ShowSameActor(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            StackPanel sp = border.Child as StackPanel;
            TextBox textBox = sp.Children.OfType<TextBox>().First();
            string name = textBox.Text.Split('(')[0];


            if (Properties.Settings.Default.ActorEditMode)
            {
                foreach (Actress actress in vieModel.ActorList)
                {
                    if (actress.name == name)
                    {
                        if (SelectedActress.Contains(actress))
                            SelectedActress.Remove(actress);
                        else
                            SelectedActress.Add(actress);
                        break;
                    }

                }
                ActorSetSelected();
            }
            else
            {
                //await Task.Delay(50);
                Actress actress = new Actress();
                foreach (Actress item in vieModel.ActorList)
                {
                    if (item.name == name) { actress = DataBase.SelectInfoFromActress(item); break; }
                }
                vieModel = new VieModel_Main();
                vieModel.GetMoviebyActressAndVetioType(actress);
                vieModel.Actress = actress;
                this.DataContext = vieModel;
                vieModel.FlipOver();
                ShowMovieGrid(sender, new RoutedEventArgs());
                ActorInfoGrid.Visibility = Visibility.Visible;
                vieModel.TextType = actress.name;
            }
        }




        WindowDetails wd;
        private void ShowDetails(object sender, MouseEventArgs e)
        {
            if (Resizing) return;
            StackPanel parent = ((sender as FrameworkElement).Parent as Grid).Parent as StackPanel;
            var TB = parent.Children.OfType<TextBox>().First();//识别码
            if (Properties.Settings.Default.EditMode)
            {
                foreach (Movie movie in vieModel.CurrentMovieList)
                {
                    if (movie.id == TB.Text)
                    {

                        if (vieModel.SelectedMovie.Contains(movie))
                        {
                            vieModel.SelectedMovie.Remove(movie);
                        }
                        else
                        {
                            vieModel.SelectedMovie.Add(movie);
                        }
                        break;
                    }

                }
                SetSelected();
            }
            else
            {
                StopDownLoad();

                if (wd != null) { wd.Close(); }
                wd = new WindowDetails(TB.Text);
                wd.Show();
                //wd.AdjustWindow();
            }

        }


        public void ShowSideBar(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.ShowSideBar)
            {
                vieModel.ShowSideBar = false;
            }
            else { vieModel.ShowSideBar = true; }

        }



        public void ShowStatus(object sender, RoutedEventArgs e)
        {
            if (StatusPopup.IsOpen == true)
                StatusPopup.IsOpen = false;
            else
                StatusPopup.IsOpen = true;

        }

        public void ShowDownloadPopup(object sender, MouseButtonEventArgs e)
        {
            DownloadPopup.IsOpen = true;
        }

        public void ShowSortPopup(object sender, MouseButtonEventArgs e)
        {
            SortPopup.IsOpen = true;
        }

        public void ShowImagePopup(object sender, MouseButtonEventArgs e)
        {
            ImageSortPopup.IsOpen = true;
        }


        public void ShowMenu(object sender, MouseButtonEventArgs e)
        {
            Grid grid = sender as Grid;
            Popup popup = grid.Children.OfType<Popup>().First();
            popup.IsOpen = true;
        }




        public void ShowDownloadMenu(object sender, MouseButtonEventArgs e)
        {
            DownloadPopup.IsOpen = true;
        }





        public void ShowSearchMenu(object sender, MouseButtonEventArgs e)
        {
            SearchOptionPopup.IsOpen = true;
        }


        public void SetTypeValue(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if ((bool)radioButton.IsChecked)
            {
                if (radioButton.Content.ToString() == Properties.Settings.Default.TypeName1)
                    Properties.Settings.Default.VedioType = "步兵";
                else if(radioButton.Content.ToString() == Properties.Settings.Default.TypeName2)
                    Properties.Settings.Default.VedioType = "骑兵";
                else if (radioButton.Content.ToString() == Properties.Settings.Default.TypeName3)
                    Properties.Settings.Default.VedioType = "欧美";
                else
                    Properties.Settings.Default.VedioType = "所有";
                Properties.Settings.Default.Save();
            }

            vieModel.VedioType =(VedioType)Enum.Parse(typeof(VedioType),Properties.Settings.Default.VedioType);
        }


        public void ShowDownloadActorMenu(object sender, MouseButtonEventArgs e)
        {
            DownloadActorPopup.IsOpen = true;
        }



        public void SetSelected()
        {
            for (int i = 0; i < MovieItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)MovieItemsControl.ItemContainerGenerator.ContainerFromItem(MovieItemsControl.Items[i]);
                Border border = FindElementByName<Border>(c, "MovieBorder");
                if (border != null)
                {
                    TextBox textBox = c.ContentTemplate.FindName("idTextBox", c) as TextBox;
                    if (textBox != null)
                    {
                        border.Background = (SolidColorBrush)Application.Current.Resources["BackgroundSide"];
                        foreach (Movie movie in vieModel.SelectedMovie)
                        {
                            if (movie.id == textBox.Text)
                            {
                                border.Background = (SolidColorBrush)Application.Current.Resources["Selected_Background"];
                                Console.WriteLine(textBox.Text);
                                break;
                            }
                        }

                    }
                }

            }

        }

        public T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            if (element == null) return childElement;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, sChildName);

                if (childElement != null)
                    break;
            }

            return childElement;
        }

        private Panel GetItemsPanel(DependencyObject itemsControl)
        {
            ItemsPresenter itemsPresenter = GetBounds.GetVisualChild<ItemsPresenter>(itemsControl);
            Panel itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
            return itemsPanel;
        }






        public void SetSortValue(object sender, RoutedEventArgs e)
        {

            RadioButton rb = sender as RadioButton;
            vieModel.SortType = rb.Content.ToString();
            if (rb.Content.ToString() == vieModel.SortType)
                Properties.Settings.Default.SortDescending = !Properties.Settings.Default.SortDescending;

            vieModel.SortDescending = Properties.Settings.Default.SortDescending;
            Properties.Settings.Default.SortType = rb.Content.ToString();
            Properties.Settings.Default.Save();
            vieModel.SortType = Properties.Settings.Default.SortType;
            vieModel.Sort();
            if (vieModel.SortDescending)
                SortImage.Source = new BitmapImage(new Uri("/Resources/Picture/sort_down.png", UriKind.Relative));
            else
                SortImage.Source = new BitmapImage(new Uri("/Resources/Picture/sort_up.png", UriKind.Relative));

            vieModel.FlipOver();


        }

        public void SaveAllSearchType(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            Properties.Settings.Default.AllSearchType = radioButton.Content.ToString();
            vieModel?.GetSearchCandidate(AllSearchTextBox.Text);
        }


        public void SaveShowViewMode(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            Properties.Settings.Default.ShowViewMode = menuItem.Header.ToString();
            Properties.Settings.Default.Save();
            vieModel.Reset();
        }


        public void SaveShowImageMode(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            string mode = radioButton.Content.ToString();
            Properties.Settings.Default.ShowImageMode = mode;
            Properties.Settings.Default.Save();

            if (mode=="列表模式")
            {
                MovieMainGrid.Visibility = Visibility.Hidden;
                DetailGrid.Visibility = Visibility.Visible;
                vieModel.ShowDetailsData();
            }
            else
            {
                MovieMainGrid.Visibility = Visibility.Visible;
                DetailGrid.Visibility = Visibility.Hidden;
                vieModel.FlowNum = 0;
                vieModel.FlipOver();
            }

            if (mode == "缩略图")
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.SmallImage_Width;
            else if (mode == "海报图")
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.BigImage_Width;
            else if (mode == "预览图")
                Properties.Settings.Default.GlobalImageWidth = Properties.Settings.Default.ExtraImage_Width;






        }


        public List<ImageSlide> ImageSlides;
        public void Loadslide()
        {
            ImageSlides?.Clear();
            ImageSlides = new List<ImageSlide>();
            for (int i = 0; i < MovieItemsControl.Items.Count; i++)
            {
                ContentPresenter myContentPresenter = (ContentPresenter)MovieItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (myContentPresenter != null)
                {
                    Movie movie = (Movie)MovieItemsControl.Items[i];
                    DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                    Image myImage = (Image)myDataTemplate.FindName("myImage", myContentPresenter);
                    Image myImage2 = (Image)myDataTemplate.FindName("myImage2", myContentPresenter);

                    ImageSlide imageSlide = new ImageSlide(BasePicPath + $"ExtraPic\\{movie.id}", myImage, myImage2);
                    ImageSlides.Add(imageSlide);
                    imageSlide.PlaySlideShow();
                }

            }
        }






        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;

            if (sv.VerticalOffset == 0 && sv.VerticalOffset == sv.ScrollableHeight && vieModel.CurrentMovieList.Count < Properties.Settings.Default.DisplayNumber && !IsFlowing)
            {
                IsFlowing = true;
                FlowTimer.Start();
            }

        }


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //流动模式
            ScrollViewer sv = sender as ScrollViewer;
            if (sv.VerticalOffset >= 500)
                GoToTopCanvas.Visibility = Visibility.Visible;
            else
                GoToTopCanvas.Visibility = Visibility.Hidden;

            if (sv.ScrollableHeight - sv.VerticalOffset <= 10 && sv.VerticalOffset != 0)
            {

                if (vieModel.CurrentMovieList.Count < Properties.Settings.Default.DisplayNumber && vieModel.CurrentMovieList.Count < vieModel.MovieList.Count && vieModel.CurrentMovieList.Count + (vieModel.CurrentPage - 1) * Properties.Settings.Default.DisplayNumber < vieModel.MovieList.Count)
                {
                    IsFlowing = true;
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - 20);
                    FlowTimer.Start();
                }

            }
            if (Properties.Settings.Default.EditMode)
            {
                SetSelected();
            }

        }

        public bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        public void GotoTop(object sender, MouseButtonEventArgs e)
        {
            ScrollViewer.ScrollToTop();
        }

        public void PlayVedio(object sender, MouseButtonEventArgs e)
        {
            StackPanel parent = ((sender as FrameworkElement).Parent as Grid).Parent as StackPanel;
            var IDTb = parent.Children.OfType<TextBox>().First();
            string filepath = DataBase.SelectInfoByID("filepath", "movie", IDTb.Text);
            PlayVedioWithPlayer(filepath, IDTb.Text);

        }

        public void PlayVedioWithPlayer(string filepath,string ID)
        {
            if (File.Exists(filepath))
            {
                
                if (!string.IsNullOrEmpty(Properties.Settings.Default.VedioPlayerPath) && File.Exists(Properties.Settings.Default.VedioPlayerPath))
                {
                    try
                    {
                        Process.Start(Properties.Settings.Default.VedioPlayerPath, filepath);
                        vieModel.AddToRecentWatch(ID);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Logger.LogE(ex);
                        Process.Start(filepath);
                    }

                }
                else
                {
                    //使用默认播放器
                    Console.WriteLine("使用默认播放器");
                    Process.Start(filepath);
                    vieModel.AddToRecentWatch(ID);
                }
            }
            else
            {
                Console.WriteLine("无法打开");
                HandyControl.Controls.Growl.Error("无法打开 " + filepath,"Main");
            }
                
        }


        public void OpenImagePath(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                MenuItem _mnu = sender as MenuItem;
                MenuItem mnu = _mnu.Parent as MenuItem;

                StackPanel sp = null;
                if (mnu != null)
                {
                    int index = mnu.Items.IndexOf(_mnu);
                    sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                    if (Properties.Settings.Default.EditMode & vieModel.SelectedMovie.Count >= 2)
                        if (new Msgbox(this, $"是否打开选中的 {vieModel.SelectedMovie.Count}个所在文件夹？").ShowDialog() == false) { return; }

                    string failpath = ""; int num = 0;
                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                    {

                        string filepath = arg.filepath;
                        if (index == 0) { filepath = arg.filepath; }
                        else if (index == 1) { filepath = BasePicPath + $"BigPic\\{arg.id}.jpg"; }
                        else if (index == 2) { filepath = BasePicPath + $"SmallPic\\{arg.id}.jpg"; }
                        else if (index == 3) { filepath = BasePicPath + $"Gif\\{arg.id}.gif"; }
                        else if (index == 4) { filepath = BasePicPath + $"ExtraPic\\{arg.id}\\"; }
                        else if (index == 5) { filepath = BasePicPath + $"ScreenShot\\{arg.id}\\"; }
                        else if (index == 6) { if (arg.actor.Length > 0) filepath = BasePicPath + $"Actresses\\{arg.actor.Split(actorSplitDict[arg.vediotype])[0]}.jpg"; else filepath = ""; }

                        if (index == 4 | index == 5)
                        {
                            if (Directory.Exists(filepath)) { Process.Start("explorer.exe", "\"" + filepath + "\""); }
                            else
                            {
                                failpath += filepath + "\n";
                                num++;
                            }
                        }
                        else
                        {
                            if (File.Exists(filepath)) { Process.Start("explorer.exe", "/select, \"" + filepath + "\""); }
                            else
                            {
                                failpath += filepath + "\n";
                                num++;
                            }
                        }




                    });
                    if (failpath != "")
                        HandyControl.Controls.Growl.Info( $"成功打开{vieModel.SelectedMovie.Count - num}个，失败{num}个，因为不存在 ：\n{failpath}","Main");

                }

            }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public void OpenFilePath(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                MenuItem _mnu = sender as MenuItem;
                MenuItem mnu = _mnu.Parent as MenuItem;
                StackPanel sp = null;
                if (mnu != null)
                {
                    sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                    if (Properties.Settings.Default.EditMode & vieModel.SelectedMovie.Count >= 2)
                        if (new Msgbox(this, $"是否打开选中的 {vieModel.SelectedMovie.Count}个所在文件夹？").ShowDialog() == false) { return; }

                    string failpath = ""; int num = 0;
                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                    {
                        if (File.Exists(arg.filepath)) { Process.Start("explorer.exe", "/select, \"" + arg.filepath + "\""); }
                        else
                        {
                            failpath += arg.filepath + "\n";
                            num++;
                        }
                    });
                    if (failpath != "")
                        HandyControl.Controls.Growl.Info( $"成功打开{vieModel.SelectedMovie.Count - num}个，失败{num}个，因为文件夹不存在 ：\n{failpath}","Main");

                }

            }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }


        public async void TranslateMovie(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_TL_BAIDU & !Properties.Settings.Default.Enable_TL_YOUDAO) { HandyControl.Controls.Growl.Info( "请设置【有道翻译】并测试","Main"); return; }


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = ((MenuItem)(sender)).Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                string result = "";
                DB dataBase = new DB("Translate");


                int successNum = 0;
                int failNum = 0;
                int translatedNum = 0;

                foreach (Movie movie in vieModel.SelectedMovie)
                {

                    //检查是否已经翻译过，如有则跳过
                    if (!string.IsNullOrEmpty(dataBase.SelectInfoByID("translate_title", "youdao",movie.id))) { translatedNum++; continue; }
                    if (movie.title != "")
                    {

                        if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.title);
                        //保存
                        if (result != "")
                        {

                            dataBase.SaveYoudaoTranslateByID(movie.id, movie.title, result, "title");

                            //显示
                            int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                            int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                            movie.title = result;
                            try
                            {
                                vieModel.CurrentMovieList[index1] = null;
                                vieModel.MovieList[index2] = null;
                                vieModel.CurrentMovieList[index1] = movie;
                                vieModel.MovieList[index2] = movie;
                                successNum++;
                            }
                            catch (ArgumentNullException) {  }

                        }

                    }
                    else { failNum++; }

                    if (movie.plot != "")
                    {
                        if (Properties.Settings.Default.Enable_TL_YOUDAO) result = await Translate.Youdao(movie.plot);
                        //保存
                        if (result != "")
                        {
                            dataBase.SaveYoudaoTranslateByID(movie.id, movie.plot, result, "plot");
                            dataBase.CloseDB();
                        }

                    }

                }
                dataBase.CloseDB();

                HandyControl.Controls.Growl.Info( $"成功：{successNum}个，失败：{failNum}个，跳过：{translatedNum}个","Main");

            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }


        public void ClearSearch(object sender, MouseButtonEventArgs e)
        {
            AllSearchTextBox.Text = "";
        }

        public async void GenerateActor(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_BaiduAI) { HandyControl.Controls.Growl.Info( "请设置【百度 AI】并测试","Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {



                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);


                if (vieModel.SelectedMovie.Count > 3 && new Msgbox(this, $"预计用时 {(float)vieModel.SelectedMovie.Count / 2} s，是否继续？").ShowDialog() == false) return;

                this.Cursor = Cursors.Wait;
                int successNum = 0;

                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    if (movie.actor == "") continue;
                    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";

                    string name;
                    if (ActorInfoGrid.Visibility == Visibility.Visible)
                        name = vieModel.Actress.name;
                    else
                        name = movie.actor.Split(actorSplitDict[movie.vediotype])[0];


                    string ActressesPicPath = Properties.Settings.Default.BasePicPath + $"Actresses\\{name}.jpg";
                    if (File.Exists(BigPicPath))
                    {
                        Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);
                        if (int32Rect != Int32Rect.Empty)
                        {
                            await Task.Delay(500);
                            //切割演员头像
                            System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
                            BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
                            ImageSource actressImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetActressRect(bitmapImage, int32Rect));
                            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(actressImage);
                            try { bitmap.Save(ActressesPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++; }
                            catch (Exception ex) { Logger.LogE(ex); }
                        }
                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error($"海报图必须存在才能切割！","Main");
                    }
                }
                HandyControl.Controls.Growl.Info( $"成功切割 {successNum} / {vieModel.SelectedMovie.Count} 个头像","Main");
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }



        public void GetGif(object sender, RoutedEventArgs e)
        {

            HandyControl.Controls.Growl.Warning("暂时取消 Gif，等待后续更新","Main");
            return;

            if (Properties.Settings.Default.EditMode) {
                HandyControl.Controls.Growl.Warning("暂不支持批量！","Main");
                return; }

            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) { HandyControl.Controls.Growl.Info( "请设置 ffmpeg.exe 的路径 ","Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = ((MenuItem)(sender)).Parent as MenuItem;
            StackPanel sp = null;

            if (mnu != null)
            {
                int successNum = 0;
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                this.Cursor = Cursors.Wait;
                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    if (!File.Exists(movie.filepath)) {HandyControl.Controls.Growl.Warning( "视频不存在","Main"); continue; }
                    bool result = false;
                    try { GenerateGif(movie); } catch (Exception ex) { Logger.LogF(ex); }

                    if (result) successNum++;
                }
                //HandyControl.Controls.Growl.Info( $"成功截图 {successNum} / {vieModel.SelectedMovie.Count} 个影片","Main");
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }



        public async void GetScreenShot(object sender, RoutedEventArgs e)
        {

            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) { HandyControl.Controls.Growl.Info( "请设置 ffmpeg.exe 的路径 ","Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = ((MenuItem)(sender)).Parent as MenuItem;
            StackPanel sp = null;

            if (mnu != null)
            {
                int successNum = 0;
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                this.Cursor = Cursors.Wait;
                cmdTextBox.Text = "";
                cmdGrid.Visibility = Visibility.Visible;
                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    if (!File.Exists(movie.filepath)) { HandyControl.Controls.Growl.Error( "视频不存在","Main"); continue; }
                    bool success = false;
                    string message = "";
                    (success, message) =  await ScreenShot(movie);
                    if (success) successNum++;
                    else this.Dispatcher.Invoke((Action)delegate { cmdTextBox.AppendText($"截图失败，原因：{message}"); });
                }
                HandyControl.Controls.Growl.Info($"成功截图 {successNum} / {vieModel.SelectedMovie.Count} 个影片", "Main");
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;

            //if (Properties.Settings.Default.ScreenShotToExtraPicPath)
            //    HandyControl.Controls.Growl.Info("开始截图到【预览图】","Main");
            //else
            //    HandyControl.Controls.Growl.Warning("开始截图到【影片截图】","Main");

        }



        public void BeginGenGif(object o)
        {
            List<object> list = o as List<object>;
            string cutoffTime = list[0] as string;
            string filePath = list[1] as string;
            string ScreenShotPath = list[2] as string;
            string ID = list[3] as string;
            ScreenShotPath += ID + ".gif";

            if (string.IsNullOrEmpty(cutoffTime)) return;
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            string str = $"\"{Properties.Settings.Default.FFMPEG_Path}\" -y -t 5 -ss {cutoffTime} -i \"{filePath}\" -s 280x170  \"{ScreenShotPath}\"";
            Console.WriteLine(str);


            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            App.Current.Dispatcher.Invoke((Action)delegate {
                //显示到界面上
                Movie movie = vieModel.MovieList.Where(arg => arg.id == ID).First();
                int idx1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == ID).First());
                int idx2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == ID).First());

                //movie.gif = null;
                //if (File.Exists(BasePicPath + $"Gif\\{movie.id}.gif"))
                //    movie.gif = new Uri("pack://siteoforigin:,,,/" + BasePicPath.Replace("\\", "/") + $"Gif/{movie.id}.gif");
                //else
                //    movie.gif = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_B.gif");


                try
                {
                    vieModel.CurrentMovieList[idx1] = null;
                    vieModel.MovieList[idx2] = null;
                    vieModel.CurrentMovieList[idx1] = movie;
                    vieModel.MovieList[idx2] = movie;
                }
                catch(ArgumentNullException ex) { }

                HandyControl.Controls.Growl.Info("成功生成 Gif","Main");

            });
        }

        public void BeginScreenShot(object o)
        {
            List<object> list=o as List<object>;
            string cutoffTime = list[0] as string;
            string i= list[1] as string;
            string filePath = list[2] as string;
            string ScreenShotPath = list[3] as string;

            if (string.IsNullOrEmpty(cutoffTime)) return;
            SemaphoreScreenShot.WaitOne();

            //--使用 ffmpeg.exe 截图
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = Properties.Settings.Default.FFMPEG_Path;
            startInfo.CreateNoWindow = true;
            string str= $"-y -threads 1 -ss {cutoffTime} -i \"{filePath}\" -f image2 -frames:v 1 \"{ScreenShotPath}\\ScreenShot-{i.PadLeft(2, '0')}.jpg\"";
            startInfo.UseShellExecute = false;
            startInfo.Arguments = str;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process.StartInfo = startInfo;
            process.Start();
            StreamReader readerOut = process.StandardOutput;
            StreamReader readerErr = process.StandardError;
            string errors = readerErr.ReadToEnd();
            string output = readerOut.ReadToEnd();
            while (!process.HasExited) { continue; }
            //--使用 ffmpeg.exe 截图
            SemaphoreScreenShot.Release();
            App.Current.Dispatcher.Invoke((Action)delegate { cmdTextBox.AppendText(str + "\n"); cmdTextBox.ScrollToEnd(); });
            lock (ScreenShotLockObject) { ScreenShotCurrent += 1; }
        }


        public Semaphore SemaphoreScreenShot;

        public int ScreenShotTotal = 0;
        public int ScreenShotCurrent = 0;
        public object ScreenShotLockObject = 0;

        public async Task<(bool,string)> ScreenShot(Movie movie)
        {
            bool result = true;
            string message = "";
            List<string> outputPath = new List<string>();
            await Task.Run(() => {
                // n 个线程截图
                if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) { result = false; message = "未配置 FFmpeg.exe 路径";return; } 

                int num = Properties.Settings.Default.ScreenShot_ThreadNum;
                string ScreenShotPath = "";
                if (Properties.Settings.Default.ScreenShotToExtraPicPath) ScreenShotPath = BasePicPath + "ExtraPic\\" + movie.id;
                else ScreenShotPath = BasePicPath + "ScreenShot\\" + movie.id;

                if (!Directory.Exists(ScreenShotPath)) Directory.CreateDirectory(ScreenShotPath);

                string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组
                int SemaphoreNum = cutoffArray.Length > 10 ? 10 : cutoffArray.Length;//最多 10 个线程截图
                SemaphoreScreenShot = new Semaphore(SemaphoreNum, SemaphoreNum);

                if (cutoffArray.Count() == 0) { result = false; message = "未成功分割影片截图"; return; }

                ScreenShotCurrent = 0;
                ScreenShotTotal = cutoffArray.Count();
                ScreenShotLockObject = new object();
                
                for (int i = 0; i < cutoffArray.Count(); i++)
                {
                    outputPath.Add($"{ScreenShotPath}\\ScreenShot-{i.ToString().PadLeft(2, '0')}.jpg");
                    List<object> list = new List<object>() { cutoffArray[i], i.ToString(), movie.filepath, ScreenShotPath };
                    Thread threadObject = new Thread(BeginScreenShot);
                    threadObject.Start(list);
                }

                //等待直到所有线程结束
                while (ScreenShotCurrent != ScreenShotTotal)
                {
                     Task.Delay(100).Wait();
                }
                //cmdTextBox.AppendText($"已启用 {cutoffArray.Count()} 个线程， 3-10S 后即可截图成功\n");
            });
            foreach (var item in outputPath)
            {
                if(!File.Exists(item))
                {
                    result = false;
                    message = $"未成功生成 {item}";
                    break;
                }
            }
            return (result, message);
        }


        public void GenerateGif(Movie movie)
        {
            if (!File.Exists(Properties.Settings.Default.FFMPEG_Path)) return;


            string GifSavePath = BasePicPath + "Gif\\";
            if (!Directory.Exists(GifSavePath)) Directory.CreateDirectory(GifSavePath);



            string[] cutoffArray = MediaParse.GetCutOffArray(movie.filepath); //获得影片长度数组

            string startTime = cutoffArray[new Random().Next(cutoffArray.Length)];

            List<object> list = new List<object>() { startTime, movie.filepath, GifSavePath , movie.id };
            Thread threadObject = new Thread(BeginGenGif);
            threadObject.Start(list);

        }

        public async void GenerateSmallImage(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.Enable_BaiduAI) { HandyControl.Controls.Growl.Info( "请设置【百度 AI】并测试","Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                if (vieModel.SelectedMovie.Count > 3 && new Msgbox(this, $"预计用时 {(float)vieModel.SelectedMovie.Count / 2} s，是否继续？").ShowDialog() == false) return;
                int successNum = 0;
                this.Cursor = Cursors.Wait;
                foreach (Movie movie in vieModel.SelectedMovie)
                {
                    string BigPicPath = Properties.Settings.Default.BasePicPath + $"BigPic\\{movie.id}.jpg";
                    string SmallPicPath = Properties.Settings.Default.BasePicPath + $"SmallPic\\{movie.id}.jpg";
                    if (File.Exists(BigPicPath))
                    {
                        Int32Rect int32Rect = await FaceDetect.GetAIResult(movie, BigPicPath);

                        if (int32Rect != Int32Rect.Empty)
                        {
                            await Task.Delay(500);
                            //切割缩略图
                            System.Drawing.Bitmap SourceBitmap = new System.Drawing.Bitmap(BigPicPath);
                            BitmapImage bitmapImage = ImageProcess.BitmapToBitmapImage(SourceBitmap);
                            ImageSource smallImage = ImageProcess.CutImage(bitmapImage, ImageProcess.GetRect(bitmapImage, int32Rect));
                            System.Drawing.Bitmap bitmap = ImageProcess.ImageSourceToBitmap(smallImage);
                            try
                            {
                                bitmap.Save(SmallPicPath, System.Drawing.Imaging.ImageFormat.Jpeg); successNum++;
                            }
                            catch (Exception ex) { Logger.LogE(ex); }


                            //读取
                            int index1 = vieModel.CurrentMovieList.IndexOf(movie);
                            int index2 = vieModel.MovieList.IndexOf(movie);
                            movie.smallimage = StaticClass.GetBitmapImage(movie.id, "SmallPic");
                            vieModel.CurrentMovieList[index1] = null;
                            vieModel.MovieList[index2] = null;
                            vieModel.CurrentMovieList[index1] = movie;
                            vieModel.MovieList[index2] = movie;
                        }

                    }
                    else
                    {
                        HandyControl.Controls.Growl.Error($"海报图必须存在才能切割！","Main");
                    }

                }
                HandyControl.Controls.Growl.Info( $"成功切割 {successNum} / {vieModel.SelectedMovie.Count} 个缩略图","Main");


            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            this.Cursor = Cursors.Arrow;
        }


        public void RenameFile(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.RenameFormat.IndexOf("{") < 0)
            {
                HandyControl.Controls.Growl.Error("请在设置中配置【重命名】规则","Main");
                return;
            }


            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                StringCollection paths = new StringCollection();
                int num = 0;
                vieModel.SelectedMovie.ToList().ForEach(arg => { if (File.Exists(arg.filepath)) { paths.Add(arg.filepath);  } });
                if (paths.Count > 0)
                {
                        //重命名文件
                        foreach (Movie m in vieModel.SelectedMovie)
                        {
                            if (!File.Exists(m.filepath)) continue;
                            DetailMovie movie = DataBase.SelectDetailMovieById(m.id);
                            //try
                            //{
                                string[] newPath = movie.ToFileName();
                                if (movie.hassubsection)
                                {
                                    for (int i = 0; i < newPath.Length; i++)
                                    {
                                        File.Move(movie.subsectionlist[i], newPath[i]);
                                    }
                                    num++;

                                    //显示
                                    int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                                    int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                                    movie.filepath = newPath[0];
                                    movie.subsection = string.Join(";", newPath);
                                    try
                                    {
                                        vieModel.CurrentMovieList[index1].filepath = movie.filepath;
                                        vieModel.MovieList[index2].filepath = movie.filepath;
                                        vieModel.CurrentMovieList[index1].subsection = movie.subsection;
                                        vieModel.MovieList[index2].subsection = movie.subsection;
                                    }
                                    catch (ArgumentNullException) { }
                                    DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                                    DataBase.UpdateMovieByID(movie.id, "subsection", movie.subsection, "string");//保存
                                    if (vieModel.SelectedMovie.Count == 1) HandyControl.Controls.Growl.Success("重命名成功！","Main");
                                }
                                else
                                {
                                        if (!File.Exists(newPath[0]))
                                        {
                                            File.Move(movie.filepath, newPath[0]);
                                            num++;
                                            //显示
                                            int index1 = vieModel.CurrentMovieList.IndexOf(vieModel.CurrentMovieList.Where(arg => arg.id == movie.id).First()); ;
                                            int index2 = vieModel.MovieList.IndexOf(vieModel.MovieList.Where(arg => arg.id == movie.id).First());
                                            movie.filepath = newPath[0];
                                            try
                                            {
                                                vieModel.CurrentMovieList[index1].filepath = movie.filepath;
                                                vieModel.MovieList[index2].filepath = movie.filepath;
                                            }
                                            catch (ArgumentNullException) { }
                                            DataBase.UpdateMovieByID(movie.id, "filepath", movie.filepath, "string");//保存
                                            if (vieModel.SelectedMovie.Count == 1) HandyControl.Controls.Growl.Success("重命名成功！", "Main");
                                    }
                                    else
                                    {
                                        HandyControl.Controls.Growl.Error("重命名失败！存在同名文件", "Main");
                                    }

                                }


                            //}catch(Exception ex)
                            //{
                            //    HandyControl.Controls.Growl.Error(ex.Message);
                            //    continue;
                            //}
                        }
                        HandyControl.Controls.Growl.Info($"已重命名 {num}/{vieModel.SelectedMovie.Count} 个文件", "Main");
                }
                else
                {
                    HandyControl.Controls.Growl.Info($"文件不存在！无法重命名！ ","Main");
                }



            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }


        



        public void CopyFile(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                StringCollection paths = new StringCollection();
                int num = 0;
                vieModel.SelectedMovie.ToList().ForEach(arg => { if (File.Exists(arg.filepath)) { paths.Add(arg.filepath); num++; } });
                if (paths.Count > 0)
                {
                    try
                    {
                        Clipboard.SetFileDropList(paths);
                        HandyControl.Controls.Growl.Info( $"已复制 {num}/{vieModel.SelectedMovie.Count} 个文件","Main");
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
                else
                {
                    HandyControl.Controls.Growl.Info( $"文件不存在！无法复制！ ","Main");
                }



            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public void DeleteFile(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                if (Properties.Settings.Default.EditMode)
                    if (new Msgbox(this, $"是否确认删除选中的 {vieModel.SelectedMovie.Count}个视频？").ShowDialog() == false) { return; }

                int num = 0;
                vieModel.SelectedMovie.ToList().ForEach(arg =>
                {
                    
                    if (arg.subsectionlist.Count > 0)
                    {
                        //分段视频
                        arg.subsectionlist.ForEach(path => {
                            if (File.Exists(path))
                            {
                                try
                                {
                                    FileSystem.DeleteFile(path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                    num++;
                                }
                                catch (Exception ex) { Logger.LogF(ex); }
                            }
                        });
                    }
                    else
                    {
                        if (File.Exists(arg.filepath))
                        {
                            try
                            {
                                FileSystem.DeleteFile(arg.filepath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                                num++;
                            }
                            catch (Exception ex) { Logger.LogF(ex); }

                        }
                    }




                });

                HandyControl.Controls.Growl.Info($"已删除 {num}/{vieModel.SelectedMovie.Count}个视频到回收站","Main");

                if (num > 0 && Properties.Settings.Default.DelInfoAfterDelFile)
                {
                    try
                    {
                        vieModel.SelectedMovie.ToList().ForEach(arg => {
                            DataBase.DelInfoByType("movie", "id", arg.id);
                            vieModel.CurrentMovieList.Remove(arg); //从主界面删除
                            vieModel.MovieList.Remove(arg);
                        });

                        //从详情窗口删除
                        if (Jvedio.GetWindow.Get("WindowDetails") != null)
                        {
                            WindowDetails windowDetails = Jvedio.GetWindow.Get("WindowDetails") as WindowDetails;
                            foreach (var item in vieModel.SelectedMovie.ToList())
                            {
                                if (windowDetails.vieModel.DetailMovie.id == item.id)
                                {
                                    windowDetails.Close();
                                    break;
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    vieModel.Statistic();
                }


            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public WindowEdit WindowEdit;


        public void EditInfo(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode) { HandyControl.Controls.Growl.Info( "编辑模式不可批量修改信息！","Main"); return; }
            if (DownLoader?.State == DownLoadState.DownLoading) { HandyControl.Controls.Growl.Warning( "请等待下载完成！","Main"); return; }
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                string id = TB.Text;
                if (WindowEdit != null) { WindowEdit.Close(); }
                WindowEdit = new WindowEdit(id);
                WindowEdit.ShowDialog();
            }
        }

        public async void DeleteID(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading) { HandyControl.Controls.Growl.Warning( "请等待下载完成！","Main"); return; }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                if (sp != null)
                {

              
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                if (Properties.Settings.Default.EditMode)
                    if (new Msgbox(this, $"是否从数据库删除 {vieModel.SelectedMovie.Count}个视频？（保留文件）").ShowDialog() == false) { return; }

                vieModel.SelectedMovie.ToList().ForEach(arg => {
                    DataBase.DelInfoByType("movie", "id", arg.id); 
                    vieModel.CurrentMovieList.Remove(arg); //从主界面删除
                    vieModel.MovieList.Remove(arg);
                });

                //从详情窗口删除
                if (Jvedio.GetWindow.Get("WindowDetails") != null)
                {
                    WindowDetails windowDetails = Jvedio.GetWindow.Get("WindowDetails") as WindowDetails;
                    foreach (var item in vieModel.SelectedMovie.ToList())
                    {
                        if (windowDetails.vieModel.DetailMovie.id == item.id)
                        {
                            windowDetails.Close();
                            break;
                        }
                    }
                }

                HandyControl.Controls.Growl.Info( $"已从数据库删除 {vieModel.SelectedMovie.Count}个视频 ","Main");

                vieModel.SelectedMovie.Clear();
                vieModel.Statistic();
               
                await Task.Run(() => { Task.Delay(1000).Wait(); });
                }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        public Movie GetMovieFromVieModel(string id)
        {
            foreach (Movie movie in vieModel.CurrentMovieList)
            {
                if (movie.id == id)
                {
                    return movie;
                }
            }
            return null;
        }

        public Actress GetActressFromVieModel(string name)
        {
            foreach (Actress actress in vieModel.ActorList)
            {
                if (actress.name == name)
                {
                    return actress;
                }
            }
            return null;
        }

        public string GetFormatGenreString(List<Movie> movies, string type = "genre")
        {
            List<string> list = new List<string>();
            if (type == "genre")
            {
                movies.ForEach(arg =>
                {
                    foreach (var item in arg.genre.Split(' '))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }
            else if (type == "label")
            {
                movies.ForEach(arg =>
                {
                    foreach (var item in arg.label.Split(' '))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }
            else if (type == "actor")
            {

                movies.ForEach(arg =>
                {

                    foreach (var item in arg.actor.Split(actorSplitDict[arg.vediotype]))
                    {
                        if (!string.IsNullOrEmpty(item) & item.IndexOf(' ') < 0)
                            if (!list.Contains(item)) list.Add(item);
                    }
                });
            }

            string result = "";
            list.ForEach(arg => { result += arg + " "; });
            return result;
        }


        //清空标签
        public void ClearLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                foreach (var movie in this.vieModel.MovieList)
                {
                    foreach (var item in vieModel.SelectedMovie)
                    {
                        if (item.id == movie.id)
                        {
                            DataBase.UpdateMovieByID(item.id, "label", "", "String");
                            break;
                        }
                    }

                }
                HandyControl.Controls.Growl.Info( $"成功清空标签","Main");

                vieModel.GetLabelList();

            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

        }

        

        //删除单个影片标签
        public void DelSingleLabel(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.EditMode)
            {
                HandyControl.Controls.Growl.Info("不支持批量","Main");
                return;
            }
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                DetailMovie CurrentMovie = DataBase.SelectDetailMovieById(TB.Text);
                LabelDelGrid.Visibility = Visibility.Visible;
                vieModel.CurrentMovieLabelList = new List<string>();
                vieModel.CurrentMovieLabelList = CurrentMovie.label.Split(' ').ToList();
                CurrentLabelMovie = CurrentMovie;
                LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;

            }
        }


        //删除多个影片标签
        public void DelLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                //string TotalLabel = GetFormatGenreString(vieModel.SelectedMovie,"label");
                var di = new DialogInput(this, "请输入需删除的标签，每个标签空格隔开", "");
                di.ShowDialog();
                if (di.DialogResult == true & di.Text != "")
                {
                    foreach (var movie in this.vieModel.MovieList)
                    {
                        foreach (var item in vieModel.SelectedMovie)
                        {
                            if (item.id == movie.id)
                            {
                                List<string> originlabel = LabelToList(movie.label);
                                List<string> newlabel = LabelToList(di.Text);
                                movie.label = string.Join(" ", originlabel.Except(newlabel).ToList());
                                DataBase.UpdateMovieByID(item.id, "label", movie.label, "String");
                                break;
                            }
                        }

                    }
                    HandyControl.Controls.Growl.Info( $"成功删除标签{di.Text}","Main");
                    vieModel.GetLabelList();
                }

            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }

        //增加标签
        public void AddLabel(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);
                var di = new DialogInput(this, "请输入需添加的标签，每个标签空格隔开", "");
                di.ShowDialog();
                if (di.DialogResult == true & di.Text != "")
                {
                    foreach (var movie in this.vieModel.MovieList)
                    {
                        foreach (var item in vieModel.SelectedMovie)
                        {
                            if (item.id == movie.id)
                            {
                                List<string> originlabel = LabelToList(movie.label);
                                List<string> newlabel = LabelToList(di.Text);
                                movie.label = string.Join(" ", originlabel.Union(newlabel).ToList());
                                originlabel.ForEach(arg => Console.WriteLine(arg));
                                newlabel.ForEach(arg => Console.WriteLine(arg));
                                originlabel.Union(newlabel).ToList().ForEach(arg => Console.WriteLine(arg));
                                DataBase.UpdateMovieByID(item.id, "label", movie.label, "String");
                                break;
                            }
                        }

                    }
                    HandyControl.Controls.Growl.Info( $"成功增加标签{di.Text}", "Main");

                    vieModel.GetLabelList();

                }

            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }





        public List<string> LabelToList(string label)
        {

            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(label)) return result;
            if (label.IndexOf(' ') > 0)
            {
                foreach (var item in label.Split(' '))
                {
                    if (item.Length > 0)
                        if (!result.Contains(item)) result.Add(item);
                }
            }
            else { if (label.Length > 0) result.Add(label.Replace(" ", "")); }
            return result;
        }

        //设置喜爱
        public void SetFavorites(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
            MenuItem mnu = sender as MenuItem;
            int favorites = int.Parse(mnu.Header.ToString());
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)((MenuItem)mnu.Parent).Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                foreach (var movie in this.vieModel.MovieList)
                {
                    foreach (var item in vieModel.SelectedMovie)
                    {
                        if (item.id == movie.id)
                        {
                            movie.favorites = favorites;
                            DataBase.UpdateMovieByID(item.id, "favorites", favorites);
                            break;
                        }
                    }
                }


            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

        }

        //打开网址
        private void OpenWeb(object sender, RoutedEventArgs e)
        {

                if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                MenuItem mnu = sender as MenuItem;
                if (mnu != null)
                {
                    StackPanel sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);


                    if (Properties.Settings.Default.EditMode & vieModel.SelectedMovie.Count >= 2)
                        if (new Msgbox(this, $"是否打开选中的 {vieModel.SelectedMovie.Count}个网站？").ShowDialog() == false) { return; }

                    vieModel.SelectedMovie.ToList().ForEach(arg =>
                    {
                        if (!string.IsNullOrEmpty(arg.sourceurl) && arg.sourceurl.IndexOf("http")>=0)
                        {
                            try
                            {
                                Process.Start(arg.sourceurl);
                            }catch(Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "Main"); }
                            
                        }
                        else
                        {
                            //为空则使用 bus 打开
                            if (!string.IsNullOrEmpty(Properties.Settings.Default.Bus) && Properties.Settings.Default.Bus.IndexOf("http") >= 0)
                            {
                                try
                                {
                                    Process.Start(Properties.Settings.Default.Bus + arg.id);
                                }
                                catch (Exception ex) { HandyControl.Controls.Growl.Error(ex.Message, "Main"); }
                            }
                            else
                            {
                                HandyControl.Controls.Growl.Error("同步信息的服务器源未设置！","Main");
                            }

                        }
                    });
                }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }




        private void DownLoadSelectMovie(object sender, RoutedEventArgs e)
        {
            if (DownLoader?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info("已有任务在下载！","Main");
            }else if (!IsServersProper())
            {
                HandyControl.Controls.Growl.Error("请在设置【同步信息】中添加服务器源并启用！","Main");
            }
                
            else
            {
                try
                {
                    if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();

                    //MenuItem _mnu = sender as MenuItem;
                    MenuItem mnu = sender as MenuItem;
                    StackPanel sp = null;

                    if (mnu != null)
                    {
                        sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                        var TB = sp.Children.OfType<TextBox>().First();
                        Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                        if (CurrentMovie != null)
                        {
                            if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);

                            StartDownload(vieModel.SelectedMovie.ToList());
                        }

                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.StackTrace); Console.WriteLine(ex.Message); }
            }
            if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
        }


        private void Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ImageSlides == null) return;
            Canvas canvas = (Canvas)sender;
            TextBlock textBlock = canvas.Children.OfType<TextBlock>().First();
            int index = int.Parse(textBlock.Text);
            if (index < ImageSlides.Count)
            {
                ImageSlides[index].PlaySlideShow();
                ImageSlides[index].Start();
            }

        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImageSlides == null) return;
            Canvas canvas = (Canvas)sender;
            TextBlock textBlock = canvas.Children.OfType<TextBlock>().First();
            int index = int.Parse(textBlock.Text);
            if (index < ImageSlides.Count)
            {
                ImageSlides[index].Stop();
            }

        }



        private void EditActress(object sender, MouseButtonEventArgs e)
        {
            vieModel.EnableEditActress = !vieModel.EnableEditActress;
        }

        private void SaveActress(object sender,KeyEventArgs e)
        {
            if (vieModel.EnableEditActress)
            {
                if (e.Key == Key.Enter)
                {
                    //ScrollViewer.Focus();
                    vieModel.EnableEditActress = false;
                    DataBase.InsertActress(vieModel.Actress);
                }
            }
            
        }

        private void BeginDownLoadActress(object sender, MouseButtonEventArgs e)
        {
            List<Actress> actresses = new List<Actress>();
            actresses.Add(vieModel.Actress);
            DownLoadActress downLoadActress = new DownLoadActress(actresses);
            downLoadActress.BeginDownLoad();
            downLoadActress.InfoUpdate += (s, ev) =>
            {
                ActressUpdateEventArgs actressUpdateEventArgs = ev as ActressUpdateEventArgs;
                try
                {
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        vieModel.Actress = null;
                        vieModel.Actress = actressUpdateEventArgs.Actress;
                        downLoadActress.State = DownLoadState.Completed;
                    });
                }
                catch (TaskCanceledException ex) { Logger.LogE(ex); }

            };
        }



        private void ProgressBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ProgressBar PB = sender as ProgressBar;
            if (PB.Value + PB.LargeChange <= PB.Maximum)
            {
                PB.Value += PB.LargeChange;
            }
            else
            {
                PB.Value = PB.Minimum;
            }
        }

        private void DelLabel(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            StackPanel stackPanel = border.Parent as StackPanel;

            Console.WriteLine(stackPanel.Parent.GetType().ToString());

        }
        


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                if (this.WindowState == WindowState.Normal) WinState = JvedioWindowState.Normal;
                else if (this.WindowState == WindowState.Maximized) WinState = JvedioWindowState.FullScreen;
                else if (this.Width == SystemParameters.WorkArea.Width & this.Height == SystemParameters.WorkArea.Height) WinState = JvedioWindowState.Maximized;

                WindowConfig cj = new WindowConfig(this.GetType().Name);
                cj.Save(new WindowProperty() { Location = new Point(this.Left, this.Top), Size = new Size(this.Width, this.Height), WinState = WinState });
            }
            Properties.Settings.Default.EditMode = false;
            Properties.Settings.Default.ActorEditMode = false;
            Properties.Settings.Default.Save();

            if (!IsToUpdate && Properties.Settings.Default.CloseToTaskBar && this.IsVisible == true)
            {
                e.Cancel = true;
                NotifyIcon.Visibility = Visibility.Visible;
                this.Hide();
                WindowSet?.Hide();
            }


        }

        private void ActorTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (e.Key == Key.Enter)
            {
                string pagestring = ((TextBox)sender).Text;
                int page = 1;
                if (pagestring == null) { page = 1; }
                else
                {
                    var isnumeric = int.TryParse(pagestring, out page);
                }
                if (page > vieModel.TotalActorPage) { page = vieModel.TotalActorPage; } else if (page <= 0) { page = 1; }
                vieModel.CurrentActorPage = page;
                vieModel.ActorFlipOver();
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (vieModel.TotalPage <= 1) return;
            if (e.Key == Key.Enter)
            {
                string pagestring = ((TextBox)sender).Text;
                int page = 1;
                if (pagestring == null) { page = 1; }
                else
                {
                    var isnumeric = int.TryParse(pagestring, out page);
                }
                if (page > vieModel.TotalPage) { page = vieModel.TotalPage; } else if (page <= 0) { page = 1; }
                vieModel.CurrentPage = page;
                vieModel.FlipOver();
            }
        }


        public void StopDownLoad()
        {
            if (DownLoader!=null && DownLoader.State==DownLoadState.DownLoading) HandyControl.Controls.Growl.Warning("已停止同步信息！","Main");
            DownLoader?.CancelDownload();
            downLoadActress?.CancelDownload();
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                ProgressBar.Visibility = Visibility.Hidden;

            });
            

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }


        private  void PreviousActorPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (vieModel.CurrentActorPage - 1 <= 0)
                vieModel.CurrentActorPage = vieModel.TotalActorPage;
            else
                vieModel.CurrentActorPage -= 1;
            vieModel.ActorFlipOver();
            
        }

        private void NextActorPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalActorPage <= 1) return;
            if (vieModel.CurrentActorPage + 1 > vieModel.TotalActorPage)
                vieModel.CurrentActorPage = 1;
            else
                vieModel.CurrentActorPage += 1;
            vieModel.ActorFlipOver();
        }



        private void PreviousPage(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.TotalPage <= 1 || vieModel.IsFlipOvering) return;
            FlipoverTimer.Stop();
            if (vieModel.CurrentPage - 1 <= 0)
                vieModel.CurrentPage = vieModel.TotalPage;
            else
                vieModel.CurrentPage -= 1;
            FlipoverTimer.Start();
            
 
        }

        private void NextPage(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("IsFlipOvering=" + vieModel.IsFlipOvering);
            vieModel.CurrentMovieList = new ObservableCollection<Movie>();
            if (vieModel.TotalPage <= 1 || vieModel.IsFlipOvering) return;
            FlipoverTimer.Stop();
            if (vieModel.CurrentPage + 1 > vieModel.TotalPage)
                vieModel.CurrentPage = 1;
            else
                vieModel.CurrentPage += 1;
            FlipoverTimer.Start();
            
        }






        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
        }


        private void ActorGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A & Properties.Settings.Default.ActorEditMode)
            {
                foreach (var item in vieModel.ActorList)
                {
                    if (!SelectedActress.Contains(item))
                    {
                        SelectedActress.Add(item);

                    }
                }
                ActorSetSelected();
            }
        }


        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.A & Properties.Settings.Default.EditMode)
            {
                foreach (var item in vieModel.CurrentMovieList)
                {
                    if (!vieModel.SelectedMovie.Contains(item))
                    {
                        vieModel.SelectedMovie.Add(item);
                    }
                }
                SetSelected();
            }

        }

        public void StopDownLoadActress(object sender, RoutedEventArgs e)
        {
            DownloadActorPopup.IsOpen = false;
            downLoadActress?.CancelDownload();
            HandyControl.Controls.Growl.Info( "已停止所有任务","Main");
        }

        public void DownLoadSelectedActor(object sender, RoutedEventArgs e)
        {
            if (downLoadActress?.State == DownLoadState.DownLoading)
            {
                HandyControl.Controls.Growl.Info( "已有任务在下载！","Main"); return;
            }

            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
            MenuItem mnu = sender as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                var TB = sp.Children.OfType<TextBox>().First();
                string name = TB.Text.Split('(')[0];
                Actress CurrentActress = GetActressFromVieModel(name);
                if (!SelectedActress.Select(g => g.name).ToList().Contains(CurrentActress.name)) SelectedActress.Add(CurrentActress);
                StartDownLoadActor(SelectedActress);

            }
            if (!Properties.Settings.Default.ActorEditMode) SelectedActress.Clear();
        }

        public void SelectAllActor(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ActorEditMode) { ActorCancelSelect(); return; }
            Properties.Settings.Default.ActorEditMode = true;
            foreach (var item in vieModel.CurrentActorList)
                if (!SelectedActress.Contains(item)) SelectedActress.Add(item);

            ActorSetSelected();
        }

        public void ActorCancelSelect()
        {
            Properties.Settings.Default.ActorEditMode = false; SelectedActress.Clear(); ActorSetSelected();
        }

        public void RefreshCurrentActressPage(object sender, RoutedEventArgs e)
        {
            ActorCancelSelect();
            vieModel.RefreshActor();
        }

        public void StartDownLoadActor(List<Actress> actresses)
        {
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "BusActress.sqlite"))return;

            downLoadActress = new DownLoadActress(actresses);
            downLoadActress?.BeginDownLoad();
            try
            {
                downLoadActress.InfoUpdate += (s, ev) =>
                {
                    ActressUpdateEventArgs actressUpdateEventArgs = ev as ActressUpdateEventArgs;
                    for (int i = 0; i < vieModel.ActorList.Count; i++)
                    {
                        if (vieModel.ActorList[i].name == actressUpdateEventArgs.Actress.name)
                        {
                            try
                            {
                                Dispatcher.Invoke((Action)delegate ()
                                {
                                    vieModel.ActorList[i] = actressUpdateEventArgs.Actress;
                                    ProgressBar.Value = actressUpdateEventArgs.progressBarUpdate.value / actressUpdateEventArgs.progressBarUpdate.maximum * 100; ProgressBar.Visibility = Visibility.Visible;
                                    if (ProgressBar.Value == ProgressBar.Maximum) downLoadActress.State = DownLoadState.Completed;
                                    if (ProgressBar.Value == ProgressBar.Maximum | actressUpdateEventArgs.state == DownLoadState.Fail | actressUpdateEventArgs.state == DownLoadState.Completed) { ProgressBar.Visibility = Visibility.Hidden; }
                                });
                            }
                            catch (TaskCanceledException ex) { Logger.LogE(ex); }
                            break;
                        }
                    }
                };
        }
            catch(Exception e) { Console.WriteLine(e.Message); }



}


        DownLoadActress downLoadActress;
        public void StartDownLoadActress(object sender, RoutedEventArgs e)
        {
            DownloadActorPopup.IsOpen = false;

            if (DownLoader?.State == DownLoadState.DownLoading)
                HandyControl.Controls.Growl.Info( "已有任务在下载！","Main");
            else
                StartDownLoadActor(vieModel.ActorList.ToList());



        }


        private void Grid_Actor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue) { downLoadActress?.CancelDownload(); ProgressBar.Visibility = Visibility.Hidden; }
        }

        private void ProgressBar_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void ShowSameFilePath(object sender, RoutedEventArgs e )
        {
            FilePathPopup.IsOpen = true;
            vieModel.LoadFilePathClassfication();
        }

        public void ShowSubsection(object sender, MouseButtonEventArgs e)
        {
            

            Image image = sender as Image;
            var grid = image.Parent as Grid;
            Popup popup = grid.Children.OfType<Popup>().First();
            popup.IsOpen = true;

        }


        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            WaitingPanel.Visibility = Visibility.Visible;

            //异步导入
            await Task.Run(() => { 
            //分为文件夹和文件
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            List<string> files = new List<string>();
            StringCollection stringCollection = new StringCollection();
            foreach (var item in dragdropFiles)
            {
                if (IsFile(item))
                    files.Add(item);
                else
                    stringCollection.Add(item);
            }
            List<string> filepaths = new List<string>();
            //扫描导入
            if (stringCollection.Count > 0)
                filepaths = Scan.ScanPaths(stringCollection, new CancellationToken());

            if (files.Count > 0) filepaths.AddRange(files);
            double _num= Scan.DistinctMovieAndInsert(filepaths, new CancellationToken());
                HandyControl.Controls.Growl.Info($"总计导入{_num}个文件","Main");
                Task.Delay(300).Wait();
            });
            
            WaitingPanel.Visibility = Visibility.Hidden;
            vieModel.Reset();
        }



        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void Button_StopDownload(object sender, RoutedEventArgs e)
        {
            DownloadPopup.IsOpen = false;
            StopDownLoad();
            
        }

        private void Button_StartDownload(object sender, RoutedEventArgs e)
        {
            DownloadPopup.IsOpen = false;

            if (!IsServersProper())
            {
                HandyControl.Controls.Growl.Error("请在设置【同步信息】中添加服务器源并启用！","Main");

            }else
            {
                if (DownLoader?.State == DownLoadState.DownLoading)
                    HandyControl.Controls.Growl.Info("已有任务在下载！","Main");
                else
                    StartDownload(vieModel.CurrentMovieList.ToList());
            }


        }



        private async void OpenUpdate(object sender, RoutedEventArgs e)
        {
            if (new Msgbox(this, "是否关闭程序开始更新？").ShowDialog() == true)
            {
                try
                {
                    //检查升级程序是否是最新的
                    string content = ""; int statusCode;bool IsToDownLoadUpdate = false;
                    try { (content, statusCode) = await Net.Http(UpdateExeVersionUrl, Proxy: null); }
                    catch (TimeoutException ex) { Logger.LogN($"URL={UpdateUrl},Message-{ex.Message}"); }
                    if (content != "")
                    {
                        //跟本地的 md5 对比
                        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe")) { IsToDownLoadUpdate = true; }
                        else
                        {
                            string md5 = GetFileMD5(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe");
                            if (md5 != content) { IsToDownLoadUpdate = true; }
                        }
                    }
                    if (IsToDownLoadUpdate)
                    {
                        (byte[] filebyte,string cookie) = Net.DownLoadFile(UpdateExeUrl);
                        try
                        {
                            using (var fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe", FileMode.Create, FileAccess.Write))
                            {
                                fs.Write(filebyte, 0, filebyte.Length);
                            }
                        }
                        catch { }
                    }
                    Process.Start(AppDomain.CurrentDomain.BaseDirectory + "JvedioUpdate.exe");
                    IsToUpdate = true;
                    Application.Current.Shutdown();//直接关闭
                }
                catch { MessageBox.Show("找不到 JvedioUpdate.exe"); }
                
            }
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            Border border = sender as Border;
            border.Opacity = 1;
        }



        public void ShowSettingsPopup(object sender, MouseButtonEventArgs e)
        {
            if (SettingsContextMenu.IsOpen)
                SettingsContextMenu.IsOpen = false;
            else
            {
                SettingsContextMenu.IsOpen = true;
                SettingsContextMenu.PlacementTarget = SettingsBorder;
                SettingsContextMenu.Placement = PlacementMode.Bottom;
            }

        }

        public void ShowSkinPopup(object sender, MouseButtonEventArgs e)
        {
            if (SkinPopup.IsOpen)
                SkinPopup.IsOpen = false;
            else
                SkinPopup.IsOpen = true;
        }

        private void ClearRecentWatched(object sender,RoutedEventArgs e)
        {
            if(new RecentWatchedConfig("").Clear())
            {
                ReadRecentWatchedFromConfig();
                vieModel.AddToRecentWatch("");
            }
        }


        private void Window_ContentRendered(object sender, EventArgs e)
        {

            if (Properties.Settings.Default.FirstRun)
            {
                BeginScanStackPanel.Visibility = Visibility.Visible;
                Properties.Settings.Default.FirstRun = false;
            }


            if (Properties.Settings.Default.Opacity_Main >= 0.5)
                this.Opacity = Properties.Settings.Default.Opacity_Main;
            else
                this.Opacity = 1;


            SetSkin();

            //监听文件改动
            //if (Properties.Settings.Default.ListenAllDir)
            //{
            //    try { AddListen(); }
            //    catch (Exception ex) { Logger.LogE(ex); }
            //}

            //显示公告
            ShowNotice();


            //检查更新
            //if (Properties.Settings.Default.AutoCheckUpdate) CheckUpdate();
            CheckUpdate();


            //检查网络连接


            this.Cursor = Cursors.Arrow;

            //ScrollViewer.Focus();


            //设置当前数据库
            for (int i = 0; i < vieModel.DataBases.Count; i++)
            {
                if (vieModel.DataBases[i].ToLower() == Properties.Settings.Default.DataBasePath.Split('\\').Last().Split('.').First().ToLower())
                {
                    DatabaseComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (vieModel.DataBases.Count == 1) DatabaseComboBox.Visibility = Visibility.Hidden;
            CheckurlTimer.Start();
            BeginCheckurlThread();
            ReadRecentWatchedFromConfig();//显示最近播放
            vieModel.AddToRecentWatch("");
            vieModel.GetFilterInfo();

            var radioButtons = SortStackPanel.Children.OfType<RadioButton>().ToList();
            for (int i = 0; i < radioButtons.Count; i++)
            {
                if (radioButtons[i].Content.ToString() == Properties.Settings.Default.SortType)
                {
                    radioButtons[i].IsChecked = true;
                    break;
                }
            }
            if (vieModel.SortDescending)
                SortImage.Source = new BitmapImage(new Uri("/Resources/Picture/sort_down.png", UriKind.Relative));
            else
                SortImage.Source = new BitmapImage(new Uri("/Resources/Picture/sort_up.png", UriKind.Relative));

        }

       



        public void SetSkin()
        {
            if (Properties.Settings.Default.Themes == "黑色")
            {
                Application.Current.Resources["BackgroundTitle"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22252A"));
                Application.Current.Resources["BackgroundMain"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B1B1F"));
                Application.Current.Resources["BackgroundSide"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#101013"));
                Application.Current.Resources["BackgroundTab"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#383838"));
                Application.Current.Resources["BackgroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#18191B"));
                Application.Current.Resources["BackgroundMenu"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
                Application.Current.Resources["ForegroundGlobal"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AFAFAF"));
                Application.Current.Resources["ForegroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                Application.Current.Resources["BorderBursh"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
                SideBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString()));
                TopBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundTitle"].ToString()));
            }
            else if (Properties.Settings.Default.Themes == "白色")
            {
                Application.Current.Resources["BackgroundTitle"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E3E5"));
                Application.Current.Resources["BackgroundMain"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9F9F9"));
                Application.Current.Resources["BackgroundSide"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F2F3F4"));
                Application.Current.Resources["BackgroundTab"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF5EE"));
                Application.Current.Resources["BackgroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D1D1"));
                Application.Current.Resources["BackgroundMenu"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                Application.Current.Resources["ForegroundGlobal"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
                Application.Current.Resources["ForegroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                Application.Current.Resources["BorderBursh"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));

                SideBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundSide"].ToString()));
                TopBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Application.Current.Resources["BackgroundTitle"].ToString()));
            }
            else if (Properties.Settings.Default.Themes == "蓝色")

            {
                Application.Current.Resources["BackgroundTitle"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B72BD"));
                Application.Current.Resources["BackgroundMain"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2BA2D2"));
                Application.Current.Resources["BackgroundSide"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#61AEDA"));
                Application.Current.Resources["BackgroundTab"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3DBEDE"));
                Application.Current.Resources["BackgroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                Application.Current.Resources["BackgroundMenu"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightBlue"));
                Application.Current.Resources["ForegroundGlobal"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                Application.Current.Resources["ForegroundSearch"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#555555"));
                Application.Current.Resources["BorderBursh"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95DCED"));


                //设置侧边栏渐变

                LinearGradientBrush myLinearGradientBrush = new LinearGradientBrush();
                myLinearGradientBrush.StartPoint = new Point(0.5, 0);
                myLinearGradientBrush.EndPoint = new Point(0.5, 1);
                myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(62, 191, 223), 0));
                myLinearGradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(11, 114, 189), 1));
                SideBorder.Background = myLinearGradientBrush;

                LinearGradientBrush myLinearGradientBrush2 = new LinearGradientBrush();
                myLinearGradientBrush2.MappingMode = BrushMappingMode.RelativeToBoundingBox;
                myLinearGradientBrush2.StartPoint = new Point(0, 0.5);
                myLinearGradientBrush2.EndPoint = new Point(1, 0);
                myLinearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromRgb(11, 114, 189), 1));
                myLinearGradientBrush2.GradientStops.Add(new GradientStop(Color.FromRgb(62, 191, 223), 0));
                TopBorder.Background = myLinearGradientBrush2;

            }
            
        }

        private void SetSkinProperty(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Themes = ((Button)sender).Content.ToString();
            Properties.Settings.Default.Save();
            SetSkin();
            SetSelected();
            ActorSetSelected();
        }


        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedMovie.Clear();
            SetSelected();
        }


        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            new Msgbox(this, "123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf123123sdfsdfsdf").ShowDialog();


        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            OpenTools(sender, e);
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
            {
                //高级检索
                if (windowSearch != null) { windowSearch.Close(); }
                windowSearch = new WindowSearch();
                windowSearch.Show();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Right)
            {
                //末页
                if (Grid_GAL.Visibility == Visibility.Hidden)
                {
                    vieModel.CurrentPage = vieModel.TotalPage;
                    vieModel.FlipOver();
                    SetSelected();
                }
                else
                {
                    vieModel.CurrentActorPage = vieModel.TotalActorPage;
                    vieModel.ActorFlipOver();
                    ActorSetSelected();
                }

            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Left)
            {
                //首页
                if (Grid_GAL.Visibility == Visibility.Hidden)
                {
                    vieModel.CurrentPage = 1;
                    vieModel.FlipOver();
                    SetSelected();
                }

                else
                {
                    vieModel.CurrentActorPage = 1;
                    vieModel.ActorFlipOver();
                    ActorSetSelected();
                }

            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Up)
            {
                //回到顶部
                //ScrollViewer.ScrollToTop();
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.Down)
            {
                //滑倒底端
                ScrollToEnd(sender, new RoutedEventArgs());
            }
            else if (Grid_GAL.Visibility == Visibility.Hidden && e.Key == Key.Right)
                NextPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (Grid_GAL.Visibility == Visibility.Hidden && e.Key == Key.Left)
                PreviousPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (Grid_GAL.Visibility == Visibility.Visible && e.Key == Key.Right)
                NextActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            else if (Grid_GAL.Visibility == Visibility.Visible && e.Key == Key.Left)
                PreviousActorPage(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));




        }

        private void Window_Activated(object sender, EventArgs e)
        {
            AllSearchTextBox.Focus();
        }

        private void DatabaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            string name = e.AddedItems[0].ToString().ToLower();
            if (name != Properties.Settings.Default.DataBasePath.Split('\\').Last().Split('.').First().ToLower())
            {
                if(name == "info")
                    Properties.Settings.Default.DataBasePath = AppDomain.CurrentDomain.BaseDirectory + $"{name}.sqlite";
                else
                    Properties.Settings.Default.DataBasePath = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{name}.sqlite";
                //切换数据库
                vieModel.IsRefresh = true;
                vieModel.Reset();
                AllRB.IsChecked = true;
                vieModel.GetFilterInfo();
                
            }
        }

        private void GotoDownloadUrl(object sender, RoutedEventArgs e)
        {
            Process.Start("https://hitchao.gitee.io/jvediowebpage/download.html");
        }

        private void RandomDisplay(object sender, MouseButtonEventArgs e)
        {
            vieModel.RandomDisplay();
        }

        private async  void ShowFilterGrid(object sender, MouseButtonEventArgs e)
        {
            if (FilterGrid.Visibility == Visibility.Visible)
            {
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(600, 0, new Duration(TimeSpan.FromMilliseconds(300)));
                FilterGrid.BeginAnimation(FrameworkElement.MaxHeightProperty, doubleAnimation1);
                await Task.Delay(300);
                FilterGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                FilterGrid.Visibility = Visibility.Visible;
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(0, 600, new Duration(TimeSpan.FromMilliseconds(300)));
                FilterGrid.BeginAnimation(FrameworkElement.MaxHeightProperty, doubleAnimation1);
                await Task.Delay(300);
                
            }
                

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private bool IsDragingSideGrid = false;

        private void DragRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsDragingSideGrid)
            {
                this.Cursor = Cursors.SizeWE;
                double width = e.GetPosition(this).X;
                if (width <= 200 || width >= 500) 
                    return;
                else
                    SideGridColumn.Width =new GridLength(width);
            }
        }

        private void DragRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsDragingSideGrid = true;
            }
        }

        private void DragRectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            IsDragingSideGrid = false;
            Properties.Settings.Default.SideGridWidth = SideGridColumn.Width.Value;
            Properties.Settings.Default.Save();
        }


        private void CheckMode_Click(object sender, RoutedEventArgs e)
        {
            vieModel.SelectedMovie.Clear();
            SetSelected();
        }

        private void ShowNewTypePopup(object sender, MouseButtonEventArgs e)
        {
            NewTypePopup.IsOpen = true;
        }

        private void RenameChildTree(object sender, MouseButtonEventArgs e)
        {

        }

        private void RenameType(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            RadioButton radioButton = contextMenu.PlacementTarget as RadioButton;
            TextBox textBox = radioButton.Content as TextBox;
            textBox.Focusable = true;
            textBox.IsReadOnly = false;
            textBox.Focus();
            textBox.SelectAll();

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveText(sender);
        }

        private void SaveText(object sender)
        {
            TextBox textBox = sender as TextBox;
            string tbname = textBox.Name;
            string name = textBox.Text;
            textBox.IsReadOnly = true;
                textBox.Focusable = false;

                AllSearchTextBox.Focus();

            //保存
            if (name != "")
            {
                if (tbname == "TypeNameTextBox1")
                {
                    Properties.Settings.Default.TypeName1 = name;
                }else if (tbname == "TypeNameTextBox2")
                {
                    Properties.Settings.Default.TypeName2 = name;
                }
                else if (tbname == "TypeNameTextBox3")
                {
                    Properties.Settings.Default.TypeName3 = name;
                }
                Properties.Settings.Default.Save();
            }
            RefreshSideRB();
        }

        private void RefreshSideRB()
        {
            TypeNameTextBox1.Text = Properties.Settings.Default.TypeName1;
            TypeNameTextBox2.Text = Properties.Settings.Default.TypeName2;
            TypeNameTextBox3.Text = Properties.Settings.Default.TypeName3;
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveText(sender);
            }

        }

        private void TopBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount>1)
            {
                MaxWindow(sender, new MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left));
            }
        }

        public void ContextMenu_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ContextMenu contextMenu = sender as ContextMenu;
            if (e.Key == Key.D)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, "删除信息(D)");
                if (menuItem != null) DeleteID(menuItem, new RoutedEventArgs());
            }else if (e.Key == Key.T)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, "删除文件(T)");
                if (menuItem != null) DeleteFile(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.S)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, "立即同步(S)");
                if (menuItem != null) DownLoadSelectMovie(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.E)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, "修改信息(E)");
                if (menuItem != null) EditInfo(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.W)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, "打开网址(W)");
                if (menuItem != null) OpenWeb(menuItem, new RoutedEventArgs());

            }
            else if (e.Key == Key.C)
            {
                MenuItem menuItem = GetMenuItem(contextMenu, "复制文件(C)");
                if (menuItem != null) CopyFile(menuItem, new RoutedEventArgs());

            }
            contextMenu.IsOpen = false;
        }


        private MenuItem GetMenuItem(ContextMenu contextMenu,string header)
        {
            foreach (MenuItem item in contextMenu.Items)
            {
                if (item.Header.ToString() == header)
                {
                    return item;
                }
            }
            return null;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && e.Key == Key.S )
            {
                //MessageBox.Show("1");
            }
            else if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key==Key.S)
            {
                //MessageBox.Show("2");
            }
        }

        private void cmdTextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            cmdGrid.Visibility = Visibility.Collapsed;
        }

        private void Border_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            var s1 = Jav321IDDict;
            

        }

        private void AllSearchTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AllSearchPopup.IsOpen = true;
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Background = (SolidColorBrush)Application.Current.Resources["BackgroundMain"];
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Background =new SolidColorBrush( Colors.Transparent);
        }

        private void ShowSamePath(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            vieModel.GetSamePathMovie(textBlock.Text);
            ShowMovieGrid(sender,new RoutedEventArgs());
        }

        private void NotifyIcon_Click(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Collapsed;
            this.Show();
            this.Opacity = 1;
            this.WindowState = WindowState.Normal;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ImageSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Properties.Settings.Default.ShowImageMode == "缩略图")
            {
                Properties.Settings.Default.SmallImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.SmallImage_Height =(int)( (double)Properties.Settings.Default.SmallImage_Width * (200/147));

            }
            else if (Properties.Settings.Default.ShowImageMode == "海报图")
            {
                Properties.Settings.Default.BigImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.BigImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }
            else if (Properties.Settings.Default.ShowImageMode == "预览图")
            {
                Properties.Settings.Default.ExtraImage_Width = Properties.Settings.Default.GlobalImageWidth;
                Properties.Settings.Default.ExtraImage_Height = (int)(Properties.Settings.Default.GlobalImageWidth * 540f / 800f);
            }

            //Console.WriteLine(Properties.Settings.Default.BigImage_Height);
            //Console.WriteLine(Properties.Settings.Default.BigImage_Width);
            Properties.Settings.Default.Save();
        }



        public List<Image> images1 = new List<Image>();

        public List<Image> images2 = new List<Image>();

        private void myImage_Loaded(object sender, RoutedEventArgs e)
        {
            //Image image = sender as Image;
            //if (image.Name.ToString() == "myImage")
            //{
            //   if(!images1.Contains(image))  images1.Add(image);
            //}
            //else
            //{
            //    if (!images2.Contains(image)) images2.Add(image);
            //}

        }

        private void Rate_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (!CanRateChange) return;
            HandyControl.Controls.Rate rate = (HandyControl.Controls.Rate)sender;
            StackPanel stackPanel = rate.Parent as StackPanel;
            StackPanel sp = stackPanel.Parent as StackPanel;
            TextBox textBox = sp.Children.OfType<TextBox>().First();

            if (vieModel.CurrentMovieList != null && vieModel.CurrentMovieList.Count > 0)
            {
                foreach (var item in vieModel.CurrentMovieList)
                {
                    if (item != null)
                    {
                        if (item.id.ToUpper() == textBox.Text.ToUpper())
                        {
                            DataBase.UpdateMovieByID(item.id, "favorites", item.favorites, "string");
                            break;
                        }
                    }


                }

            }
            CanRateChange = false;



        }

        private void StackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CanRateChange = true;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;
            if (mnu != null)
            {
                sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
                if (sp != null)
                {

                    var TB = sp.Children.OfType<TextBox>().First();
                    Movie CurrentMovie = GetMovieFromVieModel(TB.Text);
                    if (!vieModel.SelectedMovie.Select(g => g.id).ToList().Contains(CurrentMovie.id)) vieModel.SelectedMovie.Add(CurrentMovie);


                }
            }


            LabelGrid.Visibility = Visibility.Visible;
            for (int i = 0; i < vieModel.LabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    toggleButton.IsChecked = false;
                }
            }


        }


        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            HandyControl.Controls.Tag tag = contextMenu.PlacementTarget as HandyControl.Controls.Tag;
            if (tag.IsSelected) tag.IsSelected = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {



                    //获得选中的标签
                    List<string> originLabels = new List<string>();
                    for (int i = 0; i < vieModel.LabelList.Count; i++)
                    {
                        ContentPresenter c = (ContentPresenter)LabelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelItemsControl.Items[i]);
                        WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                        if (wrapPanel != null)
                        {
                            ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                            if ((bool)toggleButton.IsChecked)
                            {
                                Match match = Regex.Match(toggleButton.Content.ToString(), @"\( \d+ \)");
                                if (match != null && match.Value != "")
                                {
                                    string label = toggleButton.Content.ToString().Replace(match.Value, "");
                                    if (!originLabels.Contains(label)) originLabels.Add(label);
                                }

                            }
                        }
                    }

                    if (originLabels.Count <= 0)
                    {
                        HandyControl.Controls.Growl.Warning("请选择标签！","Main");
                        return;
                    }


                    foreach (Movie movie in vieModel.SelectedMovie)
                    {
                        List<string> labels = LabelToList(movie.label);
                        labels = labels.Union(originLabels).ToList();
                        movie.label = string.Join(" ", labels);
                        DataBase.UpdateMovieByID(movie.id, "label", movie.label, "String");
                    }
                    HandyControl.Controls.Growl.Info($"成功添加标签！","Main");
                    if (!Properties.Settings.Default.EditMode) vieModel.SelectedMovie.Clear();
                    LabelGrid.Visibility = Visibility.Hidden;

        }



        private void AddNewLabel(object sender, RoutedEventArgs e)
        {
            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.CurrentMovieLabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelDelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelDelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        string label = toggleButton.Content.ToString();
                        if (!originLabels.Contains(label)) originLabels.Add(label);
                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                HandyControl.Controls.Growl.Warning("请选择标签！","Main");
                return;
            }

            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Except(originLabels).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }

            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;

            if (vieModel.CurrentMovieList.Count == 0)
            {
                HandyControl.Controls.Growl.Info($"成功删除标签！","Main");
                LabelDelGrid.Visibility = Visibility.Hidden;
                vieModel.GetLabelList();
            }



        }


        private void ClearSingleLabel(object sender, RoutedEventArgs e)
        {
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", "", "String");
            vieModel.CurrentMovieLabelList = new List<string>();
            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;
        }

        private void AddSingleLabel(object sender, RoutedEventArgs e)
        {
            List<string> newLabel = new List<string>();

            var di = new DialogInput(this, "请输入需添加的标签，每个标签空格隔开", "");
            di.ShowDialog();
            if (di.DialogResult == true & di.Text != "")
            {
                foreach (var item in di.Text.Split(' ').ToList())
                {
                    if (!newLabel.Contains(item)) newLabel.Add(item);
                }

            }
            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Union(newLabel).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }
            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;
        }

        private void DeleteSingleLabel(object sender, RoutedEventArgs e)
        {
            //获得选中的标签
            List<string> originLabels = new List<string>();
            for (int i = 0; i < vieModel.CurrentMovieLabelList.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)LabelDelItemsControl.ItemContainerGenerator.ContainerFromItem(LabelDelItemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "LabelWrapPanel");
                if (wrapPanel != null)
                {
                    ToggleButton toggleButton = wrapPanel.Children.OfType<ToggleButton>().First();
                    if ((bool)toggleButton.IsChecked)
                    {
                        string label = toggleButton.Content.ToString();
                        if (!originLabels.Contains(label)) originLabels.Add(label);
                    }
                }
            }

            if (originLabels.Count <= 0)
            {
                HandyControl.Controls.Growl.Warning("请选择标签！","Main");
                return;
            }

            List<string> labels = LabelToList(CurrentLabelMovie.label);
            labels = labels.Except(originLabels).ToList();
            CurrentLabelMovie.label = string.Join(" ", labels);
            DataBase.UpdateMovieByID(CurrentLabelMovie.id, "label", CurrentLabelMovie.label, "String");


            vieModel.CurrentMovieLabelList = new List<string>();
            foreach (var item in labels)
            {
                vieModel.CurrentMovieLabelList.Add(item);
            }

            LabelDelItemsControl.ItemsSource = null;
            LabelDelItemsControl.ItemsSource = vieModel.CurrentMovieLabelList;

            if (vieModel.CurrentMovieList.Count == 0)
            {
                HandyControl.Controls.Growl.Info($"成功删除标签！","Main");
                LabelDelGrid.Visibility = Visibility.Hidden;
                vieModel.GetLabelList();
            }



        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            StackPanel stackPanel = (StackPanel)button.Parent;
            Grid grid = (Grid)stackPanel.Parent;
            ((Grid)grid.Parent).Visibility = Visibility.Hidden;
        }


        WindowBatch  WindowBatch;

        private void OpenBatching(object sender, RoutedEventArgs e)
        {
            if (WindowBatch != null) { WindowBatch.Close(); }
            WindowBatch = new WindowBatch();
            WindowBatch.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            new Msgbox(this, "请等待！",true).ShowDialog();
        }

        private void WaitingPanel_Cancel(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(123);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Width== SystemParameters.WorkArea.Width || this.Height == SystemParameters.WorkArea.Height )
            {
                MainGrid.Margin = new Thickness(0);
                MainBorder.Margin = new Thickness(0);
                Grid.Margin = new Thickness(0);
                this.ResizeMode = ResizeMode.NoResize;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                MainGrid.Margin = new Thickness(0);
                MainBorder.Margin = new Thickness(0);
                this.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                MainGrid.Margin = new Thickness(10);
                MainBorder.Margin = new Thickness(5);
                Grid.Margin = new Thickness(5);
                this.ResizeMode = ResizeMode.CanResize;
            }
        }

        private void PreviousCommand(object sender, MouseButtonEventArgs e)
        {
            if (vieModel.SqlCommands.Count > 0 && vieModel.SqlIndex >= 1)
            {
                vieModel.SqlIndex -= 1;
                vieModel.SwitchSqlCommand();
            }
            else
            {
                HandyControl.Controls.Growl.Clear();
                HandyControl.Controls.Growl.Info("已是最前！","Main");
            }
        }

        private void NextCommand(object sender, MouseButtonEventArgs e)
        {


            if (vieModel.SqlCommands.Count > 0 && vieModel.SqlIndex < vieModel.SqlCommands.Count - 1)
            {
                vieModel.SqlIndex += 1;
                vieModel.SwitchSqlCommand();
            }
            else
            {
                HandyControl.Controls.Growl.Clear();
                HandyControl.Controls.Growl.Info("已是最后！","Main");
            }
        }

        private async void DownLoadWithUrl(object sender, RoutedEventArgs e)
        {
            MenuItem _mnu = sender as MenuItem;
            MenuItem mnu = _mnu.Parent as MenuItem;
            StackPanel sp = null;

            if (mnu == null) return;
            sp = ((ContextMenu)mnu.Parent).PlacementTarget as StackPanel;
            var TB = sp.Children.OfType<TextBox>().First();
            string id = TB.Text;

            DialogInput dialogInput = new DialogInput(this, "请输入需要同步的影片网址");
            if (dialogInput.ShowDialog() == true)
            {
                string url = dialogInput.Text;
                if (!url.StartsWith("http"))
                {
                    HandyControl.Controls.Growl.Error("网址有误！","Main");
                }
                else
                {

                    string host = new Uri(url).Host;
                    WebSite webSite = await Net.CheckUrlType(url.Split(':')[0] + "://" + host);
                    if (webSite == WebSite.None)
                    {
                        HandyControl.Controls.Growl.Error("识别失败","Main");
                    }
                    else
                    {
                        if(webSite == WebSite.DMM || webSite == WebSite.Jav321)
                        {
                            HandyControl.Controls.Growl.Info("暂不支持解析","Main");
                        }
                        else
                        {
                            HandyControl.Controls.Growl.Info($"识别为{webSite.ToString()}，开始解析","Main");
                            bool result=await Net.ParseSpecifiedInfo(webSite, id, url);
                            if (result)
                            {
                                HandyControl.Controls.Growl.Success("解析成功！开始同步图片！","Main");
                                //更新到主界面
                                RefreshMovieByID(id);

                                //下载图片
                                DetailMovie dm = new DetailMovie();
                                dm = DataBase.SelectDetailMovieById(id);
                                //下载小图
                                await Net.DownLoadSmallPic(dm);
                                dm.smallimage = StaticClass.GetBitmapImage(dm.id, "SmallPic");
                                RefreshMovieByID(id);


                                if (dm.sourceurl?.IndexOf("fc2club") >= 0)
                                {
                                    //复制大图
                                    if (File.Exists(StaticVariable.BasePicPath + $"SmallPic\\{dm.id}.jpg") & !File.Exists(StaticVariable.BasePicPath + $"BigPic\\{dm.id}.jpg"))
                                    {
                                        File.Copy(StaticVariable.BasePicPath + $"SmallPic\\{dm.id}.jpg", StaticVariable.BasePicPath + $"BigPic\\{dm.id}.jpg");
                                    }
                                }
                                else
                                {
                                    //下载大图
                                    await Net.DownLoadBigPic(dm);
                                }
                                dm.bigimage = StaticClass.GetBitmapImage(dm.id, "BigPic");
                                RefreshMovieByID(id);


                            }
                            else
                            {
                                HandyControl.Controls.Growl.Error("解析失败","Main");
                            }
                        }
                    }
                }


            }
        }

        private void Image_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            if (StaticClass.IsFile(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.ToLower() == ".jpg")
                {
                    File.Copy(fileInfo.FullName, BasePicPath + $"Actresses\\{vieModel.Actress.name}.jpg", true);
                    Actress actress = vieModel.Actress;
                    actress.smallimage = null;
                    actress.smallimage = StaticClass.GetBitmapImage(actress.name, "Actresses");
                    vieModel.Actress = null;
                    vieModel.Actress = actress;

                    if(vieModel.ActorList==null || vieModel.ActorList.Count == 0) return;

                    for (int i = 0; i < vieModel.ActorList.Count; i++)
                    {
                        if (vieModel.ActorList[i].name == actress.name)
                        {
                            vieModel.ActorList[i] = actress;
                            break;
                        }
                    }

                }
                else
                {
                    HandyControl.Controls.Growl.Info("仅支持 jpg", "DetailsGrowl");
                }
            }
        }

        private void ActorImage_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void ActorImage_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = dragdropFiles[0];

            Image image = sender as Image;
            StackPanel stackPanel = image.Parent as StackPanel;
            TextBox textBox = stackPanel.Children.OfType<TextBox>().First();
            string name = textBox.Text.Split('(')[0];

            Actress currentActress=null;
            for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
            {
                if (vieModel.CurrentActorList[i].name == name)
                {
                    currentActress = vieModel.CurrentActorList[i];
                    break;
                }
            }

            if (currentActress == null) return;


            if (StaticClass.IsFile(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Extension.ToLower() == ".jpg")
                {
                    File.Copy(fileInfo.FullName, BasePicPath + $"Actresses\\{currentActress.name}.jpg", true);
                    Actress actress = currentActress;
                    actress.smallimage = null;
                    actress.smallimage = StaticClass.GetBitmapImage(actress.name, "Actresses");

                    if (vieModel.ActorList == null || vieModel.ActorList.Count == 0) return;

                    for (int i = 0; i < vieModel.ActorList.Count; i++)
                    {
                        if (vieModel.ActorList[i].name == actress.name)
                        {
                            vieModel.ActorList[i] = null;
                            vieModel.ActorList[i] = actress;
                            break;
                        }
                    }

                    for (int i = 0; i < vieModel.CurrentActorList.Count; i++)
                    {
                        if (vieModel.CurrentActorList[i].name == actress.name)
                        {
                            vieModel.CurrentActorList[i] = null;
                            vieModel.CurrentActorList[i] = actress;
                            break;
                        }
                    }

                }
                else
                {
                    HandyControl.Controls.Growl.Info("仅支持 jpg", "DetailsGrowl");
                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var WrapPanels = FilterStackPanel.Children.OfType<WrapPanel>().ToList(); ;

            List<int> vediotype = new List<int>();
            WrapPanel wrapPanel = WrapPanels[0];
            foreach (var item in wrapPanel.Children.OfType<ToggleButton>())
            {
                if (item.GetType() == typeof(ToggleButton))
                {
                    ToggleButton tb = item as ToggleButton;
                    if (tb != null)
                        if ((bool)tb.IsChecked)
                        {
                            if (tb.Content.ToString() == Properties.Settings.Default.TypeName1)
                                vediotype.Add(1);
                            else if (tb.Content.ToString() == Properties.Settings.Default.TypeName2)
                                vediotype.Add(2);
                            else if (tb.Content.ToString() == Properties.Settings.Default.TypeName3)
                                vediotype.Add(3);
                        }
                }
            }


            //年份
            wrapPanel = WrapPanels[1];
            ItemsControl itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> year = GetFilterFromItemsControl(itemsControl);



            //时长
            wrapPanel = WrapPanels[2];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> runtime = GetFilterFromItemsControl(itemsControl);

            //文件大小
            wrapPanel = WrapPanels[3];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> filesize = GetFilterFromItemsControl(itemsControl);

            //评分
            wrapPanel = WrapPanels[4];
            itemsControl = wrapPanel.Children[1] as ItemsControl;
            List<string> rating = GetFilterFromItemsControl(itemsControl);


            //类别
            List<string> genre = GetFilterFromItemsControl(GenreItemsControl);

            //演员
            List<string> actor = GetFilterFromItemsControl(ActorFilterItemsControl);

            //标签
            List<string> label = GetFilterFromItemsControl(LabelFilterItemsControl);

            string sql = "select * from movie where ";

            string s = "";
            vediotype.ForEach(arg => { s += $"vediotype={arg} or "; });
            if (vediotype.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s == "" | vediotype.Count == 3) s = "vediotype>0";
            sql += "(" + s + ") and "; s = "";

            year.ForEach(arg => { s += $"releasedate like '%{arg}%' or "; });
            if (year.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //类别
            genre.ForEach(arg => { s += $"genre like '%{arg}%' or "; });
            if (genre.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //演员
            actor.ForEach(arg => { s += $"actor like '%{arg}%' or "; });
            if (actor.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";

            //类别
            label.ForEach(arg => { s += $"label like '%{arg}%' or "; });
            if (label.Count >= 1) s = s.Substring(0, s.Length - 4);
            if (s != "") sql += "(" + s + ") and "; s = "";


            if (runtime.Count > 0 & rating.Count < 4)
            {
                runtime.ForEach(arg => { s += $"(runtime >={arg.Split('-')[0]} and runtime<={arg.Split('-')[1]}) or "; });
                if (runtime.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }

            if (filesize.Count > 0 & rating.Count < 4)
            {
                filesize.ForEach(arg => { s += $"(filesize >={double.Parse(arg.Split('-')[0]) * 1024 * 1024 * 1024} and filesize<={double.Parse(arg.Split('-')[1]) * 1024 * 1024 * 1024}) or "; });
                if (filesize.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }

            if (rating.Count > 0 & rating.Count < 5)
            {
                rating.ForEach(arg => { s += $"(rating >={arg.Split('-')[0]} and rating<={arg.Split('-')[1]}) or "; });
                if (rating.Count >= 1) s = s.Substring(0, s.Length - 4);
                if (s != "") sql += "(" + s + ") and "; s = "";
            }


            sql = sql.Substring(0, sql.Length - 5);
            Console.WriteLine(sql);
            vieModel.ExecutiveSqlCommand(0, "筛选", sql);
        }

        private List<string> GetFilterFromItemsControl(ItemsControl itemsControl)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {

                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null)
                    if ((bool)tb.IsChecked) result.Add(tb.Content.ToString());
            }
            return result;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var WrapPanels = FilterStackPanel.Children.OfType<WrapPanel>().ToList(); ;

            List<int> vediotype = new List<int>();
            WrapPanel wrapPanel = WrapPanels[0];
            foreach (var item in wrapPanel.Children.OfType<ToggleButton>())
            {
                if (item.GetType() == typeof(ToggleButton))
                {
                    ToggleButton tb = item as ToggleButton;
                    if (tb != null) tb.IsChecked = false;
                }
            }
            for (int j = 1; j < WrapPanels.Count; j++)
            {
                ItemsControl itemsControl = WrapPanels[j].Children[1] as ItemsControl;
                if (itemsControl == null) continue;
                for (int i = 0; i < itemsControl.Items.Count; i++)
                {

                    ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                    ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                    if (tb != null) tb.IsChecked = false;


                }
            }

            for (int i = 0; i < GenreItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)GenreItemsControl.ItemContainerGenerator.ContainerFromItem(GenreItemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null) tb.IsChecked = false;
            }
            for (int i = 0; i < ActorFilterItemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)ActorFilterItemsControl.ItemContainerGenerator.ContainerFromItem(ActorFilterItemsControl.Items[i]);
                ToggleButton tb = c.ContentTemplate.FindName("CheckBox", c) as ToggleButton;
                if (tb != null) tb.IsChecked = false;
            }


        }

        public ObservableCollection<string> tempList;
        private void Genre_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vieModel.IsRefresh = false;
            foreach (var item in vieModel.GetAllGenre())
            {
                if (vieModel.IsRefresh) break;
                if (!vieModel.Genre.Contains(item))
                     this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadGenreItem), item);
            }
            
        }

        private delegate void LoadItemDelegate(string content);

        private void LoadGenreItem(string content)
        {
            if (!vieModel.IsRefresh) vieModel.Genre.Add(content);
        }

        private void LoadActorItem(string content)
        {
            if (!vieModel.IsRefresh) vieModel.Actor.Add(content);
        }


        private void Actor_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            vieModel.IsRefresh = false;
            foreach (var item in vieModel.GetAllActor())
            {
                if (vieModel.IsRefresh) break;
                if (!vieModel.Actor.Contains(item))
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new LoadItemDelegate(LoadActorItem), item);
            }
        }

        
    }

    public class DownLoadProgress
    {
        public double maximum = 0;
        public double value = 0;
        public object lockobject;

    }


    public class BiggerWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            double width = 0;
            double.TryParse(value.ToString(), out width);
            return width;
                
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }


    public class BoolToVisibilityConverter : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value) return Visibility.Visible; else return Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }


    }

    public class IntToCheckedConverter : IValueConverter
    {
        //数字转换为选中项的地址
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null | parameter == null) { return false; }
            int intparameter = int.Parse(parameter.ToString());
            if ((int)value == intparameter)
                return true;
            else
                return false;
        }

        //选中项地址转换为数字

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null | parameter == null) return 0;
            int intparameter = int.Parse(parameter.ToString());
            return intparameter;
        }


    }


    public class StringToCheckedConverter : IValueConverter
    {
        //判断是否相同
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == parameter.ToString()) { return true; } else { return false; }
        }

        //选中项地址转换为数字

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter.ToString();
        }


    }

    public class SearchTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            return (((MySearchType)value).ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(MySearchType), parameter.ToString(), true) : null;
        }
    }

    public enum MySearchType { 识别码, 名称, 演员 }





    public class ViewTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            MyViewType myViewType = MyViewType.默认;
            Enum.TryParse<MyViewType>(value.ToString(), out myViewType);

            return myViewType.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(MyViewType), parameter.ToString(), true) : null;
        }
    }





    public class WidthToMarginConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return 
                    150;
            else
                return double.Parse(value.ToString()) - 40;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class StringToUriStringConverterMain : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "黑色")
                return $"Resources/Skin/black/{parameter.ToString()}.png";
            else if (value.ToString() == "白色")
                return $"Resources/Skin/white/{parameter.ToString()}.png";
            else if (value.ToString() == "蓝色")
                return $"Resources/Skin/black/{parameter.ToString()}.png";
            else
                return $"Resources/Skin/black/{parameter.ToString()}.png";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class StringToUriStringConverterOther : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "黑色")
                return $"pack://application:,,,/Resources/Skin/black/{parameter.ToString()}.png";
            else if (value.ToString() == "白色")
                return $"pack://application:,,,/Resources/Skin/white/{parameter.ToString()}.png";
            else if (value.ToString() == "蓝色")
                return $"pack://application:,,,/Resources/Skin/black/{parameter.ToString()}.png";
            else
                return $"pack://application:,,,/Resources/Skin/black/{parameter.ToString()}.png";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ImageTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return false;

            return (((MyImageType)value).ToString() == parameter.ToString());
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Enum.Parse(typeof(MyImageType), parameter.ToString(), true) : null;
        }
    }




    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == "预览图")
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }




    public class MovieStampTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            else
            {
                MovieStampType movieStampType = (MovieStampType)value;
                if (movieStampType == MovieStampType.高清中字)
                {
                    return "高清中字";
                }
                else if (movieStampType == MovieStampType.无码流出)
                {
                    return "无码流出";
                }
                else
                {
                    return "无";

                }


            }



        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class MovieStampTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!Properties.Settings.Default.DisplayStamp) return Visibility.Hidden;

            if (value == null)
            {
                return Visibility.Hidden;
            }
            else
            {
                MovieStampType movieStampType = (MovieStampType)value;
                if (movieStampType == MovieStampType.无)
                {
                    return Visibility.Hidden;
                }
                else
                {
                    return Visibility.Visible;
                }


            }



        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public static class GetWindow
    {
        public static Window Get(string name)
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.GetType().Name == name) return window;
            }
            return null;
        }
    }


    public class DownLoadActress
    {

        public event EventHandler InfoUpdate;
        public DownLoadProgress downLoadProgress;
        private Semaphore Semaphore;
        private ProgressBarUpdate ProgressBarUpdate;
        private bool Cancel { get; set; }
        public DownLoadState State;

        public List<Actress> ActorList { get; set; }

        public DownLoadActress(List<Actress> actresses)
        {
            ActorList = actresses;
            Cancel = false;
            Semaphore = new Semaphore(3, 3);
            ProgressBarUpdate = new ProgressBarUpdate() { value = 0, maximum = 1 };
        }

        public void CancelDownload()
        {
            Cancel = true;
            State = DownLoadState.Fail;
        }

        public void BeginDownLoad()
        {
            if (ActorList.Count == 0) { this.State = DownLoadState.Completed; return; }


            //先根据 BusActress.sqlite 获得 id
            List<Actress> actresslist = new List<Actress>();
            foreach (Actress item in ActorList)
            {
                Console.WriteLine(item.name);
                if (item.smallimage == null || item.birthday == null)
                {
                    Actress actress = item;
                    DB db = new DB("BusActress");
                    if (item.id == "")
                    {
                        
                        actress.id = db.GetInfoBySql($"select id from censored where name='{item.name}'");
                        if (item.imageurl == null) { actress.imageurl = db.GetInfoBySql($"select smallpicurl from censored where id='{actress.id}'"); }
                        
                    }
                    else
                    {
                        if (item.imageurl == null) { actress.imageurl = db.GetInfoBySql($"select smallpicurl from censored where id='{actress.id}'"); }
                    }
                    db.CloseDB();
                    actresslist.Add(actress);
                }
            }

            ProgressBarUpdate.maximum = actresslist.Count;
            //待修复
            for (int i = 0; i < actresslist.Count; i++)
            {
                Console.WriteLine("开始进程 "+ i);


                Thread threadObject = new Thread(DownLoad);
                threadObject.Start(actresslist[i]);
            }
        }

        private async void DownLoad(object o)
        {
            try
            {
                Semaphore.WaitOne();
                Actress actress = o as Actress;
                if (Cancel | actress.id == "") return;
                this.State = DownLoadState.DownLoading;

                //下载头像
                if (!string.IsNullOrEmpty(actress.imageurl))
                {
                    string url = actress.imageurl;
                    byte[] imageBytes = null; string cookies = "";
                    imageBytes = await Task.Run(() =>
                    {
                        (imageBytes, cookies) = Net.DownLoadFile(url, Host: "pics.javcdn.pw");
                        return imageBytes;
                    });
                    if (imageBytes != null)
                    {

                        StaticClass.SaveImage(actress.name, imageBytes, ImageType.ActorImage, url);
                        actress.smallimage = StaticClass.GetBitmapImage(actress.name, "Actresses");

                    }

                }
                //下载信息
                bool success = false;
                success = await Task.Run(() =>
                {
                    Task.Delay(300).Wait();
                    return Net.DownActress(actress.id, actress.name);
                });

                if (success)
                {
                    actress = DataBase.SelectInfoFromActress(actress);
                }

                ProgressBarUpdate.value += 1;
                InfoUpdate?.Invoke(this, new ActressUpdateEventArgs() { Actress = actress, progressBarUpdate = ProgressBarUpdate, state = State });
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }

    public class ActressUpdateEventArgs : EventArgs
    {
        public Actress Actress;
        public ProgressBarUpdate progressBarUpdate;
        public DownLoadState state;
    }

    public class ProgressBarUpdate
    {
        public double value;
        public double maximum;
    }


    public class CloseEventArgs : EventArgs
    {
        public bool IsExitApp = true;
    }

    public static class GetBounds
    {
        public static Rect BoundsRelativeTo(this FrameworkElement element, Visual relativeTo)
        {
            return element.TransformToVisual(relativeTo).TransformBounds(new Rect(element.RenderSize));
        }

        public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }


    }


    
    public class PlusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.ToString() == " + ")
                return Visibility.Collapsed;
            else
                return Visibility.Visible;

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class LabelToListConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return " + ";
            List<string> result= value.ToString().Split(' ').ToList();
            result.Insert(0, " + ");
            return result;

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "";
            List<string> vs = value as List<string>;
            vs.Remove(" + ");
            return string.Join(" ", vs);
        }
    }

    public class TagStampsConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter==null) return Visibility.Collapsed;

            if (value.ToString().IndexOf(parameter.ToString()) >= 0)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class FontFamilyConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value == null || value.ToString() == "") return "宋体";

            return value.ToString();

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class LabelMenuItem
    {
        public string Header { get; set; }
    }


}
