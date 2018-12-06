
using UnityEditor;
using UnityEngine;

namespace ThreadedPathfinding.Editor
{
    public static class PathfindingEditorCode
    {
        [MenuItem("Pathfinding/Create Manager")]
        public static void CreateManager()
        {
            var existing = GameObject.FindObjectOfType<PathfindingManager>();
            if(existing != null)
            {
                EditorUtility.DisplayDialog("Error", "There is already a pathfinding manager object in the scene!", "Okay");
                return;
            }

            GameObject newGO = new GameObject("Pathfinding Manager", typeof(PathfindingManager));
            newGO.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(newGO, "Created Pathfinding Manager");

            // Set script execution order, before anything else.
            MonoScript monoScript = MonoScript.FromMonoBehaviour(newGO.GetComponent<PathfindingManager>());

            // Place it way back. Hopefully nothing else will be before this.
            MonoImporter.SetExecutionOrder(monoScript, -10000);
        }
    }
}
