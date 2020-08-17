// Comment this line if not using the Command Line Interface
#define CLI

#if CLI
using ShellProgressBar;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Main BFS archiver.
/// </summary>
public static class BFSCompress
{
    /// <summary>
    /// The null-terminator, that ends all strings in the file.
    /// </summary>
    const byte NullTerminator = 0x00;

    /// <summary>
    /// The ignore-file.
    /// </summary>
    public const string IgnoreFileName = "BFS_IGNORE";

    /// <summary>
    /// Enumerates through all files in a folder, and creates a BFS archive of it.
    /// </summary>
    /// <param name="path">The target path.</param>
    /// <param name="useIgnoreFile">If set to true, the compressor will use the ignore list file in the target directory.</param>
    /// <returns>The BFS archive.</returns>
    public static byte[] CompressBFS(string path, bool useIgnoreFile = true) 
    {
        string[] filesToIgnore;

        string bfsIgnorePath = Path.Combine(path, IgnoreFileName);

        if (useIgnoreFile && File.Exists(bfsIgnorePath))
            filesToIgnore = File.ReadAllLines(bfsIgnorePath);
        else filesToIgnore = Array.Empty<string>();

        // This will contain the BFS data
        List<byte> bfs = new List<byte>
        {
            // Signature:
            66, 70, 83 // "BFS" in ASCII
        };

        // Get all files to compress
        IEnumerable<string> filesToCompress = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

#if CLI
        using (ProgressBar progressBar = new ProgressBar(filesToCompress.Count(), "Compressing to BFS..."))
        {
#endif
            // Enumerate through all files
            foreach (string file in filesToCompress)
            {
                if (filesToIgnore.Contains(file))
                    continue; // Skip

                // Get the relative path of the current file
                string filePath = Path.GetRelativePath(path, file);

#if CLI
                progressBar.Message = "Compressing file " + filePath + "...";
#endif

                bfs.AddRange(Encoding.ASCII.GetBytes(filePath)); // Add file path
                bfs.Add(NullTerminator); // Null-terminate

                // Compress
                byte[] compressedData = CLZF2.Compress(File.ReadAllBytes(file));

                // Add the data length
                bfs.AddRange(BitConverter.GetBytes((uint)compressedData.LongLength));

                // Add actual data
                bfs.AddRange(compressedData);

#if CLI
                // Report progress
                progressBar.Tick();
#endif
            }
#if CLI
        }
#endif

        // Return the ready BFS file as an byte array
        return bfs.ToArray();
    }
        
    /// <summary>
    /// Decompresses a BFS archive.
    /// </summary>
    /// <param name="bfs">The BFS archive.</param>
    /// <param name="output">The output folder.</param>
    public static void DecompressBFS(byte[] bfs, string output)
    {
        if (bfs == null) throw new ArgumentNullException(nameof(bfs));

        byte[] signature = { 66, 70, 83 }; // "BFS" in ASCII
            
        // Check signature
        for (int i = 0; i < signature.Length; i++)
        {
            if (bfs[i] != signature[i]) throw new ArgumentException($"Invalid signature in the specified file. Expected byte {(char)signature[i]}, but got {(char)bfs[i]}", nameof(bfs));
        }

        // The BFS file without the signature
        IEnumerable<byte> bfsFile = bfs.Skip(signature.Length);

#if CLI
        using ProgressBar progressBar = new ProgressBar(bfsFile.Count(), "Decompressing BFS archive...");
#endif

        for (int currentByte = 0; currentByte < bfsFile.Count(); currentByte++)
        {
            string relativePath = "";

            // Read until we hit a null-terminator
            while (true)
            {
                // The byte representation of the current character
                byte charByte = bfsFile.ElementAt(currentByte);

                // Skip 1 byte that we've just read
#if CLI
                AdvanceCounter(ref currentByte, 1, progressBar);
#else
                currentByte++;
#endif

                if (charByte == NullTerminator) break; // Stop loop if the character is a null terminator

                // Add the character to the path
                relativePath += (char)charByte;
            }

            // Get size of the data
            uint size = BitConverter.ToUInt32(bfsFile.Skip(currentByte).Take( sizeof(uint) ).ToArray());

#if CLI
            // Skip the uint we've just read
            AdvanceCounter(ref currentByte, sizeof(uint), progressBar);

            progressBar.Message = "Decompressing file " + relativePath + "...";
#else
            // Skip the uint we've just read
            currentByte += sizeof(uint);
#endif

            // Get compressed data
            byte[] data = bfsFile.Skip(currentByte).Take( (int) size ).ToArray();

            // Uncompress the data
            byte[] uncompressedData = CLZF2.Decompress(data);

#if CLI
            // Skip the data we've read (we are subtracting 1, to make it zero-based)
            AdvanceCounter(ref currentByte, data.Length - 1, progressBar);
#else
            // Skip the data we've read (we are subtracting 1, to make it zero-based)
            currentByte += data.Length - 1;
#endif

            #region Write actual file

#if CLI
            progressBar.Message = "Writing file " + relativePath + "...";
#endif

            // Get target path (output + relative path)
            string targetPath = Path.Join(output, relativePath);

            // Ensure that the target directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            // Write the byte array to the target path
            File.WriteAllBytes(targetPath, uncompressedData);

            #endregion

#if CLI
            progressBar.Tick();
#endif
        }

    }

#if CLI
    /// <summary>
    /// Advances a counter, while updating a progress bar at the same time (if using the CLI).
    /// </summary>
    /// <param name="counter">The target counter.</param>
    /// <param name="by">How much should we advance the counter by?</param>
    /// <param name="progressBar">The target progress bar.</param>
    private static void AdvanceCounter(ref int counter, int by, ProgressBar progressBar)
    {
        counter += by;        // Advance counter
        progressBar.Tick(by); // Report progress
    }
#endif
}

