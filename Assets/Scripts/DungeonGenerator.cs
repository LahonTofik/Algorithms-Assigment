using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] List<RectInt> splittingRooms = new List<RectInt>();
    [SerializeField] List<RectInt> doneRooms = new List<RectInt>();
    RectInt room;
    [SerializeField] bool isSplitting;

    [SerializeField] int dungeonWidth, dungeonHeight, maxRoomCount;
    [Tooltip("the minimum size of the room surface")][SerializeField] int minimumSize;

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
            /*AlgorithmsUtils.DebugRectInt(splittingRooms[i], Color.blue);*/
            AlgorithmsUtils.DebugRectInt(splittingRooms[i], Color.yellow);
        }
        for (int i = 0; i < doneRooms.Count; i++)
        {
            /*AlgorithmsUtils.DebugRectInt(splittingRooms[i], Color.blue);*/
            AlgorithmsUtils.DebugRectInt(doneRooms[i], Color.blue);
        }
    }

    private IEnumerator SplitRooms()
    {
        Debug.Log("SplitRooms is being called!");

        while (splittingRooms.Count > 0)
        {
            Debug.Log("For loop is started!");

            RectInt toCheckRoom = splittingRooms.Pop(0);
            if (toCheckRoom.height * toCheckRoom.width <= minimumSize) // volgende fix: minimum size toepassen
            {
                doneRooms.Add(toCheckRoom);
                continue;
            }

            bool splitHorizontally = toCheckRoom.height >= toCheckRoom.width;
            Debug.Break();
            SplitRoom(toCheckRoom, splitHorizontally);
            yield return null;
        }
    }

    /// <summary>
    /// splits the current room according to the passed parameter
    /// </summary>
    /// <param name="toCheckRoom"></param>
    /// <param name="splitHorizontally"></param>
    private void SplitRoom(RectInt toCheckRoom, bool splitHorizontally)
    {
        if (splitHorizontally)
        {
            Debug.Log("splitting horizontally");
            int splitYDown = Mathf.FloorToInt(toCheckRoom.height / 2f);
            int splitYUp = Mathf.CeilToInt(toCheckRoom.height / 2f);

            Debug.Log($"splitY1 = {splitYDown}\n"
                      + $"splityY2 = {splitYUp}");

            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y + splitYDown), new Vector2Int(toCheckRoom.width, splitYUp)));

            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(toCheckRoom.width, splitYDown + 1)));

        }
        else
        {
            Debug.Log("splitting vertically");
            int splitXDown = Mathf.FloorToInt(toCheckRoom.width / 2f);
            int splitXUp = Mathf.CeilToInt(toCheckRoom.width / 2f);

            Debug.Log($"splitX1 = {splitXDown}\n"
                      + $"splityX2 = {splitXUp}");

            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x + splitXDown, toCheckRoom.position.y), new Vector2Int(splitXUp, toCheckRoom.height)));

            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(splitXDown + 1, toCheckRoom.height)));

        }
    }

  /*  private void SplitRoomRight(RectInt toCheckRoom, bool SplitHorizontally)
    {
        if (SplitHorizontally)
        {
            int splitYDown = Mathf.RoundToInt(toCheckRoom.height / 2f);
            int splitYUp = Mathf.CeilToInt(toCheckRoom.height / 2f);

            splittingRooms.Add(new RectInt(new Vector2Int(toCheckRoom.position.x + )))
        }
        else
        {
            int splitXDown = Mathf.RoundToInt(toCheckRoom.width / 2f);
            int splitXUp = Mathf.CeilToInt(toCheckRoom.width / 2f);


        }
    }*/

    /*private IEnumerator SplitRoomDelay(float delay)
    {
        Debug.Log("delay started");

        yield return new WaitForSeconds(delay);

        SplitRooms();
        isSplitting = false;
        Debug.Log("room is split");
    }*/
}
