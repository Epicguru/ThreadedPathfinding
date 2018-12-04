using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class TileProvider
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public TileProvider(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }

    public virtual bool TileInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public abstract bool IsTileWalkable(int x, int y);
}
