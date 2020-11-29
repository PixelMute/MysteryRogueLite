using UnityEngine;

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

        //Helpers.DrawDebugLine(center.x, center.y);
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

        painter.AutoPaintWalls();
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
        //Adjust the center so it isn't too close to any of the connection points causing weird visual things
        foreach (var connection in ConnectionPoints.Values)
        {
            if (connection.x == Bounds.Left && center.x <= Bounds.Left + 1)
            {
                center.x = Bounds.Left + 2;
            }
            if (connection.x == Bounds.Right && center.x >= Bounds.Right - 1)
            {
                center.x = Bounds.Right - 2;
            }
            if (connection.y == Bounds.Bottom && center.y <= Bounds.Bottom + 1)
            {
                center.y = Bounds.Bottom + 2;
            }
            if (connection.y == Bounds.Top && center.y >= Bounds.Top - 1)
            {
                center.y = Bounds.Top - 2;
            }
        }
        return center;
    }




}

