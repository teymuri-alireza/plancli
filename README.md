# PlanCLI
A cross-platform terminal-based to-do list application written in C#, built using both **Terminal.GUI** and **Spectre.Console**

# Features

1. Supports both TUI mode and CLI mode
2. JSON-based local database for storing tasks.
3. Clean, minimal terminal interface.
4. Cross-platform support (Windows, Linux, macOS).

# Planned Features

- [ ] Evaluate LiteDB instead of JSON for storage.
- [ ] Add deadline column to tasks table.
- [ ] Publish standalone executable files
- [x] Add theme customization menu.

# Usage

Choose the default mode first

- TUI mode
```bash
dotnet run -- --mode tui
```

- CLI mode
```bash
dotnet run -- --mode cli
```

Run the application
```bash
dotnet run
```

NOTE: executable files will be created for easier access

NOTE: This application uses .net 8

# Gallery

- CLI mode (left) & TUI mode (right)
<div>
    <img width="50%" align="left" src="./images/climode.png">
    <img width="50%" align="right" src="./images/tuimode.png">
</div>

# Contribution

Contributions are welcome!

If you have an idea or improvement open an issue or submit a pull request using **Conventional Commits** for commit messages.
