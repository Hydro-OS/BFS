using CommandLine;
using System;
using System.IO;

namespace BFS
{
    /// <summary>
    /// The main program entry point. (CLI)
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The command-line options, when compressing.
        /// </summary>
        [Verb("compress", HelpText = "Compresses an folder to a BFS archive.")]
        private protected sealed class CompressVerb
        {
            /// <summary>
            /// The input file or folder.
            /// </summary>
            [Option('i', "input", Required = true, HelpText = "The input folder.")]
            public string Input { get; set; }

            /// <summary>
            /// The output file or folder.
            /// </summary>
            [Option('o', "output", Required = true, HelpText = "The output file.")]
            public string Output { get; set; }

            /// <summary>
            /// If set to true, the compressor will ignore the files that are listed in the ignore file. (see <see cref="BFSCompress.IgnoreFileName"/>)
            /// </summary>
            [Option('I', "useIgnoreFile", Required = false, Default = true, 
                HelpText = "If set to true, the compressor will ignore the files that are listed in the " + BFSCompress.IgnoreFileName + " file.")]
            public bool UseIgnoreFile { get; set; }
        }
        /// <summary>
        /// The command-line options, when extracting.
        /// </summary>
        [Verb("extract", HelpText = "Extracts a BFS archive to the specified folder.")]
        private protected sealed class ExtractVerb
        {
            /// <summary>
            /// The input file or folder.
            /// </summary>
            [Option('i', "input", Required = true, HelpText = "The input file.")]
            public string Input { get; set; }

            /// <summary>
            /// The output file or folder.
            /// </summary>
            [Option('o', "output", Required = true, HelpText = "The output folder.")]
            public string Output { get; set; }
        }

        /// <summary>
        /// The main program entry-point.
        /// </summary>
        /// <param name="args">The arguments provided to the program.</param>
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CompressVerb, ExtractVerb>(args)
                    .WithParsed<CompressVerb>(Compress)
                    .WithParsed<ExtractVerb>(Extract);
        }

        /// <summary>
        /// Extracts a BFS file.
        /// </summary>
        /// <param name="opts">The options provided to the program</param>
        private static void Extract(ExtractVerb opts)
        {
            if (!File.Exists(opts.Input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: The input path is not a file or doesn't exist.");
                Console.ResetColor();
                return;
            }

            Directory.CreateDirectory(opts.Output);

            Console.WriteLine($"Reading archive {Path.GetFileName(opts.Input)}...");
            byte[] bfsFile = File.ReadAllBytes(opts.Input);
            BFSCompress.DecompressBFS(bfsFile, opts.Output);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done.");
            Console.ResetColor();
        }

        /// <summary>
        /// Compresses a BFS file.
        /// </summary>
        /// <param name="opts">The options provided to the program.</param>
        private static void Compress(CompressVerb opts)
        {
            if (!Directory.Exists(opts.Input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: The input path is not a folder or doesn't exist.");
                Console.ResetColor();
                return;
            }

            byte[] bfs = BFSCompress.CompressBFS(opts.Input, opts.UseIgnoreFile);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Compressed to BFS with file size of " + bfs.Length + "b.");
            Console.ResetColor();

            Console.WriteLine("Writing archive to file \"" + opts.Output + "\".");
            File.WriteAllBytes(opts.Output, bfs);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done.");
            Console.ResetColor();
        }
    }
}
