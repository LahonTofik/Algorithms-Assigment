using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class DungeonGenerator : MonoBehaviour
{
    List<RectInt> rooms = new List<RectInt>();
    RectInt room;
    [SerializeField] bool splitHorizontally, isSplitting;

    [SerializeField] int width, height, roomCount;

    private void Start()
    {
        room = new RectInt(Vector2Int.zero, new Vector2Int(width, height));
        rooms.Add(room);
    }

    private void Update()
    {
        DrawRooms();
        if (rooms.Count <= roomCount && !isSplitting)
        {
            isSplitting = true;
            StartCoroutine(SplitRoomDelay(2f));
        }
    }

    private void DrawRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(rooms[i], Color.blue);
        }
    }

    private void SplitRooms()
    {
        Debug.Log("SplitRooms is being called!");

        for (int i = rooms.Count - 1; i >= 0; i--)
        {
            Debug.Log("For loop is started!");

            if (rooms[i].height <= height / 2)
            {
                Debug.Log("checking");
                splitHorizontally = false;
            }

            if (splitHorizontally)
            {
                Debug.Log("splitting horizontally");
                int splitY = rooms[i].height / 2;

                rooms.Add(new RectInt(new Vector2Int(rooms[i].position.x, rooms[i].position.y + splitY), new Vector2Int(width, splitY)));

                rooms[i] = new RectInt(rooms[i].position, new Vector2Int(width, rooms[i].height / 2));
            }
            else
            {
                Debug.Log("splitting vertically");
                int splitX = rooms[i].width / 2;

                rooms.Add(new RectInt(new Vector2Int(rooms[i].width - splitX, rooms[i].position.y), new Vector2Int(splitX, rooms[i].height)));

                rooms[i] = new RectInt(rooms[i].position, new Vector2Int(splitX, rooms[i].height));
            }
        }
    }

    private IEnumerator SplitRoomDelay(float delay)
    {
        Debug.Log("delay started");
        yield return new WaitForSeconds(delay);
        SplitRooms();
        isSplitting = false;
        Debug.Log("room is split");
    }
}
