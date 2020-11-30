﻿
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static Jvedio.StaticVariable;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Windows.Controls.Primitives;
using System.Xml;
using System.Windows.Documents;

namespace Jvedio
{
    public static class StaticClass
    {

        /// <summary>
        /// 检查是否启用服务器源且地址不为空
        /// </summary>
        /// <returns></returns>
        public static bool IsServersProper()
        {
            bool result = false;
            result = (Properties.Settings.Default.Enable321 && !string.IsNullOrEmpty(Properties.Settings.Default.Jav321))
                || (Properties.Settings.Default.EnableBus && !string.IsNullOrEmpty(Properties.Settings.Default.Bus))
                || (Properties.Settings.Default.EnableBusEu && !string.IsNullOrEmpty(Properties.Settings.Default.BusEurope))
                || (Properties.Settings.Default.EnableLibrary && !string.IsNullOrEmpty(Properties.Settings.Default.Library))
                || (Properties.Settings.Default.EnableDB && !string.IsNullOrEmpty(Properties.Settings.Default.DB))
                || (Properties.Settings.Default.EnableFC2 && !string.IsNullOrEmpty(Properties.Settings.Default.FC2))
                || (Properties.Settings.Default.EnableDMM && !string.IsNullOrEmpty(Properties.Settings.Default.DMM));

            return result;
        }



        /// <summary>
        /// 加载 321 识别码与网站转换规则，多 30M 内存
        /// </summary>
        public static void InitJav321IDConverter()
        {
            var jav321 = Resource_IDData.jav321;//来自于JavGo
            Stream jav321_stream = new MemoryStream(jav321);
            string str = "";
            using (var zip = new ZipArchive(jav321_stream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry zipArchiveEntry = zip.Entries[0];
                using (StreamReader sr = new StreamReader(zipArchiveEntry.Open()))
                {
                    str = sr.ReadToEnd();
                }
            }
            Jav321IDDict = new Dictionary<string, string>();
            str = str.Replace("\r\n", "\n").ToUpper();
            foreach (var item in str.Split('\n'))
            {
                
                if (item.IndexOf(",") > 0)
                {
                    if (Jav321IDDict.ContainsKey(item.Split(',')[1]))
                    {
                        Jav321IDDict[item.Split(',')[1]] = item.Split(',')[0];
                    }
                    else
                    {
                        Jav321IDDict.Add(item.Split(',')[1], item.Split(',')[0]);
                    }
                }
            }


        }



        /// <summary>
        /// 保存信息到 NFO 文件
        /// </summary>
        /// <param name="vedio"></param>
        /// <param name="NfoPath"></param>
        public static void SaveToNFO(DetailMovie vedio, string NfoPath)
        {
            var nfo = new NFO(NfoPath, "movie");
            nfo.SetNodeText("source", vedio.sourceurl);
            nfo.SetNodeText("title", vedio.title);
            nfo.SetNodeText("director", vedio.director);
            nfo.SetNodeText("rating", vedio.rating.ToString());
            nfo.SetNodeText("year", vedio.year.ToString());
            nfo.SetNodeText("countrycode", vedio.countrycode.ToString());
            nfo.SetNodeText("release", vedio.releasedate);
            nfo.SetNodeText("runtime", vedio.runtime.ToString());
            nfo.SetNodeText("country", vedio.country);
            nfo.SetNodeText("studio", vedio.studio);
            nfo.SetNodeText("id", vedio.id);
            nfo.SetNodeText("num", vedio.id);

            // 类别
            foreach (var item in vedio.genre?.Split(' '))
            {
                if (!string.IsNullOrEmpty(item)) nfo.AppendNewNode("genre", item);
            }
            // 系列
            foreach (var item in vedio.tag?.Split(' '))
            {
                if (!string.IsNullOrEmpty(item)) nfo.AppendNewNode("tag", item);
            }

            // Fanart
            nfo.AppendNewNode("fanart");
            foreach (var item in vedio.extraimageurl?.Split(';'))
            {
                if (!string.IsNullOrEmpty(item)) nfo.AppendNodeToNode("fanart", "thumb", item, "preview", item);
            }

            // 演员
            if (vedio.vediotype == (int)VedioType.欧美)
            {
                foreach (var item in vedio.actor?.Split('/'))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        nfo.AppendNewNode("actor");
                        nfo.AppendNodeToNode("actor", "name", item);
                        nfo.AppendNodeToNode("actor", "type", "Actor");
                    }
                }
            }
            else
            {
                foreach (var item in vedio.actor?.Split(actorSplitDict[vedio.vediotype]))
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        nfo.AppendNewNode("actor");
                        nfo.AppendNodeToNode("actor", "name", item);
                        nfo.AppendNodeToNode("actor", "type", "Actor");
                    }
                }
            }



        }


        public static  void addTag(ref Movie movie)
        {
            //添加标签戳
            if (Identify.IsHDV(movie.filepath) || movie.genre.IndexOf("高清") >= 0 || movie.tag.IndexOf("高清") >= 0 || movie.label.IndexOf("高清") >= 0) movie.tagstamps += "高清";
            if (Identify.IsCHS(movie.filepath) || movie.genre.IndexOf("中文") >= 0 || movie.tag.IndexOf("中文") >= 0 || movie.label.IndexOf("中文") >= 0) movie.tagstamps += "中文";
            if (Identify.IsFlowOut(movie.filepath) || movie.genre.IndexOf("流出") >= 0 || movie.tag.IndexOf("流出") >= 0 || movie.label.IndexOf("流出") >= 0) movie.tagstamps += "流出";
        }



        /// <summary>
        /// 判断拖入的是文件夹还是文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFile(string path)
        {
            try
            {
                FileAttributes attr = File.GetAttributes(path);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    return false;
                else
                    return true;
            }
            catch
            {
                return true;
            }

        }


        #region "配置xml"

        /// <summary>
        /// 读取原有的 config.ini到 xml
        /// </summary>
        public static void SaveScanPathToXml( )
        {
            if (!File.Exists("DataBase\\Config.ini")) return;
            Dictionary<string, StringCollection> DataBases = new Dictionary<string, StringCollection>();
            using (StreamReader sr = new StreamReader(DataBaseConfigPath))
            {
                try
                {
                    string content = sr.ReadToEnd();
                    List<string> info = content.Split('\n').ToList();
                    info.ForEach(arg => {
                        string name = arg.Split('?')[0];
                        StringCollection stringCollection = new StringCollection();
                        arg.Split('?')[1].Split('*').ToList().ForEach(path => { if (!string.IsNullOrEmpty(path)) stringCollection.Add(path); });
                       if(!DataBases.ContainsKey(name))  DataBases.Add(name, stringCollection);
                    });
                }
                catch { }
            }
            foreach(var item in DataBases)
            {
                SaveScanPathToConfig(item.Key, item.Value.Cast<string>().ToList());
            }
            File.Delete("DataBase\\Config.ini");
        }

        public static StringCollection ReadScanPathFromConfig(string name)
        {
            return new ScanPathConfig(name).Read();
        }


        public static void SaveScanPathToConfig(string name,List<string> paths)
        {
            ScanPathConfig scanPathConfig = new ScanPathConfig(name);
            scanPathConfig.Save(paths);
        }


        /// <summary>
        /// 读取原有的 config.ini到 xml
        /// </summary>
        public static void SaveServersToXml()
        {
            if (!File.Exists("ServersConfig.ini")) return;
            Dictionary<WebSite, Dictionary<string,string>> Servers = new Dictionary<WebSite, Dictionary<string, string>>();
            using (StreamReader sr = new StreamReader("ServersConfig.ini"))
            {
                try
                {
                    string content = sr.ReadToEnd();
                    List<string> rows = content.Split('\n').ToList();
                    rows.ForEach(arg => {
                        string name = arg.Split('?')[0];
                        WebSite webSite = WebSite.None;
                        Enum.TryParse<WebSite>(name, out webSite);
                        string[] infos = arg.Split('?')[1].Split('*');
                        Dictionary<string, string> info = new Dictionary<string, string>();
                        info.Add("Url", infos[0]);
                        info.Add("ServerName", infos[1]);
                        info.Add("LastRefreshDate", infos[2]);
                        if (!Servers.ContainsKey(webSite)) Servers.Add(webSite, info);
                    });
                }
                catch { }
            }
            foreach (var item in Servers)
            {
                SaveServersInfoToConfig(item.Key, item.Value.Values.ToList<string>());
            }
            File.Delete("ServersConfig.ini");
        }

        public static void SaveServersInfoToConfig(WebSite webSite, List<string> infos)
        {
            Dictionary<string, string> info = new Dictionary<string, string>();
            info.Add("Url", infos[0]);
            info.Add("ServerName", infos[1]);
            info.Add("LastRefreshDate", infos[2]);

            ServerConfig serverConfig = new ServerConfig(webSite.ToString());
            serverConfig.Save(info);
        }

        public static void DeleteServerInfoFromConfig(WebSite webSite)
        {

            if (!File.Exists("ServersConfig.ini")) return;
            ServerConfig serverConfig = new ServerConfig(webSite.ToString());
            serverConfig.Delete();
        }

        public static List<string> ReadServerInfoFromConfig(WebSite webSite)
        {
            
            if (!File.Exists("ServersConfig")) return new List<string>() { webSite.ToString(),"","" };


            List<string> result = new List<string>();

            result = new ServerConfig(webSite.ToString()).Read();
            if (result.Count < 3) result= new List<string>() { webSite.ToString(), "", "" };
            return result;
        }



        //最近观看

        public static void SaveRecentWatchedToXml()
        {
            if (!File.Exists("RecentWatch.ini")) return;
            Dictionary<string, List<string>> RecentWatchedes = new Dictionary<string, List<string>>();
            using (StreamReader sr = new StreamReader("RecentWatch.ini"))
            {
                try
                {
                    string content = sr.ReadToEnd();
                    List<string> rows = content.Split('\n').ToList();
                    rows.ForEach(arg => {
                        string date = arg.Split(':')[0];
                        var IDs = arg.Split(':')[1].Split(',').ToList();
                        if (!RecentWatchedes.ContainsKey(date)) RecentWatchedes.Add(date, IDs);
                    });
                }
                catch { }
            }
            foreach (var item in RecentWatchedes)
            {
                SaveRecentWatchedToConfig(item.Key, item.Value);
            }
            File.Delete("RecentWatch.ini");
        }

        public static void SaveRecentWatchedToConfig(string date, List<string> IDs)
        {
            Dictionary<string, string> info = new Dictionary<string, string>();
            RecentWatchedConfig  recentWatchedConfig = new RecentWatchedConfig(date);
            recentWatchedConfig.Save(IDs);
        }

        public static void ReadRecentWatchedFromConfig()
        {
            if (!File.Exists("RecentWatch")) return;
            RecentWatched = new RecentWatchedConfig("").Read();
        }


        public static void SaveRecentWatched()
        {
            foreach (var keyValuePair in RecentWatched)
            {
                if (keyValuePair.Key <= DateTime.Now && keyValuePair.Key >= DateTime.Now.AddDays(-1 * Properties.Settings.Default.RecentDays))
                {
                    if (keyValuePair.Value.Count > 0)
                    {
                        List<string> IDs = keyValuePair.Value.Where(arg => !string.IsNullOrEmpty(arg)).ToList();

                        string date = keyValuePair.Key.Date.ToString("yyyy-MM-dd");
                        RecentWatchedConfig recentWatchedConfig = new RecentWatchedConfig(date);
                        recentWatchedConfig.Save(IDs);
                    }
                }
            }






        }


        #endregion


        public static void CopyDatabaseInfo(string name)
        {
            string infoPath = AppDomain.CurrentDomain.BaseDirectory + "info.sqlite";
            string path = AppDomain.CurrentDomain.BaseDirectory + $"DataBase\\{name}.sqlite";
            DataTable  dataTable = dataTable = new DataTable();
            using (SQLiteConnection conn = new SQLiteConnection("data source=" + infoPath))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.CommandText = "SELECT * FROM movie";
                    SQLiteDataReader sQLiteDataReader = cmd.ExecuteReader();
                    dataTable.Load(sQLiteDataReader);
                }
            }

            //获取新数据库需要更新的值
            DataTable newdataTable = new DataTable();
            using (SQLiteConnection conn = new SQLiteConnection("data source=" + path))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();
                    cmd.CommandText = "SELECT id FROM movie";
                    SQLiteDataReader sQLiteDataReader = cmd.ExecuteReader();
                    newdataTable.Load(sQLiteDataReader);
                }
            }

            string sqltext = "UPDATE movie SET title=@title,releasedate=@releasedate,visits=@visits,director=@director,genre=@genre,tag=@tag," +
                "actor=@actor,studio=@studio,rating=@rating,chinesetitle=@chinesetitle,favorites=@favorites,label=@label,plot=@plot,outline=@outline,year=@year,runtime=@runtime,country=@country,countrycode=@countrycode,otherinfo=@otherinfo,extraimageurl=@extraimageurl where id=@id";



            using (SQLiteConnection conn = new SQLiteConnection("data source=" + path))
            {
                using (SQLiteCommand cmd = new SQLiteCommand())
                {
                    cmd.Connection = conn;
                    conn.Open();

                    foreach (DataRow row in newdataTable.Rows)
                    {
                        EnumerableRowCollection<DataRow> dataRows = dataTable.AsEnumerable().Where(myRow => myRow.Field<string>("id") == row["id"].ToString());
                        DataRow dataRow = null;
                        if (dataRows != null && dataRows.Count()>0)
                            dataRow = dataRows.First();
                        else continue;
                        cmd.CommandText = sqltext;
                        cmd.Parameters.Add("id", DbType.String).Value = dataRow["id"];
                        cmd.Parameters.Add("title", DbType.String).Value = dataRow["title"];
                        cmd.Parameters.Add("releasedate", DbType.String).Value = dataRow["releasedate"];
                        cmd.Parameters.Add("visits", DbType.Int16).Value = dataRow["visits"];
                        cmd.Parameters.Add("director", DbType.String).Value = dataRow["director"];
                        cmd.Parameters.Add("genre", DbType.String).Value = dataRow["genre"];
                        cmd.Parameters.Add("tag", DbType.String).Value = dataRow["tag"];
                        cmd.Parameters.Add("actor", DbType.String).Value = dataRow["actor"];
                        cmd.Parameters.Add("actorid", DbType.String).Value = dataRow["actorid"];
                        cmd.Parameters.Add("studio", DbType.String).Value = dataRow["studio"];
                        cmd.Parameters.Add("rating", DbType.Double).Value = dataRow["rating"];
                        cmd.Parameters.Add("chinesetitle", DbType.String).Value = dataRow["chinesetitle"];
                        cmd.Parameters.Add("favorites", DbType.Int16).Value = dataRow["favorites"];
                        cmd.Parameters.Add("label", DbType.String).Value = dataRow["label"];
                        cmd.Parameters.Add("plot", DbType.String).Value = dataRow["plot"];
                        cmd.Parameters.Add("outline", DbType.String).Value = dataRow["outline"];
                        cmd.Parameters.Add("year", DbType.Int16).Value = dataRow["year"];
                        cmd.Parameters.Add("runtime", DbType.Int16).Value = dataRow["runtime"];
                        cmd.Parameters.Add("country", DbType.String).Value = dataRow["country"];
                        cmd.Parameters.Add("countrycode", DbType.Int16).Value = dataRow["countrycode"];
                        cmd.Parameters.Add("otherinfo", DbType.String).Value = dataRow["otherinfo"];
                        cmd.Parameters.Add("sourceurl", DbType.String).Value = dataRow["sourceurl"];
                        cmd.Parameters.Add("source", DbType.String).Value = dataRow["source"];
                        cmd.Parameters.Add("extraimageurl", DbType.String).Value = dataRow["extraimageurl"];
                        cmd.ExecuteNonQuery();
                    }
                }
            }



        }

        public static bool IsToDownLoadInfo(Movie movie)
        {
            //if (!Properties.Settings.Default.DownLoadWhenNull)
            //    return true;
            //else
                return movie != null && (movie.title == ""  || movie.sourceurl == "" || movie.smallimageurl=="" || movie.bigimageurl=="");
        }

        public static bool IsProPerSqlite(string path)
        {
            if (!File.Exists(path)) return false;
            //是否具有表结构
            DB db = new DB(path, true);
            if (!db.IsTableExist("movie") || !db.IsTableExist("actress") || !db.IsTableExist("library") || !db.IsTableExist("javdb"))
                return false;
            
            return true;
        }

        public static string Unicode2String(string unicode)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         unicode, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }

        public static void SaveImage(string ID, byte[] ImageByte, ImageType imageType, string Url)
        {
            string FilePath;
            string ImageDir;
            if (imageType == ImageType.BigImage)
            {
                ImageDir = BasePicPath + $"BigPic\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }
            else if (imageType == ImageType.ExtraImage)
            {
                ImageDir = BasePicPath + $"ExtraPic\\{ID}\\";
                FilePath = ImageDir + Path.GetFileName(new Uri(Url).LocalPath);
            }
            else if (imageType == ImageType.SmallImage)
            {
                ImageDir = BasePicPath + $"SmallPic\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }
            else
            {
                ImageDir = BasePicPath + $"Actresses\\";
                FilePath = ImageDir + $"{ID}.jpg";
            }

            if (!Directory.Exists(ImageDir)) Directory.CreateDirectory(ImageDir);
            ByteArrayToFile(ImageByte, FilePath);
        }

        private static void ByteArrayToFile(byte[] byteArray, string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.LogF(ex);
            }
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("超时！");
                }
            }
        }

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        public static string GetFileMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }
        /// <summary>
        /// 加载 Gif
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        public static BitmapImage GifFromFile(string filepath, bool rotate = false)
        {
            try
            {
                using (var fs = new FileStream(filepath, System.IO.FileMode.Open))
                {
                    var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return null;
        }

        /// <summary>
        /// 防止图片被占用
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static BitmapImage BitmapImageFromFile(string filepath,bool rotate=false)
        {
            try
            {
                using (var fs = new FileStream(filepath, System.IO.FileMode.Open))
                {
                    var ms = new MemoryStream();
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception e) {  Console.WriteLine(e.Message); }
            return null;

        }

        //public static MemoryStream GetGifStream(string ID)
        //{
        //    try
        //    {
        //        using (var fs = new FileStream(BasePicPath + $"Gif\\{ID}.gif", System.IO.FileMode.Open))
        //        {
        //            var ms = new MemoryStream();
        //            fs.CopyTo(ms);
        //            ms.Position = 0;
        //            return ms;
        //        }
        //    }
        //    catch (Exception e) { Console.WriteLine(e.Message); }
        //    return null;
        //}





        public static BitmapImage GetBitmapImage(string filename, string imagetype, bool rotate = false)
        {
                filename = BasePicPath + $"{imagetype}\\{filename}.jpg";
                if (File.Exists(filename))
                    return BitmapImageFromFile(filename);
                else
                    return null;
        }

        public static BitmapImage GetExtraImage(string filepath)
        {
            if (File.Exists(filepath))
                return BitmapImageFromFile(filepath);
            else
                return null;
        }

    }

}
