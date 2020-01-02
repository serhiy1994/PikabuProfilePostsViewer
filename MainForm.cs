using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace PikabuProfilePostsViewer
{
	public partial class MainForm : Form
	{
        int pages = 0;
        int posts = 0;
        int currentPost = 1;
        int lastPage = 10;
        int sortColumn = -1;
        string username = string.Empty;
        string url = "https://pikabu.ru/@";
        string postNumber = "NULL";
        string postDate = "NULL";
        string postRating = "NULL";
        string postTitle = "NULL";
        string postLink = "NULL";
        Regex regex = new Regex("[0-9]+?$");
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
                            label2.Text = "Status: " + posts.ToString() + " posts found on " + pages.ToString() + " pages";
                            for (int p = 1; p <= pages; p++)
                            {
                                if (p == pages) lastPage = posts % 10;
                                try
                                {
                                    doc = web.Load(url + "?page=" + p.ToString()); //ukazat pochemu
                                    for (int i = 0; i < lastPage; i++)
                                    {
                                        postNumber = currentPost.ToString();
                                        //postDate = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'stories-feed__container')]/article[" + (i + 1).ToString() + "]/div[contains(@class, 'story__main')]/div[contains(@class, 'story__footer')]/div[contains(@class, 'story__user user')]/div[contains(@class, 'user__info-item')]/time").GetAttributeValue("datetime", "NULL");
                                        postRating = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'stories-feed__container')]/article[" + (i + 1).ToString() + "]").GetAttributeValue("data-rating", "DELETED");
                                        postTitle = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'stories-feed__container')]/article[" + (i + 1).ToString() + "]/div[contains(@class, 'story__main')]/header/h2/a").InnerText.Replace("&quot;", "\"");
                                        postLink = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'stories-feed__container')]/article[" + (i + 1).ToString() + "]/div[contains(@class, 'story__main')]/header/h2/a").GetAttributeValue("href", "NULL");
                                        if (checkBox1.Checked)
                                        {
                                            Match m = regex.Match(postLink);
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
        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            Sort(listView1, e);
        }
    }
}