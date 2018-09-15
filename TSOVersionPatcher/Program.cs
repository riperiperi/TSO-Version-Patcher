using System;
using System.Collections.Generic;
using System.IO;

namespace TSOVersionPatcher
{
    public class Program
    {
        static int Main(string[] args)
        {
            var normalArgs = new List<string>();
            bool generate = false;

            foreach (var arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    switch (arg.Substring(2))
                    {
                        case "generate":
                            generate = true;
                            break;
                    }
                } else
                {
                    normalArgs.Add(arg);
                }
            }

            if (normalArgs.Count < 2)
            {
                Console.WriteLine("Usage: TSOVersionPatcher.exe --generate <patchfile.tsop> <source folder> <dest folder>");
                Console.WriteLine("(dest folder currently only used when applying)");
                return 1;
            }

            var patchFile = normalArgs[0];
            var source = normalArgs[1].Trim('"');
            var dest = (normalArgs.Count > 2) ? normalArgs[2].Trim('"') : source;

            try { 
                if (generate)
                {
                    Console.WriteLine($"Creating a patch to transform {source} into {dest}.");
                    var builder = new PatchBuilder(source, dest);

                    using (var output = File.Open(patchFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                    {
                        builder.WritePatchFile(output);
                    }
                    Console.WriteLine("Complete!");
                } else
                {
                    Console.WriteLine($"Loading Patch {patchFile}...");
                    using (var str = File.OpenRead(patchFile)) {
                        var applier = new PatchApplier(str);
                        Console.WriteLine("Loaded! Applying Patches...");
                        applier.Apply(source, dest);
                        Console.WriteLine("Complete!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred!");
                Console.WriteLine(e.ToString());
                return 1;
            }
            return 0;
        }
    }
}
