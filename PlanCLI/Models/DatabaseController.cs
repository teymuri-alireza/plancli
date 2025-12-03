using System.Text.Json;

namespace PlanCLI.Models;

public class DatabaseController
{
    private readonly string _filePath;
    public List<TodoItem> Items { get; set; } = new();
    public DatabaseController(string filePath)
    {
        _filePath = filePath;
        Load();
    }

    public void Load()
    {
        if (!File.Exists(_filePath))
        {
            Items = new List<TodoItem>();
            Save();
            return;
        }
        var json = File.ReadAllText(_filePath);
        Items = JsonSerializer.Deserialize<List<TodoItem>>(json) ?? new();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Items, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
    }

    public void Delete(TodoItem item)
    {
        if (item == null) return;

        var removed = Items.Remove(item);

        if (!removed)
        {
            Items.RemoveAll(x => x.Id == item.Id);
        }

        Save();
    }
}