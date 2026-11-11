using System;

namespace SharableBlueprints
{
    public readonly partial struct SelectionBox
    {
        public SelectionBox IncludeFrame(int x, int y, int width, int height)
        {
            return new SelectionBox(Math.Min(Left, x), Math.Min(Bottom, checked(y - height)),
                Math.Max(Right, checked(x + width)), Math.Max(Top, y));
        }
    }
}
