using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PathEditorSettings", menuName = "Path/EditorSettings")]
public class PathEditorSettings : ScriptableObject
{
    [Header("Points path draw")]
    public float sampleDistance;
    public float resolution;

    [Header("Editor Draw")]
    public Color anchorPointColor;
    public float radiusAnchorPoint;
    public Color handlePointColor;
    public float radiusHandlesPoint;
    public Color lineEditorColor;
    public float widthLineEditor;
    public float scaleRotHandles;
}
