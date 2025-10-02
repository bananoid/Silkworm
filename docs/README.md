Silkworm
========

Silkworm is a plugin that translates Grasshopper and Rhino geometry into GCode for 3d printing. Silkworm allows for the complete and intuitive manipulation of the printer GCode, enabling novel printed material properties to be specified by non-solid geometry and techniques of digital craft.

Development
========
For the development list and notes on Silkworm development please refer to the Teambox tasklist.


Installation
========

### Building from Source (macOS/VSCode)

This project has been modernized for development with Visual Studio Code on macOS:

1. **Prerequisites**:
   - .NET SDK 9.0+ installed
   - Rhino 8 for macOS
   - VSCode with C# Dev Kit extension (recommended)

2. **Build the project**:
   ```bash
   dotnet build -c Debug
   ```
   The Debug build automatically installs to your Grasshopper Libraries folder.

3. **VSCode Tasks**:
   - Press `Cmd+Shift+B` to build
   - Use "Tasks: Run Build Task" for more options

4. **The compiled `.gha` file** will be installed to:
   ```
   ~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper (...)/Libraries/
   ```

### Legacy Installation (Visual Studio)

To install please compile in Visual Studio (make sure you specify a location for the .gha file that you can find!) and then locate your Grasshopper Components Folder and place the compiled .gha file in there.


Usage
========

### Settings Configuration

Silkworm requires a settings file (`.ini` format) to configure your 3D printer parameters. Example settings files are included in the [`settings/`](../settings/) folder:

**Standard FDM (0.4mm nozzle):**
- **[`settings/silkworm_settings_basic.ini`](../settings/silkworm_settings_basic.ini)** - Standard PLA printing
- **[`settings/silkworm_settings_high_quality.ini`](../settings/silkworm_settings_high_quality.ini)** - High quality, slower prints
- **[`settings/silkworm_settings_fast_draft.ini`](../settings/silkworm_settings_fast_draft.ini)** - Fast prototyping and draft prints

**Ceramic/Paste Printing (1.5mm nozzle):**
- **[`settings/silkworm_settings_ceramic_1.5mm.ini`](../settings/silkworm_settings_ceramic_1.5mm.ini)** - Ceramic paste extrusion with variable layer height support

### Creating Your Settings File

1. **Copy one of the example files** from the `settings/` folder as a starting point
2. **Edit the values** to match your printer specifications
3. **In Grasshopper**:
   - Add the "LoadSettings" component
   - Double-click it to browse for your `.ini` file
   - Connect the output to the Silkworm Generator's "Settings" input

### Settings File Format

The settings file uses a simple `key = value` format:

```ini
# Comments start with #
layer_height = 0.2
nozzle_diameter = 0.4
temperature = 210
perimeter_speed = 30
fill_density = 0.2
# Or use percentages (automatically converted)
fill_density = 20%
```

### Key Settings to Customize

**Printer Dimensions**:
- `nozzle_diameter` - Your nozzle size (typically 0.4mm)
- `filament_diameter` - Filament size (1.75mm or 2.85mm)

**Print Quality**:
- `layer_height` - Layer thickness (0.1-0.3mm typical)
- `perimeters` - Number of outer shells (2-4)
- `solid_layers` - Top/bottom solid layers (3-5)
- `fill_density` - Infill percentage (0.0 to 1.0 or 0% to 100%)

**Temperatures** (adjust for your material):
- `temperature` - Printing temperature (°C)
- `bed_temperature` - Bed temperature (°C)

**Speeds** (mm/s):
- `perimeter_speed` - Outer wall speed
- `infill_speed` - Infill print speed
- `travel_speed` - Non-printing move speed

**Start/End GCode**:
- `start_gcode` - Commands run before print (homing, purging)
- `end_gcode` - Commands run after print (cooling, parking)

Use `\n` for line breaks in GCode settings.

### Complete Settings Reference

See the example `.ini` files in [`settings/`](../settings/) for all available settings with detailed comments.

---

## Advanced Features

### Non-Planar & Variable Layer Height Printing

Silkworm supports advanced non-planar printing and variable layer heights - perfect for ceramic printing and complex geometries.

📚 **[Read the Non-Planar Printing Guide](NON_PLANAR_PRINTING_GUIDE.md)** for detailed instructions on:
- Variable layer height implementation
- Non-planar path generation in Grasshopper
- Flow calculation for varying heights
- Ceramic-specific workflows

🔢 **[Flow Calculation Reference](FLOW_CALCULATION_REFERENCE.md)** provides:
- Mathematical formulas for flow/extrusion
- Calculation examples for different nozzle sizes
- Troubleshooting guides
- Quick reference tables

---

## Project Structure

```
Silkworm/
├── settings/              # Printer configuration files (.ini)
│   ├── silkworm_settings_basic.ini
│   ├── silkworm_settings_high_quality.ini
│   ├── silkworm_settings_fast_draft.ini
│   └── silkworm_settings_ceramic_1.5mm.ini
│
├── docs/                  # Documentation
│   ├── README.md          # This file
│   ├── NON_PLANAR_PRINTING_GUIDE.md
│   └── FLOW_CALCULATION_REFERENCE.md
│
├── .vscode/              # VSCode configuration
│   ├── tasks.json        # Build tasks
│   ├── launch.json       # Debug configuration
│   └── settings.json     # Editor settings
│
└── [Source files...]     # C# source code
```

