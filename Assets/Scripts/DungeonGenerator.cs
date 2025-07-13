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
using Unity.AI.Navigation;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Changable Settings")]

    [Tooltip("the minimum size of the room width and height")][SerializeField] int minimumSize;

    [SerializeField] private int dungeonWidth, dungeonHeight, maxRoomCount;
    [Tooltip("Helps you itterate over the dungeon slowly.\nYou need to use the itterate further bool in the inspector")][SerializeField] private bool stepByStepDebugging = false;
    [Tooltip("Only use when stepByStepDebugging is true")][SerializeField] private bool itterateFurther = false;

    [Header("Prefab Assignment")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    [Header("Dungeon Information")]

    [SerializeField] private bool isSplitting = false;
    [SerializeField] private bool isGeneratingDoors = false;
    [SerializeField] private bool isGeneratingGraph = false;

    [Tooltip("Rooms that need to be split")][SerializeField] private List<RectInt> splittingRooms = new List<RectInt>();
    [Tooltip("Rooms that have been split")][SerializeField] private List<RectInt> doneRooms = new List<RectInt>();
    [Tooltip("all the rooms created in intersections for the doors")][SerializeField] private List<RectInt> doors = new List<RectInt>();

    private int totalCurrentRooms;
    private NavMeshSurface navMeshSurface;

    Graph<Vector2> graph = new Graph<Vector2>();
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

        yield return StartCoroutine(SpawnDungeonAssests());
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
                    RectInt WallIntsct = AlgorithmsUtils.Intersect(doneRooms[roomIndex1], doneRooms[roomIndex2]);

                    if (WallIntsct.width <= 2 && WallIntsct.height <= 2)
                        continue;

                    if (WallIntsct.width > 1 && WallIntsct.height == 1)
                    {
                        int RandomX = Random.Range(WallIntsct.position.x + 1, WallIntsct.position.x + WallIntsct.width - 1);
                        RectInt doors = new RectInt(new Vector2Int(RandomX, WallIntsct.position.y + WallIntsct.height / 2), new Vector2Int(1, 1));
                        if (!this.doors.Exists(d => d.position == doors.position)) this.doors.Add(doors);

                        if (!stepByStepDebugging)
                            yield return null;
                        else
                        {
                            yield return new WaitUntil(() => itterateFurther);
                            itterateFurther = false;
                        }
                    }
                    else if (WallIntsct.width == 1 && WallIntsct.height > 1)
                    {
                        int RandomY = Random.Range(WallIntsct.position.y + 1, WallIntsct.position.y + WallIntsct.height - 1);
                        RectInt doors = new RectInt(new Vector2Int(WallIntsct.position.x + WallIntsct.width / 2, RandomY), new Vector2Int(1, 1));
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
            Vector2Int roomCenterInt = new Vector2Int(doneRooms[roomIndex].position.x + (doneRooms[roomIndex].width / 2), doneRooms[roomIndex].position.y + (doneRooms[roomIndex].height / 2));

            graph.AddNode(roomCenterInt);
            AlgorithmsUtils.DebugRectInt(new RectInt(roomCenterInt - new Vector2Int(1, 1), new Vector2Int(2, 2)), Color.red, Mathf.Infinity);
            for (int doorIndex = 0; doorIndex < doors.Count; doorIndex++)
            {
                Vector2 doorCenter = new Vector2(doors[doorIndex].position.x + (doors[doorIndex].width / 2.0f), doors[doorIndex].position.y + (doors[doorIndex].height / 2.0f));
                graph.AddNode(doorCenter);
                AlgorithmsUtils.DebugRectInt(new RectInt(doors[doorIndex].position, new Vector2Int(1, 1)), Color.red, Mathf.Infinity);

                if (AlgorithmsUtils.Intersects(doneRooms[roomIndex], doors[doorIndex]))
                {
                    graph.AddEdge(roomCenterInt, doorCenter);

                    Debug.DrawLine(new Vector3(roomCenterInt.x, 0, roomCenterInt.y), new Vector3(doorCenter.x, 0, doorCenter.y), Color.black, Mathf.Infinity);

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


    private IEnumerator DFS<T>(Graph<T> graph, T node)
    {
        var visited = new HashSet<T>();
        var stack = new Stack<T>();

        visited.Add(node);
        stack.Push(node);

        while (stack.Count > 0)
        {
            var s = stack.Pop();

            // Visualize the node (magenta square)
            if (s is Vector2Int visitedIntNode)
            {
                AlgorithmsUtils.DebugRectInt(new RectInt(visitedIntNode - new Vector2Int(1, 1), new Vector2Int(2, 2)), Color.magenta, Mathf.Infinity);
            }
            else if (s is Vector2 visitedFloatNode) // since i am placing a node in the center of the door for better visualisation, i also need a check for a normal vec2
            {
                AlgorithmsUtils.DebugRectInt(new RectInt(Vector2Int.FloorToInt(visitedFloatNode) - new Vector2Int(1, 1), new Vector2Int(2, 2)), Color.magenta, Mathf.Infinity);
            }

            print(s);

            foreach (var neighbor in graph.GetNeighbors(s))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    stack.Push(neighbor);
                }
            }

            if (!stepByStepDebugging)
                yield return new WaitForSeconds(0.1f);
            else
            {
                yield return new WaitUntil(() => itterateFurther);
                itterateFurther = false;
            }
        }
    }

    [Button("Run DFS")]
    public void TestDFS()
    {
        if (doneRooms.Count > 0)
        {
            Vector2Int startNode = new Vector2Int(
                doneRooms[0].position.x + (doneRooms[0].width / 2),
                doneRooms[0].position.y + (doneRooms[0].height / 2)
            );
            StartCoroutine(DFS(graph, startNode));
        }
    }

    private HashSet<Vector2Int> wallDictionary = new HashSet<Vector2Int>();
    private IEnumerator SpawnDungeonAssests()
    {
        GameObject parentGameObject = new GameObject("Parent");
        GenerateFloor();

        for (int i = 0; i < doors.Count; i++)
        {
            wallDictionary.Add(new Vector2Int(doors[i].position.x, doors[i].position.y));
        }
        for (int i = 0; i < doneRooms.Count; i++)
        {
            for (int widthPosIndex = 0; widthPosIndex < doneRooms[i].width; widthPosIndex++)
            {
                Vector2Int widthPosition = new Vector2Int(doneRooms[i].position.x + widthPosIndex, doneRooms[i].position.y);
                if (wallDictionary.Contains(widthPosition))
                    continue;

                Instantiate(wallPrefab, new Vector3(widthPosition.x, 0, widthPosition.y), Quaternion.identity, parentGameObject.transform);
                wallDictionary.Add(widthPosition);

                yield return null;
            }
            for (int heightPosIndex = 0; heightPosIndex < doneRooms[i].height; heightPosIndex++)
            {
                Vector2Int heightPosition = new Vector2Int(doneRooms[i].position.x, doneRooms[i].position.y + heightPosIndex);
                if (wallDictionary.Contains(heightPosition))
                    continue;

                Instantiate(wallPrefab, new Vector3(heightPosition.x, 0, heightPosition.y), Quaternion.identity, parentGameObject.transform);
                wallDictionary.Add(heightPosition);

                yield return null;
            }
        }

        for (int i = 0; i <= dungeonWidth; i++)
        {
            Vector2Int dungeonWidthPos = new Vector2Int(i, dungeonHeight);
            if (wallDictionary.Contains(dungeonWidthPos))
                continue;
            Instantiate(wallPrefab, new Vector3(dungeonWidthPos.x, 0, dungeonHeight), Quaternion.identity, parentGameObject.transform);
            wallDictionary.Add(dungeonWidthPos);

            yield return null;
        }
        for (int i = 0; i < dungeonHeight; i++)
        {
            Vector2Int dungeonHeightPos = new Vector2Int(dungeonWidth, i);
            if (wallDictionary.Contains(dungeonHeightPos))
                continue;
            Instantiate(wallPrefab, new Vector3(dungeonWidth, 0, dungeonHeightPos.y), Quaternion.identity, parentGameObject.transform);
            wallDictionary.Add(dungeonHeightPos);

            yield return null;
        }
        navMeshSurface.BuildNavMesh();
        yield return null;
    }

    private void GenerateFloor()
    {
        Debug.Log("yippee to bomba");
        var newObject = Instantiate(floorPrefab, new Vector3(dungeonWidth / 2, 0, dungeonHeight / 2), Quaternion.identity);
        newObject.transform.localScale = new Vector3(dungeonWidth / 10, 0, dungeonHeight / 10);
        navMeshSurface = newObject.transform.GetChild(0).GetComponent<NavMeshSurface>();
    }
}
