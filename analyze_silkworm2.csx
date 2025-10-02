#!/usr/bin/env dotnet-script

using System;
using System.Reflection;
using System.Linq;
using System.IO;

// Load the assembly
var assemblyPath = "/Users/josh/dev/grasshopper/Silkworm/silkworm2_reference.gha";
var assembly = Assembly.LoadFrom(assemblyPath);

Console.WriteLine("=== SILKWORM2 ASSEMBLY ANALYSIS ===\n");

// Get all types
var types = assembly.GetTypes();
Console.WriteLine($"Total Types: {types.Length}\n");

// Group by namespace
var groupedTypes = types.GroupBy(t => t.Namespace ?? "(no namespace)");

foreach (var group in groupedTypes.OrderBy(g => g.Key))
{
    Console.WriteLine($"\n=== Namespace: {group.Key} ===");

    foreach (var type in group.OrderBy(t => t.Name))
    {
        Console.WriteLine($"\n  Type: {type.Name}");
        Console.WriteLine($"  Full Name: {type.FullName}");
        Console.WriteLine($"  Base Type: {type.BaseType?.Name ?? "none"}");

        // Get constructors
        var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (ctors.Length > 0)
        {
            Console.WriteLine($"  Constructors: {ctors.Length}");
        }

        // Get methods
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (methods.Length > 0)
        {
            Console.WriteLine($"  Methods ({methods.Length}):");
            foreach (var method in methods.Take(10)) // Limit output
            {
                var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"    - {method.ReturnType.Name} {method.Name}({parameters})");
            }
            if (methods.Length > 10)
            {
                Console.WriteLine($"    ... and {methods.Length - 10} more");
            }
        }

        // Get fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (fields.Length > 0)
        {
            Console.WriteLine($"  Fields ({fields.Length}):");
            foreach (var field in fields.Take(10))
            {
                Console.WriteLine($"    - {field.FieldType.Name} {field.Name}");
            }
            if (fields.Length > 10)
            {
                Console.WriteLine($"    ... and {fields.Length - 10} more");
            }
        }

        // Get properties
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (props.Length > 0)
        {
            Console.WriteLine($"  Properties ({props.Length}):");
            foreach (var prop in props.Take(10))
            {
                Console.WriteLine($"    - {prop.PropertyType.Name} {prop.Name}");
            }
            if (props.Length > 10)
            {
                Console.WriteLine($"    ... and {props.Length - 10} more");
            }
        }
    }
}

Console.WriteLine("\n\n=== COMPONENT GUID COMPARISON ===\n");

// Look for Grasshopper components
var ghComponentTypes = types.Where(t =>
    t.BaseType?.Name == "GH_Component" ||
    t.GetInterfaces().Any(i => i.Name.Contains("IGH_Component")));

foreach (var comp in ghComponentTypes)
{
    Console.WriteLine($"\nComponent: {comp.Name}");

    // Try to get GUID
    var guidProp = comp.GetProperty("ComponentGuid", BindingFlags.Public | BindingFlags.Instance);
    if (guidProp != null)
    {
        try
        {
            var instance = Activator.CreateInstance(comp);
            var guid = guidProp.GetValue(instance);
            Console.WriteLine($"  GUID: {guid}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  GUID: (could not instantiate: {ex.Message})");
        }
    }
}
