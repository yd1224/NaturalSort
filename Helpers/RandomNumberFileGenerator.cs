namespace NaturalSort.Helpers
{
    public class RandomNumberFileGenerator
    {
        public static void GenerateRandomInputFile(string filePath, long numberOfElements)
        {
            Random random = new Random();
            using var writer = new StreamWriter(filePath);
            // List<int> numbers = [361,609,789,561,654,122,131,585,419,439];
            // foreach (var number in numbers)
            // {
            //     writer.WriteLine(number);
            // }

            for (int i = 0; i < numberOfElements; i++)
            {
                writer.WriteLine(random.Next(1, 1000));
            }
        }
    }
}
