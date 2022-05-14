using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathFollower : MonoBehaviour
{
    [SerializeField] private PathBehaviour pathBehaviour = null;
    [SerializeField] private int segmentStart = 0;
    [SerializeField] private int pointStart = 0;
    [SerializeField] private float speed;

    public PathMade pathMade = null;

    private void Start() {
        pathMade = new PathMade(transform, pathBehaviour.path);
        pathMade.SetStartPoint(segmentStart, pointStart);
    }

    private void Update() {
        pathMade.ProgressPath(speed, Time.deltaTime);
    }

    [ContextMenu("SetStartPath")]
    public void SetStartPath()
    {
        if (pathMade == null)
            pathMade = new PathMade(transform, pathBehaviour.path);

        pathMade.SetStartPoint(segmentStart, pointStart); 
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(PathFollower))]
public class PathFollowerEditor : Editor
{
    private PathFollower pathFollower;

    private void OnEnable() {
        if (pathFollower == null)
            pathFollower = (PathFollower)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Set to start path"))
        {
            pathFollower.SetStartPath();
        }
    }
}
#endif