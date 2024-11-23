namespace NaturalSort.Helpers
{
    public class RandomNumberFileGenerator
    {
        public static void GenerateRandomInputFile(string filePath, long numberOfElements)
        {
            Random random = new Random();
            using var writer = new StreamWriter(filePath);

            for (int i = 0; i < numberOfElements; i++)
            {
                writer.WriteLine(random.Next(1, 100000000));
            }
        }
    }
}
