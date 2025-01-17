﻿using System.Text;
using NaturalSort.CustomReaders;

namespace NaturalSort.SortingMethods
{
    public class UnmodifiedNaturalSort
    {
        // Directory for temporary files
        private static string tempDirectory = "temp";
        private static int? lookAhead = null;

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

            while (true)
            {
                // Step 1: Split file A into B and C
                await SplitIntoRuns(fileA, fileB, fileC);
                
                // If one of the files is empty, sorting is done
                if (IsFileEmpty(fileC))
                {
                    File.Copy(fileB, outputFilePath, true);
                    break;
                }
                
                if (!File.Exists(fileA))
                {
                    using (File.Create(fileA)) { }
                }

                
                // Step 2: Merge runs from B and C into A
                await MergeRuns(fileB, fileC, fileA);
            }

            if (Directory.Exists((tempDirectory)))
            {
                try
                {
                    File.Delete(fileB);
                    File.Delete(fileC);
                    Directory.Delete(tempDirectory);
                }
                catch(Exception ex)
                {
                    
                }
            }
        }

        private static async Task SplitIntoRuns(string inputFile, string fileB, string fileC)
        {
            using var reader = new StreamReader(inputFile);
            using var writerB = new StreamWriter(fileB);
            using var writerC = new StreamWriter(fileC);

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
            
            File.Delete(inputFile);
        }

        private static async Task WriteRunToFile(List<int> run, StreamWriter writer)
        {
            foreach (int num in run)
            {
                await writer.WriteLineAsync(num.ToString());
            }
        }

        private static async Task MergeRuns(string fileB, string fileC, string outputFile)
        {
            using var customReaderB = new StreamReader(fileB);
            using var customReaderC = new StreamReader(fileC);
 
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
                if(lookAhead != null)
                {
                    run.Enqueue(lookAhead.Value);
                    lookAhead = null;
                }
                
                int value = int.Parse(line);
                if (value < previous)
                {
                    // End of run
                    lookAhead = value;
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

