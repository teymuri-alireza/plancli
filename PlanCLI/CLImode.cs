using PlanCLI.Models;
using Spectre.Console;

namespace PlanCLI;

public class CLImode
{
    public static void Run()
    {
        var db = new DatabaseController(Program.taskPath);

        while (true)
        {
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
                    return;
                default:
                    return;
            }
        }
    }

    public static void ListTasks(DatabaseController db)
    {
        var table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Title");
        table.AddColumn("Description");
        table.AddColumn("Done");

        if (db.Items.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Task list is empty.[/]");
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

    public static void AddTask(DatabaseController db)
    {
        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's title:[/] [grey](q for cancel)[/]"));
        if (title == 'q'.ToString())
        {
            return;
        }
        var desc = AnsiConsole.Prompt(
            new TextPrompt<string>("[grey][[optional]][/] [lightskyblue1]Enter task's description:[/]")
            .AllowEmpty());

        var newTask = new TodoItem()
        {
            Id = Program.GenerateNextID(db),
            Title = title,
            Description = desc,
        };
        db.Items.Add(newTask);
        db.Save();
        AnsiConsole.MarkupLine("[green]New task added successfully.[/]");
    }

    public static void CompleteTask(DatabaseController db)
    {
        ListTasks(db);
        var choice = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's Id:[/] [grey](empty for cancel)[/]")
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
                    AnsiConsole.MarkupLine($"[green]Task {item.Id} completed successfully[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Task not found![/]");
                }
            }
            catch
            {
                AnsiConsole.MarkupLine("[red]Id must be an integer![/]");
            }
        }
    }
    public static void DeleteTask(DatabaseController db)
    {
        ListTasks(db);
        var choice = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's Id:[/] [grey](empty for cancel)[/]")
            .AllowEmpty());

        if (!string.IsNullOrEmpty(choice))
        {
            try
            {
                var item = db.Items.FirstOrDefault(t => t.Id == int.Parse(choice));
                if (!string.IsNullOrEmpty(item?.Title)) {
                    db.Delete(item);
                    AnsiConsole.MarkupLine($"[green]Task {item.Id} deleted successfully[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Task not found![/]");
                }
            }
            catch
            {
                AnsiConsole.MarkupLine("[red]Id must be an integer![/]");
            }
        }
    }
    public static void ResetTasks(DatabaseController db)
    {
        string fileName = Program.taskPath;
        File.WriteAllText(fileName, "[]");
        db.Load();
        db.Save();
        AnsiConsole.MarkupLine("[green]Task list deleted successfully.[/]");
    }
}