# Block File Structure (BFS)
The Block File Structure format was created to compress a large file structure in a single file, and then easily extract the file structure to the hard drive. It is used in the installation process of HydroOS.
This repository contains a program that extracts and compresses BFS files.

## How to use
Type `bfs` to get all available verbs. Type `bfs help (verb)` to get help about a specific verb.
The available verbs are:
* compress - Compresses a folder to a BFS file
* extract - Extracts a BFS file to a folder.

### Examples
Extract a file named "archive.bfs" in the C:\ drive to a folder located in "D:\ArchiveContents\"
`bfs extract -i C:\archive.bfs -o D:\ArchiveContents\`

Compress a folder located in "C:\Users\ToBeCompressed\" to a BFS file in "C:\Users\compressed.bfs"
`bfs compress -i C:\Users\ToBeCompressed\ -o C:\Users\compressed.bfs`

Compress a folder in the current directory named "target" to a file named "install.bfs"
`bfs compress -i target -o install.bfs`

## The format
A file that is supposed to be stored in a BFS file needs to have a file size less or equal to 4294967295 bytes (or 4GB). Please note that C# can behave incorrectly when handling files over 2GB.
A BFS file always starts with the ASCII letters "BFS". Then, the file records follow:
Chunk Description | Size (bytes) | Data type
----------------- | ------------ | ---------
Relative path (null terminated, ASCII) | Any | string
Size (in bytes) | 4 | uint
Compressed data | As specified above | byte[]
These file records repeat until the end of the file. All file data is compressed using LZF.

## Versions
The BFS compressor/decompressor is versioned after the date it's been compiled on.
Example:
Built on 23.06.2020 = version 20.06.23
