using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomPainter
{
    public Level Level { get; set; }
    public Room Room { get; set; }

    public RoomPainter(Level level, Room room)
    {
        Level = level;
        Room = room;
    }

    public void PaintFloorArea(int minX, int maxX, int minY, int maxY)
    {
        for (int i = minX; i < maxX; i++)
        {
            for (int j = minY; j < maxY; j++)
            {
                var tile = Painter.FloorTiles.PickRandom();
                Level.Terrain.SetTile(new Vector3Int(i, j, 0), tile);
            }
        }
    }

    public void PaintFloor(int x, int y)
    {
        var tile = Painter.FloorTiles.PickRandom();
        Level.Terrain.SetTile(new Vector3Int(x, y, 0), tile);
    }

    public void PaintTerrainTile(int x, int y, Tile tile)
    {
        Level.Terrain.SetTile(new Vector3Int(x, y, 0), tile);
    }

    public void PaintLeftWall(int minY, int maxY, int xValue)
    {
        for (int i = minY; i < maxY; i++)
        {
            var tile = Painter.LeftWallTiles.PickRandom();
            Level.Terrain.SetTile(new Vector3Int(xValue, i, 0), tile);
        }
    }

    public void PaintRightWall(int minY, int maxY, int xValue)
    {
        for (int i = minY; i < maxY; i++)
        {
            var tile = Painter.RightWallTiles.PickRandom();
            Level.Terrain.SetTile(new Vector3Int(xValue, i, 0), tile);
        }
    }

    public void PaintBottomWall(int minX, int maxX, int yValue)
    {
        for (int i = minX; i < maxX; i++)
        {
            var tile = Painter.BottomWallTiles.PickRandom();
            Level.Terrain.SetTile(new Vector3Int(i, yValue, 0), tile);
        }
    }

    public void PaintTopWall(int minX, int maxX, int yValue)
    {
        for (int i = minX; i < maxX; i++)
        {
            var tile = Painter.TopWallTiles.PickRandom();
            Level.Terrain.SetTile(new Vector3Int(i, yValue, 0), tile);
        }
    }

    public void PaintBottomLeftCorner(int x, int y)
    {
        var tile = Painter.BottomLeftCorner;
        Level.Terrain.SetTile(new Vector3Int(x, y, 0), tile);
    }

    public void PaintBottomRightCorner(int x, int y)
    {
        var tile = Painter.BottomRightCorner;
        Level.Terrain.SetTile(new Vector3Int(x, y, 0), tile);
    }
}

