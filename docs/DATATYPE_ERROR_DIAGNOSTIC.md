# "Datatype Not Supported!" Error - Diagnostic Guide

## What This Error Means

The Silkworm Generator component received geometry that it doesn't recognize. This happens when you connect data that isn't one of the supported types.

---

## Supported Geometry Types (Silkworm v0.1.1)

The Generator accepts these specific Grasshopper data types:

### ✅ Directly Supported:

1. **GH_Brep** - Closed Breps (3D solids)
   - Use for solid object slicing
   - Example: Box, Sphere, Extrusion

2. **GH_Mesh** - Meshes
   - Use for mesh slicing
   - Example: Mesh from geometry

3. **GH_Curve** - Curves
   - Use for perimeter paths
   - Example: Arc, Circle, NurbsCurve

4. **GH_Line** - Lines
   - Use for straight toolpaths
   - Example: Line, Line SDL

5. **SilkwormMovement** - Silkworm Movement objects
   - From Movement component
   - Can have variable flow/speed

6. **SilkwormLine** - Silkworm Line objects
   - Individual line segments with flow data

### ❌ NOT Supported Directly:

- **Polylines** - Must convert to lines or curve first
- **Points** - Not implemented (shows as "TODO" in code)
- **Text/Numbers** - Only geometry
- **Surfaces** - Must convert to Brep
- **Raw Rhino geometry** - Must be Grasshopper-wrapped

---

## How to Diagnose Your Issue

### Step 1: Check What You're Connecting

In Grasshopper, **right-click on the wire** connecting to the Generator's "Geo" input and look at the panel output.

It should show something like:
```
GH_Line
GH_Curve
GH_Brep
```

If it shows something else, that's your problem!

### Step 2: Common Problem Types

**Problem**: "Polyline" or "PolylineCurve"
```
Solution: Use "Explode" component to convert to line segments
Polyline → Explode → Generator
```

**Problem**: "Point3d" or "Point"
```
Solution: Points aren't supported in v0.1.1
Workaround: Create tiny lines or use Movement component with point coordinates
```

**Problem**: "Surface" or "NurbsSurface"
```
Solution: Convert to Brep first
Surface → Brep component → Generator
```

**Problem**: "Text" or "Number"
```
Solution: You're connecting the wrong thing!
Check your component connections
```

**Problem**: Empty/Null data
```
Solution: Make sure geometry is actually being generated
Check for red components upstream
```

### Step 3: Use the Movement Component

For **ceramic printing with variable flow**, you should use the Movement component:

```
Workflow:
Lines/Curves → Movement Component
              ↓ (with Flow & Speed lists)
           Generator
```

**Inputs to Movement**:
- **Geometry**: Lines or Curves (GH_Line or GH_Curve)
- **Flow**: List of numbers (mm²) - one per segment
- **Speed**: List of numbers (mm/min) - one per segment
- **Delimiter**: Optional

**Output**:
- SilkwormMovement object → Connect to Generator

---

## Quick Fix Checklist

- [ ] Is your geometry actually geometry? (not text/numbers)
- [ ] Are you connecting Grasshopper types? (not raw Rhino)
- [ ] Have you exploded polylines into lines?
- [ ] Are there any null/empty items in your list?
- [ ] Is your geometry actually being generated? (check for errors)

---

## Example Workflows

### ✅ CORRECT: Simple Lines

```
Line Component → Generator's "Geo" input
```

### ✅ CORRECT: Ceramic with Variable Flow

```
Line Component → Movement's "Geometry" input
Flow List → Movement's "Flow" input
Speed List → Movement's "Speed" input
Movement output → Generator's "Geo" input
```

### ✅ CORRECT: Curve Path

```
Arc/Circle/Curve → Generator's "Geo" input
```

### ❌ WRONG: Polyline

```
Polyline → Generator  ❌ (Not supported!)

Fix:
Polyline → Explode → Generator  ✅
```

### ❌ WRONG: Points

```
Point → Generator  ❌ (Not implemented!)

Workaround:
Points → Create Lines → Movement → Generator  ✅
```

---

## Testing Your Setup

### Minimal Working Example:

1. **Create a simple line**:
   ```
   Point (0,0,0) → Line SDL → Generator
   Point (100,100,100) →
   ```

2. **Add settings**:
   ```
   LoadSettings → settings/ceramic_1.5mm_CLEAN.ini → Generator
   ```

3. **Check output**:
   - GCode should appear
   - No "Datatype Not Supported!" error

If this works, your Generator is fine. The problem is with your geometry input.

### Debugging Your Actual Geometry:

1. **Add a Panel** component
2. **Connect it** to the same wire going to Generator
3. **Read the output** - it tells you the type
4. **Compare to supported types** listed above

---

## Ceramic Printing Specific

For ceramic printing with variable layer heights:

### Required Workflow:

```
1. Generate 3D path (Lines or Curve)
   ↓
2. Calculate Flow for each segment
   Flow = LineWidth × LayerHeight
   ↓
3. Create Flow list (one value per segment)
   ↓
4. Movement Component:
   - Geometry: Your lines
   - Flow: Your flow list
   - Speed: Your speed list (optional)
   ↓
5. Generator:
   - Settings: ceramic_1.5mm_CLEAN.ini
   - Geometry: Movement output
   ↓
6. Output: GCode with variable extrusion
```

### Type Check at Each Stage:

**After generating path**:
- Should be: `List of GH_Line` or `GH_Curve`
- Panel output: `{0} Line(From:{...} To:{...})`

**After Movement component**:
- Should be: `SilkwormMovement`
- Panel output: `SilkwormMovement: F... L... E...`

**Generator output**:
- Should be: `List of String` (GCode)
- Panel output: `G1 X... Y... Z... E... F...`

---

## Still Getting the Error?

### Debug Steps:

1. **Simplify your definition**
   - Remove everything except one line
   - Does that single line work?
   - Add complexity back one step at a time

2. **Check component versions**
   - Make sure you're using the rebuilt Silkworm.gha
   - Restart Rhino/Grasshopper after rebuilding

3. **Verify settings file**
   - Use `ceramic_1.5mm_CLEAN.ini`
   - LoadSettings should show no errors

4. **Look at the full error message**
   - Right-click Generator component
   - Check all error messages
   - There might be multiple issues

5. **Check GitHub issues**
   - https://github.com/ProjectSilkworm/Silkworm/issues
   - Someone might have had the same problem

---

## Report Template

If you need help, provide this information:

```
Component Setup:
- Input to Generator "Geo": [Type shown in panel]
- Input to Generator "Settings": [LoadSettings or direct?]
- Generator "Sort" parameter: [1, 2, or 3]

Geometry Details:
- What component creates it: [Component name]
- Panel output shows: [Copy panel text]
- Number of items: [Count]

Error Message:
- Full error text: [Copy exact message]
- When it occurs: [Always? Sometimes?]

What I'm Trying to Do:
- [Describe your ceramic printing workflow]
```

---

## Next Steps

Once you identify the issue:

1. **Convert geometry** to supported type
2. **Test with simple example** first
3. **Add complexity** gradually
4. **Document what works** for your ceramic workflow

See also:
- [Non-Planar Printing Guide](NON_PLANAR_PRINTING_GUIDE.md)
- [Flow Calculation Reference](FLOW_CALCULATION_REFERENCE.md)
- [Troubleshooting Guide](TROUBLESHOOTING.md)
