using NaturalSort.Helpers;
using NaturalSort.SortingMethods;

namespace NaturalSort.Tests
{
    public class TestModifiedSort
    {
        private const string tempDirectory = "temp";
        public static async Task TestSort(long fileSize, int? ramSize = null)
        {
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }
            var intNumbers =  fileSize / sizeof(int);
            string inputFilePath = "input.txt";
            string outputFilePath = "output.txt";
            Console.WriteLine("Generating random input file with " + intNumbers + " integers");
            RandomNumberFileGenerator.GenerateRandomInputFile(inputFilePath, intNumbers);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Sorting " + intNumbers + " integers");
            await ModifiedNaturalSort.Sort(inputFilePath, outputFilePath, ramSize);
            stopwatch.Stop();
            Console.WriteLine("Time taken: " + stopwatch.Elapsed.TotalSeconds + " seconds");

            Directory.Delete(tempDirectory, true);
        }
    }
}
