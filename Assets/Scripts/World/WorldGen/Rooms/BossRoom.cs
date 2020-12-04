class BossRoom : Room
{
    public BossRoom() : base(new RoomInfo()
    {
        MaxConnections = 2,
        MinWidth = 15,
        MaxWidth = 15,
        MinHeight = 15,
        MaxHeight = 15,
        RoomType = RoomType.BOSS,
    })
    { }
}

