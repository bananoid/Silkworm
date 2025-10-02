# Quick Start: Silkworm Compiler for Ceramic Printing

## TL;DR

You now have **TWO ways** to generate GCode:

1. **Generator** (old) - Automated, needs settings.ini
2. **Compiler** (new) - Manual, variable layer heights, perfect for ceramic!

---

## Ceramic Printing Quick Setup

### Your Printer Specs
- Nozzle: 1.5mm
- Material: Ceramic paste
- Layer height: Variable (0.6mm - 1.2mm)
- No retraction needed

---

## Grasshopper Workflow

### Step 1: Calculate Flow

```
Flow Calculator Component:
  Layer Height: 0.8 mm  ← adjust per layer
  Line Width: 1.5 mm    ← your nozzle
  Multiplier: 1.0       ← adjust if needed

  Output: Flow = 1.2 mm²
```

**Variable Layers Example:**
- Thin detail layers (0.6mm): Flow = 0.9 mm²
- Normal layers (0.8mm): Flow = 1.2 mm²
- Thick fill layers (1.0mm): Flow = 1.5 mm²

### Step 2: Set Speed

```
For ceramic paste (mm/s → mm/min):
  Perimeter: 10 mm/s × 60 = 600 mm/min
  Infill: 15 mm/s × 60 = 900 mm/min
  Travel: 10 mm/s × 60 = 600 mm/min
```

### Step 3: Create Lines

```
For each curve:
  Create SilkwormLine:
    Flow: [from Flow Calculator]
    Speed: 600 mm/min
    Curve: [your line]
```

### Step 4: Create Movement

```
Movement Component:
  Lines: [collection of SilkwormLines]
  Delimiter: [retraction settings]
    For ceramic (no retraction):
      Retract Length: 0
      Retract Lift: 0
      Travel Speed: 600
```

### Step 5: Compile to GCode

```
Compiler Component:
  Movements: [all your movements]
  Absolute Extrusion: True
  Layer Height: 0.8 mm

  Output: GCode (ready to save!)
```

---

## File Locations

**Built Plugin:**
```
/Users/josh/dev/grasshopper/Silkworm/bin/Debug/Silkworm.gha
```

**Ceramic Settings (for Generator workflow):**
```
/Users/josh/dev/grasshopper/Silkworm/settings/silkworm_settings_ceramic_1.5mm.ini
```

**Documentation:**
```
docs/COMPILER_WORKFLOW_GUIDE.md     ← Full guide
SILKWORM2_COMPARISON.md             ← Technical details
IMPLEMENTATION_SUMMARY.md           ← What was done
```

---

## Component Reference

### New Components

| Component | Purpose | Key Input | Key Output |
|-----------|---------|-----------|------------|
| **Flow Calculator** | Calculate flow from layer height | Layer Height, Line Width | Flow (mm²) |
| **Compiler** | Convert movements to GCode | SilkwormMovements | GCode strings |

### Existing Components (Still Work!)

| Component | Purpose | Needs Settings File? |
|-----------|---------|---------------------|
| **Load Settings** | Load .ini file | - |
| **Generator** | Auto-generate GCode | Yes |
| **Movement** | Create movements | No |
| **Delimiter** | Set retraction | No |
| **Viewer** | Visualize toolpath | No |

---

## When to Use Which Workflow

### Use Generator (Settings-based) When:
- ✅ Printing standard materials (PLA, PETG)
- ✅ Consistent layer height throughout
- ✅ Need automatic infill
- ✅ Want fast setup

### Use Compiler (Manual) When:
- ✅ **Printing ceramic paste** ← YOU!
- ✅ Need variable layer heights
- ✅ Want precise control per segment
- ✅ Non-planar printing
- ✅ Experimental materials

---

## Common Values for 1.5mm Nozzle

### Layer Heights
```
Minimum: 0.375 mm (25% of nozzle)
Typical:  0.6 - 1.0 mm
Maximum: 1.125 mm (75% of nozzle)
```

### Flow Calculations
```
Layer 0.6mm: 1.5 × 0.6 = 0.9 mm²
Layer 0.8mm: 1.5 × 0.8 = 1.2 mm²
Layer 1.0mm: 1.5 × 1.0 = 1.5 mm²
Layer 1.2mm: 1.5 × 1.2 = 1.8 mm² (warning: >75%)
```

### Speeds (Ceramic)
```
Perimeter: 600 mm/min (10 mm/s)
Infill: 900 mm/min (15 mm/s)
Travel: 600 mm/min (10 mm/s)
```

---

## Troubleshooting

### "Incomplete Movement" Error
**Fix:** Make sure EVERY line has flow and speed set

### No GCode Output
**Fix:** Check movements are connected to Compiler input

### Weird Extrusion Amounts
**Fix:**
1. Check "Absolute Extrusion" is set to True
2. Verify flow values are correct (should be ~1.2 for 0.8mm layers)

### Layers Not Detected
**Fix:** Make sure layer height matches your actual Z spacing

---

## Next Steps

1. Load `Silkworm.gha` into Grasshopper
2. Create a simple test:
   - One curve
   - Flow Calculator: 0.8mm × 1.5mm = 1.2 mm²
   - Speed: 600 mm/min
   - Movement → Compiler → Save GCode
3. Test GCode on your printer
4. Scale up to full models with variable layers!

---

## Example: Simple Cylinder

```grasshopper
1. Create circle at Z=0 (radius 50mm)
2. Copy up at 0.8mm intervals
3. For each circle:
   - Flow Calculator: 0.8mm × 1.5mm = 1.2 mm²
   - Speed: 600 mm/min
   - Create SilkwormLines
4. Create delimiter (no retraction)
5. Create SilkwormMovement for each layer
6. Compiler: All movements → GCode
7. Save to file
```

---

## Support

For full details, see:
- `docs/COMPILER_WORKFLOW_GUIDE.md` - Complete workflow guide
- `docs/NON_PLANAR_PRINTING_GUIDE.md` - Variable layer heights
- `docs/FLOW_CALCULATION_REFERENCE.md` - Flow formula reference

Build successful! Ready to test! 🎉
