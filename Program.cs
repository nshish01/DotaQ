using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.Text;
using EasyHook;


namespace FileMon
{
    public class FileMonInterface : MarshalByRefObject
    {

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_ABSOLUTE = 0x8000; 
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004; 
        private const int MOUSEEVENTF_MOVE = 0x0001;

        public bool startup = true;
        public void MouseClick()
        {
            int x = 28200;
            int y = 30800;

            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            System.Threading.Thread.Sleep(200);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }
        public void setFocus(Int32 InClientPID)
        {

            Process proc = Process.GetProcessById(InClientPID);
            IntPtr mainWindow = proc.MainWindowHandle;
            IntPtr newPos = new IntPtr(-1);
            SetForegroundWindow(mainWindow);

        }

        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("FileMon has been installed in target {0}.\r\n", InClientPID);
        }
        public void OnCreateFile(Int32 InClientPID, String[] InFileNames)
        {
            for (int i = 0; i < InFileNames.Length; i++)
            {
                Console.WriteLine(InFileNames[i]);
                if (!InFileNames[i].Substring(InFileNames[i].Length - 4, 3).Equals("drv") && !InFileNames[i].Substring(InFileNames[i].Length - 4, 3).Equals("wav"))
                {
                    Console.WriteLine(InFileNames[i]);
                    //Environment.Exit(0);
                }

                if (InFileNames[i].Length > 28)
                {
                    if ((InFileNames[i].Substring(InFileNames[i].Length - 28, 27).Equals("Windows\\system32\\wdmaud.drv") && startup) ||
                        InFileNames[i].Substring(InFileNames[i].Length - 28, 27).Equals("out_of_game_match_ready.wav"))
                    {
                        startup = false;
                        Console.WriteLine("Game Found");
                        Process proc = Process.GetProcessById(InClientPID);
                        IntPtr mainWindow = proc.MainWindowHandle;
                        IntPtr newPos = new IntPtr(-1);
                        SetForegroundWindow(mainWindow);
                        System.Threading.Thread.Sleep(2000); 
                        MouseClick();
                        System.Threading.Thread.Sleep(100);
                        setFocus(Process.GetCurrentProcess().Id);
                    }
                }
            }

        }
        public void ReportException(Exception InInfo)
        {
            Console.WriteLine("The target process has reported an error:\r\n"
            + InInfo.ToString());
        }
        public void Ping()
        {
        }
    }
    class Program
    {
        static String ChannelName = null;
        static void Main(string[] args)
        {

            try
            {
                Process[] currentProcess = Process.GetProcessesByName("dota");

                if (currentProcess.Length <= 0) Environment.Exit(0);

                int pid = currentProcess[0].Id;

                Config.Register(
                "A FileMon like demo application.",
                "FileMon.exe",
                "FileMonInject.dll");
                RemoteHooking.IpcCreateServer<FileMonInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);
                RemoteHooking.Inject(
                pid,
                "FileMonInject.dll",
                "FileMonInject.dll",
                ChannelName);
                Console.ReadLine();
            }
            catch (Exception ExtInfo)
            {
                Console.WriteLine("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());
            }
        }
    }
}