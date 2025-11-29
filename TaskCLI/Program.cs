// This is a simple example from Terminal Gui documanets
// and needs to be modified later

using Terminal.Gui;
using TaskCLI.Models;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks.Sources;

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
                    NewTask(db, win);
                }),
                new("_Reset", "", () =>
                {
                    ResetTask(db, win);
                }),
            }),
            new("_Theme", new MenuItem[]
            {
               new("Light", "", () => {
                   ChangeTheme("light", win, db);
               }),
               new("Dark", "", () => {
                   ChangeTheme("dark", win, db);
               }),
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
        BuildCheckBoxList(db, win);

        win.Add(dayOfTheWeek,  container);
        top.Add(menu);
        top.Add(win);

        var currentTheme = GetUserSetting();
        ChangeTheme(currentTheme, win, db);

        Application.Run();
        Application.Shutdown();

        Console.ResetColor();
    }

    public static void NewTask(DatabaseController db, Window window)
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
            BuildCheckBoxList(db, window);

            Application.RequestStop(); // close dialog
        };

        var cancel = new Button("Cancel");
        cancel.Clicked += () => Application.RequestStop();

        dialog.AddButton(ok);
        dialog.AddButton(cancel);

        Application.Run(dialog);
    }

    public static void BuildCheckBoxList(DatabaseController db, Window window)
    {
        var windowBackgroundColor = window.GetNormalColor().Background;
        var windowForegroundColor = window.GetNormalColor().Foreground;
        var doneColor = new ColorScheme()
        {
            Normal = Application.Driver.MakeAttribute(Color.DarkGray, windowBackgroundColor),
            HotNormal = Application.Driver.MakeAttribute(Color.DarkGray, windowBackgroundColor),
            Focus = Application.Driver.MakeAttribute(Color.BrightYellow, windowBackgroundColor),
            HotFocus = Application.Driver.MakeAttribute(Color.BrightYellow, windowBackgroundColor),
        };
        var notDoneColor = new ColorScheme()
        {
            Normal = Application.Driver.MakeAttribute(windowForegroundColor, windowBackgroundColor),
            HotNormal = Application.Driver.MakeAttribute(windowForegroundColor, windowBackgroundColor),
            Focus = Application.Driver.MakeAttribute(Color.BrightYellow, windowBackgroundColor),
            HotFocus = Application.Driver.MakeAttribute(Color.BrightYellow, windowBackgroundColor),
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

    public static void ResetTask(DatabaseController db, Window window)
    {
        string fileName = "tasks.json";
        File.WriteAllText(fileName, "[]");
        // Refresh UI
        db.Load();
        db.Save();
        BuildCheckBoxList(db, window);
    }

    public static void ChangeTheme(string themeColor, Window window, DatabaseController db)
    {
        if (themeColor == "light")
        {
            window.ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.Black, Color.White),
            };
        }
        else
        {
            window.ColorScheme = new ColorScheme
            {
                Normal = Application.Driver.MakeAttribute(Color.White, Color.Black),
            };
        }
        BuildCheckBoxList(db, window);
        
        var newTheme = new UserSetting() { Theme = themeColor };
        var fileName = "userSettings.json".ToString();
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(newTheme, options);
        File.WriteAllText(fileName, jsonString);
    }

    public static string GetUserSetting()
    {
        var fileName = "userSettings.json".ToString();
        string jsonString = File.ReadAllText(fileName);
        UserSetting? currentTheme = JsonSerializer.Deserialize<UserSetting>(jsonString)!;
        return currentTheme.Theme;
    }
}