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
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string moviefileName = string.Empty;
            string indexfileName = string.Empty;
            if (!System.IO.File.Exists("HomePage.html"))
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(this.textBox1.Text, "HomePage.html");
                }
            }

            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            // There are various options, set as needed
            htmlDoc.OptionFixNestedTags = true;

            // filePath is a path to a file containing the html
            htmlDoc.Load("HomePage.html");

            // Use:  htmlDoc.LoadHtml(xmlString);  to load from a string (was htmlDoc.LoadXML(xmlString)

            // ParseErrors is an ArrayList containing any errors from the Load statement
            foreach (HtmlNode link in htmlDoc.DocumentNode.SelectNodes("//div[@id='sidebar1']/ul/li/div[@class='textwidget']/ul/li/a[@href]"))
            {
                if (link.InnerText.ToLower().Contains("movie index"))
                {
                    HtmlAttribute indexattr = link.Attributes["href"];
                    WebClient client = new WebClient();
                    
                    indexfileName = GetRandomName() + ".html";
                    client.DownloadFile(indexattr.Value, indexfileName);
                    HtmlAgilityPack.HtmlDocument indexhtmlDoc = new HtmlAgilityPack.HtmlDocument();

                    // There are various options, set as needed
                    indexhtmlDoc.OptionFixNestedTags = true;

                    // filePath is a path to a file containing the html
                    indexhtmlDoc.Load(indexfileName);

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
                        moviefileName = GetRandomName() + ".html";
                        client.DownloadFile(movieattr.Value, moviefileName);
                        HtmlAgilityPack.HtmlDocument moviehtmlDoc = new HtmlAgilityPack.HtmlDocument();

                        // There are various options, set as needed
                        moviehtmlDoc.OptionFixNestedTags = true;

                        // filePath is a path to a file containing the html
                        moviehtmlDoc.Load(moviefileName);

                        foreach (HtmlNode songlink in moviehtmlDoc.DocumentNode.SelectNodes("//div[@class='content']/ul/li/a[@href]"))
                        {
                            HtmlAttribute songAttr = songlink.Attributes["href"];
                            string songName = HttpUtility.HtmlDecode(songlink.InnerText);
                            string songUrl = songAttr.Value;
                            if (songName.EndsWith(".mp3"))
                            {
                                if (!System.IO.File.Exists(songName))
                                    DownloadFile(HttpUtility.UrlDecode(songUrl), filmName, songName);
                            }
                        }

                        System.IO.File.Delete(moviefileName);
                    }

                    System.IO.File.Delete(indexfileName);
                }
            }
        }

        private void DownloadFile(string songUrl, string filmName, string songName)
        {
            var client = new WebClient();
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            List<string> obj = new List<string>(new string[] { songUrl, filmName, songName });
            client.DownloadFileAsync(new Uri(songUrl), songName);

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
