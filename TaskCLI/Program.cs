// This is a simple example from Terminal Gui documanets
// and needs to be modified later

using Terminal.Gui;
using Terminal.Gui.Graphs;

class Program
{ 
    static void Main(string[] args)
    {
        // get date
        var day = DateTime.Now;
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
        var menu = new MenuBar(new MenuBarItem[]
        {
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
                    ;
                }) 
            }),
        });
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
        var button = new Button("OK")
        {
            X = 1,
            Y = 3
        };
        button.Clicked += () =>
        {
            MessageBox.Query("Info", "Button clicked", "Ok");
        };

        win.Add(dayOfTheWeek, breakLine, button);
        top.Add(menu);
        top.Add(win);

        Application.Run();
        Application.Shutdown();

        Console.ResetColor();
    }
}