using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PathfindingRequest
{
    private static Queue<PathfindingRequest> Pooled = new Queue<PathfindingRequest>();

    public List<PNode> OutputPath { get; set; }
    public int StartX { get; private set; }
    public int StartY { get; private set; }
    public int EndX { get; private set; }
    public int EndY { get; private set; }
    public PathFound ReturnEvent { get; private set; }

    public static PathfindingRequest Create(int startX, int startY, int endX, int endY, PathFound foundEvent, List<PNode> outputPath = null)
    {
        PathfindingRequest r;
        if(Pooled.Count > 0)
        {
            r = Pooled.Dequeue();
            r.StartX = startX;
            r.StartY = startY;
            r.EndX = endX;
            r.EndY = endY;
            if (r.OutputPath == null)
            {
                if(outputPath != null)
                {
                    r.OutputPath = outputPath;
                }
                else
                {
                    r.OutputPath = new List<PNode>();
                }
            }
        }
        else
        {
            if(outputPath == null)
            {
                outputPath = new List<PNode>();
            }
            r = new PathfindingRequest(startX, startY, endX, endY, foundEvent, outputPath);
        }

        if(PathfindingManager.Instance != null)
        {
            PathfindingManager.Instance.Enqueue(r);
        }

        return r;
    }

    private PathfindingRequest(int x, int y, int ex, int ey, PathFound foundEvent, List<PNode> outputPath = null)
    {
        this.StartX = x;
        this.StartY = y;
        this.EndX = ex;
        this.EndY = ey;
        this.ReturnEvent = foundEvent;
    }

    /// <summary>
    /// Returns the request to the pool. After calling this do not attempt to use this object again,
    /// instead request a new one.
    /// </summary>
    public void Dispose()
    {
        if(!Pooled.Contains(this))
            Pooled.Enqueue(this);
        OutputPath = null;
    }
}
