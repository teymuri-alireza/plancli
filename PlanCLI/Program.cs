// This is a simple terminal based to-do list app
// written in c#
// This app requires more update and modifications

using Terminal.Gui;
using PlanCLI.Models;
using System.Text.Json;

class Program
{
    // make variable accessible
    static FrameView? container;
    static void Main(string[] args)
    {
        // manage settings & database file if it doesn't exist
        if (!File.Exists("tasks.json")) { File.WriteAllText("tasks.json", "[]");}
        if (!File.Exists("userSettings.json")) { File.WriteAllText("userSettings.json", "{\"Theme\": \"dark\"}");}

        var day = DateTime.Now;
        var db = new DatabaseController("tasks.json");

        // handle arguments
        if (args.Length > 0)
        {
            Arguments.HandleArgs(args, db);
            return;
        }

        Application.Init();

        Colors.Base.Normal = Application.Driver.MakeAttribute(Color.White, Color.Black);

        var top = Application.Top;
        var win = new Window("PlanCLI")
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
        // checking user's current theme
        var currentTheme = GetUserSetting();
        ChangeTheme(currentTheme, win, db);
        // running the application & handle exiting
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
            var titleValue = titleInput.Text.ToString();
            var descriptionValue = descriptionInput.Text.ToString();
            // checks if title input is empty
            if (string.IsNullOrWhiteSpace(titleValue))
            {
                MessageBox.ErrorQuery("Validation", "Title is required.", "OK");
                return;
            }
            var newTask = new TodoItem
            {
                Id = GenerateNextID(db),
                Title = titleValue,
                Description = !string.IsNullOrWhiteSpace(descriptionValue) ? descriptionValue : ""
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
        // getting current window colors
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
            var checkbox = new CheckBox($" {task.Title} {task.Description}", task.IsDone)
            {
                X = 1,
                Y = y++,
                ColorScheme = task.IsDone ? doneColor : notDoneColor // color changes based on IsDone
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
        // Refresh UI
        BuildCheckBoxList(db, window);
        // save new theme into settings file
        var newTheme = new UserSetting() { Theme = themeColor };
        var fileName = "userSettings.json".ToString();
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(newTheme, options);
        File.WriteAllText(fileName, jsonString);
    }

    public static int GenerateNextID(DatabaseController db)
    {
        return db.Items.Count == 0 ? 1 : db.Items.Max(t => t.Id) + 1;
    }

    public static string GetUserSetting()
    {
        var fileName = "userSettings.json".ToString();
        string jsonString = File.ReadAllText(fileName);
        UserSetting? currentTheme = JsonSerializer.Deserialize<UserSetting>(jsonString)!;
        return currentTheme.Theme;
    }
}