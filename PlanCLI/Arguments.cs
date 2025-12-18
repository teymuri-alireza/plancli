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
            case "-hh":
                PrintLongHelp();
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
                HandleComplete(args, db);
                break;
            case "-e":
            case "--edit":
                HandleEdit(args, db);
                break;
            case "-d":
            case "--delete":
                HandleDelete(args, db);
                break;
            case "-r":
            case "--reset":
                CLImode.ResetTasks(db);
                break;
            case "-v":
            case "--version":
                PrintVersion();
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
        var forbidden = new[] { "[", "]", "(", ")", "/", "\\" };

        if (forbidden.Any(newTaskTitle.Contains))
        {
            AnsiConsole.Markup("[red]You can't use: [/]");
            Console.WriteLine("[ ] ( ) \\ /");
            return;
        }
        var newTaskItem = new TodoItem()
        {
            Id = Program.GenerateNextID(db),
            Title = newTaskTitle
        };
        db.Items.Add(newTaskItem);
        db.Save();
        AnsiConsole.MarkupLine("[green]New task added.[/]");
    }

    static void HandleComplete(string[] args, DatabaseController db)
    {
        if (args.Length < 2)
        {
            CLImode.CompleteTask(db);
            return;
        }
        string Id = args[1];
        var task = db.Items.FirstOrDefault(t => t.Id.ToString() == Id);
        if (string.IsNullOrWhiteSpace(task?.Title))
        {
            AnsiConsole.MarkupLine($"[red]Task Id {Id} wasn't found![/]");
            return;
        }
        task.IsDone = true;
        db.Save();
        AnsiConsole.MarkupLine($"[green]Task {Id} completed successfully[/]");
    }

    static void HandleEdit(string[] args, DatabaseController db)
    {
        if (args.Length < 2)
        {
            CLImode.EditTask(db);
            return;
        }
        string Id = args[1];
        var task = db.Items.FirstOrDefault(t => t.Id.ToString() == Id);
        if (string.IsNullOrWhiteSpace(task?.Title))
        {
            AnsiConsole.MarkupLine($"[red]Task Id {Id} wasn't found![/]");
            return;
        }
        CLImode.EditHandler(task, db);
        AnsiConsole.MarkupLine($"[green]Task {Id} Edited successfully[/]");
    }

    static void HandleDelete(string[] args, DatabaseController db)
    {
        if (args.Length < 2)
        {
            CLImode.DeleteTask(db);
            return;
        }
        // check if task list is not empty
        if (db.Items.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Task list is empty.[/]");
            return;
        }
        string Id = args[1];
        var task = db.Items.FirstOrDefault(t => t.Id.ToString() == Id);
        // check if id doesn't exist
        if (string.IsNullOrWhiteSpace(task?.Title))
        {
            AnsiConsole.MarkupLine($"[red]Task Id {Id} wasn't found![/]");
            return;
        }
        db.Delete(task);
        AnsiConsole.MarkupLine($"[green]Task {Id} deleted successfully[/]");
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

    static void PrintVersion()
    {
        Console.WriteLine(Program.Version);
    }

    public static void PrintHelp()
    {
        AnsiConsole.MarkupLine("[green]plancli usage:[/]");
        Console.WriteLine("     plancli              Run in default mode if it's set");
        Console.WriteLine("     plancli -m cli       Set default to Command-Line mode (CLI)");
        Console.WriteLine("     plancli -m tui       Set default to interactive mode (TUI)");
        AnsiConsole.MarkupLine("[green]Other arguments to handle tasks:[/]");
        Console.WriteLine("     plancli -l           Print the task list");
        Console.WriteLine("     plancli -a \"title\"   Add new task");
        Console.WriteLine("     plancli -c Id        Check/Uncheck a task");
        Console.WriteLine("     plancli -e Id        Edit a task");
        Console.WriteLine("     plancli -d Id        Delete a task");
        Console.WriteLine("     plancli -r           Reset tasks list");
        Console.WriteLine("     plancli -v           Prints plancli version");
        AnsiConsole.MarkupLine("[green]Bigger help:[/]");
        Console.WriteLine("     plancli -hh           Prints the long version of help");
    }
    public static void PrintLongHelp()
    {
        AnsiConsole.MarkupLine("[green]Changin mode:[/]");
        Console.WriteLine("     plancli -m cli           Set default to Command-Line mode (CLI)");
        Console.WriteLine("     plancli --mode cli       ");
        Console.WriteLine("     plancli -m tui           Set default to interactive mode (TUI)");
        Console.WriteLine("     plancli --mode tui       ");

        AnsiConsole.MarkupLine("[green]Print list of tasks:[/]");
        Console.WriteLine("     plancli -l               ");
        Console.WriteLine("     plancli --list           ");
        AnsiConsole.MarkupLine("[green]Add new task:[/]");
        Console.WriteLine("     plancli -a \"title\"       ");
        Console.WriteLine("     plancli --add \"title\"    ");
        Console.WriteLine("     plancli -a               Prompts for a task title");
        AnsiConsole.MarkupLine("[green]Check or Uncheck a task:[/]");
        Console.WriteLine("     plancli -c Id            ");
        Console.WriteLine("     plancli --check Id       ");
        Console.WriteLine("     plancli -c               Prompts for a task Id");
        AnsiConsole.MarkupLine("[green]Edit a task:[/]");
        Console.WriteLine("     plancli -e Id            ");
        Console.WriteLine("     plancli --edit Id        ");
        Console.WriteLine("     plancli -e               Prompts for a task Id");
        AnsiConsole.MarkupLine("[green]Delete a task:[/]");
        Console.WriteLine("     plancli -d Id            ");
        Console.WriteLine("     plancli --delete Id      ");
        Console.WriteLine("     plancli -d               Prompts for a task Id");
        AnsiConsole.MarkupLine("[green]Reset tasks list:[/]");
        Console.WriteLine("     plancli -r               ");
        Console.WriteLine("     plancli --reset          ");
        AnsiConsole.MarkupLine("[green]Prints plancli version:[/]");
        Console.WriteLine("     plancli -v               ");
        Console.WriteLine("     plancli --version        ");
    }
}