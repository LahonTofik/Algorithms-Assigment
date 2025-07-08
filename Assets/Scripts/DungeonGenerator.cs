using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class DungeonGenerator : MonoBehaviour
{
    [Tooltip("Rooms that need to be split")][SerializeField] List<RectInt> splittingRooms = new List<RectInt>();
    [Tooltip("Rooms that have been split")][SerializeField] List<RectInt> doneRooms = new List<RectInt>();
    RectInt room;
    [SerializeField] bool isSplitting;

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
            Debug.Log("For loop is started!");

            RectInt toCheckRoom = splittingRooms.Pop(0);
            if (toCheckRoom.height <= minimumSize || toCheckRoom.width <= minimumSize) // volgende fix: minimum size toepassen
            {
                doneRooms.Add(toCheckRoom);
                continue;
            }

            bool splitHorizontally = toCheckRoom.height >= toCheckRoom.width;
            /* Debug.Break();*/
            SplitRoom(toCheckRoom, splitHorizontally);
            yield return null;
        }
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
            // Integer division can't always split the height exactly in half, so we use floor and ceil to cover all rows.
            int splitYDown = Mathf.FloorToInt(toCheckRoom.height / 2f);
            int splitYUp = Mathf.CeilToInt(toCheckRoom.height / 2f);

            Debug.Log($"splitY1 = {splitYDown}\n"
                      + $"splityY2 = {splitYUp}");

            // Create and add the upper room
            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y + splitYDown), new Vector2Int(toCheckRoom.width, splitYUp)));

            // Create and add the lower room
            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(toCheckRoom.width, splitYDown + 1)));

        }
        else
        {
            Debug.Log("splitting vertically");
            // Integer division can't always split the width exactly in half, so we use floor and ceil to cover all columns.
            int splitXDown = Mathf.FloorToInt(toCheckRoom.width / 2f);
            int splitXUp = Mathf.CeilToInt(toCheckRoom.width / 2f);

            Debug.Log($"splitX1 = {splitXDown}\n"
                      + $"splityX2 = {splitXUp}");

            // Create and add the right room
            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x + splitXDown, toCheckRoom.position.y), new Vector2Int(splitXUp, toCheckRoom.height)));

            // Create and add the left room
            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(splitXDown + 1, toCheckRoom.height)));

        }
    }
}
