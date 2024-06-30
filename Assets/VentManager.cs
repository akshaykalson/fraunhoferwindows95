using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentManager : MonoBehaviour
{
    public GameObject cubePrefab;       // The prefab for the cube segment
    public int initialMaxPipeLength = 100; // Initial maximum length of the pipe in cubes
    public float cubeDistance = 1.0f;   // Distance between cubes
    public float straightLineDuration = 1.0f; // Duration in seconds for cubes to appear in a straight line
    public int initialGridSize = 10;    // Initial size of the grid
    public int gridSizeIncrement = 10;  // Amount to increase grid size when stuck
    public int maxFailedAttempts = 10;  // Maximum number of failed attempts before expanding grid

    private Vector3 currentPos;         // Current position of the end of the pipe
    private Vector3 currentDir;         // Current direction of the pipe
    private List<GameObject> pipeSegments = new List<GameObject>(); // List to manage pipe segments
    private List<Vector3> directions = new List<Vector3>()
    {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };
    private int failedAttempts = 0;     // Counter for failed attempts
    private int currentGridSize;        // Current grid size
    private int currentMaxPipeLength;   // Current maximum length of the pipe in cubes
    private Color currentColor;         // Current color of the pipe

    void Start()
    {
        VentFormation();
    }

    void VentFormation()
    {
        // Initialize the pipe at the origin
        currentPos = Vector3.zero;
        currentDir = Vector3.forward;
        failedAttempts = 0;
        currentGridSize = initialGridSize;
        currentMaxPipeLength = initialMaxPipeLength;

        // Initialize the current color
        currentColor = Random.ColorHSV();

        // Clear existing segments
        foreach (GameObject segment in pipeSegments)
        {
            Destroy(segment);
        }
        pipeSegments.Clear();

        // Start creating the pipe segments
        StartCoroutine(CreateVentSegments());
    }

    IEnumerator CreateVentSegments()
    {
        while (true)
        {
            // Create a straight line segment
            bool success = CreateStraightLine();

            // Change direction randomly if collision occurred
            if (!success)
            {
                ChangeDirection();
                failedAttempts++;
                if (failedAttempts >= maxFailedAttempts)
                {
                    // Expand the grid and increase max pipe length
                    currentGridSize += gridSizeIncrement;
                    currentMaxPipeLength += 10 * gridSizeIncrement;
                    failedAttempts = 0; // Reset failed attempts counter
                }
            }
            else
            {
                failedAttempts = 0; // Reset failed attempts counter on successful creation
                // Wait before creating the next segment
                yield return new WaitForSeconds(straightLineDuration);
            }
        }
    }

    bool CreateStraightLine()
    {
        // Calculate new position
        Vector3 newPosition = currentPos + currentDir * cubeDistance;

        // Check if the new position is within bounds
        if (IsWithinBounds(newPosition))
        {
            // Check for collision with existing pipe segments
            if (!IsOverlapping(newPosition))
            {
                // Create cube segment
                GameObject newSegment = Instantiate(cubePrefab, newPosition, Quaternion.identity);
                Renderer segmentRenderer = newSegment.GetComponent<Renderer>();
                segmentRenderer.material.color = currentColor;

                // Add new segment to the pipe list
                pipeSegments.Add(newSegment);

                // Destroy oldest segment if pipe exceeds maxPipeLength
                if (pipeSegments.Count > currentMaxPipeLength)
                {
                    GameObject oldestSegment = pipeSegments[0];
                    pipeSegments.RemoveAt(0);
                    Destroy(oldestSegment);
                }

                // Update current position
                currentPos = newPosition;

                return true; // Successfully created segment
            }
        }

        return false; // Failed to create segment
    }

    void ChangeDirection()
    {
        // Change direction randomly but avoid reversing
        Vector3 newDir;
        do
        {
            newDir = directions[Random.Range(0, directions.Count)];
        } while (newDir == -currentDir);

        // Update current direction
        currentDir = newDir;

        // Change the color when the direction changes
        currentColor = Random.ColorHSV();
    }

    bool IsWithinBounds(Vector3 position)
    {
        return Mathf.Abs(position.x) <= currentGridSize && Mathf.Abs(position.y) <= currentGridSize && Mathf.Abs(position.z) <= currentGridSize;
    }

    bool IsOverlapping(Vector3 position)
    {
        // Check if the new position overlaps with existing pipe segments
        foreach (GameObject segment in pipeSegments)
        {
            if (Vector3.Distance(segment.transform.position, position) < cubeDistance * 0.9f)
            {
                return true;
            }
        }
        return false;
    }
}
