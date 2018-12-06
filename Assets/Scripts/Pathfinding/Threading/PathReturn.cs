
using System.Collections.Generic;

namespace ThreadedPathfinding.Internal
{
    public struct PathReturn
    {
        public PathfindingResult Result;
        public List<PNode> Path;
        public PathFound Callback;
    }
}
