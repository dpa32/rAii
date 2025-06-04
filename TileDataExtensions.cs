using System.Collections.Generic;

public static class TileDataExtensions
{
    public static List<string> GetCompatible(this TileData tile, Directions dir)
    {
        if (tile.CompatibleTiles.TryGetValue(dir, out var list))
        {
            return list.List;
        }

        var newList = new StringListWrapper();
        tile.CompatibleTiles[dir] = newList;
        return newList.List;
    }

    public static void SetCompatible(this TileData tile, Directions dir, StringListWrapper names)
    {
        tile.CompatibleTiles[dir] = names;
    }
}
