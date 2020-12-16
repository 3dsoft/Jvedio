﻿using Jvedio.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static Jvedio.FileProcess;

namespace Jvedio
{
    /// <summary>
    /// WindowEdit.xaml 的交互逻辑
    /// </summary>
    public partial class WindowEdit : Jvedio_BaseWindow
    {



        VieModel_Edit vieModel;

        private string ID ;


        public WindowEdit(string id="")
        {
            InitializeComponent();
            ID = id;
            vieModel = new VieModel_Edit();
            
            if (ID == "")
                vieModel.Reset();
            else
                vieModel.Query(ID);
            this.DataContext = vieModel;
        }

        public void ChoseMovie(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            OpenFileDialog1.Title = "选择一个视频";
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter = "常见视频文件(*.avi, *.mp4, *.mkv, *.mpg, *.rmvb)| *.avi; *.mp4; *.mkv; *.mpg; *.rmvb|其它视频文件((*.rm, *.mov, *.mpeg, *.flv, *.wmv, *.m4v)| *.rm; *.mov; *.mpeg; *.flv; *.wmv; *.m4v|所有文件 (*.*)|*.*";
            OpenFileDialog1.FilterIndex = 1;
            OpenFileDialog1.RestoreDirectory = true;
            if (OpenFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (vieModel.DetailMovie == null) vieModel.DetailMovie = new DetailMovie();
                SaveInfo(OpenFileDialog1.FileName);
            }
            
        }


        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;

                string result = Identify.SearchSimilarityAnalysis(tb.Text, vieModel.MovieIDList.ToList()); //相似度分析


                if (result != "")
                {
                    for (int i = 0; i <= IdListBox.Items.Count - 1; i++)
                    {
                        if (IdListBox.Items[i].ToString().ToUpper() == result.ToUpper())
                        {
                            IdListBox.SelectedItem = IdListBox.Items[i];
                            IdListBox.ScrollIntoView(IdListBox.Items[i]);
                            break;
                        }
                    }
                }

            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "") { SearchTextBox.Text = "Search"; }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Search") { SearchTextBox.Text = ""; }
        }

        private void ClearSearctTextBox(object sender, MouseButtonEventArgs e)
        {
            SearchTextBox.Text = "";
            SearchTextBox.Focus();
        }

        public void SearchContent(object sender, MouseButtonEventArgs e)
        {
            //vieModel.Search = SearchTextBox.Text;
        }

        private void IdListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string movieid = IdListBox.SelectedItem.ToString();
            vieModel.Query(movieid);
        }

        private void UpdateDetail()
        {
            if (Jvedio.GetWindow.Get("WindowDetails") != null)
            {
                WindowDetails windowDetails = Jvedio.GetWindow.Get("WindowDetails") as WindowDetails;
                windowDetails.vieModel.Query(vieModel.DetailMovie.id);
            }
        }

        private  void UpdateMain(string oldID,string newID)
        {
            Main main = App.Current.Windows[0] as Main;

            for (int i = 0; i < main.vieModel.CurrentMovieList.Count; i++)
            {
                try
                {
                    if (main.vieModel.CurrentMovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        Movie movie =  DataBase.SelectMovieByID(newID);
                        movie.smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");
                        movie.bigimage = ImageProcess.GetBitmapImage(movie.id, "BigPic");
                        main.vieModel.CurrentMovieList[i] = null;
                        main.vieModel.CurrentMovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }


            for (int i = 0; i < main.vieModel.MovieList.Count; i++)
            {
                try
                {
                    if (main.vieModel.MovieList[i]?.id.ToUpper() == oldID.ToUpper())
                    {
                        Movie movie = DataBase.SelectMovieByID(newID);
                        movie.smallimage = ImageProcess.GetBitmapImage(movie.id, "SmallPic");
                        movie.bigimage = ImageProcess.GetBitmapImage(movie.id, "BigPic");
                        
                        main.vieModel.MovieList[i] = null;
                        main.vieModel.MovieList[i] = movie;
                        break;
                    }
                }
                catch { }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (vieModel.DetailMovie.id == "") { new Msgbox(this, "识别码为空！").ShowDialog(); return; }
            if (vieModel.DetailMovie.vediotype <= 0) { new Msgbox(this, "请选择视频类型！").ShowDialog(); return; }

            string oldID = vieModel.DetailMovie.id;
            string newID = idTextBox.Text;
            bool success=vieModel.SaveModel(idTextBox.Text);
            if (success)
            {
                UpdateMain(oldID,newID);//更新主窗口
                UpdateDetail();//更新详情窗口
                HandyControl.Controls.Growl.Success("保存成功", "EditGrowl");
            }
            else
            {
                HandyControl.Controls.Growl.Error("保存失败，已存在该识别码", "EditGrowl");
            }

        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
            }
            else
            {
                scrollViewer.LineLeft();
            }
            e.Handled = true;
        }

        private void SetDateTime(object sender, MouseButtonEventArgs e)
        {
            CalenderPopup.IsOpen = true;

            DateTime date = DateTime.Now; ;
            bool success = DateTime.TryParse(vieModel.DetailMovie.releasedate, out date);
            if (success) Calendar.SelectedDate = date;
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            this.vieModel.DetailMovie.releasedate = Calendar.SelectedDate?.ToString("yyyy-MM-dd ");
            ReleaseDateTextBox.Text= Calendar.SelectedDate?.ToString("yyyy-MM-dd ");
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ClearPopupFocus();
        }



        /// <summary>
        /// 傻逼 Popup ，就是他么的没法获得焦点，cntmd
        /// </summary>

        public void ClearPopupFocus()
        {
            CalenderPopup.IsOpen = false;
        }



        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (IdListBox.Items.Count > 0)
            {
                for (int i = 0; i < IdListBox.Items.Count; i++)
                {
                    string movieid = IdListBox.Items[i].ToString();
                    if (movieid.ToLower() == ID.ToLower())
                    {
                       IdListBox.SelectedItem = IdListBox.Items[i];
                        IdListBox.ScrollIntoView(IdListBox.Items[i]);
                        break;
                    }
                }
            }
        }

        private void ChoseMovieBorder_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;//必须加
        }

        private void ChoseMovieBorder_Drop(object sender, DragEventArgs e)
        {
            string[] dragdropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var dragdropFile in dragdropFiles)
            {
                if (IsFile(dragdropFile))
                {
                    if (Scan.IsProperMovie(dragdropFile))
                    {
                        SaveInfo(dragdropFile);
                        break;
                    }
                }
            }
        }

        private void SaveInfo(string filepath)
        {
            if (vieModel.DetailMovie == null) return;
            if (!string.IsNullOrEmpty(vieModel.DetailMovie.id))
            {
                //视频类型、文件大小、创建时间
                vieModel.DetailMovie.filepath = filepath;

                FileInfo fileInfo = new FileInfo(filepath);
                
                string id = Identify.GetFanhao(fileInfo.Name);
                int vt = (int)Identify.GetVedioType(id);
                if(vt>0)  vieModel.DetailMovie.vediotype = vt;
                if (File.Exists(filepath)) { 
                    
                    vieModel.DetailMovie.filesize = fileInfo.Length;
                    string createDate = "";
                    try { createDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"); }
                    catch { }
                    if (createDate == "") createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    vieModel.DetailMovie.otherinfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    vieModel.DetailMovie.scandate = createDate;
                }

               vieModel.SaveModel();
                vieModel.Query(vieModel.id);
                HandyControl.Controls.Growl.Success("路径、视频类型、文件大小、创建时间、导入时间成功更新！", "EditGrowl");
            }
            else
            {
                vieModel.Refresh(filepath);
            }
        }
    }

    public class BitToGBConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "0 GB";

            //保留2位小数
            double.TryParse(value.ToString(), out double filesize);

            filesize = filesize / 1024 / 1024 / 1024;//GB
            if (filesize >= 0.9)
                return $"{Math.Round(filesize, 2)} GB";//GB
            else
                return $"{Math.Ceiling(filesize * 1024)} MB";//MB
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class IntToVedioTypeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "未识别";

            if (value.ToString() == "1")
                return Properties.Settings.Default.TypeName1;
            else if (value.ToString() == "2")
                return Properties.Settings.Default.TypeName2;
            else if (value.ToString() == "3")
                return Properties.Settings.Default.TypeName3;
            else 
                return "未识别";

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
