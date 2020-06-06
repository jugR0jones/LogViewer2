namespace LogViewer2.DataTypes
{
    /// <summary>
    /// Structure of the configuration as it is saved to disk.
    /// </summary>
    public class TomlConfiguration
    {
        public string HighlightColour { get; set; }
        public string ContextColour { get; set; }
        public int MultiSelectLimit { get; set; }
        public int NumContextLines { get; set; }
    }
}
