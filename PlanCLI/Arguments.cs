using System.Text.Json;
using Spectre.Console;
using PlanCLI.Models;
using PlanCLI;

class Arguments
{
    public static void HandleArgs(string[] args, DatabaseController db)
    {
        switch (args[0])
        {
            case "-h":
            case "--help":
                PrintHelp();
                break;
            case "-m":
            case "--mode":
                ModeHandler(args);
                break;
            case "-l":
            case "--list":
                CLImode.ListTasks(db);
                break;
            case "-a":
            case "--add":
                HandleAddTask(args, db);
                break;
            case "-c":
            case "--check":
                CLImode.CompleteTask(db);
                break;
            case "-d":
            case "--delete":
                CLImode.DeleteTask(db);
                break;
            case "-r":
            case "--reset":
                CLImode.ResetTasks(db);
                break;
            default:
                Console.WriteLine("Unkown Option.");
                break;
        }
    }

    static void ModeHandler(string[] args)
    {
        if (args.Length < 2)
        {
            AnsiConsole.MarkupLine("[red]Missing mode value. Usage: --mode (cli|tui)[/]");
            return;
        }
        switch (args [1])
        {
            case "cli":
                ChangeUserMode("cli");
                CLImode.Run();
                break;
            case "tui":
                ChangeUserMode("tui");
                TUImode.Run();
                break;
            default:
                PrintHelp();
                break;
        }
    }

    static void HandleAddTask(string[] args, DatabaseController db)
    {
        if (args.Length < 2)
        {
            CLImode.AddTask(db);
            return;
        }
        string newTaskTitle = args[1];
        var newTaskItem = new TodoItem()
        {
            Id = Program.GenerateNextID(db),
            Title = newTaskTitle
        };
        db.Items.Add(newTaskItem);
        db.Save();
        AnsiConsole.MarkupLine("[green]New task added.[/]");
    }

    public static void ChangeUserMode(string mode)
    {
        var theme = Program.GetUserSetting()[0];
        var newFile = new UserSetting() { 
            Theme = theme,
            Mode = mode 
            };
        var fileName = Program.settingPath.ToString();
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(newFile, options);
        File.WriteAllText(fileName, jsonString);
    }

    public static void PrintHelp()
    {
        AnsiConsole.MarkupLine("[green]plancli usage:[/]");
        Console.WriteLine("     dotnet run                 Run in default mode if it's set");
        Console.WriteLine("     dotnet run -- -m cli       Set default to Command-Line mode (CLI)");
        Console.WriteLine("     dotnet run -- -m tui       Set default to interactive mode (TUI)");
        AnsiConsole.MarkupLine("[green]For easier access to tasks:[/]");
        Console.WriteLine("     dotnet run -- -l           Print the task list");
        Console.WriteLine("     dotnet run -- -a \"title\"   Add new task");
        Console.WriteLine("     dotnet run -- -c           Complete a task");
        Console.WriteLine("     dotnet run -- -d           Delete a task");
        Console.WriteLine("     dotnet run -- -r           Reset tasks list");
    }
}