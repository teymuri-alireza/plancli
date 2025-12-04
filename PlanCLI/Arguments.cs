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
            case "-c":
            case "--check":
                CheckTheTask(args, db);
                break;
            case "-d":
            case "--delete":
                DeleteTask(args, db);
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
        if (db.Items.Count == 0)
        {
            Console.WriteLine("Task list is empty");
            return;
        }
        var lines = new string('-', 15);
        Console.WriteLine($"+ {lines} +");
        
        foreach (var task in db.Items)
        {
            var mark = task.IsDone ? "[x]" : "[ ]";
            Console.WriteLine($"{mark} {task.Id}. {task.Title}");
            Console.WriteLine($"+ {lines} +");
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
        Console.WriteLine("Task added\n");
        ListTasks(db);
    }

    static void CheckTheTask(string[] args, DatabaseController db)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: missing task Id.");
            return;
        }
        try
        {
            var taskToDelete = db.Items.FirstOrDefault(t => t.Id == int.Parse(args[1]));
            if (!string.IsNullOrEmpty(taskToDelete?.Id.ToString()))
            {
                taskToDelete.IsDone = !taskToDelete.IsDone;
                db.Save();
                ListTasks(db);
            }
            else
            {
                Console.WriteLine("Error: task Id does not exist.");
                return;
            }
        }
        catch
        {
            Console.WriteLine("Error: task Id is not an integer!");
            return;
        }
    }

    static void DeleteTask(string[] args, DatabaseController db)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Error: missing task Id.");
            return;
        }
        var item = db.Items.FirstOrDefault(t => t.Id == int.Parse(args[1]));
        if (!string.IsNullOrEmpty(item?.Title)) {
            db.Delete(item);
            Console.WriteLine("Task deleted successfully.");
            ListTasks(db);
        }
    }

    static void ResetTask(DatabaseController db)
    {
        string fileName = "tasks.json";
        File.WriteAllText(fileName, "[]");
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
        Console.WriteLine("  dotnet run -- -c Id      \tcheck a task as complete");
        Console.WriteLine("  dotnet run -- -d Id      \tdelete a task");
        Console.WriteLine("  dotnet run -- -r       \tReset tasks list");
    }
}