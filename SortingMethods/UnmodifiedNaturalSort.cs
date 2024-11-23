namespace NaturalSort.SortingMethods
{
    /// <summary>
    /// Provides functionality for sorting a list of integers from an input file 
    /// using a natural sort method that creates sorted runs and merges them.
    /// </summary>
    public class UnmodifiedNaturalSort
    {
        /// <summary>
        /// Sorts the integers in the input file and writes the sorted list to the output file.
        /// </summary>
        /// <param name="inputFilePath">The file path of the input file containing the list of integers.</param>
        /// <param name="outputFilePath">The file path where the sorted integers will be written.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task Sort(string inputFilePath, string outputFilePath)
        {
            var fileContent = await ReadFileAsync(inputFilePath);
            var sortedRuns = CreateSortedRuns(fileContent);
            var fullySortedRun = MergeSortedRuns(sortedRuns);
            await WriteFileAsync(outputFilePath, fullySortedRun);
        }

        /// <summary>
        /// Reads the content from the specified input file asynchronously and returns a list of integers.
        /// </summary>
        /// <param name="inputFilePath">The path of the input file to read from.</param>
        /// <returns>A task representing the asynchronous operation. The result contains a list of integers.</returns>
        static async Task<List<int>> ReadFileAsync(string inputFilePath)
        {
            List<int> fileContent = new List<int>();
            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    fileContent.Add(int.Parse(line));
                }
            }
            return fileContent;
        }

        /// <summary>
        /// Writes the sorted list of integers to the specified output file asynchronously.
        /// </summary>
        /// <param name="outputFilePath">The path of the output file where the sorted content will be written.</param>
        /// <param name="fileContent">The sorted list of integers to write.</param>
        /// <returns>A task representing the asynchronous write operation.</returns>
        static async Task WriteFileAsync(string outputFilePath, List<int> fileContent)
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (var number in fileContent)
                {
                    await writer.WriteLineAsync(number.ToString());
                }
            }
        }

        /// <summary>
        /// Creates sorted runs from the original list of integers. A run is a sequence of consecutive integers
        /// that is sorted individually before being merged with other sorted runs.
        /// </summary>
        /// <param name="originalFileContent">The list of integers to be split into sorted runs.</param>
        /// <returns>A list of sorted runs (each run is a sorted list of integers).</returns>
        static List<List<int>> CreateSortedRuns(List<int> originalFileContent)
        {
            List<List<int>> sortedRuns = new List<List<int>>();
            List<int> currentRun = new List<int>();
            for (int i = 0; i < originalFileContent.Count; i++)
            {
                currentRun.Add(originalFileContent[i]);
                if (i == originalFileContent.Count - 1 || originalFileContent[i] > originalFileContent[i + 1])
                {
                    currentRun.Sort();
                    sortedRuns.Add(currentRun);
                    currentRun = new List<int>();
                }
            }
            return sortedRuns;
        }

        /// <summary>
        /// Merges a list of sorted runs into a single sorted list by repeatedly merging pairs of runs.
        /// </summary>
        /// <param name="sortedRuns">A list of sorted runs to be merged.</param>
        /// <returns>A single sorted list of integers obtained from merging all sorted runs.</returns>
        static List<int> MergeSortedRuns(List<List<int>> sortedRuns)
        {
            while (sortedRuns.Count > 1)
            {
                List<List<int>> mergedRuns = new List<List<int>>();
                for (int i = 0; i < sortedRuns.Count; i += 2)
                {
                    if (i + 1 < sortedRuns.Count)
                    {
                        mergedRuns.Add(MergeTwoRuns(sortedRuns[i], sortedRuns[i + 1]));
                    }
                    else
                    {
                        // Add the leftover list as-is.
                        mergedRuns.Add(sortedRuns[i]);
                    }
                }
                sortedRuns = mergedRuns;
            }
            // Return the single merged list
            return sortedRuns[0];
        }

        /// <summary>
        /// Merges two sorted runs into one sorted run by comparing elements from both lists.
        /// </summary>
        /// <param name="run1">The first sorted run to be merged.</param>
        /// <param name="run2">The second sorted run to be merged.</param>
        /// <returns>A sorted list containing the elements of both input runs.</returns>
        static List<int> MergeTwoRuns(List<int> run1, List<int> run2)
        {
            List<int> mergedRun = new List<int>();
            int i = 0, j = 0;
            while (i < run1.Count && j < run2.Count)
            {
                if (run1[i] < run2[j])
                {
                    mergedRun.Add(run1[i]);
                    i++;
                }
                else
                {
                    mergedRun.Add(run2[j]);
                    j++;
                }
            }
            while (i < run1.Count)
            {
                mergedRun.Add(run1[i]);
                i++;
            }
            while (j < run2.Count)
            {
                mergedRun.Add(run2[j]);
                j++;
            }
            return mergedRun;
        }
    }
}
