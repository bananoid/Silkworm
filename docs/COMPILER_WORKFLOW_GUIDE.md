# Silkworm Compiler Workflow Guide

## Overview

The **Silkworm Compiler** is a new component ported from Silkworm2 that enables manual GCode generation workflows without requiring settings files. This is perfect for:

- **Ceramic 3D printing** with variable layer heights
- **Non-planar printing** with custom toolpaths
- **Experimental workflows** where you want precise control over every segment
- **Multi-material printing** with different flow rates per region

---

## What's New

### 1. New Components

#### **Flow Calculator Component**
- **Location:** Silkworm → Flow Calculator
- **Purpose:** Calculate extrusion flow from layer height and line width
- **Formula:** `Flow (mm²) = LineWidth × LayerHeight × Multiplier`

**Inputs:**
- `Layer Height` - Height of each printed layer (mm)
- `Line Width` - Width of extruded line (mm), typically same as nozzle diameter
- `Flow Multiplier` - Optional flow rate adjustment (default: 1.0)

**Outputs:**
- `Flow` - Calculated flow rate in mm²
- `Info` - Calculation details and recommendations

**Example:**
```
Layer Height: 0.8 mm
Line Width: 1.5 mm (nozzle diameter)
Multiplier: 1.0

Flow = 1.5 × 0.8 × 1.0 = 1.2 mm²
```

#### **Silkworm Compiler Component**
- **Location:** Silkworm → Compiler
- **Purpose:** Compile SilkwormMovement objects into GCode without settings files
- **Key Feature:** Works with movements that already have flow and speed set

**Inputs:**
- `Silkworm Movements` - List of movements with flow/speed already defined
- `Absolute Extrusion` - Use absolute (true) or relative (false) extrusion
- `Layer Height` - For layer detection (optional)
- `Start GCode` - Custom start sequence (optional)
- `End GCode` - Custom end sequence (optional)

**Outputs:**
- `GCode` - Complete GCode ready to save as .gcode file
- `Layer Count` - Number of detected layers
- `Layer Z Values` - List of unique Z heights
- `Info` - Compilation statistics

---

## Two Workflows Comparison

### Workflow 1: Automated (Settings-Based) - EXISTING

**Best for:** Standard prints, consistent settings, batch production

```
Settings.ini File
      ↓
Load Settings Component
      ↓
Grasshopper Geometry → Generator Component → GCode
                             ↓
                    Silkworm Viewer
```

**Advantages:**
- Fast setup with printer profiles
- Consistent settings across print
- Automated slicing and infill
- Best for standard materials (PLA, PETG)

**Limitations:**
- All segments use same settings
- Requires .ini file
- Less control over individual segments

---

### Workflow 2: Manual (Compiler-Based) - NEW!

**Best for:** Ceramic printing, non-planar, variable parameters

```
Grasshopper Geometry
      ↓
Segment/Slice manually
      ↓
Flow Calculator → Set flow per segment
      ↓
Set speed per segment
      ↓
Movement Component → Create SilkwormMovements
      ↓
Compiler Component → GCode
      ↓
Save .gcode file
```

**Advantages:**
- Variable layer height per segment
- Variable flow rate per region
- No settings file required
- Perfect for non-standard materials
- Maximum control for experimental workflows

**Limitations:**
- More manual setup
- Need to calculate flow/speed yourself
- No automatic infill generation

---

## Ceramic Printing Workflow Example

### Step 1: Prepare Geometry

```grasshopper
1. Create your 3D model in Rhino
2. Slice it at variable layer heights (e.g., 0.6mm to 1.2mm)
3. Create curves representing your toolpath
```

### Step 2: Calculate Flow for Each Layer

```grasshopper
For each layer with different height:

Flow Calculator Component:
  Layer Height: 0.8 mm  (this layer)
  Line Width: 1.5 mm    (nozzle size)
  Multiplier: 1.0

  Output Flow: 1.2 mm²

Next layer with different height:
  Layer Height: 1.0 mm  (thicker layer)
  Line Width: 1.5 mm
  Multiplier: 1.0

  Output Flow: 1.5 mm²
```

### Step 3: Set Speed

```grasshopper
For ceramic paste:
  Perimeter Speed: 10 mm/s  (600 mm/min)
  Infill Speed: 15 mm/s     (900 mm/min)

Convert to mm/min (multiply by 60)
```

### Step 4: Create Movements

```grasshopper
For each curve segment:
  1. Create Line from curve
  2. Attach flow value (from Flow Calculator)
  3. Attach speed value (600 mm/min for perimeter)
  4. Create SilkwormLine
  5. Collect lines into SilkwormMovement
```

### Step 5: Add Delimiter (Retraction Settings)

```grasshopper
Delimiter Component:
  For ceramic (no retraction):
    Retract Length: 0 mm
    Retract Lift: 0 mm
    Travel Speed: 600 mm/min

  Or custom start/end vectors for pressure control
```

### Step 6: Compile to GCode

```grasshopper
Compiler Component:
  Movements: [all your SilkwormMovements]
  Absolute Extrusion: True
  Layer Height: 0.8 mm (for layer detection)
  Start GCode: "G21\nG90\nM82" (optional)
  End GCode: "M84" (optional)

  Output: GCode ready to save
```

### Step 7: Save GCode

```grasshopper
Use Panel or File Writer to save GCode output to .gcode file
```

---

## Code Changes Made

### 1. SilkwormDataTypes.cs

Added new method to `SilkwormMovement` class:

```csharp
/// <summary>
/// ToGCode method that works without Configuration dictionary
/// Use this when flow and speed are already set on individual SilkwormLines
/// Perfect for manual workflows (ceramic printing, non-planar, etc.)
/// </summary>
public List<GH_String> ToGCodeSimple(bool absoluteExtrusion = true)
```

**Key Features:**
- No Configuration dictionary required
- Uses flow/speed already set on SilkwormLines
- Supports both absolute and relative extrusion
- Handles point-based movements (blobs)
- Includes delimiter GCode for travel/retraction

Added helper method:

```csharp
public int CountDecimalPlaces(double value)
```

**Purpose:** Control GCode precision for layer detection

---

### 2. CompilerComponent.cs (NEW)

Grasshopper component that compiles movements to GCode:

**Key Methods:**
- `isCompleteLine()` - Validates movements have flow and speed
- `CountDecimalPlaces()` - GCode precision control
- `FindUniqueZValues()` - Detects unique layers from Z heights
- `IsOdd()` - For layer alternation logic (future use)

**Features:**
- Automatic layer detection and numbering
- Validation of complete movements
- Custom start/end GCode support
- Compilation statistics output

---

### 3. FlowComponent.cs (NEW)

Flow calculator component:

**Formula Implementation:**
```csharp
double flow = lineWidth * layerHeight * multiplier;
```

**Validation:**
- Warns if layer height > 75% of line width (weak layers)
- Warns if layer height < 25% of line width (poor adhesion)
- Provides recommended layer height range

---

## Variable Layer Height Example

```grasshopper
# Layer 1 (base, thick)
Layer Height: 1.0 mm
Line Width: 1.5 mm
Flow: 1.5 mm²
Speed: 600 mm/min

# Layer 2-5 (detail, thin)
Layer Height: 0.6 mm
Line Width: 1.5 mm
Flow: 0.9 mm²
Speed: 500 mm/min

# Layer 6+ (fill, thick)
Layer Height: 1.2 mm
Line Width: 1.5 mm
Flow: 1.8 mm²
Speed: 800 mm/min
```

Each layer can have different settings!

---

## Comparison with Original Generator

| Feature | Generator (Settings-based) | Compiler (Manual) |
|---------|---------------------------|-------------------|
| Settings File | Required (.ini) | Not required |
| Flow Calculation | Automatic | Manual (Flow Calculator) |
| Layer Height | Fixed | Variable per layer |
| Speed Control | Profile-based | Per-segment control |
| Infill | Automatic | Manual |
| Best For | Standard printing | Ceramic, non-planar |
| Setup Time | Fast | Longer |
| Control Level | Medium | Maximum |

---

## Troubleshooting

### "Incomplete Silkworm Movement" Error

**Cause:** Movement doesn't have flow or speed set

**Fix:**
1. Make sure each SilkwormLine has flow value set
2. Make sure each SilkwormLine has speed value set
3. Use Flow Calculator component to calculate flow
4. Multiply speed by 60 to convert mm/s to mm/min

### No Layers Detected

**Cause:** Layer height precision doesn't match actual Z values

**Fix:**
1. Check your layer height input matches actual layer thickness
2. Round Z values to match layer height decimal places
3. Example: If layer height = 0.8, Z values should be 0.8, 1.6, 2.4, etc.

### GCode Looks Wrong

**Cause:** Absolute vs relative extrusion mismatch

**Fix:**
1. Check your printer firmware (M82 = absolute, M83 = relative)
2. Set Compiler "Absolute Extrusion" input accordingly
3. Most printers use absolute (M82)

---

## Next Steps

1. ✅ **Build successful** - Silkworm.gha created (100KB)
2. ✅ **New components added** - Compiler and Flow Calculator
3. ✅ **ToGCodeSimple() method** - Settings-free GCode generation
4. 🔄 **Test in Grasshopper** - Load Silkworm.gha and test workflow
5. 🔄 **Create example file** - Ceramic printing with variable layers

---

## Files Modified

- `SilkwormDataTypes.cs` - Added ToGCodeSimple() and CountDecimalPlaces()
- `CompilerComponent.cs` - NEW: Compiler component
- `FlowComponent.cs` - NEW: Flow calculator component
- `Silkworm.csproj` - Excluded analysis tools, fixed build

**Build Output:** `/Users/josh/dev/grasshopper/Silkworm/bin/Debug/Silkworm.gha` (100KB)

---

## Summary

You now have **TWO workflows** in Silkworm:

1. **Generator Workflow** (Automated)
   - Load settings.ini → Generator → GCode
   - Fast, consistent, great for standard printing

2. **Compiler Workflow** (Manual) ⭐ NEW!
   - Flow Calculator → Movement → Compiler → GCode
   - Variable parameters, perfect for ceramic/non-planar

**Perfect for your ceramic 3D printing with 1.5mm nozzle and variable layer heights!** 🎉
