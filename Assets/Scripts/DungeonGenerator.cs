using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.LightTransport.PostProcessing;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using System.Net;
using System.Linq;
using System.Data;
using NaughtyAttributes;
using UnityEngine.InputSystem;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Changable Settings")]

    [Tooltip("the minimum size of the room width and height")][SerializeField] int minimumSize;

    [SerializeField] private int dungeonWidth, dungeonHeight, maxRoomCount;
    [Tooltip("Helps you itterate over the dungeon slowly.\nYou need to use the itterate further bool in the inspector")][SerializeField] private bool stepByStepDebugging = false;
    [Tooltip("Only use when stepByStepDebugging is true")][SerializeField] private bool itterateFurther = false;

    [Header("Dungeon Information")]

    [SerializeField] private bool isSplitting = false;
    [SerializeField] private bool isGeneratingDoors = false;
    [SerializeField] private bool isGeneratingGraph = false;

    [Tooltip("Rooms that need to be split")][SerializeField] private List<RectInt> splittingRooms = new List<RectInt>();
    [Tooltip("Rooms that have been split")][SerializeField] private List<RectInt> doneRooms = new List<RectInt>();
    [Tooltip("all the rooms created in intersections for the doors")][SerializeField] private List<RectInt> doors = new List<RectInt>();

    private int totalCurrentRooms;

    Graph<Vector2Int> graph = new Graph<Vector2Int>();
    private void Start()
    {
        splittingRooms.Add(new RectInt(Vector2Int.zero, new Vector2Int(dungeonWidth, dungeonHeight)));
        StartCoroutine(SplitRooms());
    }

    private void Update()
    {
        DrawRooms();
        totalCurrentRooms = splittingRooms.Count + doneRooms.Count;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("works now :P");
            itterateFurther = true;
        }
    }
    /// <summary>
    /// Draws all rooms and doors in the scene using debug colors.
    /// Yellow for rooms to be split, blue for finished rooms, green for doors.
    /// </summary>
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
        for (int i = 0; i < doors.Count; i++)
        {
            AlgorithmsUtils.DebugRectInt(doors[i], Color.green);
        }
    }

    /// <summary>
    /// Coroutine that keeps splitting rooms until all are below the minimum size.
    /// After splitting, generates doors and adds nodes to the graph.
    /// </summary>
    private IEnumerator SplitRooms()
    {
        isSplitting = true;
        while (splittingRooms.Count > 0)
        {
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

            if (!stepByStepDebugging)
                yield return null;
            else
            {
                yield return new WaitUntil(() => itterateFurther);
                itterateFurther = false;
            }
        }
        isSplitting = false;
        yield return StartCoroutine(GenerateDoors());
        yield return StartCoroutine(GenerateRoomGraph());
    }

    /// <summary>
    /// Splits a room into two, either horizontally or vertically, and adds the new rooms to the splitting list.
    /// </summary>
    /// <param name="toCheckRoom">Room to split.</param>
    /// <param name="splitHorizontally">If true, splits horizontally; otherwise, vertically.</param>
    private void SplitRoom(RectInt toCheckRoom, bool splitHorizontally)
    {
        if (splitHorizontally)
        {
            int randomSplitY = Random.Range(minimumSize, toCheckRoom.height - minimumSize);

            RectInt upperRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y + randomSplitY), new Vector2Int(toCheckRoom.width, toCheckRoom.height - randomSplitY));
            splittingRooms.Add(upperRoom);

            RectInt lowerRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(toCheckRoom.width, randomSplitY + 1));
            splittingRooms.Add(lowerRoom);
        }
        else
        {
            int randomSplitX = Random.Range(minimumSize, toCheckRoom.width - minimumSize);

            RectInt rightRoom = new RectInt(new Vector2Int(toCheckRoom.position.x + (toCheckRoom.width - randomSplitX), toCheckRoom.position.y), new Vector2Int(randomSplitX, toCheckRoom.height));
            splittingRooms.Add(rightRoom);

            RectInt leftRoom = new RectInt(new Vector2Int(toCheckRoom.position.x, toCheckRoom.position.y), new Vector2Int(toCheckRoom.width - randomSplitX + 1, toCheckRoom.height));
            splittingRooms.Add(leftRoom);
        }
    }

    /// <summary>
    /// Finds where finished rooms touch and creates doorways at those intersections.
    /// Ensures no duplicate doors are created.
    /// </summary>
    private IEnumerator GenerateDoors()
    {
        isGeneratingDoors = true;
        for (int roomIndex1 = 0; roomIndex1 < doneRooms.Count; roomIndex1++)
        {
            for (int roomIndex2 = 1 + roomIndex1; roomIndex2 < doneRooms.Count; roomIndex2++)
            {
                if (roomIndex1 != roomIndex2 && AlgorithmsUtils.Intersects(doneRooms[roomIndex1], doneRooms[roomIndex2]))
                {
                    RectInt DoorIntsct = AlgorithmsUtils.Intersect(doneRooms[roomIndex1], doneRooms[roomIndex2]);

                    if (DoorIntsct.width <= 2 && DoorIntsct.height <= 2)
                        continue;

                    if (DoorIntsct.width > 1 && DoorIntsct.height == 1)
                    {
                        int RandomX = Random.Range(DoorIntsct.position.x + 1, DoorIntsct.position.x + DoorIntsct.width - 1);
                        RectInt doors = new RectInt(new Vector2Int(RandomX, DoorIntsct.position.y + DoorIntsct.height / 2), new Vector2Int(1, 1));
                        if (!this.doors.Exists(d => d.position == doors.position)) this.doors.Add(doors);

                        if (!stepByStepDebugging)
                            yield return null;
                        else
                        {
                            yield return new WaitUntil(() => itterateFurther);
                            itterateFurther = false;
                        }
                    }
                    else if (DoorIntsct.width == 1 && DoorIntsct.height > 1)
                    {
                        int RandomY = Random.Range(DoorIntsct.position.y + 1, DoorIntsct.position.y + DoorIntsct.height - 1);
                        RectInt doors = new RectInt(new Vector2Int(DoorIntsct.position.x + DoorIntsct.width / 2, RandomY), new Vector2Int(1, 1));
                        if (!this.doors.Exists(d => d.position == doors.position)) this.doors.Add(doors);

                        if (!stepByStepDebugging)
                            yield return null;
                        else
                        {
                            yield return new WaitUntil(() => itterateFurther);
                            itterateFurther = false;
                        }
                    }
                }
            }
        }
        isGeneratingDoors = false;
    }

    /// <summary>
    /// Adds the center of each finished room and each door as nodes in the graph.
    /// Draws these nodes for debugging and prints the graph structure.
    /// </summary>
    private IEnumerator GenerateRoomGraph()
    {
        isGeneratingGraph = true;
        for (int roomIndex = 0; roomIndex < doneRooms.Count; roomIndex++)
        {
            Vector2Int roomCenter = new Vector2Int(doneRooms[roomIndex].position.x + (doneRooms[roomIndex].width / 2), doneRooms[roomIndex].position.y + (doneRooms[roomIndex].height / 2));
            graph.AddNode(roomCenter);
            AlgorithmsUtils.DebugRectInt(new RectInt(roomCenter, new Vector2Int(1, 1)), Color.red, Mathf.Infinity);
            for (int doorIndex = 0; doorIndex < doors.Count; doorIndex++)
            {
                graph.AddNode(doors[doorIndex].position);
                AlgorithmsUtils.DebugRectInt(new RectInt(doors[doorIndex].position, new Vector2Int(1, 1)), Color.red, Mathf.Infinity);

                if (AlgorithmsUtils.Intersects(doneRooms[roomIndex], doors[doorIndex]))
                {
                    graph.AddEdge(roomCenter, doors[doorIndex].position);

                    Debug.DrawLine(new Vector3Int(roomCenter.x, 0, roomCenter.y), new Vector3Int(doors[doorIndex].x, 0, doors[doorIndex].y), Color.black, Mathf.Infinity);

                    if (!stepByStepDebugging)
                        yield return null;
                    else
                    {
                        yield return new WaitUntil(() => itterateFurther);
                        itterateFurther = false;
                    }
                }
            }
        }
        isGeneratingGraph = false;
    }


    void DFS<T>(Graph<T> graph, T node)
    {
        var visited = new HashSet<T>();
        var stack = new Stack<T>();

        visited.Add(node);
        stack.Push(node);

        while (stack.Count > 0)
        {
            var s = stack.Pop();
            print(s);

            foreach (var neighbor in graph.GetNeighbors(s))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    stack.Push(neighbor);
                }
            }

        }
    }
}