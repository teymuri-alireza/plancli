using System.Text.Json;
using PlanCLI;
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
            case "-m":
            case "--mode":
                ModeHandler(args);
                break;

            default:
                Console.WriteLine("Unkown Option.");
                break;
        }
    }

    static void ModeHandler(string[] args)
    {
        switch (args [1])
        {
            case "cli":
                ChangeUserMode("cli");
                CLImode.Run();
                break;
            case "tui":
                ChangeUserMode("tui");
                TUImode.Run();
                break;
            default:
                PrintHelp();
                break;
        }
    }

    static void ChangeUserMode(string mode)
    {
        var theme = Program.GetUserSetting()[0];
        var newFile = new UserSetting() { 
            Theme = theme,
            Mode = mode 
            };
        var fileName = "userSettings.json".ToString();
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(newFile, options);
        File.WriteAllText(fileName, jsonString);
    }

    public static void PrintHelp()
    {
        Console.WriteLine("plancli usage:");
        Console.WriteLine("     dotnet run                 Run in default mode if it's set");
        Console.WriteLine("     dotnet run -- -m cli       Set default to Command-Line mode (CLI)");
        Console.WriteLine("     dotnet run -- -m tui       Set default to interactive mode (TUI)");
    }
}