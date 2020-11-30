using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomPainter
{
    private enum TypeOfTile { Left, Right, Top, Bottom, None, Floor }
    public Level Level { get; set; }
    public Room Room { get; set; }

    public RoomPainter(Level level, Room room)
    {
        Level = level;
        Room = room;
    }

    public void PaintFloorArea(RogueRect rect)
    {
        PaintFloorArea(rect.Left, rect.Right, rect.Bottom, rect.Top);
    }
    public void PaintFloorArea(int minX, int maxX, int minY, int maxY)
    {
        for (int i = minX; i <= maxX; i++)
        {
            for (int j = minY; j <= maxY; j++)
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

    public void AutoPaintWalls()
    {
        for (int i = Room.Bounds.Left; i < Room.Bounds.Right; i++)
        {
            for (int j = Room.Bounds.Bottom; j < Room.Bounds.Top; j++)
            {
                if (!IsFloor(i, j))
                {
                    var tile = GetWallTile(i, j);
                    if (tile != null)
                    {
                        Level.Terrain.SetTile(new Vector3Int(i, j, 0), tile);
                    }
                }
            }
        }
    }

    public void AutoDecorate()
    {
        for (int i = Room.Bounds.Left; i < Room.Bounds.Right; i++)
        {
            for (int j = Room.Bounds.Bottom; j < Room.Bounds.Top; j++)
            {
                var type = GetTypeOfWall(i, j);
                if (type == TypeOfTile.Top)
                {
                    if (Random.RandBool(.125f))
                    {
                        Level.DecorativeTileMap.SpawnTorch(i, j);
                    }
                    else if (Random.RandBool(.125f))
                    {
                        Level.Decorations.SetTile(new Vector3Int(i, j, 0), Painter.GetRandomTopWallDecoration());
                    }
                }
                if (type == TypeOfTile.Left && Random.RandBool(.125f) && IsFloor(i + 1, j))
                {
                    Level.DecorativeTileMap.SpawnSideTorch(i + 1, j, true);
                }
                if (type == TypeOfTile.Right && Random.RandBool(.125f) && IsFloor(i - 1, j))
                {
                    Level.DecorativeTileMap.SpawnSideTorch(i - 1, j, false);
                }
                if (type == TypeOfTile.Floor && Random.RandBool(.02f))
                {
                    Level.Decorations.SetTile(new Vector3Int(i, j, 0), Painter.GetRandomFloorDecoration());
                }
            }
        }
    }

    private Tile GetWallTile(int x, int y)
    {
        //This is terrible but no super easy way of doing it right now
        int maxX = Room.Bounds.Right;
        int maxY = Room.Bounds.Top;
        if (IsFloor(x, y - 1))
        {
            return Painter.TopWallTiles.PickRandom();
        }
        if (IsFloor(x + 1, y) && IsFloor(x, y + 1))
        {
            return Painter.TurnRightTiles.PickRandom();
        }
        if (IsFloor(x - 1, y) && IsFloor(x, y + 1))
        {
            return Painter.TurnLeftTiles.PickRandom();
        }
        if (IsFloor(x - 1, y))
        {
            return Painter.RightWallTiles.PickRandom();
        }
        if (IsFloor(x + 1, y))
        {
            return Painter.LeftWallTiles.PickRandom();
        }
        if (IsFloor(x, y + 1))
        {
            return Painter.BottomWallTiles.PickRandom();
        }
        if (IsFloor(x + 1, y - 1))
        {
            Room.Corners.Add(new Vector2Int(x, y));
            return Painter.LeftWallTiles.PickRandom();
        }
        if (IsFloor(x - 1, y - 1))
        {
            Room.Corners.Add(new Vector2Int(x, y));
            return Painter.RightWallTiles.PickRandom();
        }
        if (IsFloor(x + 1, y + 1))
        {
            Room.Corners.Add(new Vector2Int(x, y));
            return Painter.BottomLeftCorner;
        }
        if (IsFloor(x - 1, y + 1))
        {
            Room.Corners.Add(new Vector2Int(x, y));
            return Painter.BottomRightCorner;
        }

        return null;
    }

    public bool IsFloor(int x, int y)
    {
        var tile = Level.Terrain.GetTile(new Vector3Int(x, y, 0));
        return tile != null && tile.name.Contains("Floor");
    }

    private TypeOfTile GetTypeOfWall(int x, int y)
    {
        var tile = Level.Terrain.GetTile(new Vector3Int(x, y, 0));
        if (tile == null)
        {
            return TypeOfTile.None;
        }
        if (tile.name.Contains("Floor"))
        {
            return TypeOfTile.Floor;
        }
        if (tile.name.Contains("BottomWall"))
        {
            return TypeOfTile.Bottom;
        }
        if (tile.name.Contains("TopWall"))
        {
            return TypeOfTile.Top;
        }
        if (tile.name.Contains("LeftWall"))
        {
            return TypeOfTile.Left;
        }
        if (tile.name.Contains("RightWall"))
        {
            return TypeOfTile.Right;
        }
        return TypeOfTile.None;
    }

    public void PlaceCobwebs(int x, int y, bool right)
    {
        Level.Decorations.SetTile(new Vector3Int(x, y, 0), Painter.Cobwebs);
        if (right)
        {
            Level.Decorations.SetTransformMatrix(new Vector3Int(x, y, 0), Matrix4x4.Scale(new Vector3(-1, 1, 1)));
        }
    }
}

