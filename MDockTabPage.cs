using Global;
using Markdig;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;
using Markdig.Syntax;
using Markdig.Renderers.Html;
using System;
using System.IO;
using System.Windows.Forms;
//using Microsoft.Web.WebView2.Core;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Media;

namespace mdock;

internal class MDockTabPage : System.Windows.Forms.TabPage
{
    private MDockMemo text = null;
    public MDockTabPage(MDockMemo text)
    {
        this.text = text;
        this.Text = text.Title;
        this.Padding = new System.Windows.Forms.Padding(3);
        //this.Size = new System.Drawing.Size(792, 400);
        //this.TabIndex = 0;
        this.UseVisualStyleBackColor = true;
        //
        this.splitContainer1 = new System.Windows.Forms.SplitContainer();
        this.richTextBox1 = new System.Windows.Forms.RichTextBox();
        //
        // 
        // splitContainer1
        // 
        this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitContainer1.Location = new System.Drawing.Point(3, 3);
        this.splitContainer1.Name = "splitContainer1";
        // 
        // splitContainer1.Panel1
        // 
        this.splitContainer1.Panel1.Controls.Add(this.richTextBox1);
        this.splitContainer1.Size = new System.Drawing.Size(786, 394);
        this.splitContainer1.SplitterDistance = 393;
        this.splitContainer1.TabIndex = 1;
        // 
        // richTextBox1
        // 
        this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.richTextBox1.Location = new System.Drawing.Point(0, 0);
        this.richTextBox1.Name = "richTextBox1";
        this.richTextBox1.Size = new System.Drawing.Size(393, 394);
        this.richTextBox1.TabIndex = 0;
        //this.richTextBox1.Text = "";
        this.richTextBox1.Text = text.Text;
        this.richTextBox1.SelectionStart = 0;
        //
        this.richTextBox1.ZoomFactor = 2;
        //
        this.browser = new Browser();
        this.Render();
        //InitializeBrowser().Wait();
        //this.browser.NavigationCompleted += WebView_NavigationCompleted;
        /*
        this.browser.NavigationStarting += (s, e) =>
        {
            Util.Message(e.Uri.ToString());
        };
        */
        //Util.Message("1");
        //InitializeBrowser().Wait();
        //Util.Message("2");
        //WebView_NavigationCompleted(null, null);
        //Util.Message("3");
        this.browser.Dock = DockStyle.Fill;
        this.splitContainer1.Panel2.Controls.Add(this.browser);
        //
        this.richTextBox1.LinkClicked += (s, e) =>
        {
            System.Diagnostics.Process.Start(e.LinkText);
            Program.form2.Visible = false;
        };
        this.richTextBox1.TextChanged += (s, e) =>
        {
            this.Render();
            //this.StartRenderTimer();
        };
        this.renderTimer.Tick += (s, e) =>
        {
            this.renderTimer.Stop();
            this.Render();
        };
        this.Controls.Add(this.splitContainer1);
    }

    public void Unload()
    {
        //this.browser.Dispose();
        this.browser.Unload();
    }

    public void FindPrev()
    {
        if (Program.form2.SearchPattern == "") return;
        int idx = this.richTextBox1.Find(Program.form2.SearchPattern, 0, this.richTextBox1.SelectionStart, RichTextBoxFinds.Reverse);
        if (idx == -1) SystemSounds.Beep.Play();
    }
    public void FindNext()
    {
        if (Program.form2.SearchPattern == "") return;
        try
        {
            int idx = this.richTextBox1.Find(Program.form2.SearchPattern, this.richTextBox1.SelectionStart + 1, RichTextBoxFinds.None);
            if (idx == -1) SystemSounds.Beep.Play();
        }
        catch
        {
        }
#if false
        if (idx < 0)
        {
            Util.Message($"「{Program.form2.SearchPattern}」が見つかりませんでした");
        }
#endif
    }

    public string DumpText()
    {
        var lines = Util.SplitTextIntoLines(this.richTextBox1.Text);
        var result = "# !!!!" + this.Text + "\n";
        foreach(var line in lines)
        {
            result += line + "\n";
        }
        return result;
    }
    private void StartRenderTimer()
    {
        this.renderTimer.Stop();
        this.renderTimer.Start();
    }
    public void Render()
    {
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
          .UseAutoLinks()
          .Build();
        var document = Markdown.Parse("# " + this.Text + "\n\n" + this.richTextBox1.Text, pipeline);
        foreach (var descendant in document.Descendants())
        {
            string url;
            if (descendant is AutolinkInline autoLink) url = autoLink.Url;
            else if (descendant is LinkInline linkInline) url = linkInline.Url;
            else continue;
            if (url != null && Uri.TryCreate(url, UriKind.Absolute, out _))
                descendant.GetAttributes().AddPropertyIfNotExist("target", "_blank");
        }
        var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        pipeline.Setup(renderer);
        renderer.Render(document);
        string html = writer.ToString();
        html = """<meta charset="utf-8">""" + "\n" + html;
        this.browser.LoadHtml(html);
    }
#if false
#endif
    public void GotoBottom()
    {
        this.richTextBox1.Select(this.richTextBox1.Text.Length, 0);
        this.richTextBox1.Focus();
    }
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.RichTextBox richTextBox1;
    private System.Threading.Timer timer;
    private Browser browser;
    private bool isFirst = true;
    private Timer renderTimer = new Timer() { Interval = 500 };
}
