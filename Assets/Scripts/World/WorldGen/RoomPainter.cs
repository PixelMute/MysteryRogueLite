using UnityEngine.Tilemaps;

public abstract class RoomPainter
{
    public Room Room { get; set; }

    public abstract void PaintRoom(Tilemap terrain);

    protected void PaintBottomRow(Tilemap terrain)
    {

    }
}

public class EmptyRoomPainter : RoomPainter
{
    public override void PaintRoom(Tilemap terrain)
    {

    }
}

