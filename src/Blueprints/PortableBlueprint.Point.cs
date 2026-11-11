using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightJson;

namespace SharableBlueprints
{
    public sealed partial class PortableBlueprint
    {
        static void Point(JsonValue point)
        {
            if (!point.IsJsonArray || point.AsJsonArray.Count != 2 || !point[0].IsInteger || !point[1].IsInteger
                || Math.Abs(point[0].AsNumber) > 10000000 || Math.Abs(point[1].AsNumber) > 10000000)
                throw new InvalidDataException("Invalid blueprint coordinates.");
        }
    }
}
