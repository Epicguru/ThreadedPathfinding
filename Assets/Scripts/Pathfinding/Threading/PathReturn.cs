
using System.Collections.Generic;

public struct PathReturn
{
    public PathfindingResult Result;
    public List<PNode> Path;
    public PathFound Callback;
}