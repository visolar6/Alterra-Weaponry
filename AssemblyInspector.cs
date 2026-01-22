using System;
using System.Reflection;

class AssemblyInspector
{
    static void Main()
    {
        try
        {
            var asm = Assembly.LoadFrom(@"D:\SteamLibrary\steamapps\common\Subnautica\Subnautica_Data\Managed\Assembly-CSharp.dll");
            var type = asm.GetType("ExosuitTorpedoArm");

            if (type == null)
            {
                Console.WriteLine("ExosuitTorpedoArm not found!");
                return;
            }

            Console.WriteLine("=== ExosuitTorpedoArm Properties ===");
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Console.WriteLine($"Property: {prop.Name} ({prop.PropertyType.Name})");
            }

            Console.WriteLine("\n=== ExosuitTorpedoArm Fields ===");
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                Console.WriteLine($"Field: {field.Name} ({field.FieldType.Name})");
            }

            Console.WriteLine("\n=== ExosuitTorpedoArm Methods ===");
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_"))
                {
                    Console.WriteLine($"Method: {method.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }
}
