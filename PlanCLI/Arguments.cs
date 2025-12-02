using PlanCLI.Models;

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
            case "-l":
            case "--list":
                ListTasks(db);
                break;
            case "-n":
            case "--new":
                AddNewTask(args, db);
                break;
            case "-r":
            case "--reset":
                ResetTask(db);
                break;
            
            default:
                Console.WriteLine("Unkown Option.");
                break;
        }
    }

    static void ListTasks(DatabaseController db)
    {
        foreach (var task in db.Items)
        {
            var mark = task.IsDone ? "[x]" : "[ ]";
            Console.WriteLine($"{mark}  {task.Title} - {task.Description}");
        }
    }

    static void AddNewTask(string[] args, DatabaseController db)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: missing task title & description.");
            return;
        }
        string title = args[1];

        db.Items.Add(new TodoItem
        {
            Id = Program.GenerateNextID(db),
            Title = title,
        });

        db.Save();
        Console.WriteLine("Task added");
    }

    static void ResetTask(DatabaseController db)
    {
        string fileName = "tasks.json";
        File.WriteAllText(fileName, "[]");
        // Refresh UI
        db.Load();
        db.Save();
        Console.WriteLine("Tasks reset successfully");
    }

    static void PrintHelp()
    {
        Console.WriteLine("plancli usage:");
        Console.WriteLine("  dotnet run             \tRun interactive mode (TUI)");
        Console.WriteLine("  dotnet run -- -l       \tList tasks");
        Console.WriteLine("  dotnet run -- -n \"title\"\tAdd new task");
    }
}