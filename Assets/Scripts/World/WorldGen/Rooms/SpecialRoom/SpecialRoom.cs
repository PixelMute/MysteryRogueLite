using System;

[Serializable]
class SpecialRoom : Room
{
    public SpecialRoom() : base(new RoomInfo()
    {
        MinWidth = 5,
        MinHeight = 5,
        MaxWidth = 10,
        MaxHeight = 10,
        MinConnections = 1,
        MaxConnections = 1,
    })
    { }
}

