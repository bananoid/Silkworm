# Silkworm

Silkworm is a Grasshopper plugin that translates Grasshopper and Rhino geometry into GCode for 3D printing. It enables complete and intuitive manipulation of printer GCode, allowing novel printed material properties to be specified through non-solid geometry and techniques of digital craft.

## Features

✨ **Direct Geometry to GCode** - Convert Grasshopper geometry directly to 3D printer instructions
🎨 **Custom Toolpaths** - Full control over print paths and parameters
🌊 **Non-Planar Printing** - Variable layer heights and non-planar toolpaths
🏺 **Ceramic/Paste Support** - Optimized for large nozzle paste extrusion
⚡ **Variable Parameters** - Per-segment control of flow and speed

## Quick Start

### Installation

#### Building from Source (macOS/VSCode)

1. **Prerequisites**:
   - .NET SDK 9.0+
   - Rhino 8 for macOS
   - VSCode with C# Dev Kit (recommended)

2. **Build**:
   ```bash
   dotnet build -c Debug
   ```

3. The `.gha` file installs automatically to:
   ```
   ~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper (...)/Libraries/
   ```

4. **VSCode**: Press `Cmd+Shift+B` to build

#### Legacy (Visual Studio)

Compile in Visual Studio and copy the `.gha` file to your Grasshopper Components folder.

### Basic Usage

1. **Load Settings** - Use the LoadSettings component with a `.ini` file from [`settings/`](settings/)
2. **Create Geometry** - Design your toolpath in Grasshopper
3. **Generate GCode** - Connect to the Silkworm Generator
4. **Visualize** - Use the Silkworm Viewer for visual feedback

## Documentation

📖 **[Full Documentation](docs/README.md)** - Complete usage guide and reference

📚 **[Non-Planar Printing Guide](docs/NON_PLANAR_PRINTING_GUIDE.md)** - Variable layer heights and ceramic printing

🔢 **[Flow Calculation Reference](docs/FLOW_CALCULATION_REFERENCE.md)** - Math and formulas for extrusion

🔧 **[Troubleshooting Guide](docs/TROUBLESHOOTING.md)** - Common issues and solutions

🐛 **[Datatype Error Diagnostic](docs/DATATYPE_ERROR_DIAGNOSTIC.md)** - Fix "Datatype Not Supported!" errors

## Example Settings

Pre-configured settings files are available in [`settings/`](settings/):

**Standard FDM (0.4mm nozzle):**
- [`basic.ini`](settings/silkworm_settings_basic.ini) - Standard PLA
- [`high_quality.ini`](settings/silkworm_settings_high_quality.ini) - Fine detail
- [`fast_draft.ini`](settings/silkworm_settings_fast_draft.ini) - Rapid prototyping

**Ceramic/Paste (1.5mm nozzle):**
- [`ceramic_1.5mm.ini`](settings/silkworm_settings_ceramic_1.5mm.ini) - Ceramic paste extrusion

## Advanced Features

### Non-Planar Printing

Silkworm's Movement component supports **variable layer heights** and **non-planar paths**:

```
Movement Component Inputs:
├── Geometry: 3D line segments
├── Flow: Variable flow per segment (mm²)
├── Speed: Variable speed per segment (mm/min)
└── Delimiter: Movement end behavior
```

**Example: Variable Layer Height**
```python
# For each segment:
layerHeight = abs(z_current - z_previous)
flow = lineWidth × layerHeight
```

See the [Non-Planar Printing Guide](docs/NON_PLANAR_PRINTING_GUIDE.md) for detailed workflows.

### Ceramic Printing

Large nozzle (1.5mm) ceramic paste printing with:
- Variable layer heights (0.5-1.2mm)
- No retraction (paste-based)
- Slower speeds (5-20 mm/s)
- Custom flow calculations

See [`settings/ceramic_1.5mm.ini`](settings/silkworm_settings_ceramic_1.5mm.ini) for complete configuration.

## Project Structure

```
Silkworm/
├── settings/              # Printer configuration files
│   ├── silkworm_settings_basic.ini
│   ├── silkworm_settings_high_quality.ini
│   ├── silkworm_settings_fast_draft.ini
│   └── silkworm_settings_ceramic_1.5mm.ini
│
├── docs/                  # Documentation
│   ├── README.md
│   ├── NON_PLANAR_PRINTING_GUIDE.md
│   └── FLOW_CALCULATION_REFERENCE.md
│
├── .vscode/              # VSCode configuration
├── Properties/           # Assembly info
├── References/           # DLL references
├── Resources/            # Icons and assets
│
└── [*.cs]                # C# source files
```

## Development

### Building

```bash
# Debug (auto-install to Grasshopper)
dotnet build -c Debug

# Release (output to bin/Release/)
dotnet build -c Release
```

### VSCode Tasks

- `Cmd+Shift+B` - Default build
- Tasks menu:
  - Build (Debug)
  - Build (Release)
  - Clean
  - Rebuild

## Credits

Silkworm is an open project initiated by:
- Adam Holloway
- Arthur Mamou-Mani ([mamou-mani.com](http://mamou-mani.com))
- Karl Kjelstrup-Johnson ([krk-j.com](http://krk-j.com))

Licensed under [Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License](http://creativecommons.org/licenses/by-nc-sa/3.0/)

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Resources

- **McNeel Forum Discussions**:
  - [Non-Planar 3D Printing](https://discourse.mcneel.com/t/non-planar-3d-printing/151873)
  - [Ceramic Printing Design](https://discourse.mcneel.com/t/designing-for-3d-printing-especially-ceramics/79137)

- **Related Projects**:
  - [nonplanar3d](https://github.com/r3dsign/nonplanar3d) - Grasshopper non-planar path generation

---

**Download**: Visit [ProjectSilkworm.com](http://ProjectSilkworm.com) for more information
