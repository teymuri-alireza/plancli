// This is a simple example from Terminal Gui documanets
// and needs to be modified later

using Terminal.Gui;
using Terminal.Gui.Graphs;
using TaskCLI.Models;
using System.Data.Common;

class Program
{
    // make variable accessible
    static FrameView? container;
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
            Height = Dim.Fill(),
            ColorScheme = new ColorScheme()
            {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
            }
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
        container = new FrameView("Tasks")
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        // build checkbox
        BuildCheckBoxList(db);

        win.Add(dayOfTheWeek,  container);
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
            // inputs can not be null
            if (
                string.IsNullOrWhiteSpace(titleInput.Text.ToString()) || 
                string.IsNullOrWhiteSpace(descriptionInput.Text.ToString())
                )
            {
                MessageBox.ErrorQuery("Validation", "Title and Description are required.", "OK");
                return;
            }
            var newTask = new TodoItem
            {
                Title = titleInput.Text.ToString(),
                Description = descriptionInput.Text.ToString()
            };
            // logic to save into database
            db.Items.Add(newTask);
            db.Save();
            // refresh UI
            BuildCheckBoxList(db);

            Application.RequestStop(); // close dialog
        };

        var cancel = new Button("Cancel");
        cancel.Clicked += () => Application.RequestStop();

        dialog.AddButton(ok);
        dialog.AddButton(cancel);

        Application.Run(dialog);
    }

    public static void BuildCheckBoxList(DatabaseController db)
    {
        var doneColor = new ColorScheme()
        {
            Normal = Application.Driver.MakeAttribute(Color.DarkGray, Color.Black),
            Focus = Application.Driver.MakeAttribute(Color.DarkGray, Color.Black),
        };
        var notDoneColor = new ColorScheme()
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
            Focus = Application.Driver.MakeAttribute(Color.White, Color.Black),
        };

        container?.RemoveAll();

        int y = 0;
        foreach (var task in db.Items)
        {
            var checkbox = new CheckBox($" {task.Title} - {task.Description}", task.IsDone)
            {
                X = 1,
                Y = y++,
                ColorScheme = task.IsDone ? doneColor : notDoneColor
            };
            checkbox.Toggled += (prev) =>
            {
                task.IsDone = checkbox.Checked;
                checkbox.ColorScheme = checkbox.Checked ? doneColor : notDoneColor;
                db.Save();
            };

            container?.Add(checkbox);
        }
    }
}