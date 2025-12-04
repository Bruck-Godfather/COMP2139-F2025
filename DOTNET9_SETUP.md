# Using .NET 9 from ~/.dotnet

This project is now configured to use .NET 9.0.308 from your local installation at `~/.dotnet`.

## Quick Start

### Option 1: Using the Helper Script (Easiest)
```bash
# Run the application
./dotnet9.sh run

# Build the project
./dotnet9.sh build

# Run with auto-reload
./dotnet9.sh watch run

# Run migrations
./dotnet9.sh ef database update
```

### Option 2: Export PATH (For Current Terminal Session)
```bash
# Set PATH for current session
export PATH="$HOME/.dotnet:$PATH"

# Now use dotnet normally
dotnet run
dotnet build
dotnet watch run
```

### Option 3: Add to Your Shell Profile (Permanent)
Add this line to your `~/.bashrc` or `~/.zshrc`:
```bash
export PATH="$HOME/.dotnet:$PATH"
```

Then reload your shell:
```bash
source ~/.bashrc  # or source ~/.zshrc
```

## How It Works

1. **global.json** - Specifies that this project requires .NET SDK 9.0.308
2. **PATH Priority** - By putting `~/.dotnet` first in PATH, your local .NET 9 takes precedence over system .NET 8
3. **JetBrains Rider** - Rider should automatically detect and use the .NET 9 SDK from `~/.dotnet`

## Verify .NET Version
```bash
# Check which .NET is being used
export PATH="$HOME/.dotnet:$PATH"
dotnet --version
# Should output: 9.0.308

# List all installed SDKs
dotnet --list-sdks
# Should show: 9.0.308 [/home/annup76779/.dotnet/sdk]
```

## Current Status
✅ Application is running on http://localhost:5000 with .NET 9.0.308
✅ All .NET 9 features (MapStaticAssets, WithStaticAssets) are now working
✅ Project builds to `bin/Debug/net9.0/`

## Files Created
- [global.json](file:///home/annup76779/FreelanceWork/COMP2138-ICE/global.json) - SDK version specification
- [dotnet9.sh](file:///home/annup76779/FreelanceWork/COMP2138-ICE/dotnet9.sh) - Helper script for running .NET 9 commands
