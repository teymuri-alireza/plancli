// This is the main program which handles user's default mode
// and also calls arguments if needed

using System.Text.Json;
using PlanCLI.Models;
using PlanCLI;

class Program
{
    static void Main(string[] args)
    {
        // manage settings & database file if it doesn't exist
        if (!File.Exists("tasks.json")) { 
            File.WriteAllText("tasks.json", "[]");
            }
        if (!File.Exists("userSettings.json")) { 
            File.WriteAllText("userSettings.json", "{\"Theme\": \"dark\", \"Mode\":\"not set\"}");
            }

        var db = new DatabaseController("tasks.json");

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
                Console.WriteLine("Set your mode first");
                Arguments.PrintHelp();
                break;
            default:
                Arguments.PrintHelp();
                break;
        }
    }

    public static int GenerateNextID(DatabaseController db)
    {
        return db.Items.Count == 0 ? 1 : db.Items.Max(t => t.Id) + 1;
    }

    public static List<string> GetUserSetting()
    {
        var fileName = "userSettings.json".ToString();
        string jsonString = File.ReadAllText(fileName);
        UserSetting? usersettings = JsonSerializer.Deserialize<UserSetting>(jsonString)!;
        List<string> results = [usersettings.Theme, usersettings.Mode];
        return results;
    }
}