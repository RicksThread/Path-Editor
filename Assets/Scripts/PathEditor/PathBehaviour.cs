using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathBehaviour : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    [SerializeField] private PathEditorSettings pathEditorSettings;
    [SerializeField] private bool showPointDraw = false;

    [Header("Mode")]
    [SerializeField] private bool _is2D = false;
    [SerializeField] private bool _showRotateHandlers;

    public bool is2D 
    {
        get 
        {
            return _is2D; 
        }
    }

    public bool isInitialized 
    {
        get
        {
            return path != null;
        }
    }

    public Color anchorPointColor { get{ return pathEditorSettings.anchorPointColor; }}
    public Color handlePointColor { get{ return pathEditorSettings.handlePointColor; }}
    public Color lineEditorColor  { get{ return pathEditorSettings.lineEditorColor; }}

    public float scaleRotHandles { get{ return pathEditorSettings.scaleRotHandles; } }
    public float radiusAnchorPoint { get { return pathEditorSettings.radiusAnchorPoint; } }
    public float radiusHandlesPoint { get { return pathEditorSettings.radiusHandlesPoint; } }
    public float widthLineEditor { get { return pathEditorSettings.widthLineEditor; } }
    public bool showRotateHandlers { get { return _showRotateHandlers; } }
    
    private void Start() {
        if (!Application.isPlaying) return;
        CreatePath();
    }

    private void LateUpdate() {
        if (!isInitialized) return;
        if (Application.isPlaying) return;
        
        path.Draw(pathEditorSettings.sampleDistance, pathEditorSettings.resolution);
    }

    public void CreatePath()
    {
        Vector3 firstPos = -Vector3.right;
        Vector3 secondPos = Vector3.right;
        if (path == null)
            path = new Path(transform,firstPos,secondPos);
        path.Draw(pathEditorSettings.sampleDistance, pathEditorSettings.resolution);
    }

    public void RecreatePath()
    {
        Vector3 firstPos = -Vector3.right;
        Vector3 secondPos = Vector3.right;
        path = new Path(transform,firstPos,secondPos);
        path.Draw(pathEditorSettings.sampleDistance, pathEditorSettings.resolution);
    }

    private void OnDrawGizmos() {
        if (!showPointDraw || !isInitialized) return;
        for (int i = 0; i < path.segmentsCount; i++)
        {
            for (int j = 0; j < path.GetSegment(i).pointsCount; j++)
            {
                Gizmos.DrawSphere(path.GetSegment(i).GetPoint(j), 0.05f);
            }
        }
    }
}
