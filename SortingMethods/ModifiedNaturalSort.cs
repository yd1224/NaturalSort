using NaturalSort.CustomReaders;

namespace NaturalSort.SortingMethods
{
    public class NaturalSort
    {
        // Directory for temporary files
        private static string tempDirectory = "temp";
        
        private static string FILE_B = Path.Combine(tempDirectory, "fileB.txt");
        private static string FILE_C = Path.Combine(tempDirectory, "fileC.txt");
        private static string FILE_A = Path.Combine(tempDirectory, "fileA.txt");
        
        private const int minRunSize = 1024*1024*1024/40;

        public static async Task Sort(string inputFilePath, string outputFilePath)
        {
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }
            
            await PreSortInputFile(inputFilePath);
            
            string currentA = FILE_A;
            string mergedFile = Path.Combine(tempDirectory, "merged.txt");
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Sorting...");
            while (true)
            {
                // Step 1: Split file A into B and C
                await SplitIntoRuns(currentA, FILE_B, FILE_C);

                // If one of the files is empty, sorting is done
                if (IsFileEmpty(FILE_C))
                {
                    File.Copy(currentA, outputFilePath, true);
                    break;
                }

                // Step 2: Merge runs from B and C into A
                await MergeRuns(FILE_B, FILE_C, mergedFile);

                // Step 3: Prepare for the next iteration
                File.Copy(mergedFile, currentA, true);
                File.Delete(mergedFile);
            }
            
            stopwatch.Stop();
            Console.WriteLine("Time taken: " + stopwatch.Elapsed.TotalSeconds + " seconds");
            
            if (Directory.Exists((tempDirectory)))
            {
                try
                {
                    Directory.Delete(tempDirectory);
                }
                catch(Exception)
                {
                    
                }
            }
        }

        private static async Task PreSortInputFile(string inputFilePath)
        {
            using var reader = new StreamReader(inputFilePath);
            await using var writer = new StreamWriter(FILE_A, append: false);
    
            List<int> numbers = new(minRunSize); // Preallocate based on minRunSize
            string? line;
    
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (int.TryParse(line, out int parsedNumber))
                {
                    numbers.Add(parsedNumber);
                }
        
                if (numbers.Count >= minRunSize)
                {
                    WriteSortedChunk(numbers, writer);
                }
            }
    
            // Write any remaining numbers
            if (numbers.Count > 0)
            {
                WriteSortedChunk(numbers, writer);
            }
        }

        private static void WriteSortedChunk(List<int> numbers, StreamWriter writer)
        {
            numbers.Sort();
            foreach (int num in numbers)
            {
                writer.WriteLine(num.ToString()); // Avoids async overhead for each line
            }
            numbers.Clear(); // Clear for reuse
        }

        private static async Task SplitIntoRuns(string inputFile, string fileB, string fileC)
        {
            using var reader = new StreamReader(inputFile);
            await using var writerB = new StreamWriter(fileB, append: false);
            await using var writerC = new StreamWriter(fileC, append: false);

            bool writeToB = true;
            List<int> currentRun = new();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                int value = int.Parse(line);

                if (currentRun.Count == 0 || value >= currentRun[^1])
                {
                    // Continue the current run
                    currentRun.Add(value);
                }
                else
                {
                    // Write the completed run to the appropriate file
                    await WriteRunToFile(currentRun, writeToB ? writerB : writerC);
                    writeToB = !writeToB; // Switch files
                    currentRun.Clear();
                    currentRun.Add(value);
                }
            }

            // Write the final run
            if (currentRun.Count > 0)
            {
                await WriteRunToFile(currentRun, writeToB ? writerB : writerC);
            }
        }

        private static async Task WriteRunToFile(List<int> run, StreamWriter writer)
        {
            foreach (int value in run)
            {
                await writer.WriteLineAsync(value.ToString());
            }
        }

        private static async Task MergeRuns(string fileB, string fileC, string outputFile)
        {
            using var customReaderB = new StreamReaderExtended();
            customReaderB.reader = new StreamReader(fileB);
            
            using var customReaderC = new StreamReaderExtended();
            customReaderC.reader = new StreamReader(fileC);
        
            await using var writer = new StreamWriter(outputFile, append:true);
        
            Queue<int> runB = await ReadNextRun(customReaderB);
            Queue<int> runC = await ReadNextRun(customReaderC);
        
            while ((runB != null || runC != null))
            {
                if (runB == null)
                {
                    await WriteRemainingFile(customReaderC, writer);
                    runC = null;
                }
                else if (runC == null)
                {
                    await WriteRemainingFile(customReaderB, writer);
                    runB = null;
                }
                else
                {
                    // Merge the two runs
                    while (runB.Count > 0 && runC.Count > 0)
                    {
                        if (runB.Peek() <= runC.Peek())
                        {
                            var value = runB.Dequeue().ToString();
                            await writer.WriteLineAsync(value);
                        }
                        else
                        {
                            var value = runC.Dequeue().ToString();
                            await writer.WriteLineAsync(value);
                        }
                    }
                    if (runB.Count == 0)
                    {
                        runB = await ReadNextRun(customReaderB);
                    }
                    if (runC.Count == 0)
                    {
                        runC = await ReadNextRun(customReaderC);
                    }
                }
            }
        
            // Write any remaining numbers in either queue
            if (runB is { Count: > 0 })
            {
                await WriteRemainingRuns(runB, writer);
            }
        
            if (runC is { Count: > 0 })
            {
                await WriteRemainingRuns(runC, writer);
            }
        }  

        private static async Task WriteRemainingRuns(Queue<int> run, StreamWriter writer)
        {
            while (run?.Count > 0)
            {
                await writer.WriteLineAsync(run.Dequeue().ToString());
            }
        }
        
        private static async Task WriteRemainingFile(StreamReaderExtended reader, StreamWriter writer)
        {
            string? line;
            while ((line = await reader.reader.ReadLineAsync()) != null)
            {
                await writer.WriteLineAsync(line);
            }
        }
        
        private static async Task<Queue<int>>? ReadNextRun(StreamReaderExtended reader)
        {
            Queue<int> run = new();
            string? line;
            int previous = -1;

            while ((line = await reader.reader.ReadLineAsync()) != null)
            {
                if(reader.lookahead != null)
                {
                    run.Enqueue(reader.lookahead.Value);
                    reader.lookahead = null;
                }
                
                int value = int.Parse(line);
                if (value < previous)
                {
                    reader.lookahead = value;
                    return run;
                }

                run.Enqueue(value);
                previous = value;
            }

            // Ensure the last run is returned if not empty
            return run.Count > 0 ? run : null;
        }


        private static bool IsFileEmpty(string filePath)
        {
            return new FileInfo(filePath).Length == 0;
        }
    }
}
