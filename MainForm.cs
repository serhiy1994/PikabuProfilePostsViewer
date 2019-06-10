using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace PikabuProfilePostsViewer
{
	public partial class MainForm : Form
	{
		string username=string.Empty;
		string url="https://pikabu.ru/@";
		//HtmlWeb web = new HtmlWeb();
		public MainForm()
		{
			InitializeComponent();
		}
		void Button1Click(object sender, EventArgs e)
		{
			username=textBox1.Text;
			if (username.Length<5) MessageBox.Show("Invalid username: too short");
			else{
				url+=username;
				WebClient client = new WebClient(); //TODO: change to overrideencoding in HAP
				var data = client.DownloadData(url);
				var html = Encoding.GetEncoding("windows-1251").GetString(data);				
				HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
				doc.LoadHtml(html);
				textBox2.Text=doc.DocumentNode.OuterHtml;
				string NoP =doc.DocumentNode.SelectSingleNode("//section[contains(@class, 'section_padding_none')]").
					SelectSingleNode("(//div[@class='profile__section'])[2]").
					SelectSingleNode("(//span[@class='profile__digital'])[2]").
					SelectSingleNode("//b").InnerText;
				MessageBox.Show(NoP.ToString());
			}
		}
	}
}