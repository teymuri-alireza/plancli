using PlanCLI.Models;
using Spectre.Console;

namespace PlanCLI;

public class CLImode
{
    public static void Run()
    {
        var db = new DatabaseController("tasks.json");

        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose your action?")
                .PageSize(6)
                .MoreChoicesText("[grey](Move up and down)[/]")
                .AddChoices(new[] {
                    "List tasks", "Add new", "Complete task",
                    "Delete task", "Reset tasks", "exit"
            }));
        switch (option)
        {
            case "List tasks":
                ListTasks(db);
                break;
            case "Add new":
                AddTask(db);
                break;
            case "Complete task":
                CompleteTask(db);
                break;
            case "Delete task":
                DeleteTask(db);
                break;
            case "Reset tasks":
                ResetTasks(db);
                break;
            case "exit":
                break;
            default:
                break;
        }
    }

    static void ListTasks(DatabaseController db)
    {
        var table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Title");
        table.AddColumn("Description");
        table.AddColumn("Done");

        if (db.Items.Count == 0)
        {
            Console.WriteLine("Task list is empty.");
        }
        else
        {
            foreach (var task in db.Items)
            {
                table.AddRow(
                    task.Id.ToString(),
                    !string.IsNullOrEmpty(task.Title) ? task.Title : "",
                    !string.IsNullOrEmpty(task.Description) ? task.Description : "",
                    task.IsDone ? "[green]✓[/]" : "[red]x[/]"
                );
            }
            AnsiConsole.Write(table);
        }
    }

    static void AddTask(DatabaseController db)
    {
        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's title:[/]"));
        var desc = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1][[optional]] Enter task's description:[/]")
            .AllowEmpty());

        var newTask = new TodoItem()
        {
            Id = Program.GenerateNextID(db),
            Title = title,
            Description = desc,
        };
        db.Items.Add(newTask);
        db.Save();
        ListTasks(db);
    }

    static void CompleteTask(DatabaseController db)
    {
        ListTasks(db);
        var choice = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's Id:[/]")
            .AllowEmpty());
        if (!string.IsNullOrEmpty(choice))
        {
            try
            {
                var item = db.Items.FirstOrDefault(t => t.Id == int.Parse(choice));
                if (!string.IsNullOrEmpty(item?.Title))
                {
                    item.IsDone = true;
                    db.Save();
                    Console.WriteLine("Task checked successfully");
                    ListTasks(db);
                }
            }
            catch
            {
                
            }
        }
    }
    static void DeleteTask(DatabaseController db)
    {
        ListTasks(db);
        var choice = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's Id:[/]")
            .AllowEmpty());

        if (!string.IsNullOrEmpty(choice))
        {
            try
            {
                var item = db.Items.FirstOrDefault(t => t.Id == int.Parse(choice));
                if (!string.IsNullOrEmpty(item?.Title)) {
                    db.Delete(item);
                    Console.WriteLine("Task deleted successfully");
                    ListTasks(db);
                }
            }
            catch
            {
                
            }
        }
    }
    static void ResetTasks(DatabaseController db)
    {
        string fileName = "tasks.json";
        File.WriteAllText(fileName, "[]");
        db.Load();
        db.Save();
        Console.Write("Done. ");
        ListTasks(db);
    }
}