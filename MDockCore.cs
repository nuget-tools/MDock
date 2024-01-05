using System;
using System.Collections.Generic;
using System.IO;
using Global;
using static mdock.Form2;

namespace mdock;

internal class MDockCore
{
    private string currentPath = null;
    private string currentText = null;

    public string CurrentPath
    {
        get
        {
            return this.currentPath;
        }
        set
        {
            this.currentPath = value;
            Program.form1.Text = $"{Dirs.GetFileName(this.currentPath)} [{Dirs.GetDirectory(this.currentPath)}] - MDock";
        }
    }

    public string CurrentText
    {
        get
        {
            return this.currentText;
        }
        set
        {
            this.currentText = value;
        }
    }

    public List<MDockMemo> LoadMemoList(string path)
    {
        var result = new List<MDockMemo>();
        try
        {


            this.currentText = File.ReadAllText(path);
            var lines = Util.SplitTextIntoLines(this.currentText);
            //Util.Message(lines, "lines");
            MDockMemo memo = null;
            if (lines.Count == 0)
            {
                lines.Add("# !!!!" + Dirs.GetFileNameWithoutExtension(path));
            }
            foreach (var line in lines)
            {
                if (line.StartsWith("# !!!!"))
                {
                    string title = line.Substring(6);
                    memo = new MDockMemo()
                    {
                        Title = title,
                        Text = ""
                    };
                    result.Add(memo);
                }
                else if (line.StartsWith("#!!!!"))
                {
                    string title = line.Substring(5);
                    memo = new MDockMemo()
                    {
                        Title = title,
                        Text = ""
                    };
                    result.Add(memo);
                }
                else
                {
                    if (memo is null)
                    {
                        memo = new MDockMemo()
                        {
                            Title = Dirs.GetFileNameWithoutExtension(path),
                            Text = ""
                        };
                        result.Add(memo);
                    }
                    memo.Text += line + "\n";
                }
            }
        }
        catch (Exception ex)
        {
            ;
        }
        return result;
    }

#if false
    public bool FindInDocument(string path, string pattern)
    {

        var list = LoadMemoList(path);
        foreach (var memo in list)
        {
            if (memo.Title.ToUpper().Contains(pattern.ToUpper())) return true;
            if (memo.Text.ToUpper().Contains(pattern.ToUpper())) return true;
        }
        return false;
    }
#else
    public List<MyListBoxItem> FindInDocument(string path, string pattern)
    {
        var result = new List<MyListBoxItem>();
        var list = LoadMemoList(path);
        for (int i = 0; i < list.Count; i++)
        {
            var memo = list[i];
            if (memo.Title.ToUpper().Contains(pattern.ToUpper()) || memo.Text.ToUpper().Contains(pattern.ToUpper()))
            {
                result.Add(new MyListBoxItem()
                {
                    Name = $"{Dirs.GetFileName(path)} [{memo.Title}]　　",
                    FullPath = path,
                    Position = i
                });

            }
        }
        return result;
    }
#endif
}
