
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A character is a guy that walks around the map using the pathfinding system.
/// This particular implementation simply walks to a random position and once it gets there, selects a new destination.
/// </summary>
public class Character : MonoBehaviour
{
    // Holds the currently active request.
    public PathfindingRequest CurrentRequest;

    // The X and Y coordinates, in tiles.
    public int X, Y;

    // A reference to the map. Used to get width and height of map.
    public Map Map;

    // The movement speed, in tiles per second.
    public float MovementSpeed = 3f;

    private List<PNode> currentPath;
    private int previousNodeIndex;
    private float movementTimer;

    public void Start()
    {
        // Initialize the current path list.
        currentPath = new List<PNode>();

        // Create the current request.
        CurrentRequest = CreateNewRequest();
    }

    public void Update()
    {
        if (CurrentRequest != null)
            return;

        movementTimer += Time.deltaTime;

        PNode previousNode = currentPath[previousNodeIndex];
        PNode nextNode = currentPath[previousNodeIndex + 1];
        float dst = GetDistance(previousNode, nextNode);

        float progress = Mathf.Clamp01(dst / movementTimer * MovementSpeed);
        if(progress == 1f)
        {
            previousNodeIndex++;
            if(previousNodeIndex == currentPath.Count)
            {
                // Reached the end of the path. Request a new path...
                // Setting current request also stops us from moving, see the first line of Update.
                CurrentRequest = CreateNewRequest();
            }
        }

        // Set position. Note that PNode can be used as a Vector. Useful and clean.
        transform.position = Vector2.Lerp(previousNode, nextNode, progress);
    }

    private float GetDistance(PNode a, PNode b)
    {
        // Assumes  that the nodes (tiles) are touching each other, or diagonal to each other.
        if(a.X != b.X && a.Y != b.Y)
        {
            // The tiles are diagonal. The distance between their centers is sqrt(2) which is 1.414...
            return 1.41421f;
        }
        else
        {
            // Assume that they are horizontal or vertical from each other.
            return 1;
        }
    }

    private PathfindingRequest CreateNewRequest()
    {
        // Creates a new request object, with the destination being a random position on the map.

        int endX;
        int endY;

        while (true)
        {
            endX = Random.Range(0, Map.Width);
            endY = Random.Range(0, Map.Height);

            // Is this end point inside a wall?
            if (Map.Tiles[endX, endY] == true)
                continue;

            // Is this end point the same as the current position?
            if(this.X == endX && this.Y == endY)            
                continue;

            // Passed all the conditions, end the loop.
            break;
        }

        // Create the request, with the callback method being UponPathCompleted.
        var request = PathfindingRequest.Create(X, Y, endX, endY, UponPathCompleted);

        return request;
    }

    private void UponPathCompleted(PathfindingResult result, List<PNode> path)
    {

        // This means that the request has completed, so it is important to dispose of it now.
        CurrentRequest.Dispose();
        CurrentRequest = null;

        // Check the result...
        if(result != PathfindingResult.SUCCESSFUL)
        {
            Debug.LogWarning("Pathfinding failed: " + result);
        }
        else
        {
            // Do something with the calculated path...
            currentPath = path;
        }
    }
}
