using System;
using System.Linq;
using System.IO;
using Mono.Cecil;

class Program
{
    static void Main(string[] args)
    {
        var assemblyPath = "/Users/josh/dev/grasshopper/Silkworm/bin/Debug/Silkworm.gha";

        if (!File.Exists(assemblyPath))
        {
            Console.WriteLine($"Error: Assembly not found at {assemblyPath}");
            return;
        }

        // Read the assembly using Cecil (doesn't require loading dependencies)
        var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);

        Console.WriteLine("=== SILKWORM2 ASSEMBLY ANALYSIS (using Mono.Cecil) ===\n");
        Console.WriteLine($"Assembly: {assembly.Name.Name}");
        Console.WriteLine($"Version: {assembly.Name.Version}");
        Console.WriteLine($"Runtime: {assembly.MainModule.Runtime}");
        Console.WriteLine($"Total Types: {assembly.MainModule.Types.Count}\n");

        // Group by namespace
        var groupedTypes = assembly.MainModule.Types.GroupBy(t => t.Namespace ?? "(no namespace)");

        foreach (var group in groupedTypes.OrderBy(g => g.Key))
        {
            Console.WriteLine($"\n{'='} Namespace: {group.Key} {'='}");

            foreach (var type in group.OrderBy(t => t.Name))
            {
                Console.WriteLine($"\n  Type: {type.Name}");
                Console.WriteLine($"  Full Name: {type.FullName}");
                Console.WriteLine($"  Base Type: {type.BaseType?.FullName ?? "none"}");
                Console.WriteLine($"  IsClass: {type.IsClass}, IsInterface: {type.IsInterface}, IsEnum: {type.IsEnum}");

                // Get constructors
                var ctors = type.Methods.Where(m => m.IsConstructor).ToList();
                if (ctors.Count > 0)
                {
                    Console.WriteLine($"  Constructors ({ctors.Count}):");
                    foreach (var ctor in ctors)
                    {
                        var parameters = string.Join(", ", ctor.Parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                        Console.WriteLine($"    - {ctor.Name}({parameters})");
                    }
                }

                // Get methods (exclude constructors)
                var methods = type.Methods.Where(m => !m.IsConstructor && !m.IsGetter && !m.IsSetter).ToList();
                if (methods.Count > 0)
                {
                    Console.WriteLine($"  Methods ({methods.Count}):");
                    foreach (var method in methods.Take(30))
                    {
                        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                        var modifiers = "";
                        if (method.IsPublic) modifiers += "public ";
                        else if (method.IsPrivate) modifiers += "private ";
                        else if (method.IsFamily) modifiers += "protected ";
                        if (method.IsStatic) modifiers += "static ";
                        if (method.IsVirtual && !method.IsFinal) modifiers += "virtual ";
                        if (method.IsAbstract) modifiers += "abstract ";

                        Console.WriteLine($"    - {modifiers}{method.ReturnType.Name} {method.Name}({parameters})");
                    }
                    if (methods.Count > 30)
                    {
                        Console.WriteLine($"    ... and {methods.Count - 30} more");
                    }
                }

                // Get fields
                var fields = type.Fields.ToList();
                if (fields.Count > 0)
                {
                    Console.WriteLine($"  Fields ({fields.Count}):");
                    foreach (var field in fields)
                    {
                        var modifiers = "";
                        if (field.IsPublic) modifiers += "public ";
                        else if (field.IsPrivate) modifiers += "private ";
                        if (field.IsStatic) modifiers += "static ";

                        Console.WriteLine($"    - {modifiers}{field.FieldType.Name} {field.Name}");
                    }
                }

                // Get properties
                var props = type.Properties.ToList();
                if (props.Count > 0)
                {
                    Console.WriteLine($"  Properties ({props.Count}):");
                    foreach (var prop in props)
                    {
                        var getset = "";
                        if (prop.GetMethod != null) getset += "get; ";
                        if (prop.SetMethod != null) getset += "set; ";

                        Console.WriteLine($"    - {prop.PropertyType.Name} {prop.Name} {{ {getset}}}");
                    }
                }

                // Get custom attributes
                var attrs = type.CustomAttributes.ToList();
                if (attrs.Count > 0)
                {
                    Console.WriteLine($"  Attributes ({attrs.Count}):");
                    foreach (var attr in attrs)
                    {
                        Console.WriteLine($"    - [{attr.AttributeType.Name}]");
                    }
                }
            }
        }

        Console.WriteLine("\n\n=== KEY COMPONENTS ANALYSIS ===\n");

        // Find GH_Component derived types
        var componentTypes = assembly.MainModule.Types
            .Where(t => t.BaseType?.FullName?.Contains("GH_Component") == true)
            .ToList();

        Console.WriteLine($"Found {componentTypes.Count} Grasshopper components:\n");

        foreach (var comp in componentTypes)
        {
            Console.WriteLine($"\nComponent: {comp.Name}");
            Console.WriteLine($"  Namespace: {comp.Namespace}");
            Console.WriteLine($"  Base Type: {comp.BaseType.FullName}");

            // Find ComponentGuid property
            var guidProp = comp.Properties.FirstOrDefault(p => p.Name == "ComponentGuid");
            if (guidProp != null)
            {
                // Try to extract GUID from getter method
                if (guidProp.GetMethod != null && guidProp.GetMethod.HasBody)
                {
                    Console.WriteLine($"  Has ComponentGuid property (check IL for value)");
                }
            }

            // List all methods
            var publicMethods = comp.Methods.Where(m => m.IsPublic && !m.IsConstructor && !m.IsGetter && !m.IsSetter).ToList();
            if (publicMethods.Count > 0)
            {
                Console.WriteLine($"  Public Methods:");
                foreach (var method in publicMethods)
                {
                    var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    Console.WriteLine($"    - {method.ReturnType.Name} {method.Name}({parameters})");
                }
            }
        }
    }
}
