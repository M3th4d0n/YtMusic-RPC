﻿using System.Runtime.InteropServices;
using people2json.utils;
using people2json.Services;
using Spectre.Console;
using Panel = Spectre.Console.Panel;

namespace people2json
{
    class Program
    {
        private static string LastVersion = "N\\A";
        static string version = "1.0.4";
        static string author = "m3th4d0n+Anf1)";
        private static string githubUrl = "https://github.com/M3th4d0n/YtMusic-RPC";
        static Logger logger = new Logger();
        static NotifyIcon trayIcon;
        static bool running = true;
        static DiscordService discordService;
        static WebSocketService webSocketService;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static void SetConsoleWindowVisibility(bool visible){
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero){
                if (visible) ShowWindow(hWnd, 1); //1 = SW_SHOWNORMAL           
                else ShowWindow(hWnd, 0); //0 = SW_HIDE               
            }
        }
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);
        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        private static bool Handler(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_BREAK_EVENT:
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    Console.WriteLine("Closing");
                    MinimizeToTray();
                    return false;
                default:
                    return false;
            }
        }
        [STAThread]
        static async Task Main(string[] args){
            await ShowApplicationInfo();
            await InitializeServices();
            await InitializeTrayIcon();

            SetConsoleCtrlHandler(Handler, true);

            Application.Run();
        }

        static async Task InitializeTrayIcon(){
            trayIcon = new NotifyIcon(){
                Icon = new Icon("icon.ico"),
                Visible = true,
                Text = "YtMusic-RPC",
            };
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, (s, e) => { OnExit(s, e); });
            trayIcon.ContextMenuStrip = contextMenu;

            trayIcon.DoubleClick += (sender, e) => { RestoreFromTray(); };
        }

        static async Task ShowApplicationInfo(){
            LastVersion = await GithubService.GetLatestVersionAsync();

            AnsiConsole.Write(
                new Panel(
                        $"[yellow]author:[/] [green]{author}[/]\n[yellow]current version:[/] [green]{version}[/]\n[yellow]github url:[/] [link={githubUrl}]{githubUrl}[/]")
                    .BorderColor(new Spectre.Console.Color(0, 255, 255))
                    .Header("Info"));
            logger.LogInfo("Program initialized");

            bool isAnalyticsEnabled = ConfigManager.IsAnalyticsEnabled();
            if (isAnalyticsEnabled){
                AnsiConsole.MarkupLine("[yellow]Analytics enabled[/]");
                AnsiConsole.Progress()
                    .Start(async ctx => {
                        var task = ctx.AddTask("[green]Collecting and sending analytics...[/]");
                        while (!task.IsFinished){
                            await AnalyticsService.CollectAndSendAnalyticsAsync(version);
                            task.Increment(100);
                        }
                    });
            }

            if (IsNewerVersion(LastVersion, version)){
                logger.LogWarning($"Latest version: {LastVersion}");
                logger.LogWarning("A newer version is available. Please consider updating");
            }
        }

        static async Task InitializeServices(){
            discordService = new DiscordService("1194717480627740753");
            discordService.Initialize();

            webSocketService = new WebSocketService("/trackInfo", discordService);
            webSocketService.Start();
        }

        static bool IsNewerVersion(string lastVersion, string currentVersion){
            var last = new Version(lastVersion);
            var current = new Version(currentVersion);
            return last > current;
        }

        private static void OnExit(object sender, EventArgs e){
            Cleanup();
        }

        static void Cleanup(){
            webSocketService.Stop();
            discordService.Dispose();
            trayIcon.Dispose();
            Application.Exit();
        }

        private static void RestoreFromTray(){
            SetConsoleWindowVisibility(true);
        }

        private static void MinimizeToTray(){
            SetConsoleWindowVisibility(false);
        }
    }
}