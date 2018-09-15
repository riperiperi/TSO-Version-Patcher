using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using FSO.Files.Utils;
using deltaq;

namespace TSOVersionPatcher
{
    public class PatchBuilder
    {
        public HashSet<string> SourceFiles; //aka before
        public HashSet<string> DestFiles; //aka after

        public HashSet<string> AddFiles;
        public HashSet<string> RemovedFiles;
        public HashSet<string> SameFiles;

        public string SourcePath;
        public string DestPath;

        public PatchBuilder(string source, string dest)
        {
            SourceFiles = new HashSet<string>();
            RecursiveDirectoryScan(source, SourceFiles, source);

            DestFiles = new HashSet<string>();
            RecursiveDirectoryScan(dest, DestFiles, dest);

            AddFiles = new HashSet<string>(DestFiles);
            AddFiles.ExceptWith(SourceFiles);

            RemovedFiles = new HashSet<string>(SourceFiles);
            RemovedFiles.ExceptWith(DestFiles);

            SameFiles = new HashSet<string>(SourceFiles);
            SameFiles.IntersectWith(DestFiles);

            SourcePath = source;
            DestPath = dest;
        }

        public void WritePatchFile(Stream stream)
        {
            using (var io = IoWriter.FromStream(stream, ByteOrder.LITTLE_ENDIAN))
            {
                io.WriteCString("TSOp", 4);
                io.WriteInt32(0); //version

                //generate patches for the files that are the same
                var patches = new List<Tuple<string, byte[]>>();
                Console.Write("Progress: ");
                var progress = 0;
                foreach (var file in SameFiles)
                {
                    Console.Write($"\rProgress: {progress++}/{SameFiles.Count}");

                    var srcDat = File.ReadAllBytes(Path.Combine(SourcePath, file));
                    var dstDat = File.ReadAllBytes(Path.Combine(DestPath, file));
                    
                    var same = srcDat.Length == dstDat.Length && srcDat.SequenceEqual(dstDat);
                    if (!same)
                    {
                        using (var outips = new MemoryStream()) {
                            BsDiff.Create(srcDat, dstDat, outips);
                            patches.Add(new Tuple<string, byte[]>(file, outips.ToArray()));
                        }
                    }
                }
                Console.WriteLine();
                Console.WriteLine($"Done generating patches ({patches.Count}). Inserting additions and deletions...");

                io.WriteCString("IPS_", 4);
                io.WriteInt32(patches.Count);
                foreach (var piff in patches)
                {
                    io.WriteVariableLengthPascalString(piff.Item1);
                    io.WriteInt32(piff.Item2.Length);
                    io.WriteBytes(piff.Item2);
                }

                io.WriteCString("ADD_", 4);
                io.WriteInt32(AddFiles.Count);
                foreach (var add in AddFiles)
                {
                    io.WriteVariableLengthPascalString(add);
                    var data = File.ReadAllBytes(Path.Combine(DestPath, add));
                    io.WriteInt32(data.Length); 
                    io.WriteBytes(data);
                }

                io.WriteCString("DEL_", 4);
                io.WriteInt32(RemovedFiles.Count);
                foreach (var del in RemovedFiles)
                {
                    io.WriteVariableLengthPascalString(del);
                }
            }
        }

        private void RecursiveDirectoryScan(string folder, HashSet<string> fileNames, string basePath)
        {
            var files = Directory.GetFiles(folder);
            foreach (var file in files)
            {
                fileNames.Add(GetRelativePath(basePath, file));
            }

            var dirs = Directory.GetDirectories(folder);
            foreach (var dir in dirs)
            {
                RecursiveDirectoryScan(dir, fileNames, basePath);
            }
        }

        private string GetRelativePath(string relativeTo, string path)
        {
            if (!(relativeTo.EndsWith("/") || relativeTo.EndsWith("\\"))) relativeTo += "/";
            var uri = new Uri(relativeTo);
            
            var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (rel.Contains(Path.DirectorySeparatorChar.ToString()) == false)
            {
                rel = $".{ Path.DirectorySeparatorChar }{ rel }";
            }
            return rel;
        }
    }
}
