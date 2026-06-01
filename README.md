# PlanCLI

**⚠️ This project is archived and no longer maintained.**

A cross-platform terminal-based to-do list application written in C#, built using **Spectre.Console**

# Features

<!-- 1. Supports both TUI mode and CLI mode -->
1. JSON-based local database for storing tasks.
2. Clean, minimal terminal interface.
3. Cross-platform support (Windows, Linux, macOS).

# Planned Features

<!-- - [ ] Add open-calendar button for deadline column (requires Terminal.GUI version > 2).
- [ ] Add mouse hadnler for deadline field (requires Terminal.GUI version > 2).
- [ ] Update Terminal.GUI dependency to version > 2 -->
- [x] Add deadline column to tasks table.
- [x] Publish standalone executable files
- [x] Add theme customization menu.

# Installation

## Linux

1. Download the Linux release (`plancli-<version>-linux-x64.tar.gz`) and extract it.
2. Copy the binary to a directory in your PATH:
```bash
sudo cp plancli-linux-x64/plancli /usr/bin/plancli
```
3. Make sure it is executable:
```bash
sudo chmod +x /usr/bin/plancli
```
4. Verify the installation:
```bash
plancli --version
```

## Windows

1. Download the Windows release (`plancli-<version>-win-x64.zip`) and extract it.
2. Open the Start Menu and search for `Edit the system environment variables`.
3. Click the `Environment Variables` button.
4. Under User variables or System variables, select Path, then click Edit.
5. Click New and add the folder containing plancli.exe.
6. Verify the installation:
```powershell
plancli --version
```

# Usage

Run the application
```bash
plancli
```

You can also check other commands with
```bash
plancli --help
```

# Contribution

Contributions are welcome!

If you have an idea or improvement open an issue or submit a pull request using **Conventional Commits** for commit messages.


# Note

This application used to have both TUI and CLI modes. For some reasons the TUI mode has been disabled. The CLI has set to dafault mode.
