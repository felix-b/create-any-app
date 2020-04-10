namespace LLang.Abstractions.Languages
{
    public class Location
    {
        public Location(string filePath, int line, int column)
        {
            FilePath = filePath;
            Line = line;
            Column = column;
        }

        public string FilePath { get; }
        public int Line { get; }
        public int Column { get; }
        public bool IsEmpty => FilePath == string.Empty && Line == 0 && Column == 0;
        public static Location Empty { get; } = new Location(string.Empty, 0, 0);
    }
}

