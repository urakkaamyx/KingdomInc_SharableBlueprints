using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LightJson;

namespace SharableBlueprints
{
    public static partial class BlueprintCompactCode
    {
        static string Text(BinaryReader reader)
        {
            uint length = Var(reader);
            if (length > Max || length > reader.BaseStream.Length - reader.BaseStream.Position) throw new InvalidDataException("Invalid compact blueprint text length.");
            return Utf8.GetString(reader.ReadBytes((int)length));
        }
    }
}
