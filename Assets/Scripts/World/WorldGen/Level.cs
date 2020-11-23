using System.Collections.Generic;


public class Level
{
    public List<Room> Rooms { get; private set; }
    public Entrance Entrance { get; private set; }
    public Exit Exit { get; private set; }



    public void Build()
    {
        var rooms = InitRooms();
        var builder = new Builder();
        do
        {
            foreach (var room in rooms)
            {
                room.Reset();
            }
        } while (!builder.Build(rooms));
        Rooms = builder.Rooms;
    }

    private List<Room> InitRooms()
    {
        var rooms = new List<Room>();
        var entrance = new Entrance();
        var exit = new Exit();
        rooms.Add(entrance);
        rooms.Add(exit);

        for (int i = 0; i < 4; i++)
        {
            rooms.Add(new StandardRoom());
        }
        return rooms;
    }
}

