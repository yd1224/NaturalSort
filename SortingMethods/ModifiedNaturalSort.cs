namespace NaturalSort.SortingMethods
{
    /// <summary>
    /// Implements an external sort for large files of integers using a modified natural sort approach.
    /// </summary>
    public class ModifiedNaturalSort
    {
        /// <summary>
        /// Directory for storing temporary files during sorting.
        /// </summary>
        private static string tempDirectory = "temp";

        /// <summary>
        /// Maximum amount of RAM to use for sorting, in bytes.
        /// Default is 1 GB.
        /// </summary>
        static int MAX_RAM = 1024 * 1024 * 1000; // 1 GB

        /// <summary>
        /// Maximum number of integers that fit in the allocated memory.
        /// </summary>
        static int MAX_RUN_INTS = MAX_RAM / sizeof(int);

        /// <summary>
        /// Sorts a large file of integers using an external sorting algorithm.
        /// </summary>
        /// <param name="inputFilePath">Path to the input file containing unsorted integers.</param>
        /// <param name="outputFilePath">Path where the sorted file will be saved.</param>
        /// <param name="maxRam">Maximum amount of RAM to use for sorting, in bytes.</param>
        public static async Task Sort(string inputFilePath, string outputFilePath, int? maxRam = null)
        {
            // Update the maximum RAM based on user input
            MAX_RAM = maxRam ?? MAX_RAM;

            // Step 1: Create initial sorted runs
            List<string> tempFiles = CreateSortedRuns(inputFilePath);

            // Step 2: Iteratively merge runs until only one remains
            while (tempFiles.Count > 1)
            {
                List<string> mergedFiles = new List<string>();

                for (int i = 0; i < tempFiles.Count; i += 2)
                {
                    if (i + 1 < tempFiles.Count)
                    {
                        // Merge two files and add the result to the list
                        mergedFiles.Add(await MergeRunFiles(tempFiles[i], tempFiles[i + 1]));
                    }
                    else
                    {
                        // Add the last file if there is no pair
                        mergedFiles.Add(tempFiles[i]);
                    }
                }
                tempFiles = mergedFiles;
            }

            // Step 3: Copy the final sorted file to the output path
            File.Copy(tempFiles[0], outputFilePath);
        }

        /// <summary>
        /// Creates initial sorted runs from the input file.
        /// </summary>
        /// <param name="inputFilePath">Path to the input file.</param>
        /// <returns>A list of file paths containing sorted runs.</returns>
        static List<string> CreateSortedRuns(string inputFilePath)
        {
            List<string> tempFiles = new List<string>();
            List<int> currentRun = new(MAX_RUN_INTS);

            using var reader = new StreamReader(inputFilePath);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                currentRun.Add(int.Parse(line));

                // Write to a temporary file when the run reaches maximum size
                if (currentRun.Count >= MAX_RUN_INTS)
                {
                    tempFiles.Add(WriteStaticTempFile(currentRun));
                    currentRun.Clear();
                }
            }

            // Write any remaining integers to a file
            if (currentRun.Count > 0)
                tempFiles.Add(WriteStaticTempFile(currentRun));

            return tempFiles;
        }

        /// <summary>
        /// Writes a sorted run to a temporary file.
        /// </summary>
        /// <param name="currentRun">List of integers representing the current sorted run.</param>
        /// <returns>The path to the temporary file containing the sorted run.</returns>
        static string WriteStaticTempFile(List<int> currentRun)
        {
            currentRun.Sort(); // Sort the current run
            string tempFilePath = Path.Combine(tempDirectory, $"{Guid.NewGuid()}.txt");
            File.WriteAllLines(tempFilePath, currentRun.Select(x => x.ToString()));
            return tempFilePath;
        }

        /// <summary>
        /// Merges two sorted run files into a single sorted file.
        /// </summary>
        /// <param name="file1">Path to the first sorted file.</param>
        /// <param name="file2">Path to the second sorted file.</param>
        /// <returns>The path to the merged sorted file.</returns>
        static async Task<string> MergeRunFiles(string file1, string file2)
        {
            string mergedFilePath = Path.Combine(tempDirectory, $"{Guid.NewGuid()}.txt");

            using (var reader1 = new StreamReader(file1))
            using (var reader2 = new StreamReader(file2))
            using (var writer = new StreamWriter(mergedFilePath))
            {
                int? num1 = ReadNextInt(reader1);
                int? num2 = ReadNextInt(reader2);

                // Merge files line by line
                while (num1 != null || num2 != null)
                {
                    if (num1 == null || num2 != null && num2 <= num1)
                    {
                        await writer.WriteLineAsync(num2.ToString());
                        num2 = ReadNextInt(reader2);
                    }
                    else
                    {
                        await writer.WriteLineAsync(num1.ToString());
                        num1 = ReadNextInt(reader1);
                    }
                }
            }

            // Delete the original files
            File.Delete(file1);
            File.Delete(file2);

            return mergedFilePath;
        }

        /// <summary>
        /// Reads the next integer from a file.
        /// </summary>
        /// <param name="reader">StreamReader for the file.</param>
        /// <returns>The next integer, or null if the end of the file is reached.</returns>
        static int? ReadNextInt(StreamReader reader)
        {
            string? line = reader.ReadLine();
            return line != null ? int.Parse(line) : null;
        }
    }
}
