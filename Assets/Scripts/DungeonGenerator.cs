using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    List<RectInt> rooms;
    RectInt room;

    private void Start()
    {
        room = new RectInt(Vector2Int.zero, new Vector2Int(100, 50));
        rooms.Add(room);
    }

    private void Update() // testing shit cz yes
    {
        DrawRooms();
    }


    private void DrawRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(rooms[i], Color.red); 
        }
    }



}
