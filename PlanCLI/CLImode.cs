using PlanCLI.Models;
using Spectre.Console;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PlanCLI;

public class CLImode
{
    public static void Run()
    {
        var culture = new CultureInfo("en-GB");

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        
        var db = new DatabaseController(Program.taskPath);

        while (true)
        {
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose your action?")
                    .PageSize(7)
                    .MoreChoicesText("[grey](Move up and down)[/]")
                    .AddChoices(new[] {
                        "List tasks", "Add new", "Complete/Uncheck task",
                        "Edit task", "Delete task", "Reset tasks", "exit"
                }));
            switch (option)
            {
                case "List tasks":
                    ListTasks(db);
                    break;
                case "Add new":
                    AddTask(db);
                    break;
                case "Complete/Uncheck task":
                    CompleteTask(db);
                    break;
                case "Delete task":
                    DeleteTask(db);
                    break;
                case "Edit task":
                    EditTask(db);
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
        var table = new Table
        {
            Border = TableBorder.Rounded
        };
        table.AddColumn("Id");
        table.AddColumn("Title");
        table.AddColumn("Description");
        table.AddColumn("Date");
        table.AddColumn("Done");

        if (db.Items.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Task list is empty.[/]");
        }
        else
        {
            // create an instance of current day to compare
            var fullDay = DateTime.Now;
            var day = new DateOnly(fullDay.Year, fullDay.Month, fullDay.Day);

            foreach (var task in db.Items)
            {
                // print the done tasks in grey
                if (task.IsDone)
                {
                    table.AddRow(
                        $"[grey]{task.Id}[/]",
                        !string.IsNullOrEmpty(task.Title) ? $"[grey]{task.Title}[/]" : "",
                        !string.IsNullOrEmpty(task.Description) ? $"[grey]{task.Description}[/]" : "",
                        $"[grey]{task.Date.ToString() ?? ""}[/]",
                        task.IsDone ? "[green]✓[/]" : "[red]x[/]"
                    );    
                }
                // print the overdue tasks in red
                else if (task.Date < day)
                {
                    table.AddRow(
                        $"[red]{task.Id}[/]",
                        !string.IsNullOrEmpty(task.Title) ? $"[red]{task.Title}[/]" : "",
                        !string.IsNullOrEmpty(task.Description) ? $"[red]{task.Description}[/]" : "",
                        $"[red]{task.Date.ToString() ?? ""}[/]",
                        task.IsDone ? "[green]✓[/]" : "[red]x[/]"
                    );    
                }
                else
                {
                    table.AddRow(
                        task.Id.ToString(),
                        !string.IsNullOrEmpty(task.Title) ? task.Title : "",
                        !string.IsNullOrEmpty(task.Description) ? task.Description : "",
                        task.Date.ToString() ?? "",
                        task.IsDone ? "[green]✓[/]" : "[red]x[/]"
                    );  
                }
            }
            AnsiConsole.Write(table);
        }
    }

    public static void AddTask(DatabaseController db)
    {
        var title = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's title:[/] [grey](q for cancel)[/]")
            .Validate(input =>
            {
                if (Regex.IsMatch(input, @"[\[\]\(\)/\\]"))
                {
                    Console.WriteLine("You can't use: [ ] ( ) \\ /");
                    return ValidationResult.Error("[red]Invalid characters detected[/]");
                }
                return ValidationResult.Success();
            })
        );
        if (title == 'q'.ToString())
        {
            return;
        }
        var desc = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's description:[/] [grey][[optional]][/] ")
            .AllowEmpty());

        var IsDeadline = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[lightskyblue1]Do you want to add deadline?[/]")
            .AddChoices("no", "yes"));
        DateOnly? date = new DateOnly();
        if (IsDeadline == "yes")
        {
            var year = AnsiConsole.Prompt(new SelectionPrompt<int>()
                .Title("Select a year")
                .AddChoices(2024, 2025, 2026, 2027));

            var month = AnsiConsole.Prompt(new SelectionPrompt<int>()
                .Title("Select a month")
                .AddChoices(Enumerable.Range(1, 12)));

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var day = AnsiConsole.Prompt(new SelectionPrompt<int>()
                .Title("Select a day")
                .AddChoices(Enumerable.Range(1, daysInMonth)));

            date = new DateOnly(year, month, day);
        }
        else
        {
            date = null;
        }
        var newTask = new TodoItem()
        {
            Id = Program.GenerateNextID(db),
            Title = title,
            Description = desc,
            Date = date
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
                    item.IsDone = !item.IsDone;
                    db.Save();
                    string action = item.IsDone ? "completed" : "unchecked";
                    AnsiConsole.MarkupLine($"[green]Task {item.Id} {action} successfully[/]");
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
    public static void EditTask(DatabaseController db)
    {
        ListTasks(db);
        var choice  = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter task's Id: [/] [grey](empty for cancel)[/]")
            .AllowEmpty()
        );
        if (!string.IsNullOrWhiteSpace(choice))
        {
            try
            {
                var item = db.Items.FirstOrDefault(t => t.Id == int.Parse(choice));
                if (!string.IsNullOrEmpty(item?.Title)) {
                    EditHandler(item, db);
                    AnsiConsole.MarkupLine($"[green]Task {item.Id} Edited successfully[/]");
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
    public static void EditHandler(TodoItem item, DatabaseController db)
    {
        var newTitle = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter new title:[/]")
            .DefaultValue(item.Title ?? "".ToString())
            .Validate(input =>
            {
                if (Regex.IsMatch(input, @"[\[\]\(\)/\\]"))
                {
                    Console.WriteLine("You can't use: [ ] ( ) \\ /");
                    return ValidationResult.Error("[red]Invalid characters detected[/]");
                }
                return ValidationResult.Success();
            }));
        var newDescription = AnsiConsole.Prompt(
            new TextPrompt<string>("[lightskyblue1]Enter new desciption:[/]")
            .AllowEmpty()
            .Validate(input =>
            {
                if (Regex.IsMatch(input, @"[\[\]\(\)/\\]"))
                {
                    Console.WriteLine("You can't use: [ ] ( ) \\ /");
                    return ValidationResult.Error("[red]Invalid characters detected[/]");
                }
                return ValidationResult.Success();
            }));
        var IsDeadline = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[lightskyblue1]Do you want to edit deadline?[/]")
            .AddChoices("no", "yes", "delete deadline"));
        var date = new DateOnly();
        if (IsDeadline == "yes")
        {
            var year = AnsiConsole.Prompt(new SelectionPrompt<int>()
                .Title("Select a year")
                .AddChoices(2025, 2026, 2027, 2028));
            var month = AnsiConsole.Prompt(new SelectionPrompt<int>()
                .Title("Select a month")
                .AddChoices(Enumerable.Range(1, 12)));

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var day = AnsiConsole.Prompt(new SelectionPrompt<int>()
                .Title("Select a day")
                .AddChoices(Enumerable.Range(1, daysInMonth)));
            date = new DateOnly(year, month, day);
            item.Date = date;
        }
        else if (IsDeadline == "delete deadline")
        {
            item.Date = null;
        }
        item.Title = newTitle;
        item.Description = newDescription;
        db.Save();
    }
    public static void DeleteTask(DatabaseController db)
    {
        // check if task list is empty
        if (db.Items.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Task list is empty.[/]");
            return;
        }
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