using Terminal.Gui;
using PlanCLI.Models;
using System.Text.Json;
using System.Globalization;

class TUImode
{
    // make variable accessible
    static FrameView? container;
    public static void Run()
    {
        var culture = new CultureInfo("en-GB");

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        
        var day = DateTime.Now;
        var db = new DatabaseController(Program.taskPath);

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

        var statusBar = new StatusBar(
            [
                new StatusItem(Key.CtrlMask | Key.X, "~^X~ Exit", () => Application.RequestStop()),
                new StatusItem(Key.CtrlMask | Key.N, "~^N~ New Task", () => NewTask(db, win)),
                new StatusItem(Key.CtrlMask | Key.R, "~^R~ Clear Screen", () => ResetTask(db, win)),
                new StatusItem(Key.CtrlMask | Key.T, "~^T~ Change Theme", () => OpenChangeTheme(db, win)),
                new StatusItem(Key.CtrlMask | Key.H, "~^H~ Help", () => ShowHelp()),
            ]);
        statusBar.ColorScheme = new ColorScheme()
        {
            Normal = Application.Driver.MakeAttribute(Color.White, Color.Black)
        };

        win.Add(dayOfTheWeek,  container);
        top.Add(win);
        top.Add(statusBar);
        // checking user's current theme
        var currentTheme = Program.GetUserSetting()[0];
        ChangeTheme(currentTheme, win, db);
        // running the application & handle exiting
        Application.Run();
        Application.Shutdown();

        Console.ResetColor();
    }

    public static void NewTask(DatabaseController db, Window window)
    {
        var dialog = new Dialog("Add Task", 60, 12);
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
        var checkDate = new CheckBox("Enable deadline", false)
        {
            X = 1,
            Y = 5,
        };
        string dateLabelText = "Date: (DD/MM/YYYY)";
        var dateLabel = new Label(dateLabelText)
        {
            X = 1,
            Y = 7
        };
        // align all text inputs
        int gap = 60 - dateLabelText.Length - 19;
        var dateInput = new DateField()
        {
            X = Pos.Right(dateLabel) + gap,
            Y = Pos.Top(dateLabel),
            Date = DateTime.Now
        };

        dialog.Add(titleLabel, titleInput);
        dialog.Add(descriptionLabel, descriptionInput);
        dialog.Add(checkDate);
        dialog.Add(dateLabel, dateInput);

        dateLabel.Y = 20;
        dateInput.Y = 20;
        dateLabel.Enabled = false;
        dateInput.Enabled = false;

        checkDate.KeyPress += (kb) =>
        {
            if (kb.KeyEvent.Key == Key.Enter)
            {
                if (dateLabel.Y.Equals(Pos.At(20)))
                {
                    dateLabel.Y = 7;
                    dateInput.Y = 7;
                    checkDate.Checked = true;
                    checkDate.Text = "Disable deadline";
                    dateLabel.Enabled = true;
                    dateInput.Enabled = true;
                }
                else
                {
                    dateLabel.Y = 20;
                    dateInput.Y = 20;
                    checkDate.Checked = false;
                    checkDate.Text = "Enable deadline";
                    dateLabel.Enabled = false;
                    dateInput.Enabled = false;
                }
            }
        };

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
            var forbidden = new[] { "[", "]", "(", ")", "/", "\\" };

            if (forbidden.Any(titleValue.Contains))
            {
                MessageBox.ErrorQuery("Validation", "Input can not contain [ ] ( ) \\ /", "OK");
                return;
            }
            if (!string.IsNullOrWhiteSpace(descriptionValue))
            {
                if (forbidden.Any(descriptionValue.Contains))
                {
                    MessageBox.ErrorQuery("Validation", "Input can not contain [ ] ( ) \\ /", "OK");
                    return;
                }
            }
            DateOnly? newDate;
            if (checkDate.Checked)
            {
                newDate = DateOnly.FromDateTime(dateInput.Date);
            }
            else
            {
                newDate = null;
            }
            var newTask = new TodoItem
            {
                Id = Program.GenerateNextID(db),
                Title = titleValue,
                Description = !string.IsNullOrWhiteSpace(descriptionValue) ? descriptionValue : "",
                Date = newDate
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
            // Use a plain Label instead of a CheckBox so no glyph is drawn.
            var itemLabel = new Label($"{task.Title} {task.Description} {task.Date}")
            {
                X = 1,
                Y = y++,
                ColorScheme = task.IsDone ? doneColor : notDoneColor,
                CanFocus = true // allow keyboard focus so Enter can open edit
            };

            // Open edit dialog on Enter key
            itemLabel.KeyPress += (kb) =>
            {
                if (kb.KeyEvent.Key == Key.Enter)
                {
                    OpenEditTaskDialog(task, db, window);
                    kb.Handled = true;
                }
            };

            // Open edit dialog on mouse click (left button)
            itemLabel.MouseClick += (me) =>
            {
                if ((me.MouseEvent.Flags & MouseFlags.Button1Clicked) != 0)
                {
                    OpenEditTaskDialog(task, db, window);
                }
            };

            container?.Add(itemLabel);
        }
    }

    public static void OpenEditTaskDialog(TodoItem task, DatabaseController db, Window window)
    {
        var dialog = new Dialog("Edit Task", 60, 16);

        var titleLabel = new Label("Title:")
        {
            X = 1,
            Y = 1,
        };
        var titleInput = new TextField(task.Title)
        {
            X = Pos.Right(titleLabel) + 10,
            Y = Pos.Top(titleLabel),
            Width = 40
        };
        var descriptionLabel = new Label("Description:")
        {
            X = 1,
            Y = 3,
        };
        var descriptionInput = new TextField(task.Description)
        {
            X = Pos.Right(descriptionLabel) + 4,
            Y = Pos.Top(descriptionLabel),
            Width = 40
        };
        string btnText = "Mark as done (press space)";
        if (task.IsDone)
        {
            btnText = "Mark as Undone (press space)";
        }
        var isDoneCheckBox = new CheckBox(btnText, task.IsDone)
        {
            X = 1,
            Y = 5,
        };
        isDoneCheckBox.KeyPress += (kb) =>
        {
            if (kb.KeyEvent.Key == Key.Space)
            {
                if (isDoneCheckBox.Text == btnText)
                {
                    isDoneCheckBox.Text = btnText;
                }
                else
                {
                    isDoneCheckBox.Text = btnText;
                }
                isDoneCheckBox.Checked = !isDoneCheckBox.Checked;
                kb.Handled = true;
            }
        };

        bool checkDateValue = true;
        if (task.Date == null)
        {
            checkDateValue = false;
        }
        var checkDate = new CheckBox("Enable deadline", checkDateValue)
        {
            X = 1,
            Y = 7,
        };
        string dateLabelText = "Date: (DD/MM/YYYY)";
        var dateLabel = new Label(dateLabelText)
        {
            X = 1,
            Y = 9
        };
        // align all text inputs
        int gap = 60 - dateLabelText.Length - 19;
        var dateInput = new DateField()
        {
            X = Pos.Right(dateLabel) + gap,
            Y = Pos.Top(dateLabel),
            Date = DateTime.Now
        };

        dialog.Add(titleLabel, titleInput);
        dialog.Add(descriptionLabel, descriptionInput);
        dialog.Add(isDoneCheckBox);
        dialog.Add(checkDate);
        dialog.Add(dateLabel, dateInput);

        if (checkDate.Checked == false)
        {
            dateLabel.Y = 20;
            dateInput.Y = 20;
            dateLabel.Enabled = false;
            dateInput.Enabled = false;
        }
        checkDate.KeyPress += (kb) =>
        {
            if (kb.KeyEvent.Key == Key.Space)
            {
                kb.Handled = true; // Prevent Space from toggling the checkbox
                return;
            }
            if (kb.KeyEvent.Key == Key.Enter)
            {
                if (dateLabel.Y.Equals(Pos.At(20)))
                {
                    dateLabel.Y = 9;
                    dateInput.Y = 9;
                    checkDate.Checked = true;
                    checkDate.Text = "Disable deadline";
                    dateLabel.Enabled = true;
                    dateInput.Enabled = true;
                }
                else
                {
                    dateLabel.Y = 20;
                    dateInput.Y = 20;
                    checkDate.Checked = false;
                    checkDate.Text = "Enable deadline";
                    dateLabel.Enabled = false;
                    dateInput.Enabled = false;
                }
            }
        };

        var save = new Button("Save");
        save.Clicked += () =>
        {
            var titleValue = titleInput.Text.ToString();
            var descriptionValue = descriptionInput.Text.ToString();
        
            if (string.IsNullOrWhiteSpace(titleValue))
            {
                MessageBox.ErrorQuery("Validation", "Title is required.", "OK");
                return;
            }
            var forbidden = new[] { "[", "]", "(", ")", "/", "\\" };
            
            if (forbidden.Any(titleValue.Contains))
            {
                MessageBox.ErrorQuery("Validation", "Input can not contain [ ] ( ) \\ /", "OK");
                return;
            }
            if (!string.IsNullOrWhiteSpace(descriptionValue))
            {
                if (forbidden.Any(descriptionValue.Contains))
                {
                    MessageBox.ErrorQuery("Validation", "Input can not contain [ ] ( ) \\ /", "OK");
                    return;
                }
            }
            DateOnly? newDate;
            if (checkDate.Checked)
            {
                newDate = DateOnly.FromDateTime(dateInput.Date);
            }
            else
            {
                newDate = null;
            }
            task.Title = titleInput.Text.ToString();
            task.Description = descriptionInput.Text.ToString();
            task.IsDone = isDoneCheckBox.Checked;
            task.Date = newDate;
            db.Save();
            BuildCheckBoxList(db, window);
            Application.RequestStop();
        };
        var cancel = new Button("Cancel");
        cancel.Clicked += () => Application.RequestStop();

        var delete = new Button("Delete");
        delete.Clicked += () =>
        {
            var item = db.Items.FirstOrDefault(t => t.Id == task.Id);
            if (!string.IsNullOrEmpty(item?.Title)) {
                db.Delete(item);
                Application.RequestStop();
                BuildCheckBoxList(db, window);
            }
        };

        dialog.AddButton(save);
        dialog.AddButton(cancel);
        dialog.AddButton(delete);

        Application.Run(dialog);
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
        // save new settings into file
        var mode = Program.GetUserSetting()[1];
        var newFile = new UserSetting() { 
            Theme = themeColor,
            Mode = mode 
            };
        var fileName = Program.settingPath.ToString();
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(newFile, options);
        File.WriteAllText(fileName, jsonString);
    }

    public static void OpenChangeTheme(DatabaseController db, Window window)
    {
        var dialog = new Dialog("Change Theme", 60, 8);
        var light = new Button("Light");
        light.Clicked += () => 
        {
            ChangeTheme("light", window, db);
            Application.RequestStop();
        };
        var dark = new Button("Dark");
        dark.Clicked += () => 
        {
            ChangeTheme("dark", window, db);
            Application.RequestStop();
        };

        dialog.AddButton(light);
        dialog.AddButton(dark);

        Application.Run(dialog);
    }

    public static void ShowHelp()
    {
        var dialog = new Dialog("Help", 60, 20);
        string helpMessage = $"""
            Navigation:
                Arrow keys — Move between tasks and buttons
            Button shortcuts:
                Enter — Accept dialog
                Esc — Close dialog
            App Shortcuts:
                Ctrl+H — Show this help screen
                Ctrl+N — Add new task
                Ctrl+X — Close application
            Change App Mode:
                plancli --mode tui
                plancli --mode cli
            Current version:
                {Program.Version}
        """;
        var msg = new TextView()
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 3,
            ReadOnly = true,
            Text = helpMessage
        };

        var allright = new Button("Allright");
        allright.Clicked += () => Application.RequestStop();

        dialog.Add(msg);
        dialog.AddButton(allright);

        Application.Run(dialog);
    }

    public static void ResetTask(DatabaseController db, Window window)
    {
        string fileName = Program.taskPath;
        File.WriteAllText(fileName, "[]");
        // Refresh UI
        db.Load();
        db.Save();
        BuildCheckBoxList(db, window);
    }
}