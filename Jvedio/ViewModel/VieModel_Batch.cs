﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using static Jvedio.GlobalVariable;

namespace Jvedio.ViewModel
{
    public class VieModel_Batch : ViewModelBase
    {




        /// <summary>
        /// 如果目录下的图片数目少于网址的数目，则下载
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        private bool IsToDownload(Movie movie)
        {
            if (Net.IsToDownLoadInfo(movie) || movie.extraimageurl=="")
                return true;
            else
            {
                //判断预览图个数
                List<string> extraImageList = new List<string>();
                if (!string.IsNullOrEmpty(movie.extraimageurl) && movie.extraimageurl.IndexOf(";") > 0)
                {
                    //预览图地址不为空
                    extraImageList = movie.extraimageurl.Split(';').ToList().Where(arg => !string.IsNullOrEmpty(arg) && arg.IndexOf("http") >= 0 && arg.IndexOf("dmm") >= 0)?.ToList(); 

                    int count = 0;
                    try
                    {
                        var files = Directory.GetFiles(BasePicPath + "ExtraPic\\" + movie.id + "\\", "*.*", SearchOption.TopDirectoryOnly);
                        if (files != null)  count = files.Count(); 
                    } catch { }

                    if (extraImageList.Count > count)
                        return true;
                    else 
                        return false;
                }
                else
                {
                    return false;
                }
            }
        }

        private bool IsScreenShotExist(string path)
        {
            if (!Directory.Exists(path)) return false;
            try
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
                if (files.Count() > 0)
                    return true;
                else
                    return false;
            }
            catch { }
            return false;
        }


        public bool Reset(int idx,Action<string> callback)
        {
            Movies = new ObservableCollection<string>();
            var movies = DataBase.SelectMoviesBySql("SELECT * FROM movie");
            switch (idx)
            {
                case 0:
                    if (Info_ForceDownload)
                    {
                        movies.ForEach(arg => Movies.Add(arg.id));
                    }
                    else
                    {
                        //判断哪些需要下载
                        movies.ForEach(arg => {
                            if (IsToDownload(arg))
                            {
                                Movies.Add(arg.id);
                            }
                        });
                    }
                    break;

                case 1:
                    if (Gif_Skip)
                    {
                        //跳过已截取的
                        movies.ForEach(arg => { 
                            if(!File.Exists(Path.Combine(Properties.Settings.Default.BasePicPath, "Gif", arg.id +".gif")))
                            {
                                Movies.Add(arg.id);
                            }
                        });
                    }
                    else
                    {
                        movies.ForEach(arg => { Movies.Add(arg.id); });
                    }
                    break;

                case 2:
                    if (ScreenShot_Skip)
                    {
                        movies.ForEach(arg => {
                            if (!IsScreenShotExist(Path.Combine(Properties.Settings.Default.BasePicPath, "ScreenShot", arg.id) ))
                            {
                                Movies.Add(arg.id);
                            }
                        });
                    }
                    else
                    {
                        movies.ForEach(arg => { Movies.Add(arg.id); });
                    }
                    break;

                case 3:
                    //重命名
                    movies.ForEach(arg => { 
                        if (File.Exists(arg.filepath)) Movies.Add(arg.id);
                    });
                    break;
                default:

                    break;
            }
            callback.Invoke(Jvedio.Language.Resources.Complete);
            TotalNum = Movies.Count;
            return true;
        }




        private int _TotalNum = 0;

        public int TotalNum
        {
            get { return _TotalNum; }
            set
            {
                _TotalNum = value;
                RaisePropertyChanged();
            }
        }

        private int _CurrentNum = 0;

        public int CurrentNum
        {
            get { return _CurrentNum; }
            set
            {
                _CurrentNum = value;
                if (TotalNum != 0) Progress = (int)((double)value / (double)TotalNum * 100);
                Console.WriteLine(Progress);
                RaisePropertyChanged();
                
            }
        }


        private int _Progress = 0;

        public int Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                RaisePropertyChanged();
            }
        }



        private ObservableCollection<string> _Movies = new ObservableCollection<string>();

        public ObservableCollection<string> Movies
        {
            get { return _Movies; }
            set
            {
                _Movies = value;
                RaisePropertyChanged();
            }
        }


        private int _Timeout_Short = 300;

        public int Timeout_Short
        {
            get { return _Timeout_Short; }
            set
            {
                _Timeout_Short = value;
                RaisePropertyChanged();
            }
        }

        private int _Timeout_Medium = 1000;

        public int Timeout_Medium
        {
            get { return _Timeout_Medium; }
            set
            {
                _Timeout_Medium = value;
                RaisePropertyChanged();
            }
        }


        private int _Timeout_Long = 2000;

        public int Timeout_Long
        {
            get { return _Timeout_Long; }
            set
            {
                _Timeout_Long = value;
                RaisePropertyChanged();
            }
        }



        #region "Gif"




        private bool _Gif_Skip = true;

        public bool Gif_Skip
        {
            get { return _Gif_Skip; }
            set
            {
                _Gif_Skip = value;
                RaisePropertyChanged();
            }
        }




        private int _Gif_Length = 5;

        public int Gif_Length
        {
            get { return _Gif_Length; }
            set
            {
                _Gif_Length = value;
                RaisePropertyChanged();
            }
        }


        private int _Gif_Width = 280;

        public int Gif_Width
        {
            get { return _Gif_Width; }
            set
            {
                _Gif_Width = value;
                RaisePropertyChanged();
            }
        }





        private int _Gif_Height = 170;

        public int Gif_Height
        {
            get { return _Gif_Height; }
            set
            {
                _Gif_Height = value;
                RaisePropertyChanged();
            }
        }



        


        #endregion



        #region "ScreenShot"




        private bool _ScreenShot_Skip = true;

        public bool ScreenShot_Skip
        {
            get { return _ScreenShot_Skip; }
            set
            {
                _ScreenShot_Skip = value;
                RaisePropertyChanged();
            }
        }

        private bool _ScreenShot_DefaultSave = true;

        public bool ScreenShot_DefaultSave
        {
            get { return _ScreenShot_DefaultSave; }
            set
            {
                _ScreenShot_DefaultSave = value;
                RaisePropertyChanged();
            }
        }


        private int _ScreenShot_Num = 5;

        public int ScreenShot_Num
        {
            get { return _ScreenShot_Num; }
            set
            {
                _ScreenShot_Num = value;
                RaisePropertyChanged();
            }
        }








        #endregion


        #region "Download"




        private bool _Info_ForceDownload = false;

        public bool Info_ForceDownload
        {
            get { return _Info_ForceDownload; }
            set
            {
                _Info_ForceDownload = value;
                RaisePropertyChanged();
            }
        }

        private bool _DownloadSmallPic = false;

        public bool DownloadSmallPic
        {
            get { return _DownloadSmallPic; }
            set
            {
                _DownloadSmallPic = value;
                RaisePropertyChanged();
            }
        }

        private bool _DownloadBigPic = false;

        public bool DownloadBigPic
        {
            get { return _DownloadBigPic; }
            set
            {
                _DownloadBigPic = value;
                RaisePropertyChanged();
            }
        }

        private bool _DownloadExtraPic = true;

        public bool DownloadExtraPic
        {
            get { return _DownloadExtraPic; }
            set
            {
                _DownloadExtraPic = value;
                RaisePropertyChanged();
            }
        }









        #endregion

    }
}
