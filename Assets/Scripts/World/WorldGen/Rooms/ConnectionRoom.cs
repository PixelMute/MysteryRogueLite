using UnityEngine;
using UnityEngine.Tilemaps;

class ConnectionRoom : Room
{
    public static RoomInfo ConnectionRoomInfo = new RoomInfo()
    {
        MaxConnections = 16,
        MinWidth = 4,
        MaxWidth = 10,
        MinHeight = 4,
        MaxHeight = 10,
        RoomType = RoomType.CONNECTION,
        MinConnections = 2,
    };

    public ConnectionRoom() : base(ConnectionRoomInfo) { }

    public override void Paint(Level level)
    {
        var painter = new RoomPainter(level, this);
        var center = GetCenterPoint();

        Helpers.DrawDebugLine(center.x, center.y);
        foreach (var door in ConnectionPoints.Values)
        {

            var start = new Vector2Int(door.x, door.y);
            if (start.y == Bounds.Top)
                start.y--;
            else if (start.x == Bounds.Right)
                start.x--;

            int rightShift = (int)center.x - start.x;
            int downShift = (int)center.y - start.y;

            Vector2Int mid;
            Vector2Int end;

            //always goes inward first
            if (door.x == Bounds.Left || door.x == Bounds.Right)
            {
                mid = new Vector2Int(start.x + rightShift, start.y);
                end = new Vector2Int(mid.x, mid.y + downShift);

            }
            else
            {
                mid = new Vector2Int(start.x, start.y + downShift);
                end = new Vector2Int(mid.x + rightShift, mid.y);

            }

            var firstHalf = new RogueRect(start, mid);
            var secondHalf = new RogueRect(mid, end);
            painter.PaintFloorArea(firstHalf);
            painter.PaintFloorArea(secondHalf);
        }

        for (int i = Bounds.Left; i < Bounds.Right; i++)
        {
            for (int j = Bounds.Bottom; j < Bounds.Top; j++)
            {
                if (!IsFloor(i, j, level))
                {
                    var tile = GetWallTile(i, j, level);
                    if (tile != null)
                    {
                        level.Terrain.SetTile(new Vector3Int(i, j, 0), tile);
                    }
                }
            }
        }
    }

    private Vector2Int GetCenterPoint()
    {
        var center = new Vector2Int();
        foreach (var door in ConnectionPoints.Values)
        {
            center.x += door.x;
            center.y += door.y;
        }
        center.x /= ConnectionPoints.Count;
        center.y /= ConnectionPoints.Count;
        return center;
    }

    private Tile GetWallTile(int x, int y, Level level)
    {
        //This is terrible but no super easy way of doing it right now
        int maxX = Bounds.Right;
        int maxY = Bounds.Top;
        if (IsFloor(x, y - 1, level))
        {
            return Painter.TopWallTiles.PickRandom();
        }
        if (IsFloor(x + 1, y, level) && IsFloor(x, y + 1, level))
        {
            return Painter.TurnRightTiles.PickRandom();
        }
        if (IsFloor(x - 1, y, level) && IsFloor(x, y + 1, level))
        {
            return Painter.TurnLeftTiles.PickRandom();
        }
        if (IsFloor(x - 1, y, level))
        {
            return Painter.RightWallTiles.PickRandom();
        }
        if (IsFloor(x + 1, y, level))
        {
            return Painter.LeftWallTiles.PickRandom();
        }
        if (IsFloor(x, y + 1, level))
        {
            return Painter.BottomWallTiles.PickRandom();
        }
        if (IsFloor(x + 1, y - 1, level))
        {
            Corners.Add(new Vector2Int(x, y));
            return Painter.LeftWallTiles.PickRandom();
        }
        if (IsFloor(x - 1, y - 1, level))
        {
            Corners.Add(new Vector2Int(x, y));
            return Painter.RightWallTiles.PickRandom();
        }
        if (IsFloor(x + 1, y + 1, level))
        {
            Corners.Add(new Vector2Int(x, y));
            return Painter.BottomLeftCorner;
        }
        if (IsFloor(x - 1, y + 1, level))
        {
            Corners.Add(new Vector2Int(x, y));
            return Painter.BottomRightCorner;
        }

        return null;
    }

    private bool IsFloor(int x, int y, Level level)
    {
        var tile = level.Terrain.GetTile(new Vector3Int(x, y, 0));
        return tile != null && tile.name.Contains("Floor");
    }
}

