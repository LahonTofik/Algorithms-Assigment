using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.LightTransport.PostProcessing;

public class DungeonGenerator : MonoBehaviour
{
    [Tooltip("Rooms that need to be split")][SerializeField] List<RectInt> splittingRooms = new List<RectInt>();
    [Tooltip("Rooms that have been split")][SerializeField] List<RectInt> doneRooms = new List<RectInt>();
    [Tooltip("all the rooms created in intersections for the doors")][SerializeField] List<RectInt> doorRooms = new List<RectInt>();
    RectInt room;
    [SerializeField] bool isSplitting;
    int totalCurrentRooms;

    [SerializeField] int dungeonWidth, dungeonHeight, maxRoomCount;
    [Tooltip("the minimum size of the room width and height")][SerializeField] int minimumSize;
    private void Start()
    {
        room = new RectInt(Vector2Int.zero, new Vector2Int(dungeonWidth, dungeonHeight));
        splittingRooms.Add(room);
        StartCoroutine(SplitRooms());
    }

    private void Update()
    {
        DrawRooms();
        totalCurrentRooms = splittingRooms.Count + doneRooms.Count;
    }

    private void DrawRooms()
    {
        for (int i = 0; i < splittingRooms.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(splittingRooms[i], Color.yellow);
        }
        for (int i = 0; i < doneRooms.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(doneRooms[i], Color.blue);
        }
        for (int i = 0; i < doorRooms.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(doorRooms[i], Color.green);
        }
    }
    // Assessment tip: op basis van de graph informatie laten zien dat de dungeon connected is en niet visueel

    /// <summary>
    /// Coroutine that repeatedly splits rooms in the dungeon until all rooms are below the minimum size.
    /// </summary>
    private IEnumerator SplitRooms()
    {
        Debug.Log("SplitRooms is being called!");

        // Continue splitting while there are rooms left to process
        while (splittingRooms.Count > 0)
        {
            print("while is loop running");
            RectInt toCheckRoom = splittingRooms.Pop(0);
            if (toCheckRoom.height <= minimumSize * 2 && toCheckRoom.width <= minimumSize * 2)
            {
                doneRooms.Add(toCheckRoom);
                continue;
            }

            bool splitHorizontally = toCheckRoom.height >= toCheckRoom.width;
            if (totalCurrentRooms < maxRoomCount)
                SplitRoom(toCheckRoom, splitHorizontally);
            else
                doneRooms.Add(toCheckRoom);

            /*Debug.Break();*/
            yield return null;
        }
        GenerateDoors();
    }

    /// <summary>
    /// Splits the given room either horizontally or vertically and adds the resulting rooms to the splittingRooms list.
    /// </summary>
    /// <param name="toCheckRoom">The room to split.</param>
    /// <param name="splitHorizontally">If true, splits horizontally; otherwise, splits vertically.</param>
    private void SplitRoom(RectInt toCheckRoom, bool splitHorizontally)
    {
        if (splitHorizontally)
        {
            Debug.Log("splitting horizontally");

            int randomSplitY = Random.Range(minimumSize, toCheckRoom.height - minimumSize);

            RectInt upperRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y + randomSplitY), new Vector2Int(toCheckRoom.width, toCheckRoom.height - randomSplitY));
            splittingRooms.Add(upperRoom);

            RectInt lowerRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(toCheckRoom.width, randomSplitY + 1));
            splittingRooms.Add(lowerRoom);

            Debug.Log($"Splitting room {toCheckRoom} into two rooms being upperRoom: {upperRoom} and lowerRoom: {lowerRoom}");

        }
        else
        {
            Debug.Log("splitting vertically");

            int randomSplitX = Random.Range(minimumSize, toCheckRoom.width - minimumSize);

            RectInt rightRoom = new RectInt(new Vector2Int(toCheckRoom.position.x + (toCheckRoom.width - randomSplitX), toCheckRoom.position.y), new Vector2Int(randomSplitX, toCheckRoom.height));
            splittingRooms.Add(rightRoom);

            RectInt leftRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(toCheckRoom.width - randomSplitX + 1, toCheckRoom.height));
            splittingRooms.Add(leftRoom);

            Debug.Log($"Splitting room {toCheckRoom} into two rooms being leftRoom: {leftRoom} and rightRoom: {rightRoom}");
        }
    }
    private void GenerateDoors()
    {
        Debug.Log("DrawDoors is being called!");
        for (int room1 = 0; room1 < doneRooms.Count; room1++)
        {
            for (int room2 = 0; room2 < doneRooms.Count; room2++)
            {
                Debug.Log("doors are supposed to be created");

                if (room1 != room2 && AlgorithmsUtils.Intersects(doneRooms[room1], doneRooms[room2]))
                {
                    RectInt DoorIntsct = AlgorithmsUtils.Intersect(doneRooms[room1], doneRooms[room2]);

                    if (DoorIntsct.width <= 2 && DoorIntsct.height <= 2)
                        continue;

                    if (DoorIntsct.width > 1 && DoorIntsct.height == 1 || DoorIntsct.width == 1 && DoorIntsct.height > 1)
                    {
                        RectInt Door = new RectInt(new Vector2Int(DoorIntsct.position.x + DoorIntsct.width / 2, DoorIntsct.position.y + DoorIntsct.height / 2), new Vector2Int(1, 1));

                        doorRooms.Add(Door);
                    }
                }
            }
        }
    }
}
