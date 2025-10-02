# Silkworm Project Organization

This document explains the reorganized project structure.

## Directory Structure

```
Silkworm/
│
├── settings/              # Printer Configuration Files
│   ├── silkworm_settings_basic.ini
│   ├── silkworm_settings_ceramic_1.5mm.ini
│   ├── silkworm_settings_fast_draft.ini
│   └── silkworm_settings_high_quality.ini
│
├── docs/                  # Documentation
│   ├── README.md          # Main documentation
│   ├── NON_PLANAR_PRINTING_GUIDE.md
│   └── FLOW_CALCULATION_REFERENCE.md
│
├── examples/              # Example Grasshopper files (coming soon)
│
├── .vscode/              # VSCode Configuration
│   ├── tasks.json        # Build tasks
│   ├── launch.json       # Debug configuration
│   └── settings.json     # Editor settings
│
├── Properties/           # Assembly Information
│   ├── AssemblyInfo.cs
│   ├── Resources.Designer.cs
│   └── Settings.Designer.cs
│
├── References/           # External DLL References
│   ├── GH_IO.dll
│   ├── Grasshopper.dll
│   └── RhinoCommon.dll
│
├── Resources/            # Icons and Assets
│   └── [PNG files]
│
└── [*.cs]                # C# Source Files
    ├── GeneratorComponent.cs
    ├── MovementComponent.cs
    ├── SlicingComponent.cs
    ├── LoadSettings.cs
    └── ...
```

## Files & Folders

### `/settings/`
Contains `.ini` configuration files for different printing scenarios:

- **Basic FDM** (`basic.ini`) - Standard PLA printing with 0.4mm nozzle
- **High Quality** (`high_quality.ini`) - Fine detail prints, slower speeds
- **Fast Draft** (`fast_draft.ini`) - Rapid prototyping
- **Ceramic** (`ceramic_1.5mm.ini`) - Large nozzle paste extrusion with variable layer height support

### `/docs/`
Project documentation:

- **README.md** - Main usage documentation
- **NON_PLANAR_PRINTING_GUIDE.md** - Guide for variable layer heights and non-planar printing
- **FLOW_CALCULATION_REFERENCE.md** - Mathematical formulas and calculation reference

### `/examples/`
Example Grasshopper files and workflows (to be added)

### `/.vscode/`
Visual Studio Code configuration:

- **tasks.json** - Build, clean, and rebuild tasks
- **launch.json** - Debug configurations for Rhino
- **settings.json** - C# editor settings

### `/Properties/`
.NET assembly information and resources

### `/References/`
Rhino and Grasshopper DLL references

### `/Resources/`
Component icons and images

### Root Source Files
C# component implementations:

- **GeneratorComponent.cs** - Main GCode generator
- **MovementComponent.cs** - Variable flow/speed movements
- **LoadSettings.cs** - Settings file loader
- **SlicingComponent.cs** - Geometry slicing
- **SilkwormDataTypes.cs** - Core data structures
- **DelimiterComponent.cs** - Movement delimiters
- **FillingComponent.cs** - Infill generation
- **SegmentComponent.cs** - Path segmentation
- **Segmenter.cs** - Segmentation logic
- **ViewerComponent.cs** - 3D visualization
- **Utility.cs** - Helper functions
- **SilkwormSkein.cs** - Toolpath management

## Quick Links

### For Users
- 📖 [Main Documentation](../docs/README.md)
- ⚙️ [Settings Files](../settings/)
- 🎯 [Non-Planar Guide](../docs/NON_PLANAR_PRINTING_GUIDE.md)
- 🔢 [Flow Calculations](../docs/FLOW_CALCULATION_REFERENCE.md)

### For Developers
- 🛠️ [VSCode Config](../.vscode/)
- 📦 [Project File](../Silkworm.csproj)
- 🔗 [References](../References/)

## Recent Changes

### 2025-10-02
- Reorganized settings files into `settings/` folder
- Moved documentation to `docs/` folder
- Created `examples/` folder for future use
- Added comprehensive documentation:
  - Non-planar printing guide
  - Flow calculation reference
  - Ceramic printing support
- Created ceramic 1.5mm nozzle profile
- Updated all README files with new structure
