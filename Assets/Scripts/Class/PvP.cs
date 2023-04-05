using System.Collections;
using System.Collections.Generic;

public class PvP
{
    private static int landSpawnPlayer;
    public static void SetLand(int land)
    {
        landSpawnPlayer = land;
    }
    public static int GetLandSpawnPlayer()
    {
        return landSpawnPlayer;
    }

    private static string roomId;
    public static void SetRoom(string room)
    {
        roomId = room;
    }
    public static string GetRoom()
    {
        return roomId;
    }
}
