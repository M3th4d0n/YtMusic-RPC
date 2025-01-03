﻿using Spectre.Console;

namespace YTMusicRPC.utils;

public class Logger
{
    private static readonly Logger instance = new Logger();

    public static Logger Instance => instance;

    private Logger()
    {
        
    }
    public  void LogInfo(string message){
        AnsiConsole.MarkupLine("[cyan][[INFO]][/] [bold]{0}[/]", message);
    }

    public void LogError(string message){
        AnsiConsole.MarkupLine("[red][[ERROR]][/] [bold]{0}[/]", message);
    }

    public void LogWarning(string message){
        AnsiConsole.MarkupLine("[yellow][[WARN]][/] [bold]{0}[/]", message);
    }

    public void LogSocket(string message){
        AnsiConsole.MarkupLine("[green][[WebSocket]][/] [bold]{0}[/]", message);
    }

    public void LogTrack(string trackMessage){
        string paddedMessage =
            $"\r[blue][[TRACK]][/] [bold]{trackMessage}[/]{new string(' ', Console.WindowWidth - trackMessage.Length - 10)}";
        AnsiConsole.Markup(paddedMessage);
    }
}