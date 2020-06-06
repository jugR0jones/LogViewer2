using System.Drawing;

namespace LogViewer2.DataTypes
{
    /// <summary>
    /// Configuration data used by application
    /// </summary>
    public class Configuration
    {
        #region Public Properties

        public Color HighlightColour;
        public Color ContextColour;
        public int MultiSelectLimit;
        public int NumContextLines;

        #endregion
    }
}
