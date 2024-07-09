using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManagerFinal : MonoBehaviour
{
    public GameObject pipePrefab; // prefab reference
    public GameObject bendPrefab;
    public float spawnRate = 0.01f; //rate at which pipes will appear
    public float pipeLength = 1.0f; //length of each pipe segment
    public float collisionThreshold = 1.0f;

    public Vector3 minBounds = new Vector3(-10.0f, -10.0f, -10.0f);  //limits of the space
    public Vector3 maxBounds = new Vector3(10.0f, 10.0f, 10.0f);

    public int minStraightSegments = 10;  //min no. of pipe segments before considering a bend
    public LayerMask pipeLayer; // Assigned in the Unity Inspector , mask for collision

    private Vector3 currentPos; //current position of pipe generation
    private Vector3 currentDir; //current direction of pipe generation
    private HashSet<Vector3> usedPositions = new HashSet<Vector3>(); // used positions track, fast lookups
    private List<Vector3> directions = new List<Vector3>()   // all possible directions 
    {
        Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
    };
    private int straightSegmentCount = 0; // no. of straight line segments placed
    private Color currentColor; // color in use

    private List<Vector3> potentialStartingPoints = new List<Vector3>();  //to keep track of starting points
    private List<Color> usedColors = new List<Color>(); //keep track of used ones
    private Queue<GameObject> recentSegments = new Queue<GameObject>();   //to keep track of recent segments to bypass collision mechanism on them

    void Start()
    {
        InitializePotentialStartingPoints();
        ChangeColor();
        ResetPipeGeneration(); // This will set initial position and direction
        StartCoroutine(SpawnPipes());
    }

    void InitializePotentialStartingPoints()
    {
        potentialStartingPoints.Clear();
        for (float x = minBounds.x; x <= maxBounds.x; x += pipeLength)
        {
            for (float y = minBounds.y; y <= maxBounds.y; y += pipeLength)
            {
                for (float z = minBounds.z; z <= maxBounds.z; z += pipeLength)
                {
                    Vector3 point = new Vector3(x, y, z);
                    if (!usedPositions.Contains(point))
                    {
                        potentialStartingPoints.Add(point);
                    }
                }
            }
        }
    }

    IEnumerator SpawnPipes()
    {
        while (true)
        {
            if (!CreatePipeSegment())  //recursively calls this method
            {
                if (potentialStartingPoints.Count > 0)  // if we have space available in bounding box
                {
                    Debug.Log("No valid position available, starting new pipe generation.");
                    ResetPipeGeneration();
                    yield return new WaitForSeconds(2f);
                }
                else
                {
                    Debug.Log("Bounding box is full, stopping generation.");
                    yield break;
                }
            }
            yield return new WaitForSeconds(spawnRate);
        }
    }

    bool CreatePipeSegment()
    {
        Vector3 newPosition = currentPos + currentDir * pipeLength; // to determine next position to place next pipe segment

        if (!IsValidPosition(newPosition) || (straightSegmentCount >= minStraightSegments && Random.value > 0.7f)) // either there is no valid position, or just randomly place a bend after a definite no. of pipesegments, to maintain zig zag
        {
            return PlaceBend();
        }
        else
        {
            PlaceStraightPipe(newPosition);
            return true;
        }
    }

    void PlaceStraightPipe(Vector3 newPosition)
    {
        if (currentDir != Vector3.zero)
        {
            GameObject pipe = Instantiate(pipePrefab, currentPos + currentDir * (pipeLength / 2), Quaternion.LookRotation(currentDir) * Quaternion.Euler(90, 0, 0));
            pipe.transform.localScale = new Vector3(1.0f, pipeLength, 1.0f);
            pipe.GetComponent<Renderer>().material.color = currentColor;

            CapsuleCollider collider = pipe.AddComponent<CapsuleCollider>(); // capsule based collision system
            collider.radius = 0.5f;
            collider.height = pipeLength;
            collider.direction = 1; // Y-axis

            pipe.layer = LayerMask.NameToLayer("Pipe");

            AddToRecentSegments(pipe);

            currentPos = newPosition;
            UpdateUsedPositions(currentPos);
            straightSegmentCount++;
        }
        else
        {
            Debug.LogWarning("Current direction is zero. Skipping pipe placement.");
        }
    }

    bool PlaceBend()
    {
        List<Vector3> allDirections = new List<Vector3>(directions);
        allDirections.Remove(-currentDir); // Remove the opposite direction

        foreach (Vector3 dir in allDirections)
        {
            if (IsValidPosition(currentPos + dir * pipeLength))
            {
                GameObject bend = Instantiate(bendPrefab, currentPos, Quaternion.identity);
                bend.GetComponent<Renderer>().material.color = currentColor;

                SphereCollider collider = bend.AddComponent<SphereCollider>();
                collider.radius = 0.5f;

                bend.layer = LayerMask.NameToLayer("Pipe");

                AddToRecentSegments(bend);

                currentDir = dir;
                currentPos = currentPos + currentDir * pipeLength;
                UpdateUsedPositions(currentPos);
                straightSegmentCount = 0;

                return true;
            }
        }

        return false; // No valid direction found after checking all possibilities
    }

    void AddToRecentSegments(GameObject segment)
    {
        recentSegments.Enqueue(segment);
        if (recentSegments.Count > 2)
        {
            recentSegments.Dequeue();
        }
    }

    void ChangeColor()
    {
        Color newColor;
        do
        {
            float h = Random.value;
            float s = Random.Range(0.5f, 1.0f);
            float v = Random.Range(0.5f, 1.0f);
            newColor = Color.HSVToRGB(h, s, v);
        }
        while (usedColors.Contains(newColor));

        currentColor = newColor;
        usedColors.Add(currentColor);
    }

    bool IsValidPosition(Vector3 position)
    {
        if (!IsWithinBounds(position))
        {
            return false;
        }

        Collider[] colliders = Physics.OverlapBox(position, Vector3.one * (pipeLength / 2), Quaternion.identity, pipeLayer);

        foreach (Collider collider in colliders)
        {
            if (!recentSegments.Contains(collider.gameObject))
            {
                return false; // Collision detected with a non-recent segment
            }
        }

        return true;
    }

    bool IsWithinBounds(Vector3 position)
    {
        return position.x >= minBounds.x && position.x <= maxBounds.x &&
               position.y >= minBounds.y && position.y <= maxBounds.y &&
               position.z >= minBounds.z && position.z <= maxBounds.z;
    }

    void ResetPipeGeneration()  //gives ground for next pipe generation
    {
        if (potentialStartingPoints.Count > 0)
        {
            Vector3 bestStartingPoint = FindBestStartingPoint();  //find best starting point and direction
            currentPos = bestStartingPoint;
            currentDir = FindBestInitialDirection(currentPos);
            straightSegmentCount = 0;
            UpdateUsedPositions(currentPos);

            ChangeColor();   //color for next pipe is also changed
            recentSegments.Clear();
        }
        else
        {
            Debug.Log("No more potential starting points available.");
        }
    }

    Vector3 FindBestStartingPoint()
    {
        Vector3 bestPoint = Vector3.zero;
        int maxEmptyNeighbors = -1;

        foreach (Vector3 point in potentialStartingPoints)
        {
            int emptyNeighbors = CountEmptyNeighbors(point);
            if (emptyNeighbors > maxEmptyNeighbors)
            {
                maxEmptyNeighbors = emptyNeighbors;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    int CountEmptyNeighbors(Vector3 point)  //count the directions in which it can go from current point
    {
        int count = 0;
        foreach (Vector3 dir in directions)
        {
            if (IsValidPosition(point + dir * pipeLength))
            {
                count++;  //increment count if valid position is found in a particular direction
            }
        }
        return count;
    }

    Vector3 FindBestInitialDirection(Vector3 startPoint)  //it will check for all possible directions, in which direction next pipe should be placed
    {
        foreach (Vector3 dir in directions)
        {
            if (IsValidPosition(startPoint + dir * pipeLength))
            {
                return dir;
            }
        }
        return Vector3.forward; // Fallback, should never happen if starting point is valid
    }

    void UpdateUsedPositions(Vector3 position)  //used to update the used positions list, and also remove the position from starting points list
    {
        usedPositions.Add(position);
        potentialStartingPoints.Remove(position);
    }
}
