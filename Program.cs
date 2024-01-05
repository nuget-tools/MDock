using System;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Windows.Forms;
using Global;

namespace mdock;

internal static class Program
{
    public static JsonProps Props = new JsonProps("JavaCommons Technologies", "MDock");
    public static MDockCore Core = null; // new MDockCore();
    public static Form1 form1 = null;
    public static Form2 form2 = null;
    private static Browser dummyBrowser;
    /// <summary>
    /// アプリケーションのメイン エントリ ポイントです。
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            Util.Message(args, "Too many arguments.");
            return;
        }
        string? path = null;
        int pos = 0;
        if (args.Length > 0)
        {
            path = Dirs.GetFullPath(args[0]);
            if (args.Length > 1)
            {
                pos = Int32.Parse(args[1]);
            }
        }
        string appDataFolder = Dirs.AppDataFolderPath("JavaCommons Technologies", "MDock");
        string tempFolder = Path.Combine(appDataFolder, "temp");
        Dirs.Prepare(tempFolder);
        Dirs.SetCurrentDirectory(tempFolder);
        //Util.Message(Util.SessionId());
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        //Util.ReallocConsole();
        string APP_NAME = Application.ProductName;
        string APP_ID = $"{APP_NAME}:{Util.SessionId()}";
        string OBJECT_NAME = "Form1";
        //string OBJECT_NAME = $"{Util.SessionId()}";
        string URL = "ipc://" + APP_ID +  "/" + OBJECT_NAME;
        bool createdNew = false;
        //Mutex mutex = new Mutex(true, APP_NAME, out createdNew);
        Mutex mutex = new Mutex(true, APP_ID, out createdNew);
        try
        {
            if (createdNew)
            {
                // 初回起動時
#if true
                LifetimeServices.LeaseTime = TimeSpan.Zero;
                LifetimeServices.RenewOnCallTime = TimeSpan.Zero;
#else
                LifetimeServices.LeaseTime = TimeSpan.FromDays(1000000);
                LifetimeServices.RenewOnCallTime = TimeSpan.FromDays(1000000);
#endif
                IpcChannel ipc = new IpcChannel(APP_ID);
                ChannelServices.RegisterChannel(ipc, false);
                Program.Core = new MDockCore();
                Program.form1 = new Form1();
                Program.form2 = new Form2(/*Program.form1*/);
                Program.dummyBrowser = new Browser();
                Program.form1.StartupNextInstance(path, pos);
                RemotingServices.Marshal(form1, OBJECT_NAME);
                Application.Run(Program.form1);
                mutex.ReleaseMutex();
                mutex.Close();
            }
            else
            {
                try
                {
                    // 多重起動時
                    IRemoteObject remoteObject = (IRemoteObject)RemotingServices.Connect(typeof(IRemoteObject), URL);
                    // 引数を渡す
                    remoteObject.StartupNextInstance(path, pos);
                }
                catch (Exception ex)
                {
                    ;
                }
                mutex.Close();
            }
        }
        catch(Exception ex)
        {
            Util.Log(ex.ToString());
            Util.Message(ex.ToString());
        }
    }
}
