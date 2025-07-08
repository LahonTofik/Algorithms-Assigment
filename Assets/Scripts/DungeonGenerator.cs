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

            RectInt toCheckRoom = splittingRooms.Pop(0);
            if (toCheckRoom.height <= minimumSize || toCheckRoom.width <= minimumSize) // volgende fix: minimum size toepassen
            {
                doneRooms.Add(toCheckRoom);
                continue;
            }

            bool splitHorizontally = toCheckRoom.height >= toCheckRoom.width;
            SplitRoom(toCheckRoom, splitHorizontally);
            Debug.Break();
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
            
            int randomSplitY = Random.Range(minimumSize, toCheckRoom.height - minimumSize);

            // Create and add the upper room
            RectInt upperRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y + randomSplitY), new Vector2Int(toCheckRoom.width, toCheckRoom.height - randomSplitY + 1));
            splittingRooms.Add(upperRoom);
            // Create and add the lower room
            RectInt lowerRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(toCheckRoom.width, randomSplitY));
            splittingRooms.Add(lowerRoom);

            Debug.Log($"Splitting room {toCheckRoom} into two rooms being upperRoom: {upperRoom} and lowerRoom: {lowerRoom}");

        }
        else
        {
            Debug.Log("splitting vertically");
            
            int randomSplitX = Random.Range(minimumSize, toCheckRoom.width - minimumSize);

            // Create and add the right room
            RectInt rightRoom = new RectInt(new Vector2Int(toCheckRoom.position.x + (toCheckRoom.width - randomSplitX), toCheckRoom.position.y), new Vector2Int(randomSplitX, toCheckRoom.height));
            splittingRooms.Add(rightRoom);

            // Create and add the left room
            RectInt leftRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(toCheckRoom.width - randomSplitX + 1, toCheckRoom.height));
            splittingRooms.Add(leftRoom);

            Debug.Log($"Splitting room {toCheckRoom} into two rooms being leftRoom: {leftRoom} and rightRoom: {rightRoom}");
        }
    }
}
