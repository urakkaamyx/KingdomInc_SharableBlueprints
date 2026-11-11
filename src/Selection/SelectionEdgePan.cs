using System;
using System.Collections.Generic;
using System.Linq;

namespace SharableBlueprints
{
    public static partial class SelectionEdgePan
    {
        // Screen coordinates increase upward, like the native camera's Move method.
        public static double Axis(double position, double extent, double margin)
        {
            if (double.IsNaN(position) || double.IsNaN(extent) || double.IsNaN(margin)
                || double.IsInfinity(position) || double.IsInfinity(extent) || double.IsInfinity(margin)
                || extent <= 0 || margin <= 0 || position < 0 || position > extent) return 0;
            margin = Math.Min(margin, extent / 2);
            if (position < margin) return -(margin - position) / margin;
            if (position > extent - margin) return (position - (extent - margin)) / margin;
            return 0;
        }
    }
}
