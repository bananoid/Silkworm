using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using Silkworm.Type;

namespace Silkworm
{
    /// <summary>
    /// Silkworm Compiler Component
    /// Compiles SilkwormMovement objects into GCode without requiring settings files.
    /// Perfect for manual workflows where flow and speed are already set per segment.
    /// Ported from Silkworm2 to enable ceramic/non-planar printing workflows.
    /// </summary>
    public class SilkwormCompiler : GH_Component
    {
        public SilkwormCompiler()
            : base("Silkworm Compiler", "Compiler",
                "Compiles Silkworm Movements into GCode (no settings file required)",
                "Silkworm", "Silkworm")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Silkworm Movements", "Movements", "List of Silkworm Movements with flow and speed already set", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Absolute Extrusion", "Absolute", "Use absolute extrusion values (true) or relative (false)", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Layer Height", "LayerH", "Layer height for detecting layers (optional)", GH_ParamAccess.item, 0.8);
            pManager.AddTextParameter("Start GCode", "Start", "Custom start GCode (optional)", GH_ParamAccess.item, "");
            pManager.AddTextParameter("End GCode", "End", "Custom end GCode (optional)", GH_ParamAccess.item, "");

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("GCode", "GCode", "Output GCode ready to save as .gcode file");
            pManager.Register_IntegerParam("Layer Count", "Layers", "Number of unique Z heights (layers)");
            pManager.Register_GenericParam("Layer Z Values", "Z", "List of unique Z heights");
            pManager.Register_StringParam("Info", "Info", "Compilation information");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // === INPUTS ===
            List<GH_ObjectWrapper> movementWrappers = new List<GH_ObjectWrapper>();
            if (!DA.GetDataList(0, movementWrappers)) return;

            bool absoluteExtrusion = true;
            DA.GetData(1, ref absoluteExtrusion);

            double layerHeight = 0.8;
            DA.GetData(2, ref layerHeight);

            string startGCode = "";
            DA.GetData(3, ref startGCode);

            string endGCode = "";
            DA.GetData(4, ref endGCode);

            // === UNWRAP MOVEMENTS ===
            List<SilkwormMovement> movements = new List<SilkwormMovement>();
            int incompleteCount = 0;

            foreach (var wrapper in movementWrappers)
            {
                if (wrapper.Value is SilkwormMovement movement)
                {
                    movements.Add(movement);
                    if (!movement.complete)
                    {
                        incompleteCount++;
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        $"Input item is not a SilkwormMovement: {wrapper.Value.GetType().Name}");
                }
            }

            if (movements.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No valid SilkwormMovement objects found");
                return;
            }

            // === VALIDATE COMPLETENESS ===
            if (incompleteCount > 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"{incompleteCount} incomplete movements (missing flow or speed). These will be skipped.");
            }

            // === DETECT LAYERS ===
            int layerHeightDP = CountDecimalPlaces(layerHeight);
            List<double> uniqueZValues = FindUniqueZValues(movements, layerHeightDP);
            int layerCount = uniqueZValues.Count;

            // === COMPILE GCODE ===
            List<GH_String> gCodeLines = new List<GH_String>();
            int compiledMovements = 0;
            int skippedMovements = 0;

            // Add start GCode if provided
            if (!string.IsNullOrEmpty(startGCode))
            {
                foreach (string line in startGCode.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    gCodeLines.Add(new GH_String(line.Trim()));
                }
            }
            else
            {
                // Default start GCode
                gCodeLines.Add(new GH_String("; Silkworm Compiler Output"));
                gCodeLines.Add(new GH_String($"; Generated: {DateTime.Now}"));
                gCodeLines.Add(new GH_String($"; Total Movements: {movements.Count}"));
                gCodeLines.Add(new GH_String($"; Detected Layers: {layerCount}"));
                gCodeLines.Add(new GH_String(""));
                gCodeLines.Add(new GH_String("G21 ; set units to millimeters"));
                gCodeLines.Add(new GH_String("G90 ; use absolute coordinates"));
                if (absoluteExtrusion)
                {
                    gCodeLines.Add(new GH_String("M82 ; use absolute extrusion"));
                }
                else
                {
                    gCodeLines.Add(new GH_String("M83 ; use relative extrusion"));
                }
                gCodeLines.Add(new GH_String("G92 E0 ; reset extrusion distance"));
                gCodeLines.Add(new GH_String(""));
            }

            // Compile each movement
            for (int i = 0; i < movements.Count; i++)
            {
                SilkwormMovement movement = movements[i];

                if (!movement.complete && !movement.isGCode)
                {
                    gCodeLines.Add(new GH_String($"; Skipping incomplete movement {i + 1}"));
                    skippedMovements++;
                    continue;
                }

                // Add layer marker comment
                if (i == 0 || (i > 0 && !movements[i].ZDomain.Min.Equals(movements[i - 1].ZDomain.Min)))
                {
                    int layerNumber = uniqueZValues.IndexOf(Math.Round(movement.ZDomain.Min, layerHeightDP)) + 1;
                    gCodeLines.Add(new GH_String(""));
                    gCodeLines.Add(new GH_String($"; Layer {layerNumber} - Z = {Math.Round(movement.ZDomain.Min, layerHeightDP)}"));
                }

                // Convert movement to GCode using the new simple method
                List<GH_String> movementGCode = movement.ToGCodeSimple(absoluteExtrusion);
                gCodeLines.AddRange(movementGCode);
                compiledMovements++;
            }

            // Add end GCode if provided
            gCodeLines.Add(new GH_String(""));
            if (!string.IsNullOrEmpty(endGCode))
            {
                foreach (string line in endGCode.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    gCodeLines.Add(new GH_String(line.Trim()));
                }
            }
            else
            {
                // Default end GCode
                gCodeLines.Add(new GH_String("; End of print"));
                gCodeLines.Add(new GH_String("G92 E0 ; reset extrusion"));
                gCodeLines.Add(new GH_String("G91 ; relative positioning"));
                gCodeLines.Add(new GH_String("G1 Z5 F300 ; lift nozzle"));
                gCodeLines.Add(new GH_String("G90 ; absolute positioning"));
                gCodeLines.Add(new GH_String("M104 S0 ; turn off hotend"));
                gCodeLines.Add(new GH_String("M140 S0 ; turn off bed"));
                gCodeLines.Add(new GH_String("M84 ; disable motors"));
            }

            // === BUILD INFO STRING ===
            string info = $"Compiled {compiledMovements} movements\n" +
                          $"Skipped {skippedMovements} incomplete movements\n" +
                          $"Detected {layerCount} layers\n" +
                          $"Total GCode lines: {gCodeLines.Count}\n" +
                          $"Extrusion mode: {(absoluteExtrusion ? "Absolute" : "Relative")}";

            // === OUTPUTS ===
            DA.SetDataList(0, gCodeLines);
            DA.SetData(1, layerCount);
            DA.SetDataList(2, uniqueZValues);
            DA.SetData(3, info);
        }

        /// <summary>
        /// Check if a SilkwormLine is complete (has flow and speed set)
        /// </summary>
        public bool isCompleteLine(SilkwormLine sline)
        {
            return sline.Flow > 0 && sline.Speed > 0;
        }

        /// <summary>
        /// Count decimal places in a number for precision control
        /// </summary>
        public int CountDecimalPlaces(double value)
        {
            string valueStr = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            int decimalIndex = valueStr.IndexOf('.');
            if (decimalIndex == -1)
                return 0;
            return valueStr.Length - decimalIndex - 1;
        }

        /// <summary>
        /// Find unique Z values (layers) from a list of movements
        /// </summary>
        public List<double> FindUniqueZValues(List<SilkwormMovement> movements, int layerHeightDP)
        {
            List<double> zValues = new List<double>();

            foreach (SilkwormMovement movement in movements)
            {
                if (movement.isGCode || movement.isPt)
                    continue;

                double zMin = Math.Round(movement.ZDomain.Min, layerHeightDP);

                if (!zValues.Contains(zMin))
                {
                    zValues.Add(zMin);
                }
            }

            zValues.Sort();
            return zValues;
        }

        /// <summary>
        /// Check if a number is odd (for layer alternation logic)
        /// </summary>
        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
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
            get { return new Guid("B7E90246-8CF4-4C1D-9C7E-8E6F4A5D3B2C"); }
        }
    }
}
