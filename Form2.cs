using Global;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace mdock
{
    class MyBackgroundWorker : BackgroundWorker
    {
        public long _id = 0;
        public MyBackgroundWorker(long id)
        {
            _id = id;
        }
    }

    public partial class Form2 : Form
    {
        List<MyListBoxItem> items = null;
        BindingSource bs = null;
        long workerId = 0;
        MyBackgroundWorker worker;
        string[] list = new string[] { };
        string pattern = null;
        public string SearchPattern
        {
            get { return this.textBox1.Text; }
        }
        public Form2()
        {
            InitializeComponent();
            //
            this.Text = "MDock 検索";
            this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            //
            this.items = new List<MyListBoxItem>();
            this.bs = new BindingSource();
            this.bs.DataSource = this.items;
            this.listBox1.Items.Clear();
            this.listBox1.DisplayMember = "Name";
            this.listBox1.ValueMember = "FullPath";
            this.listBox1.DataSource = bs;
            //
            this.TopMost = true;
            this.listBox1.Click += (s, e) =>
            {
                if (this.listBox1.SelectedItem != null)
                {
                    var item = (MyListBoxItem)this.listBox1.SelectedItem;
                    Program.form1.StartupNextInstance(item.FullPath, item.Position);
                }
            };
            this.textBox1.TextChanged += (s, e) =>
            {
                this.StartSearch();
            };
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F3)
            {
                //Util.Message("(keyData == Keys.F3)");
                //Program.form1.Focus();
                Program.form1.次を検索_Click(null, null);
                return true;
            }
            else if (keyData == (Keys.Shift | Keys.F3))
            {
                //Util.Message("(keyData == (Keys.Shift | Keys.F3))");
                //Program.form1.Focus();
                Program.form1.前を検索_Click(null, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void StartSearch()
        {
            //if (this.textBox1.Text == "") return;
            this.pattern = textBox1.Text;
#if false
            if (worker != null)
            {
                if (!worker.CancellationPending) worker.CancelAsync();
                worker = null;
            }
#endif
            worker = new MyBackgroundWorker(++this.workerId);
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += this.worker_DoWork;
            worker.RunWorkerCompleted += this.workerRunWorkerCompleted;
            worker.RunWorkerAsync();
        }
        public void ShowUp(string[] list)
        {
            this.list = list;
            //
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
                Program.form1.Location.X + (Program.form1.Width - this.Width),
                Program.form1.Location.Y);
            //
            this.Visible = true;
            var timer = new System.Threading.Timer((state) =>
            {
                this.Invoke((MethodInvoker)(() => {
                    this.textBox1.Focus();
                    this.textBox1.SelectAll();
                }));
                ((System.Threading.Timer)state).Dispose();
            });
            timer.Change(TimeSpan.FromMilliseconds(50), TimeSpan.Zero);
            this.StartSearch();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var items = new List<MyListBoxItem>();
            this.items = items;
            //items.Clear();
            bs.ResetBindings(false);
            foreach (var elem in list)
            {
                //if (this.pattern == "") continue;
                var result = Program.Core.FindInDocument(elem, this.pattern);
                items.AddRange(result);
            }
        }

        private void workerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MyBackgroundWorker worker = (MyBackgroundWorker)sender;
            if (worker._id != this.workerId) return;
            this.bs.DataSource = this.items;
            this.listBox1.DataSource = this.bs;
            this.bs.ResetBindings(false);
            this.listBox1.SelectedItem = null;
            //this.listBox1.Update();
            //this.Update();
        }

        internal class MyListBoxItem
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public int Position { get; set; }
        }

        private void btn再検索_Click(object sender, EventArgs e)
        {
            this.StartSearch();
        }
    }
}
