using System.IO;
using Mono.Cecil;

namespace RogueLibsPatcher.Console
{
    public static class Program
    {
        public static void Main()
        {
            System.Console.WriteLine("Enter path to the Assembly-CSharp.dll you want to patch:");
            string path = System.Console.ReadLine()!;
            path = path.Trim('"');
            string newPath = path + "2";
            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters
            {
                AssemblyResolver = new CustomResolver(),
            });
            RogueLibsPatcher_Gen2.Patch(assembly);
            assembly.Write(newPath);
            assembly.Dispose();
            File.Copy(newPath, path, true);
            File.Delete(newPath);

            System.Console.WriteLine("Patched successfully! Press any key to exit...");
            System.Console.ReadKey();
        }
        private class CustomResolver : BaseAssemblyResolver
        {
            private readonly DefaultAssemblyResolver _defaultResolver;
            public CustomResolver() => _defaultResolver = new DefaultAssemblyResolver();

            public override AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                AssemblyDefinition assembly;
                try
                {
                    assembly = _defaultResolver.Resolve(name);
                }
                catch (AssemblyResolutionException)
                {
                    assembly = AssemblyDefinition.ReadAssembly(@"D:\Steam\steamapps\common\Streets of Rogue\StreetsOfRogue_Data\Managed\XGamingRuntime.dll");
                }
                return assembly;
            }
        }
    }
}
