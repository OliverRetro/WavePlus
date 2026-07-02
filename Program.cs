using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using log4net;
using log4net.Config;
using log4net.Repository;
using Plus.Core;

namespace Plus
{
    public static class Program
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
            var fileInfo = new FileInfo(@"log4net.config");

            XmlConfigurator.Configure(repository, fileInfo);

            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;

            PlusEnvironment.Initialize();

            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter) {
                    Console.Write("plus> ");
                    string input = Console.ReadLine();

                    if (input.Length > 0) {
                        string s = input.Split(' ')[0];

                        ConsoleCommands.InvokeCommand(s);
                    }
                }
            }
        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            var e = (Exception) args.ExceptionObject;
            //Logger.LogCriticalException("SYSTEM CRITICAL EXCEPTION: " + e);
            PlusEnvironment.PerformShutDown();
        }

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private delegate bool EventHandler(CtrlType sig);
    }
}