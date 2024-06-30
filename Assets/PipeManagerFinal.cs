using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManagerFinal : MonoBehaviour
{
    public GameObject pipePrefab;
    public GameObject bendPrefab;
    public float spawnRate = 0.01f;
    public float pipeLength = 0.5f;
    public Color[] pipeColors; // Array of colors to cycle through
    public int minStraightSegments = 10;
    public float collisionThreshold = 0.1f;

    private Vector3 currentPos;
    private Vector3 currentDir;
    private HashSet<Vector3> usedPositions = new HashSet<Vector3>();
    private List<Vector3> directions = new List<Vector3>()
    {
        Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
    };
    private int straightSegmentCount = 0;
    private float colorChangeTimer = 0f;
    private float colorChangeInterval = 7f;
    private int currentColorIndex = 0;
    private Color currentColor;
    private bool pendingColorChange = false;

    void Start()
    {
        if (pipeColors == null || pipeColors.Length == 0)
        {
            pipeColors = new Color[] { Color.green, Color.red, Color.blue, Color.yellow };
        }
        currentColor = pipeColors[currentColorIndex];
        currentPos = Vector3.zero;
        currentDir = Vector3.forward;
        usedPositions.Add(currentPos);
        StartCoroutine(SpawnPipes());
    }

    IEnumerator SpawnPipes()
    {
        while (true)
        {
            colorChangeTimer += spawnRate;

            if (colorChangeTimer >= colorChangeInterval)
            {
                pendingColorChange = true;
                colorChangeTimer = 0f;
            }

            if (!CreatePipeSegment())
            {
                // Handle no valid path scenario if needed
            }
            yield return new WaitForSeconds(spawnRate);
        }
    }

    bool CreatePipeSegment()
    {
        Vector3 newPosition = currentPos + currentDir * pipeLength;

        if (!IsValidPosition(newPosition) || (straightSegmentCount >= minStraightSegments && Random.value > 0.7f))
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
        GameObject pipe = Instantiate(pipePrefab, currentPos + currentDir * (pipeLength / 2), Quaternion.LookRotation(currentDir) * Quaternion.Euler(90, 0, 0));
        pipe.transform.localScale = new Vector3(1.0f, pipeLength, 1.0f);
        pipe.GetComponent<Renderer>().material.color = currentColor;

        currentPos = newPosition;
        usedPositions.Add(currentPos);
        straightSegmentCount++;
    }

    bool PlaceBend()
    {
        List<Vector3> validDirections = new List<Vector3>();
        foreach (Vector3 dir in directions)
        {
            if (dir != -currentDir && dir != currentDir && IsValidPosition(currentPos + dir * pipeLength))
            {
                validDirections.Add(dir);
            }
        }

        if (validDirections.Count == 0)
        {
            return false; // No valid direction to place a bend
        }

        Vector3 newDir = validDirections[Random.Range(0, validDirections.Count)];
        if (newDir != currentDir) // Check to avoid placing a bend if the new direction is forward
        {
            GameObject bend = Instantiate(bendPrefab, currentPos, Quaternion.identity);
            bend.GetComponent<Renderer>().material.color = currentColor;

            Quaternion rotation = Quaternion.LookRotation(currentDir) * Quaternion.FromToRotation(Vector3.forward, newDir);
            bend.transform.rotation = rotation;
        }

        currentDir = newDir;
        currentPos += currentDir * pipeLength; // Move the position to the end of the bend
        straightSegmentCount = 0;

        // Change the color if pending
        if (pendingColorChange)
        {
            ChangeColor();
            pendingColorChange = false;
        }

        return true;
    }

    void ChangeColor()
    {
        currentColorIndex = (currentColorIndex + 1) % pipeColors.Length;
        currentColor = pipeColors[currentColorIndex];
    }

    bool IsValidPosition(Vector3 position)
    {
        foreach (Vector3 usedPos in usedPositions)
        {
            if (Vector3.Distance(position, usedPos) < collisionThreshold)
            {
                return false;
            }
        }
        return true;
    }
}