using NaturalSort.CustomReaders;

namespace NaturalSort.SortingMethods
{
    public class NaturalSort
    {
        // Directory for temporary files
        private static string tempDirectory = "temp";

        public static async Task Sort(string inputFilePath, string outputFilePath)
        {
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            string fileB = Path.Combine(tempDirectory, "fileB.txt");
            string fileC = Path.Combine(tempDirectory, "fileC.txt");
            string fileA = Path.Combine(tempDirectory, "fileA.txt");
            
            File.Copy(inputFilePath, fileA, true);
            string currentA = fileA;
            string mergedFile = Path.Combine(tempDirectory, "merged.txt");

            while (true)
            {
                // Step 1: Split file A into B and C
                SplitIntoRuns(currentA, fileB, fileC);

                // If one of the files is empty, sorting is done
                if (IsFileEmpty(fileC))
                {
                    File.Copy(currentA, outputFilePath, true);
                    break;
                }

                // Step 2: Merge runs from B and C into A
                await MergeRuns(fileB, fileC, mergedFile);

                // Step 3: Prepare for the next iteration
                File.Copy(mergedFile, currentA, true);
                File.Delete(mergedFile);
            }

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

        private static void SplitIntoRuns(string inputFile, string fileB, string fileC)
        {
            using var reader = new StreamReader(inputFile);
            using var writerB = new StreamWriter(fileB);
            using var writerC = new StreamWriter(fileC);

            bool writeToB = true;
            List<int> currentRun = new();

            string? line;
            while ((line = reader.ReadLine()) != null)
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
                    WriteRunToFile(currentRun, writeToB ? writerB : writerC);
                    writeToB = !writeToB; // Switch files
                    currentRun.Clear();
                    currentRun.Add(value);
                }
            }

            // Write the final run
            if (currentRun.Count > 0)
            {
                WriteRunToFile(currentRun, writeToB ? writerB : writerC);
            }
        }

        private static void WriteRunToFile(List<int> run, StreamWriter writer)
        {
            foreach (int num in run)
            {
                writer.WriteLine(num);
            }
        }

        private static async Task MergeRuns(string fileB, string fileC, string outputFile)
        {
            using var customReaderB = new StreamReaderExtended();
            customReaderB.reader = new StreamReader(fileB);
            
            using var customReaderC = new StreamReaderExtended();
            customReaderC.reader = new StreamReader(fileC);
 
            await using var writer = new StreamWriter(outputFile);

            Queue<int> runB = ReadNextRun(customReaderB);
            Queue<int> runC = ReadNextRun(customReaderC);

            while ((runB != null || runC != null))
            {
                if (runB == null)
                {
                    await WriteRemainingRuns(runC, writer);
                    runC = ReadNextRun(customReaderC);
                }
                else if (runC == null)
                {
                    await WriteRemainingRuns(runB, writer);
                    runB = ReadNextRun(customReaderB);
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
                        runB = ReadNextRun(customReaderB);
                    }
                    if (runC.Count == 0)
                    {
                        runC = ReadNextRun(customReaderC);
                    }
                }
            }

            // Write any remaining numbers in either queue
            if (runB != null && runB.Count > 0)
            {
                await WriteRemainingRuns(runB, writer);
            }

            if (runC != null && runC.Count > 0)
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

        private static Queue<int>? ReadNextRun(StreamReader reader)
        {
            Queue<int> run = new();
            string? line;
            int previous = -1;

            while ((line = reader.ReadLine()) != null)
            {
                
                int value = int.Parse(line);
                if (value < previous)
                {
                    // End of run
                    run.Enqueue(value);
                    return run;
                }

                run.Enqueue(value);
                previous = value;
            }

            // Ensure the last run is returned if not empty
            return run.Count > 0 ? run : null;
        }
        
        private static Queue<int>? ReadNextRun(StreamReaderExtended reader)
        {
            Queue<int> run = new();
            string? line;
            int previous = -1;

            while ((line = reader.reader.ReadLine()) != null)
            {
                if(reader.lookahead != null)
                {
                    run.Enqueue(reader.lookahead.Value);
                    reader.lookahead = null;
                }
                
                int value = int.Parse(line);
                if (value < previous)
                {
                    // End of run 361 609 789 561 654 439
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

