class ConnectionRoom : Room
{
    public static RoomInfo ConnectionRoomInfo = new RoomInfo()
    {
        MaxConnections = 16,
        MinWidth = 3,
        MaxWidth = 10,
        MinHeight = 3,
        MaxHeight = 10,
        RoomType = RoomType.CONNECTION,
        MinConnections = 2,
    };

    public ConnectionRoom() : base(ConnectionRoomInfo) { }
}

