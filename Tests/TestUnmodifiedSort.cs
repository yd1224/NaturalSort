
using NaturalSort.Helpers;
using NaturalSort.SortingMethods;

namespace NaturalSort.Tests
{
    public class TestUnmodifiedSort
    {
        public static async Task TestSort(int totalIntegers)
        {
            string inputFilePath = "input.txt";
            string outputFilePath = "output.txt";
            RandomNumberFileGenerator.GenerateRandomInputFile(inputFilePath, totalIntegers);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await UnmodifiedNaturalSort.Sort(inputFilePath, outputFilePath);
            stopwatch.Stop();
            Console.WriteLine("Time taken: " + stopwatch.Elapsed.TotalSeconds + " seconds");
        }
    }
}
