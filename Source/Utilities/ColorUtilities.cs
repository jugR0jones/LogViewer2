using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer2.Utilities
{
    public static class ColorUtilities
    {

        public static bool TryParseColour(string name, out Color color)
        {
            color = Color.FromName(name);
            if (color.IsKnownColor)
            {
                return false;
            }

            return true;
        }

    }
}
