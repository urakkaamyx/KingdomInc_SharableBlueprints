using System;

namespace SharableBlueprints
{
    public readonly partial struct SelectionBox
    {
        public bool EnclosesContainer(int x, int y, int width, int height)
        {
            // Containers have decorative borders. Allow a small margin without
            // capturing a much larger ancestor or a container merely crossed by the box.
            const int borderTolerance = 32; // Half of the game's 64-unit placement grid.
            return HasArea && width > 0 && height > 0
                && x >= (long)Left - borderTolerance && y <= (long)Top + borderTolerance
                && (long)x + width <= (long)Right + borderTolerance
                && (long)y - height >= (long)Bottom - borderTolerance;
        }
    }
}
