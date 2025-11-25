// This is a simple example from Terminal Gui documanets
// and needs to be modified later

using Terminal.Gui;
using Terminal.Gui.Graphs;
using TaskCLI.Models;
using System.Text.Json;

class Program
{ 
    static void Main(string[] args)
    {
        var day = DateTime.Now;
        var db = new DatabaseController("tasks.json");
        // main function
        Application.Init();

        Colors.Base.Normal = Application.Driver.MakeAttribute(Color.White, Color.Black);

        var top = Application.Top;
        var win = new Window("TaskCLI")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        win.ColorScheme = new()
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
        };
        var menu = new MenuBar(
        [
            new("_File", new MenuItem[]
            {
                new("_Exit", "", () =>
                {
                    Application.RequestStop();
                }) 
            }),
            new("_Task", new MenuItem[]
            {
                new("_New", "", () =>
                {
                    NewTask(db);
                }) 
            }),
        ]);
        var dayOfTheWeek = new Label(day.ToString("dddd") + " - " + day.ToString("M"))
        {
            X = Pos.Center(),
            Y = 1
        };
        var breakLine = new LineView()
        {
            X = 0,
            Y = 2,
            Width = Dim.Fill(),
            Orientation = Orientation.Horizontal
        };
        string tasksText = string.Join("\n", db.Items.Select(t => $"{t.Title} - {t.Description}"));
        var tasksWindow = new Label(tasksText)
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        win.Add(dayOfTheWeek, breakLine, tasksWindow);
        top.Add(menu);
        top.Add(win);

        Application.Run();
        Application.Shutdown();

        Console.ResetColor();
    }

    public static void NewTask(DatabaseController db)
    {
        var dialog = new Dialog("Add Task", 60, 10);
        var titleLabel = new Label("Title:")
        {
            X = 1,
            Y = 1,
        };
        var titleInput = new TextField("")
        {
            X = Pos.Right(titleLabel) + 7,
            Y = Pos.Top(titleLabel),
            Width = 40
        };
        var descriptionLabel = new Label("Description:")
        {
            X = 1,
            Y = 3,
        };
        var descriptionInput = new TextField("")
        {
            X = Pos.Right(descriptionLabel) + 1,
            Y = Pos.Top(descriptionLabel),
            Width = 40
        };

        dialog.Add(titleLabel, titleInput);
        dialog.Add(descriptionLabel, descriptionInput);

        var ok = new Button("OK");
        ok.Clicked += () =>
        {
            var newTask = new TodoItem
            {
                Title = titleInput.Text.ToString(),
                Description = descriptionInput.Text.ToString()
            };
            // logic to save into database
            db.Items.Add(newTask);
            db.Save();

            Application.RequestStop(); // close dialog
        };

        var cancel = new Button("Cancel");
        cancel.Clicked += () => Application.RequestStop();

        dialog.AddButton(ok);
        dialog.AddButton(cancel);

        Application.Run(dialog);
    }
}