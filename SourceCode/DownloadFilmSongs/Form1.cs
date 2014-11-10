using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;
using TagLib;

namespace DownloadFilmSongs
{
    public partial class Form1 : Form
    {
        Queue<SongInfo> songQueues = new Queue<SongInfo>();
        string outputPath = Environment.CurrentDirectory;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WebClient HomePageclient = new WebClient();
            string path = outputPath + "\\HTMLPAGES";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            HomePageclient.DownloadFileAsync(new Uri(HttpUtility.UrlDecode(this.textBox1.Text)), path + "\\HomePage.html");
            HomePageclient.DownloadFileCompleted += HomePageclient_DownloadFileCompleted;
        }

        void HomePageclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {

            string indexfileName = string.Empty;
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            if (!System.IO.File.Exists(outputPath + "\\HTMLPAGES\\HomePage.html"))
                return;
            htmlDoc.Load(outputPath + "\\HTMLPAGES\\HomePage.html");
            if (htmlDoc.DocumentNode.SelectNodes("//div[@id='sidebar1']/ul/li/div[@class='textwidget']/ul/li/a[@href]") == null)
                return;
            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//div[@id='sidebar1']/ul/li/div[@class='textwidget']/ul/li/a[@href]"))
            {
                if (link.InnerText.ToLower().Contains("movie index"))
                {
                    HtmlAttribute indexattr = link.Attributes["href"];
                    indexfileName = outputPath + "\\HTMLPAGES\\" + GetRandomName() + ".html";
                    WebClient movieindexpageclient = new WebClient();
                    movieindexpageclient.DownloadFileAsync(new Uri(HttpUtility.UrlDecode(indexattr.Value)), indexfileName, indexfileName);
                    movieindexpageclient.DownloadFileCompleted += movieindexpageclient_DownloadFileCompleted;
                    //System.IO.File.Delete(indexfileName);
                }
            }
        }

        void movieindexpageclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            HtmlAgilityPack.HtmlDocument indexhtmlDoc = new HtmlAgilityPack.HtmlDocument();
            string movieIndexPageName = e.UserState as string;
            string moviefileName = string.Empty;
            // There are various options, set as needed
            indexhtmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            indexhtmlDoc.Load(movieIndexPageName);

            if (indexhtmlDoc.DocumentNode.SelectNodes("//div[starts-with(@id, 'azindex-')]/div[@class='azindex']/ul/li/a[@href]") == null)
                return;
            foreach (HtmlNode movielink in indexhtmlDoc.DocumentNode.SelectNodes("//div[starts-with(@id, 'azindex-')]/div[@class='azindex']/ul/li/a[@href]"))
            {

                SongInfo songInfo = new SongInfo();
                HtmlAttribute movieattr = movielink.Attributes["href"];
                string filmName = "Unknown";
                if (movielink.ChildNodes.Count > 0)
                {
                    filmName = movielink.ChildNodes[0].InnerText;
                    songInfo.MovieName = filmName;
                }
                WebClient movieclient = new WebClient();
                moviefileName = outputPath + "\\HTMLPAGES\\" + GetRandomName() + ".html";
                if (!Directory.Exists(moviefileName))
                    Directory.CreateDirectory(moviefileName);
                movieclient.DownloadFileAsync(new Uri(HttpUtility.UrlDecode(movieattr.Value)), moviefileName, moviefileName);
                movieclient.DownloadFileCompleted += movieclient_DownloadFileCompleted;
            }
        }

        void movieclient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            HtmlAgilityPack.HtmlDocument moviehtmlDoc = new HtmlAgilityPack.HtmlDocument();
            string currMovieName = e.UserState as string;
            moviehtmlDoc.OptionFixNestedTags = true;
            moviehtmlDoc.Load(currMovieName);
            if (moviehtmlDoc.DocumentNode.SelectNodes("//div[@class='content']/ul/li/a[@href]") == null)
                return;

            foreach (HtmlNode songlink in moviehtmlDoc.DocumentNode.SelectNodes("//div[@class='content']/ul/li/a[@href]"))
            {
                HtmlAttribute songAttr = songlink.Attributes["href"];
                string songName = outputPath + "\\SONGS\\" + currMovieName;
                if (!Directory.Exists(songName))
                    Directory.CreateDirectory(songName);
                string songUrl = songAttr.Value;
                if (songName.EndsWith(".mp3"))
                {
                    if (!System.IO.File.Exists(songName))
                        DownloadFile(HttpUtility.UrlDecode(songUrl), currMovieName, songName + "\\" + HttpUtility.HtmlDecode(songlink.InnerText));
                }
            }
            //System.IO.File.Delete(moviefileName);
        }

        private void DownloadFile(string songUrl, string filmName, string songName)
        {
            var songclient = new WebClient();
            //List<string> obj = new List<string>(new string[] { songUrl, filmName, songName });
            songclient.DownloadFileAsync(new Uri(songUrl), songName, songName);
            songclient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //if (!e.Cancelled && e.Error == null)
            //{
            //    List<string> args = e.UserState as List<string>;
            //    TagLib.File tagFile = TagLib.File.Create(args[2]);
            //    tagFile.Tag.Album = args[1];
            //    tagFile.Save();
            //    System.IO.File.Move(args[2], this.textBox2.Text + "\\" + args[1] + "\\" + args[2]);
            //}
            string songName = e.UserState as string;
            MessageBox.Show("File downloaded");
        }

        private string GetRandomName()
        {
            string returnValue = string.Empty;
            Random r = new Random();
            int i = r.Next(1000000000);
            if (System.IO.File.Exists(i.ToString() + ".html"))
                returnValue = GetRandomName();
            else
                returnValue = i.ToString();
            return returnValue;
        }
    }

    public class SongInfo
    {
        public string fileName;
        public string MovieName;
    }
}
