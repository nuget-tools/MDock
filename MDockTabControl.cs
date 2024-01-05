using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System;
using Global;

namespace mdock;

/// <summary>
/// タブの移動可能タブコントロールサンプル
/// </summary>
internal class MDockTabControl : TabControl
{
    private MDockTabControl()
    {
    }
    public MDockTabControl(Form1 form1): base()
    {
        this.form1 = form1;
        InitializeComponent();
        this.AllowDrop = true;
    }
    Form1 form1 = null;

    public void FindPrev()
    {
        if (this.SelectedTab != null)
        {
            MDockTabPage tabPage = this.SelectedTab as MDockTabPage;
            tabPage.FindPrev();
        }
    }
    public void FindNext()
    {
        if (this.SelectedTab != null)
        {
            MDockTabPage tabPage = this.SelectedTab as MDockTabPage;
            tabPage.FindNext();
        }
    }
    public List<TabPage> TabPageList()
    {
        List<TabPage> list = new List<TabPage>();
        foreach(TabPage tab in this.TabPages)
        {
            list.Add(tab);
        }
        return list;
    }
    public void Remove(TabPage tab)
    {
        MDockTabPage tabPage = tab as MDockTabPage;
        tabPage.Unload();
        this.TabPages.Remove(tab);
    }
    public void Clear(List<TabPage> list)
    {
        foreach (TabPage tab in list)
        {
            this.Remove(tab);
        }
    }
    public void AddMDocText(MDockMemo text)
    {
        if (text is null)
        {
            text = new MDockMemo()
            {
                Title = "ページ",
                Text = ""
            };
        }
        this.Controls.Add(new MDockTabPage(text));
    }

    public string DumpText()
    {
        string result = "";
        for (int i = 0; i < this.TabPages.Count; i++)
        {
            result += ((MDockTabPage)this.TabPages[i]).DumpText();
        }
        return result;
    }

    private TabPage predraggedTab;
    /// <summary> 
    /// 必要なデザイナー変数です。
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// 使用中のリソースをすべてクリーンアップします。
    /// </summary>
    /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary> 
    /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
    /// コード エディターで変更しないでください。
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        //  this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.DoubleClick += (s, e) => this.EditTabText();
        //
        this.SelectedIndexChanged += (s, e) =>
        {
            var page = (MDockTabPage) this.SelectedTab;
            page.Render();
        };

    }

    private void EditTabText()
    {
        placeHolder = new Form();
        placeHolder.SuspendLayout();

        textbox = new TextBox();
        textbox.BackColor = SystemColors.InactiveCaption;
        textbox.BorderStyle = BorderStyle.None;
        textbox.TextAlign = HorizontalAlignment.Center;
        textbox.KeyPress += Textbox_KeyPress;

        placeHolder.AutoScaleMode = AutoScaleMode.Font;
        placeHolder.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        placeHolder.Controls.Add(textbox);
        placeHolder.FormBorderStyle = FormBorderStyle.None;
        placeHolder.TopMost = true;
        placeHolder.Load += PlaceHolder_Load;
        placeHolder.Deactivate += PlaceHolder_Deactivate;

        placeHolder.ResumeLayout(false);
        placeHolder.PerformLayout();

        placeHolder.Show();
    }

    private void PlaceHolder_Load(object sender, EventArgs e)
    {
        Rectangle tabRect = this.GetTabRect(this.SelectedIndex);
        tabRect.Location = this.FindForm().PointToScreen(tabRect.Location);
        placeHolder.DesktopBounds = tabRect;
        textbox.Bounds = new Rectangle(0, (tabRect.Height - textbox.Height) / 2, tabRect.Width, tabRect.Height);
        textbox.Text = this.SelectedTab.Text;
    }

    private void PlaceHolder_Deactivate(object sender, EventArgs e)
    {
#if true
        if (string.IsNullOrEmpty(textbox.Text))
        {
            textbox.Text = Dirs.GetFileNameWithoutExtension(Program.Core.CurrentPath);
        }
#endif
        if (!string.IsNullOrEmpty(textbox.Text))
        {
            this.SelectedTab.Text = textbox.Text;
            ((MDockTabPage)this.SelectedTab).Render();
        }
        placeHolder.Close();
    }

    private void Textbox_KeyPress(object sender, KeyPressEventArgs e)
    {
        switch ((Keys)e.KeyChar)
        {
            case Keys.Escape:
                textbox.Text = string.Empty;
                goto case Keys.Enter;

            case Keys.Enter:
                e.Handled = true;
                placeHolder.Hide();
                return;
        }
    }

    Form placeHolder = null;
    TextBox textbox = null;

#if false
    public MDockTabControl()
    {
        InitializeComponent();
        this.AllowDrop = true;
    }
#endif

    protected override void OnMouseDown(MouseEventArgs e)
    {
        predraggedTab = getPointedTab();

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        predraggedTab = null;

        base.OnMouseUp(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && predraggedTab != null)
            this.DoDragDrop(predraggedTab, DragDropEffects.Move);

        base.OnMouseMove(e);
    }

    protected override void OnDragOver(DragEventArgs drgevent)
    {
        //TabPage draggedTab = (TabPage)drgevent.Data.GetData(typeof(TabPage));
        MDockTabPage draggedTab = (MDockTabPage)drgevent.Data.GetData(typeof(MDockTabPage));
        TabPage pointedTab = getPointedTab();

        if (draggedTab == predraggedTab && pointedTab != null)
        {
            drgevent.Effect = DragDropEffects.Move;

            if (pointedTab != draggedTab)
                swapTabPages(draggedTab, pointedTab);
        }

        base.OnDragOver(drgevent);
    }

    private TabPage getPointedTab()
    {
        //Util.Message($"{this.TabPages.Count}");
        for (int i = 0; i < this.TabPages.Count; i++)
            if (this.GetTabRect(i).Contains(this.PointToClient(Cursor.Position)))
                return this.TabPages[i];

        return null;
    }

    private void swapTabPages(TabPage src, TabPage dst)
    {
        int srci = this.TabPages.IndexOf(src);
        int dsti = this.TabPages.IndexOf(dst);

        this.TabPages[dsti] = src;
        this.TabPages[srci] = dst;

        if (this.SelectedIndex == srci)
            this.SelectedIndex = dsti;
        else if (this.SelectedIndex == dsti)
            this.SelectedIndex = srci;

        this.Refresh();
    }
}