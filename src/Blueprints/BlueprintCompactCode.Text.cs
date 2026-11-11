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
        static void Text(BinaryWriter writer, string text)
        {
            byte[] bytes = Utf8.GetBytes(text); Var(writer, (uint)bytes.Length); writer.Write(bytes);
        }
    }
}
