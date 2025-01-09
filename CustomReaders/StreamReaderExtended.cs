namespace NaturalSort.CustomReaders;

public class StreamReaderExtended: IDisposable
{
    public int? lookahead = null;
    public StreamReader? reader;
    
    public void Dispose()
    {
        reader?.Dispose();
    }
    
}