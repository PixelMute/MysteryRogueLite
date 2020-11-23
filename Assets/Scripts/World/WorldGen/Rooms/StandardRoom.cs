public class StandardRoom : Room
{
    public static RoomInfo StandardRoomInfo = new RoomInfo()
    {
        MaxConnections = 16,
        MinWidth = 4,
        MaxWidth = 10,
        MinHeight = 4,
        MaxHeight = 10,
        RoomType = RoomType.STANDARD,
    };

    public StandardRoom() : base(StandardRoomInfo) { }
}

