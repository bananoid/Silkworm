# Non-Planar & Variable Layer Height Printing Guide

## Overview

Silkworm supports **non-planar 3D printing** and **variable layer heights** through its Movement component. Unlike traditional slicers that create horizontal planar layers, Silkworm allows you to define custom 3D paths with varying extrusion parameters for each segment.

## Key Capabilities

✅ **Variable Layer Height** - Different layer heights per segment
✅ **Variable Flow Rate** - Adjust extrusion amount per segment
✅ **Variable Speed** - Control print speed per segment
✅ **Non-Planar Paths** - Follow surface contours in 3D space
✅ **Ceramic/Paste Printing** - Optimized for large nozzle paste extrusion

---

## How Silkworm Handles Variable Parameters

### The Movement Component

The **Silkworm Movement** component is the key to variable parameter printing:

```
Inputs:
- Geometry: List of Lines or Curve (3D path segments)
- Speed: List of speed values (mm/min) - one per segment
- Flow: List of flow values (mm²) - one per segment
- Delimiter: Optional delimiter for movement end
```

### How It Works

1. **Per-Segment Control**: Each line segment in your path can have unique flow and speed values
2. **3D Coordinates**: Lines can have any Z-height - Silkworm preserves 3D geometry
3. **Automatic Extrusion**: Flow value × segment length = extrusion amount
4. **GCode Generation**: Outputs proper G1 commands with E (extrusion) values

---

## Variable Layer Height Implementation

### Understanding Layer Height

**Layer Height** = Z-distance between consecutive path segments

For non-planar printing:
- Layer height varies based on surface geometry
- Each segment may have different vertical displacement
- Flow must adjust proportionally to maintain consistent line width

### Flow Calculation Formula

The fundamental relationship:

```
Flow (mm²) = LineWidth (mm) × LayerHeight (mm)
```

**For standard 0.4mm nozzle:**
- Line Width: 0.4-0.8mm (1.0-2.0× nozzle diameter)
- Layer Height: 0.1-0.3mm
- Flow: 0.04-0.24 mm²

**For 1.5mm ceramic nozzle:**
- Line Width: 1.5-3.0mm (1.0-2.0× nozzle diameter)
- Layer Height: 0.5-1.2mm
- Flow: 0.75-3.6 mm²

### Example Calculation

Printing with variable layer heights:

```
Segment 1: LayerHeight = 0.8mm, LineWidth = 2.0mm
→ Flow = 2.0 × 0.8 = 1.6 mm²

Segment 2: LayerHeight = 1.0mm, LineWidth = 2.0mm
→ Flow = 2.0 × 1.0 = 2.0 mm²

Segment 3: LayerHeight = 0.6mm, LineWidth = 2.0mm
→ Flow = 2.0 × 0.6 = 1.2 mm²
```

---

## Grasshopper Workflow

### Basic Non-Planar Workflow

```
1. Generate 3D Surface/Path
   ↓
2. Extract Line Segments
   - Use curves, polylines, or point sequences
   - Ensure proper connectivity
   ↓
3. Calculate Parameters for Each Segment
   - Layer Height: Z-difference between points
   - Flow: LineWidth × LayerHeight
   - Speed: Based on curvature/complexity
   ↓
4. Create Lists of Values
   - Flow values (same count as segments)
   - Speed values (same count as segments)
   ↓
5. Feed to Movement Component
   - Geometry → line segments
   - Flow → flow list
   - Speed → speed list
   ↓
6. Connect to Generator
   - Settings → your printer profile
   - Geometry → Movement output
   ↓
7. Output GCode
```

### Pseudocode Example

```python
# In Grasshopper (Python script):

# 1. Get your 3D curve/surface
surface = input_surface
lineWidth = 2.0  # mm (for 1.5mm nozzle)

# 2. Extract path segments
segments = extract_segments_from_surface(surface)

# 3. Calculate variable parameters
flowList = []
speedList = []

for i in range(len(segments)):
    # Get current and previous point
    pt_current = segments[i].PointAtEnd
    pt_previous = segments[i].PointAtStart

    # Calculate layer height (Z-difference)
    layerHeight = abs(pt_current.Z - pt_previous.Z)

    # Clamp to safe range (0.5-1.2mm for ceramic)
    layerHeight = max(0.5, min(1.2, layerHeight))

    # Calculate flow
    flow = lineWidth * layerHeight
    flowList.append(flow)

    # Calculate speed (slower for tall layers)
    baseSpeed = 600  # mm/min
    speed = baseSpeed * (0.8 / layerHeight)  # inverse relationship
    speedList.append(speed)

# 4. Output to Movement component
output_segments = segments
output_flow = flowList
output_speed = speedList
```

### Advanced: Surface-Following Paths

For true non-planar printing following surface contours:

```
1. Create isocurves or UV curves on surface
2. Sample points along curves at desired resolution
3. Connect points into polyline segments
4. Calculate normal vectors at each point
5. Adjust layer height based on surface curvature
6. Calculate flow accordingly
```

---

## Ceramic Printing Specifics

### Large Nozzle Considerations (1.5mm)

**Key Differences from Standard FDM:**

| Parameter | Standard (0.4mm) | Ceramic (1.5mm) |
|-----------|------------------|-----------------|
| Nozzle Diameter | 0.4mm | 1.5mm |
| Layer Height | 0.1-0.3mm | 0.5-1.2mm |
| Line Width | 0.4-0.8mm | 1.5-3.0mm |
| Flow Range | 0.04-0.24 mm² | 0.75-3.6 mm² |
| Print Speed | 30-60 mm/s | 5-20 mm/s |
| Retraction | Yes (1-6mm) | No (0mm) |

### Ceramic Paste Properties

**Viscosity Considerations:**
- Higher viscosity = slower speeds required
- Flow must overcome paste resistance
- No stringing/oozing like thermoplastic
- Material settles/dries instead of solidifying

**Structural Support:**
- Paste needs time to partially dry between layers
- Overhangs limited by paste viscosity (typically <45°)
- Internal structure important for tall prints
- Consider adding dwell time in GCode for drying

### Flow Multipliers for Ceramics

Adjust base flow calculations with these multipliers:

```
Standard Extrusion:     Flow × 1.0
First Layer Adhesion:   Flow × 1.3
Overhangs/Bridges:      Flow × 1.1
Fine Details:           Flow × 0.85
Infill (sparse):        Flow × 0.9
Perimeters (strong):    Flow × 1.05
```

---

## Tips & Best Practices

### Design Considerations

1. **Gradual Height Changes**
   - Avoid sudden layer height jumps
   - Smooth transitions prevent flow inconsistencies
   - Maximum ΔHeight per segment: ±40% of previous height

2. **Curvature Analysis**
   - High curvature areas need more segments
   - Flatter areas can use larger segments
   - Maintain consistent segment length when possible

3. **Overhang Strategy**
   - Reduce speed for overhangs (×0.7)
   - Increase flow slightly (×1.1)
   - Consider support structures for >45° angles

### Troubleshooting

**Problem: Inconsistent Extrusion**
- Check flow calculations match layer heights
- Verify paste consistency
- Ensure pressure system is stable

**Problem: Path Following Errors**
- Reduce segment length for complex curves
- Check for discontinuities in path
- Verify Z-values are sequential

**Problem: Structural Failure**
- Reduce layer height for overhangs
- Increase perimeter count
- Add internal support structures

---

## External Resources

### Related Projects

- **[nonplanar3d](https://github.com/r3dsign/nonplanar3d)** - Grasshopper algorithm for non-planar paths
  - Required plugins: Pufferfish, Wombat, Silkworm, Lunchbox
  - Modular code structure
  - Community-maintained

### Further Reading

- McNeel Forum: [Non Planar 3D printing](https://discourse.mcneel.com/t/non-planar-3d-printing/151873)
- McNeel Forum: [Designing for ceramics](https://discourse.mcneel.com/t/designing-for-3d-printing-especially-ceramics/79137)
- McNeel Forum: [Adaptive Layer Height](https://discourse.mcneel.com/t/adaptive-layer-height/158058)

---

## Example Settings Files

This repository includes optimized settings for different printing scenarios:

- `silkworm_settings_basic.ini` - Standard PLA (0.4mm nozzle)
- `silkworm_settings_ceramic_1.5mm.ini` - Ceramic paste (1.5mm nozzle)
- `silkworm_settings_high_quality.ini` - Fine detail (0.4mm nozzle)
- `silkworm_settings_fast_draft.ini` - Rapid prototyping (0.4mm nozzle)

See individual files for detailed parameter explanations.
