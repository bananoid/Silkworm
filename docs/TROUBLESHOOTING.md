# Silkworm Troubleshooting Guide

## Common Issues and Solutions

### Settings File Errors

#### Error: "The given key 'layer_height' was not present in the dictionary"

**Cause**: The settings parser couldn't find the required key in your settings file.

**Solutions**:

1. **Fixed in Latest Version** - This issue has been resolved in the updated version. Rebuild the plugin:
   ```bash
   dotnet build -c Debug
   ```

2. **Check Your Settings File Format**:
   - Make sure settings are in `key = value` format
   - Each setting on its own line
   - No extra spaces around the equals sign (or just one)
   - Values should not have trailing spaces

3. **Verify Required Settings** - Make sure your `.ini` file contains at minimum:
   ```ini
   layer_height = 0.8
   nozzle_diameter = 1.5
   filament_diameter = 1.5
   perimeters = 2
   solid_layers = 2
   # ... (see settings files for complete list)
   ```

4. **Use Clean Settings File** - Try the minimal version without comments:
   - [`settings/silkworm_settings_ceramic_1.5mm_CLEAN.ini`](../settings/silkworm_settings_ceramic_1.5mm_CLEAN.ini)

#### Error: Settings Not Loading

**Symptoms**: LoadSettings component shows warning "You must declare some valid settings"

**Solutions**:

1. **Double-click the LoadSettings component** to browse for your `.ini` file
2. **Check file path** - Make sure the file actually exists at that location
3. **Verify file permissions** - Ensure the file is readable
4. **Check for Unicode characters** - Settings file should be plain ASCII text

### Parser Issues (Fixed)

The following issues have been **fixed in the latest version**:

✅ **Comment lines** starting with `#` are now properly skipped
✅ **Empty lines** are now properly skipped
✅ **Trailing spaces** in values are now trimmed
✅ **Duplicate keys** are now handled (last value wins)
✅ **Line ending issues** (Windows vs Unix) are handled

### Build Issues

#### Error: Build Failed

**Check**:
1. **.NET SDK version** - Requires .NET SDK 9.0+
   ```bash
   dotnet --version
   ```

2. **References** - Ensure DLLs are in `References/` folder:
   - GH_IO.dll
   - Grasshopper.dll
   - RhinoCommon.dll

3. **Clean and Rebuild**:
   ```bash
   dotnet clean
   dotnet build -c Debug
   ```

#### Build Succeeds but Plugin Not Loading

1. **Check Grasshopper Libraries folder**:
   ```
   ~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper (...)/Libraries/
   ```

2. **Verify `.gha` file exists**:
   ```bash
   ls -la ~/Library/Application\ Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper*/Libraries/Silkworm*
   ```

3. **Check Rhino version** - Plugin built for Rhino 8
   - Won't work in Rhino 7 or earlier without recompiling with older references

4. **Unblock the file** (macOS Gatekeeper):
   ```bash
   xattr -dr com.apple.quarantine "~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper (b45a29b1-4343-4035-989e-044e8580d9cf)/Libraries/Silkworm.gha"
   ```

### Ceramic Printing Issues

#### Flow Too High/Too Low

**Symptoms**: Over-extrusion or under-extrusion

**Solutions**:

1. **Check layer height calculation**:
   ```python
   # Make sure you're calculating correctly
   layerHeight = abs(pt_current.Z - pt_previous.Z)
   ```

2. **Verify line width**:
   - For 1.5mm nozzle: use 1.5-3.0mm line width
   - Calculate: `flow = lineWidth × layerHeight`

3. **Test with constant values first**:
   ```
   LineWidth = 2.0mm
   LayerHeight = 0.8mm
   Flow = 1.6 mm²
   ```

4. **Adjust flow multiplier** in settings or Grasshopper:
   ```
   flow = baseFlow × 1.1  // increase by 10%
   flow = baseFlow × 0.9  // decrease by 10%
   ```

#### Non-Planar Paths Not Working

1. **Verify Movement component inputs**:
   - Geometry: List of Line segments (3D)
   - Flow: List of numbers (same count as segments)
   - Speed: List of numbers (same count as segments)

2. **Check list lengths match**:
   ```python
   print(f"Segments: {len(segments)}")
   print(f"Flow values: {len(flowList)}")
   print(f"Speed values: {len(speedList)}")
   # All should be equal!
   ```

3. **Validate Z-coordinates**:
   - Make sure Z values actually vary (non-planar)
   - Check for discontinuities or jumps

### GCode Output Issues

#### Wrong Extrusion Amounts

**Check**:
1. **Extrusion mode** in settings:
   ```ini
   absolute_extrudersteps = 0  # Relative (recommended for paste)
   absolute_extrudersteps = 1  # Absolute
   ```

2. **Filament diameter setting**:
   - For paste: set `filament_diameter = nozzle_diameter`
   - For thermoplastic: use actual filament diameter (1.75 or 2.85)

#### Missing Start/End GCode

**Issue**: GCode doesn't include homing or shutdown commands

**Solution**: Check your settings file has complete GCode:
```ini
start_gcode = G28\nG1 Z5 F300\nM106 S255\nG92 E0
end_gcode = M107\nG1 Z10 F300\nG28 X0 Y0\nM84
```

Use `\n` for line breaks in the `.ini` file.

### Visualization Issues

#### Viewer Shows Nothing

1. **Connect Silkworm Model output** from Generator to Viewer input
2. **Check model was generated** - Look for GCode output first
3. **Zoom extents** in Rhino viewport
4. **Toggle visibility options** in Viewer component

#### Flow Visualization Incorrect

The Viewer shows flow as line thickness:
- Thicker lines = higher flow
- Color gradient = speed variation

If visualization looks wrong:
1. Check your flow values are reasonable (0.75-3.6 mm² for 1.5mm nozzle)
2. Verify speed values are in mm/min (not mm/s)

## Getting Help

If you're still stuck:

1. **Check documentation**:
   - [Main README](README.md)
   - [Non-Planar Guide](NON_PLANAR_PRINTING_GUIDE.md)
   - [Flow Calculations](FLOW_CALCULATION_REFERENCE.md)

2. **McNeel Forum**:
   - Search existing discussions
   - Post your issue with:
     - Settings file (or relevant portion)
     - Error message
     - What you've tried
     - Grasshopper definition (if possible)

3. **GitHub Issues**:
   - Check existing issues
   - Open new issue with details

## Debug Checklist

When troubleshooting, check:

- [ ] Latest Silkworm build installed
- [ ] Settings file in correct format
- [ ] All required settings present
- [ ] LoadSettings component shows no errors
- [ ] Flow/Speed lists match segment count
- [ ] Values are in correct units (mm, mm/s, etc.)
- [ ] GCode is being generated
- [ ] File paths are correct
- [ ] Rhino/Grasshopper version compatible

## Quick Fixes

### Reset Everything

1. Delete `.gha` from Libraries folder
2. Clean build:
   ```bash
   dotnet clean
   dotnet build -c Debug
   ```
3. Restart Rhino
4. Load fresh settings file

### Minimal Working Example

Use this minimal workflow to test:

1. **Settings**: [`settings/silkworm_settings_ceramic_1.5mm_CLEAN.ini`](../settings/silkworm_settings_ceramic_1.5mm_CLEAN.ini)
2. **Geometry**: Simple vertical line or curve
3. **Movement**: Single flow value
4. **Generator**: Basic GCode output
5. **Verify**: GCode has G1 commands with X Y Z E values

If this works, incrementally add complexity:
- Variable flow
- Non-planar paths
- Complex geometry
