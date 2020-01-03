using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace PikabuProfilePostsViewer
{
	public partial class MainForm : Form
	{
        int pages = 0, posts = 0;
        int currentPost = 1;
        int lastPage = 10; //posts on the last page; literally also posts per page
        int sortColumn = -1; //sorting ListView flag
        string username = string.Empty;
        string url = "https://pikabu.ru/@";
        string postNumber = "NULL", postDate = "NULL", postRating = "NULL", postTitle = "NULL", postLink = "NULL";
        Regex postCode = new Regex("[0-9]+?$");
        HtmlWeb web = new HtmlWeb();        
        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
        //ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType) 768; //doesn't work. WHY? :(

        void Sort(ListView listView, ColumnClickEventArgs e) 
        {
            if (e.Column != sortColumn)
            {
                sortColumn = e.Column;
                listView.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView.Sorting == SortOrder.Ascending)
                    listView.Sorting = SortOrder.Descending;
                else
                    listView.Sorting = SortOrder.Ascending;
            }
            listView.Sort();
            listView.ListViewItemSorter = new ListViewItemComparer(e.Column, listView.Sorting);
        }
        public MainForm()
		{
			InitializeComponent();
		}
        void Button1Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            username = textBox1.Text;
            if (username.Length < 5) MessageBox.Show("Invalid username: too short");
            else
            {
                url += username;
                web.AutoDetectEncoding = false;
                web.OverrideEncoding = Encoding.GetEncoding("windows-1251");
                doc = web.Load(url);
                switch (web.StatusCode)
                {
                    case HttpStatusCode.OK:
                        {
                            posts = Convert.ToInt32(doc.DocumentNode.SelectSingleNode("//div[contains(@class,'profile__section')][2]/span[contains(@class,'profile__digital')][4]/b").InnerText);
                            pages = posts % 10 == 0 ? posts / 10 : (posts / 10 + 1);
                            label2.Text = "Status: " + posts.ToString() + " posts found on " + pages.ToString() + " pages. 0 posts loaded";
                            for (int p = 1; p <= pages; p++)
                            {
                                if (p == pages) lastPage = posts % 10;
                                try
                                {
                                    doc = web.Load(url + "?page=" + p.ToString()); //if you place this line in the end of the page loop (near line 95-96), it causes a bug: the first post from the 1st page will be also the first on the 2nd page, istead the first post from the 2nd page will be the first on the 3rd page, etc.
                                    List<HtmlNode> nodes = doc.DocumentNode.SelectNodes("//div[@class='story__user user']").Cast<HtmlNode>().ToList(); //lines 72 and 77 have been written because the line 76 doesn't work
                                    for (int i = 1; i <= lastPage; i++)
                                    {
                                        postNumber = currentPost.ToString();
                                        //postDate = doc.DocumentNode.SelectSingleNode("//div[@class='story__main']/div[@class='story__footer']/div[@class='story__user user']/div[@class='user__info user__info_left']/div[@class='user__info-item'][" + (i * 2).ToString() + "]/time").GetAttributeValue("datetime", "NULL"); //doesn't work. WHY? :(
                                        postDate = nodes[i - 1].SelectSingleNode(".//div[@class='user__info-item']/time").GetAttributeValue("datetime", "NULL");
                                        DateTimeOffset dto = DateTimeOffset.Parse(postDate, CultureInfo.InvariantCulture);
                                        postDate = dto.ToLocalTime().ToString("dd'/'MM'/'yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                        postRating = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'stories-feed__container')]/article[" + i.ToString() + "]").GetAttributeValue("data-rating", "DELETED");
                                        postTitle = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'stories-feed__container')]/article[" + i.ToString() + "]/div[contains(@class, 'story__main')]/header/h2/a").InnerText.Replace("&quot;", "\"");
                                        postLink = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'stories-feed__container')]/article[" + i.ToString() + "]/div[contains(@class, 'story__main')]/header/h2/a").GetAttributeValue("href", "NULL");
                                        if (checkBox1.Checked)
                                        {
                                            Match m = postCode.Match(postLink);
                                            postLink = "https://pikabu.ru/story/_" + m.Value;
                                        }
                                        ListViewItem postRow = new ListViewItem();
                                        postRow.SubItems[0].Text = postNumber;
                                        postRow.SubItems.AddRange(new string[] { postDate, postRating, postTitle, postLink });
                                        listView1.Items.Add(postRow);
                                        currentPost++;
                                        label2.Text = "Status: " + posts.ToString() + " posts found on " + pages.ToString() + " pages. " + listView1.Items.Count.ToString() + " posts loaded";
                                    }
                                }
                                catch { MessageBox.Show("An unexpected error occured. Please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                            }
                            break;
                        }
                    case HttpStatusCode.NotFound:
                        MessageBox.Show("Page not found: wrong user's name or the account has been deleted", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
                    case HttpStatusCode.Forbidden:
                        MessageBox.Show("Access forbidden", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.GatewayTimeout:
                        MessageBox.Show("Could not access remote server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); break;
                }
                url = "https://pikabu.ru/@";
                pages = 0;
                posts = 0;
                currentPost = 1;
                lastPage = 10;
                postNumber = "NULL";
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(@"Pikabu userprofiles");
            if (di.Exists == false) di.Create();
            StreamWriter sw = new StreamWriter(@"Pikabu userprofiles\" + textBox1.Text + ".txt", false, Encoding.Unicode);
            sw.AutoFlush = true;
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("List of {0}'s Pikabu posts:", textBox1.Text));
            sb.Append(Environment.NewLine);
            foreach (ListViewItem lvi in listView1.Items)
            {
                foreach (ListViewItem.ListViewSubItem listViewSubItem in lvi.SubItems)
                    sb.Append(string.Format("{0}\t", listViewSubItem.Text));
                sb.Append(Environment.NewLine);
            }
            sb.Append("Timestamp: " + DateTime.Now);
            sw.WriteLine(sb.ToString());
            sw.Close();
            FileInfo file = new FileInfo(@"Pikabu userprofiles\" + textBox1.Text + ".txt");
            if (file.Exists == true)
                label2.Text = "Status: Saving process completed. Ready for the next user";
        }
        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Pikabu profile posts viewer\nVersion 1.0 (04.01.2020)\nMade by serhiy1994 from Pikabu\nThanks to: Jawad, Jeff Mercado (stackoverflow), OwenGlendower (Cyberforum), iranaut, Malica (Pikabu)", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            Sort(listView1, e);
        }
    }
}