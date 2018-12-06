
using ThreadedPathfinding;
using UnityEngine;

public class CustomTileProvider : TileProvider
{
    private Map map;

    public CustomTileProvider(Map map) : base(map.Width, map.Height)
    {
        this.map = map;
    }

    public override bool IsTileWalkable(int x, int y)
    {
        bool solid = map.Tiles[x, y];
        return solid == false;
    }
}
