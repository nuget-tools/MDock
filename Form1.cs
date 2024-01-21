using Global;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Antlr4.Runtime.Tree.Xpath;

namespace mdock;

public partial class Form1 : Form, IRemoteObject
{
    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    // 画面を元の大きさに戻す
    public const int SW_RESTORE = 9;

    //public void StartupNextInstance(string[] args)
    public void StartupNextInstance(string? path, int pos)
    {
        // メイン・ウィンドウが最小化されていれば元に戻す
        if (IsIconic(this.Handle))
        {
            ShowWindowAsync(this.Handle, SW_RESTORE);
        }
        // メイン・ウィンドウを最前面に表示する
        SetForegroundWindow(this.Handle);
        //
        //Util.Message(args, "既に起動しています");
        this.Invoke((MethodInvoker)delegate
        {
#if false
            if (path is null)
            {
                path = this.GetOpenFilePath();
                if (path is null) return;
            }
            this.LoadFromPath(path, pos);
#else
            if (path is null)
            {
                //path = this.GetOpenFilePath();
                //if (path is null) return;
                if (Program.Props.Props.lastDir != null)
                {
                    string lastDir = Program.Props.Props.lastDir;
                    var list = Util.ExpandWildcard(lastDir + "\\*.mdock");
                    if (list.Length > 0)
                    {
                        Program.form2.ShowUp(list);
                        return;
                    }
                }
                //path = this.GetOpenFilePath();
                //if (path is null) return;
            }
            else
            {
                this.LoadFromPath(path, pos);
            }
#endif
        });
    }

    public Form1(/*string? path*/)
    {
        InitializeComponent();
        //
        this.Text = "MDock";
        //
        this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //
        this.tabControl1 = new MDockTabControl(this);
        this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
        //this.tabControl1.Location = new System.Drawing.Point(0, 0);
        this.tabControl1.Name = "tabControl1";
        this.tabControl1.SelectedIndex = 0;
        //this.tabControl1.Size = new System.Drawing.Size(800, 426);
        this.tabControl1.TabIndex = 0;
        //
        this.panel1.Controls.Add(this.tabControl1);
        //
        this.StartPosition = FormStartPosition.Manual;
        Rectangle screen = Screen.FromPoint(Cursor.Position).WorkingArea;
        this.ClientSize = new Size((int)(screen.Width * 0.75), (int)(screen.Height * 0.75)); /**/
        //this.Size = new Size((int)(screen.Width * 0.75), (int)(screen.Height * 0.75)); /**/
        int w = Width >= screen.Width ? screen.Width : (screen.Width + Width) / 2;
        int h = Height >= screen.Height ? screen.Height : (screen.Height + Height) / 2;
        this.Location = new Point(screen.Left + (screen.Width - w) / 2, screen.Top + (screen.Height - h) / 2);
        this.Size = new Size(w, h);
        //
        this.FormClosing += (s, e) =>
        {
            //this.tabPage1.WriteToFile(path);
            this.SaveMemoList();
        };
        //this.StartupNextInstance(path);
    }

    private void NewMemo()
    {
        if (Program.Core.CurrentPath == null)
        {
            MessageBox.Show("ドキュメントを開くか新規作成してください");
            return;
        }
        string name = Microsoft.VisualBasic.Interaction.InputBox("ページ名称を入力してください", "確認", "新規ページ", -1, -1);
        if (name == "") return;
        var memo = new MDockMemo()
        {
            Title = name,
            Text = ""

        };
        this.tabControl1.AddMDocText(memo);
        var timer = new System.Threading.Timer((state) =>
        {
            this.Invoke((MethodInvoker)(() =>
            {
                this.tabControl1.SelectedTab = this.tabControl1.TabPages[this.tabControl1.TabPages.Count - 1];
                ((MDockTabPage)this.tabControl1.SelectedTab).GotoBottom();
            }));
            ((System.Threading.Timer)state).Dispose();
        });
        timer.Change(TimeSpan.FromMilliseconds(50), TimeSpan.Zero);
    }

    private void DeleteMemo()
    {
        if (Program.Core.CurrentPath == null)
        {
            MessageBox.Show("ドキュメントを開くか新規作成してください");
            return;
        }
    }

    //private System.Windows.Forms.TabControl tabControl1;
    private MDockTabControl tabControl1;
    private MDockTabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;

    private void SaveMemoList(bool force = false)
    {
        if (Program.Core.CurrentPath is null) return;
        string text = this.tabControl1.DumpText();
        if (!force && text == Program.Core.CurrentText)
        {
            //Util.Message("変更されてません");
            return;
        }
        else
        {
            File.WriteAllText(Program.Core.CurrentPath, text);
            Program.Core.CurrentText = text;
            //Util.Message("保存しました");
        }
    }

    private void SaveAsMemoList()
    {
        string path = this.GetSaveFilePath();
        if (path is null) return;
        string text = this.tabControl1.DumpText();
        File.WriteAllText(path, text);
        Program.Core.CurrentText = text;
        Program.Core.CurrentPath = path;
    }

    private string? GetOpenFilePath()
    {
        OpenFileDialog ofd = new OpenFileDialog();
        if (Program.Props.Props.lastDir != null)
        {
            ofd.InitialDirectory = Program.Props.Props.lastDir;
        }
        else
        {
            ofd.InitialDirectory = Dirs.DocumentsPath();
        }
        ofd.Filter = "MDockファイル(*.mdock)|*.mdock";
        ofd.Title = "開くファイルを選択してください";
        ofd.RestoreDirectory = true;
        ofd.CheckFileExists = true;
        ofd.CheckPathExists = true;
        if (ofd.ShowDialog() != DialogResult.OK)
        {
            return null;
        }
        Program.Props.Props.lastDir = Dirs.GetParent(ofd.FileName);
        return ofd.FileName;
    }
    private string? GetSaveFilePath()
    {
        SaveFileDialog sfd = new SaveFileDialog();
        if (Program.Props.Props.lastDir != null)
        {
            sfd.InitialDirectory = Program.Props.Props.lastDir;
        }
        else
        {
            sfd.InitialDirectory = Dirs.DocumentsPath();
        }
        long count = 1;
        string initName = null;
        while (true)
        {
            if (count == 1)
            {
                initName = "新規 MDock ドキュメント.mdock";
            }
            else
            {
                initName = $"新規 MDock ドキュメント({count}).mdock";
            }
            if (!File.Exists(Path.Combine(sfd.InitialDirectory, initName)))
            {
                break;
            }
            count++;
        }
        sfd.FileName = initName;
        sfd.Filter = "MDockファイル(*.mdock)|*.mdock";
        sfd.Title = "保存先のファイルを指定してください";
        sfd.RestoreDirectory = true;
        sfd.OverwritePrompt = true;
        sfd.CheckPathExists = false;
        if (sfd.ShowDialog() != DialogResult.OK)
        {
            return null;
        }
        Program.Props.Props.lastDir = Dirs.GetParent(sfd.FileName);
        return sfd.FileName;
    }
    private string? GetRenameFilePath(string path)
    {
        SaveFileDialog sfd = new SaveFileDialog();
        if (Program.Props.Props.lastDir != null)
        {
            sfd.InitialDirectory = Program.Props.Props.lastDir;
        }
        else
        {
            sfd.InitialDirectory = Dirs.DocumentsPath();
        }
        long count = 1;
        string initName = Dirs.GetFileName(path);
        sfd.FileName = initName;
        sfd.Filter = "MDockファイル(*.mdock)|*.mdock";
        sfd.Title = "新しいファイル名を指定してください";
        sfd.RestoreDirectory = true;
        sfd.OverwritePrompt = true;
        sfd.CheckPathExists = false;
        if (sfd.ShowDialog() != DialogResult.OK)
        {
            return null;
        }
        Program.Props.Props.lastDir = Dirs.GetParent(sfd.FileName);
        return sfd.FileName;
    }
    private void LoadFromPath(string path, int pos)
    {
        Util.Log(path, "path");
        if (Program.Core.CurrentPath != null)
        {
            this.SaveMemoList();
        }
        Program.Core.CurrentPath = path;
        Program.Props.Props.lastDir = Dirs.GetParent(path);
        var list = this.tabControl1.TabPageList();
        var memoList = Program.Core.LoadMemoList(path);
        foreach (var memo in memoList)
        {
            this.tabControl1.AddMDocText(memo);
        }
        this.tabControl1.Clear(list);
        if (pos >= this.tabControl1.TabPages.Count)
        {
            pos = this.tabControl1.TabPages.Count - 1;
        }
        this.tabControl1.SelectedIndex = pos;
    }
    private void OpenDocument()
    {
        string path = this.GetOpenFilePath();
        if (path is null) return;
        this.LoadFromPath(path, 0);
    }
    private void NewDocument()
    {
        string path = this.GetSaveFilePath();
        if (path is null) return;
        File.WriteAllText(path, "");
        this.LoadFromPath(path, 0);
        Program.form2.Visible = false;
    }
    private void ページ削除_Click(object sender, EventArgs e)
    {
        this.DeleteMemo();
    }

    private void ページ作成_Click(object sender, EventArgs e)
    {
        this.NewMemo();
    }

    private void ファイル作成_Click(object sender, EventArgs e)
    {
        this.NewDocument();
    }

    private void ファイル開く_Click(object sender, EventArgs e)
    {
        this.OpenDocument();
    }

    private void ファイル保存_Click(object sender, EventArgs e)
    {
        this.SaveMemoList();
    }

    private void 名前をつけて保存_Click(object sender, EventArgs e)
    {
        this.SaveAsMemoList();
    }

    private void 終了_Click(object sender, EventArgs e)
    {
        this.SaveMemoList();
        this.Close();
    }

    private void 検索_Click(object sender, EventArgs e)
    {
        //Util.Message("検索_Click");
        if (Program.Core.CurrentPath == null)
        {
            if (Program.Props.Props.lastDir != null)
            {
                string lastDir = Program.Props.Props.lastDir;
                var list = Util.ExpandWildcard(lastDir + "\\*.mdock");
                if (list.Length > 0)
                {
                    Program.form2.ShowUp(list);
                    return;
                }
            }
            MessageBox.Show("ドキュメントを開いてください。");
            //return;
        }
        else
        {
            string parent = Dirs.GetParent(Program.Core.CurrentPath);
            var list = Util.ExpandWildcard(parent + "\\*.mdock");
            Program.form2.ShowUp(list);
        }
    }

    public void 前を検索_Click(object sender, EventArgs e)
    {
        this.tabControl1.FindPrev();
    }

    public void 次を検索_Click(object sender, EventArgs e)
    {
        this.tabControl1.FindNext();
    }

    private void 削除_Click(object sender, EventArgs e)
    {
        if (Program.Core.CurrentPath is null) return;
        var timer = new System.Threading.Timer((state) =>
        {
            this.Invoke((MethodInvoker)(() =>
            {
                var result = MessageBox.Show(
                    $"「{Program.Core.CurrentPath}」を削除しますか？",
                    "確認",
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    var list = this.tabControl1.TabPageList();
                    this.tabControl1.Clear(list);
                    try
                    {
                        File.Delete(Program.Core.CurrentPath);
                    }
                    catch
                    {
                        ;
                    }
                    Program.Core.CurrentPath = null;
                    this.Text = "MDock";
                    Program.form2.Visible = false;
                }
            }));
            ((System.Threading.Timer)state).Dispose();
        });
        timer.Change(TimeSpan.FromMilliseconds(50), TimeSpan.Zero);
    }

    private void 名前を変更_Click(object sender, EventArgs e)
    {
        if (Program.Core.CurrentPath is null) return;
        string path = this.GetRenameFilePath(Program.Core.CurrentPath);
        if (path is null) return;
        string oldPath = Program.Core.CurrentPath;
        Program.Core.CurrentPath = path;
        this.SaveMemoList(true);
        try
        {
            File.Delete(oldPath);
        }
        catch
        {
            ;
        }
        Program.form2.Visible = false;
    }
    //private Form2 form2 = new Form2();
}
