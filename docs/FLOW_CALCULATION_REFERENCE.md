# Flow & Extrusion Calculation Reference

## Overview

This document explains the mathematical relationships between nozzle size, layer height, line width, and flow rate in 3D printing, with specific focus on Silkworm's implementation.

---

## Fundamental Concepts

### What is Flow?

**Flow** in Silkworm represents the **cross-sectional area** of extruded material (mm²).

```
Flow (mm²) = LineWidth (mm) × LayerHeight (mm)
```

This value is multiplied by segment length to calculate total extrusion:

```
Extrusion (mm³) = Flow (mm²) × SegmentLength (mm)
```

### How Silkworm Calculates Extrusion

From `SilkwormDataTypes.cs`:

```csharp
// Flow parameter is stored per line segment
public double sFlow = -1;  // mm²

// Calculate extrusion for GCode
public void calcExtrusion() {
    Extrusion = (sCurve.Length * sFlow);  // mm³
}
```

---

## Standard FDM Printing (Thermoplastic)

### Flow Multiplier (FlM) Calculation

Silkworm calculates a flow multiplier based on filament and nozzle geometry:

```
FlM = (NozzleDia² × π / 4) / (FilamentDia² × π / 4)
FlM = (NozzleDia / FilamentDia)²
```

**Example: 0.4mm nozzle, 1.75mm filament**

```
FlM = (0.4 / 1.75)²
FlM = 0.052  (approximately)
```

This accounts for the compression ratio between filament volume and extruded bead volume.

### Line Width Guidelines

| Nozzle Size | Min Width | Typical Width | Max Width |
|-------------|-----------|---------------|-----------|
| 0.2mm | 0.2mm | 0.25mm | 0.4mm |
| 0.4mm | 0.4mm | 0.4-0.5mm | 0.8mm |
| 0.6mm | 0.6mm | 0.65mm | 1.2mm |
| 0.8mm | 0.8mm | 0.9mm | 1.6mm |
| 1.0mm | 1.0mm | 1.2mm | 2.0mm |

**Rule of Thumb**: LineWidth = 1.0-2.0× NozzleDiameter

### Layer Height Guidelines

| Nozzle Size | Min Layer | Typical Layer | Max Layer |
|-------------|-----------|---------------|-----------|
| 0.2mm | 0.05mm | 0.1mm | 0.15mm |
| 0.4mm | 0.1mm | 0.2mm | 0.3mm |
| 0.6mm | 0.2mm | 0.35mm | 0.45mm |
| 0.8mm | 0.3mm | 0.5mm | 0.6mm |
| 1.0mm | 0.4mm | 0.6mm | 0.75mm |

**Rule of Thumb**: LayerHeight = 0.25-0.75× NozzleDiameter

### Example Flow Calculations (0.4mm Nozzle)

**Standard Print (0.2mm layer, 0.45mm width):**
```
Flow = 0.45 × 0.2 = 0.09 mm²
```

**High Quality (0.1mm layer, 0.4mm width):**
```
Flow = 0.4 × 0.1 = 0.04 mm²
```

**Fast Draft (0.3mm layer, 0.5mm width):**
```
Flow = 0.5 × 0.3 = 0.15 mm²
```

---

## Large Nozzle Printing (Paste/Ceramic)

### 1.5mm Ceramic Nozzle Parameters

**Different from thermoplastic:**
- No filament compression (paste is direct-feed)
- No melting zone considerations
- Viscosity instead of temperature
- No retraction

### Flow Calculation for Paste

Since paste is directly extruded (no filament compression):

```
Flow (mm²) = LineWidth × LayerHeight

# No FlM multiplier needed for paste!
```

### Ceramic Nozzle Ranges (1.5mm)

| Parameter | Minimum | Recommended | Maximum |
|-----------|---------|-------------|---------|
| Layer Height | 0.5mm | 0.8mm | 1.2mm |
| Line Width | 1.5mm | 2.0mm | 3.0mm |
| Flow | 0.75 mm² | 1.6 mm² | 3.6 mm² |
| Print Speed | 5 mm/s | 10 mm/s | 20 mm/s |

### Example Flow Calculations (1.5mm Ceramic)

**Standard Ceramic Print:**
```
LayerHeight = 0.8mm
LineWidth = 2.0mm
Flow = 2.0 × 0.8 = 1.6 mm²
```

**Fine Detail Ceramic:**
```
LayerHeight = 0.5mm
LineWidth = 1.5mm
Flow = 1.5 × 0.5 = 0.75 mm²
```

**Rapid Deposition:**
```
LayerHeight = 1.2mm
LineWidth = 3.0mm
Flow = 3.0 × 1.2 = 3.6 mm²
```

---

## Variable Layer Height Calculations

### Principle

For non-planar printing, layer height varies per segment. Flow must adjust proportionally to maintain consistent line width.

```
Constant LineWidth, Variable LayerHeight
→ Variable Flow

Flow[i] = LineWidth × LayerHeight[i]
```

### Grasshopper Implementation

```python
# Define constant line width
lineWidth = 2.0  # mm (for 1.5mm nozzle)

# For each segment in path:
for i in range(len(segments)):

    # Calculate layer height from Z-difference
    z_current = segments[i].PointAtEnd.Z
    z_previous = segments[i].PointAtStart.Z
    layerHeight = abs(z_current - z_previous)

    # Clamp to safe range
    layerHeight = max(minHeight, min(maxHeight, layerHeight))

    # Calculate flow for this segment
    flow = lineWidth * layerHeight

    flowList.append(flow)
```

### Example: Non-Planar Path

```
Segment    Z-Start   Z-End    ΔZ (Layer)   Flow (W=2.0mm)
   1        0.0      0.8       0.8mm       1.6 mm²
   2        0.8      1.5       0.7mm       1.4 mm²
   3        1.5      2.7       1.2mm       2.4 mm²
   4        2.7      3.2       0.5mm       1.0 mm²
   5        3.2      3.9       0.7mm       1.4 mm²
```

---

## Advanced: Speed-Flow Relationships

### Volumetric Flow Rate

The actual material deposition rate:

```
VolumetricFlow (mm³/s) = Flow (mm²) × Speed (mm/s)
```

**Example:**
```
Flow = 1.6 mm²
Speed = 10 mm/s
VolumetricFlow = 1.6 × 10 = 16 mm³/s
```

### Maximum Flow Rate Limits

Each extruder has a maximum volumetric flow rate:

**Thermoplastic (0.4mm nozzle):**
- Standard hotend: ~11 mm³/s
- High-flow hotend: ~24 mm³/s
- Volcano hotend: ~33 mm³/s

**Paste/Ceramic (1.5mm nozzle):**
- Depends on:
  - Air pressure (PSI)
  - Paste viscosity
  - Syringe diameter
  - Typical range: 10-50 mm³/s

### Speed Calculation from Flow Limit

```
MaxSpeed (mm/s) = MaxVolumetricFlow (mm³/s) / Flow (mm²)
```

**Example for ceramic:**
```
MaxVolumetricFlow = 30 mm³/s
Flow = 1.6 mm²
MaxSpeed = 30 / 1.6 = 18.75 mm/s
```

---

## Silkworm's Flow Module (Code Reference)

### From `SilkwormDataTypes.cs` Lines 443-470

```csharp
#region Extrusion Flow Module
/*
 * Josef Prusa's Calculator formulas:
 * layer height (LH) = height of each layer
 * width over height (WOH) = line width / layer height
 * free extrusion diameter (extDia) = nozzle diameter + 0.08mm
 * line width (LW) = LH * WOH
 * free extrusion cross section (freeExt) = (extDia/2)² * π
 * minimal extrusion cross section (minExt) = freeExt * 0.5
 * extruded line cross section (extLine) = LW * LH
 * bridge flow multiplier = (freeExt * 0.7) / extLine
 */

double nozDia = double.Parse(Settings["nozzle_diameter"]);
double filDia = double.Parse(Settings["filament_diameter"]);

// Calculate Flow Rate as Ratio of areas
double FlM = ((Math.Pow(nozDia/2, 2) * Math.PI) /
              (Math.Pow(filDia/2, 2) * Math.PI));

sLines.Add(new SilkwormLine(FlM, fRate, line));
#endregion
```

### What This Means

For thermoplastic FDM:
- Silkworm auto-calculates FlM from nozzle and filament diameter
- This is the **flow per unit length** (mm³/mm)
- Gets multiplied by segment length for total extrusion

For paste extrusion:
- Set `filament_diameter = nozzle_diameter` in settings
- This makes FlM ≈ 1.0
- Then manually provide flow values per segment via Movement component

---

## Quick Reference Tables

### Flow Values by Application (0.4mm Nozzle)

| Application | Layer (mm) | Width (mm) | Flow (mm²) |
|-------------|-----------|------------|-----------|
| Ultra Detail | 0.08 | 0.40 | 0.032 |
| High Quality | 0.12 | 0.45 | 0.054 |
| Standard | 0.20 | 0.48 | 0.096 |
| Fast Draft | 0.28 | 0.55 | 0.154 |
| Structural | 0.30 | 0.60 | 0.180 |

### Flow Values for Ceramic (1.5mm Nozzle)

| Application | Layer (mm) | Width (mm) | Flow (mm²) |
|-------------|-----------|------------|-----------|
| Fine Detail | 0.5 | 1.5 | 0.75 |
| Standard | 0.8 | 2.0 | 1.60 |
| Rapid Fill | 1.0 | 2.5 | 2.50 |
| Maximum | 1.2 | 3.0 | 3.60 |

### Flow Adjustments (Multipliers)

| Condition | Multiplier | Example (Base = 1.6) |
|-----------|-----------|---------------------|
| First Layer | ×1.2-1.4 | 1.92-2.24 mm² |
| Overhangs | ×1.05-1.1 | 1.68-1.76 mm² |
| Bridges | ×0.9-1.0 | 1.44-1.60 mm² |
| Fine Detail | ×0.85-0.9 | 1.36-1.44 mm² |
| Infill | ×0.9-1.0 | 1.44-1.60 mm² |
| Perimeters | ×1.0-1.05 | 1.60-1.68 mm² |

---

## Troubleshooting

### Under-Extrusion Symptoms
- Gaps between perimeters
- Weak layer adhesion
- Thin walls

**Solutions:**
- Increase flow by 5-10% (×1.05-1.10)
- Reduce print speed
- Check for clogs/restrictions

### Over-Extrusion Symptoms
- Blobbing/bulging
- Rough surface
- Nozzle catching on print

**Solutions:**
- Decrease flow by 5-10% (×0.90-0.95)
- Increase print speed
- Verify line width settings

### Variable Height Issues
- Inconsistent extrusion in non-planar paths
- Flow varies unpredictably

**Solutions:**
- Verify flow calculation: Flow = Width × Height
- Check layer height clamp values (min/max)
- Ensure smooth height transitions (no jumps)
- Debug with visualization in Grasshopper

---

## Summary Formulas

### Essential Equations

```
# Basic Flow
Flow (mm²) = LineWidth (mm) × LayerHeight (mm)

# Total Extrusion
Extrusion (mm³) = Flow (mm²) × SegmentLength (mm)

# Volumetric Flow Rate
VolumetricFlow (mm³/s) = Flow (mm²) × Speed (mm/s)

# Max Speed from Flow Limit
MaxSpeed (mm/s) = MaxVolumetricFlow (mm³/s) / Flow (mm²)

# Flow Multiplier (FDM only)
FlM = (NozzleDia / FilamentDia)²

# Practical Line Width Range
LineWidth = (1.0 to 2.0) × NozzleDiameter

# Practical Layer Height Range
LayerHeight = (0.25 to 0.75) × NozzleDiameter
```

### For Ceramic/Paste (1.5mm Nozzle)

```
# Recommended Ranges
LayerHeight: 0.5 - 1.2 mm
LineWidth: 1.5 - 3.0 mm
Flow: 0.75 - 3.6 mm²
Speed: 5 - 20 mm/s

# Variable Layer Height
for each segment:
    layerHeight = abs(z_current - z_previous)
    flow = lineWidth × layerHeight

# Speed adjustment for tall layers
speed = baseSpeed × (referenceHeight / layerHeight)
```
