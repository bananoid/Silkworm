using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Silkworm.Type;

namespace Silkworm
{
    /// <summary>
    /// Silkworm Flow Calculator Component
    /// Calculates extrusion flow based on nozzle diameter, layer height, and line width.
    /// Returns flow as area (mm²) for use with SilkwormMovement.
    /// Formula: Flow = LineWidth × LayerHeight
    /// </summary>
    public class FlowCalculator : GH_Component
    {
        public FlowCalculator()
            : base("Flow Calculator", "Flow",
                "Calculate extrusion flow from layer height and line width\nFlow (mm²) = LineWidth × LayerHeight",
                "Silkworm", "Silkworm")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Layer Height", "LayerH", "Height of each printed layer (mm)", GH_ParamAccess.item, 0.8);
            pManager.AddNumberParameter("Line Width", "LineW", "Width of extruded line (mm) - typically same as nozzle diameter", GH_ParamAccess.item, 1.5);
            pManager.AddNumberParameter("Flow Multiplier", "Mult", "Flow rate multiplier (optional, default 1.0)", GH_ParamAccess.item, 1.0);

            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Flow", "Flow", "Calculated flow rate (mm²)");
            pManager.Register_StringParam("Info", "Info", "Flow calculation information");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // === INPUTS ===
            double layerHeight = 0.8;
            if (!DA.GetData(0, ref layerHeight)) return;

            double lineWidth = 1.5;
            if (!DA.GetData(1, ref lineWidth)) return;

            double multiplier = 1.0;
            DA.GetData(2, ref multiplier);

            // === VALIDATE ===
            if (layerHeight <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Layer height must be greater than 0");
                return;
            }

            if (lineWidth <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Line width must be greater than 0");
                return;
            }

            if (multiplier <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Flow multiplier should be greater than 0, using 1.0");
                multiplier = 1.0;
            }

            // === CALCULATE FLOW ===
            // Flow (mm²) = LineWidth × LayerHeight × Multiplier
            double flow = lineWidth * layerHeight * multiplier;

            // === BUILD INFO ===
            string info = $"Flow Calculation:\n" +
                          $"  Layer Height: {layerHeight} mm\n" +
                          $"  Line Width: {lineWidth} mm\n" +
                          $"  Multiplier: {multiplier}\n" +
                          $"  Flow = {lineWidth} × {layerHeight} × {multiplier}\n" +
                          $"  Flow = {flow:F4} mm²\n\n" +
                          $"Recommended layer height range:\n" +
                          $"  Min: {lineWidth * 0.25:F2} mm (25% of line width)\n" +
                          $"  Max: {lineWidth * 0.75:F2} mm (75% of line width)";

            // Add warnings for extreme values
            if (layerHeight > lineWidth * 0.75)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Layer height ({layerHeight}mm) is >75% of line width ({lineWidth}mm). May cause weak layers.");
            }
            else if (layerHeight < lineWidth * 0.25)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Layer height ({layerHeight}mm) is <25% of line width ({lineWidth}mm). May cause poor adhesion.");
            }

            // === OUTPUTS ===
            DA.SetData(0, flow);
            DA.SetData(1, info);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add an icon here later
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("C8F91357-9DA5-4D2E-AD8F-9F7G5B6E4C3D"); }
        }
    }
}
