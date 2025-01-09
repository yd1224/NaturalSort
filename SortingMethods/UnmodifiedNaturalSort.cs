namespace NaturalSort.SortingMethods
{
    public class UnmodifiedNaturalSort
    {
        public static async Task Sort(string inputFilePath, string outputFilePath)
        {
            var fileContent = await ReadFileAsync(inputFilePath);
            var sortedRuns = CreateSortedRuns(fileContent);
            var fullySortedRun = MergeSortedRuns(sortedRuns);
            await WriteFileAsync(outputFilePath, fullySortedRun);
        }
        
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
        
        static List<List<int>> CreateSortedRuns(List<int> originalFileContent)
        {
            List<List<int>> sortedRuns = new List<List<int>>();
            List<int> currentRun = new List<int>();
            for (int i = 0; i < originalFileContent.Count; i++)
            {
                currentRun.Add(originalFileContent[i]);
                if (i == originalFileContent.Count - 1 || originalFileContent[i] > originalFileContent[i + 1])
                {
                    sortedRuns.Add(currentRun);
                    currentRun = new List<int>();
                }
            }
            return sortedRuns;
        }
        
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
