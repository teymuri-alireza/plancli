// This is the main program which handles user's default mode
// and also calls arguments if needed

using System.Text.Json;
using Spectre.Console;
using PlanCLI.Models;
using PlanCLI;

class Program
{
    public static string configDir = GetConfigDirectory();
    public static string taskPath = Path.Combine(configDir, "tasks.json");
    public static string settingPath = Path.Combine(configDir, "userSettings.json");
    static void Main(string[] args)
    {
        // manage settings & database file if it doesn't exist
        Directory.CreateDirectory(configDir);
        
        if (!File.Exists(taskPath)) {
            File.WriteAllText(taskPath, "[]");
        }
        if (!File.Exists(settingPath)) {
            File.WriteAllText(settingPath, "{\"Theme\": \"dark\", \"Mode\":\"not set\"}");
        }

        var db = new DatabaseController(taskPath);

        // handle arguments
        if (args.Length > 0)
        {
            Arguments.HandleArgs(args, db);
            return;
        }
        var userMode = GetUserSetting()[1];
        switch (userMode)
        {
            case "cli":
                CLImode.Run();
                break;
            case "tui":
                TUImode.Run();
                break;
            case "not set":
                SetupApp();
                break;
            default:
                Arguments.PrintHelp();
                break;
        }
    }

    static void SetupApp()
    {
        // initializing settings and app's mode
        AnsiConsole.MarkupLine("[green]Welcome to plancli. Let's setup the setttings.[/]");
        while (true)
        {
            var setMode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("Choose default mode for next use:")
                .PageSize(3)
                .MoreChoicesText("[grey](Move up and down)[/]")
                .AddChoices(new [] { "cli", "tui" })
            );
            switch (setMode)
            {
                case "cli":
                    AnsiConsole.MarkupLine("[green]Setting default mode to cli[/]");
                    Arguments.ChangeUserMode("cli");
                    AnsiConsole.MarkupLine("[green]Starting app...[/]\n");
                    Thread.Sleep(1500);
                    CLImode.Run();
                    return;
                case "tui":
                    AnsiConsole.MarkupLine("[green]Setting default mode to tui[/]");
                    Arguments.ChangeUserMode("tui");
                    AnsiConsole.MarkupLine("[green]Starting app...[/]\n");
                    Thread.Sleep(1500);
                    TUImode.Run();
                    return;
                default:
                    break;
            }
        }
    }
    public static string GetConfigDirectory()
    {
        string basePath;

        if (OperatingSystem.IsWindows())
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, "PlanCLI");
        }
        else
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(basePath, ".plancli");
        }
    }

    public static int GenerateNextID(DatabaseController db)
    {
        // generates a new ID for database
        return db.Items.Count == 0 ? 1 : db.Items.Max(t => t.Id) + 1;
    }

    public static List<string> GetUserSetting()
    {
        // returns user's theme & app's mode
        var fileName = settingPath.ToString();
        string jsonString = File.ReadAllText(fileName);
        UserSetting? usersettings = JsonSerializer.Deserialize<UserSetting>(jsonString)!;
        List<string> results = [usersettings.Theme, usersettings.Mode];
        return results;
    }
}