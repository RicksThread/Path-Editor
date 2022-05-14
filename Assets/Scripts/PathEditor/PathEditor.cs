using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor(typeof(PathBehaviour))]
public class PathEditor : Editor
{
    private PathBehaviour pathBehaviour;

    private Path path
    {
        get
        {
            if (pathBehaviour == null) return null;
            return pathBehaviour.path;
        }
    }
    private Event guiEvent
    {
        get
        {
            return Event.current;
        }
    }

    private void OnEnable() 
    {
        if (pathBehaviour==null)
        {
            pathBehaviour = (PathBehaviour)target;
        } 

        if (!pathBehaviour.isInitialized)
        {
            pathBehaviour.CreatePath();
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Reset path"))
        {
            pathBehaviour.RecreatePath();
        }
    }

    private void OnSceneGUI() 
    {
        Vector3 mousePos = GetMousePos();

        for (int i = 0; i < path.segmentsCount; i++)
        {
            Path.SegmentHandlers segment = new Path.SegmentHandlers(path.GetPointHandle(i), path.GetPointHandle(i+1));
            Handles.DrawBezier(segment.anchorPointA.pos, segment.anchorPointB.pos, segment.frontHandlePointA, segment.rearHandlePointB, pathBehaviour.lineEditorColor,null, pathBehaviour.widthLineEditor);
        }

        for (int i = 0; i < path.pointsHandleCount; i++)
        {
            //Event guiEvent = Event.current;
            Path.PointPathHandle pointsHandle = path.GetPointHandle(i);
            Handles.color = pathBehaviour.handlePointColor;
            
            Vector3 newRearHandlePointPos = 
                Handles.FreeMoveHandle(
                    pointsHandle.rearhandle,
                    Quaternion.identity,
                    pathBehaviour.radiusHandlesPoint,
                    Vector3.zero,
                    Handles.CylinderHandleCap
                    );
            
            Vector3 newFrontHandlePointPos = 
                Handles.FreeMoveHandle(
                    pointsHandle.frontHandle,
                    Quaternion.identity,
                    pathBehaviour.radiusHandlesPoint,
                    Vector3.zero,
                    Handles.CylinderHandleCap
                    );
        
            Handles.color = pathBehaviour.lineEditorColor;
            Handles.DrawLine(newRearHandlePointPos, pointsHandle.anchorPoint);
            Handles.DrawLine(newFrontHandlePointPos, pointsHandle.anchorPoint);
            
            if (newRearHandlePointPos != pointsHandle.rearhandle)
            {
                Undo.RegisterCompleteObjectUndo(pathBehaviour, "Modified_segment_RearHandle: " + i);
                Undo.FlushUndoRecordObjects();
                pointsHandle.MoveHandles(newRearHandlePointPos, Path.PointPathHandle.HandleType.REAR);
            }
            else if (newFrontHandlePointPos != pointsHandle.frontHandle)
            {
                Undo.RegisterCompleteObjectUndo(pathBehaviour, "Modified_segment_FrontHandle: " + i);
                Undo.FlushUndoRecordObjects();
                pointsHandle.MoveHandles(newFrontHandlePointPos, Path.PointPathHandle.HandleType.FRONT);
            }
            Handles.color = pathBehaviour.anchorPointColor;
            Vector3 newAnchorPointPos = Handles.FreeMoveHandle(pointsHandle.anchorPoint, Quaternion.identity, pathBehaviour.radiusAnchorPoint, Vector3.zero, Handles.CylinderHandleCap);
            
            if (newAnchorPointPos != pointsHandle.anchorPoint)
            {
                Undo.RegisterCompleteObjectUndo(pathBehaviour, "Modified_segment_Anchor_POS: " + i);
                Undo.FlushUndoRecordObjects();
                pointsHandle.MoveAnchorPoint(newAnchorPointPos);
            }

            if (pathBehaviour.showRotateHandlers)
            {
                Matrix4x4 startScaleMatrix = Handles.matrix;
                Handles.matrix = Matrix4x4.Translate(pointsHandle.anchorPoint) * Matrix4x4.Scale(Vector3.one * pathBehaviour.scaleRotHandles);
                Quaternion newRotation = Handles.RotationHandle(pointsHandle.rotationAnchor, Vector3.zero);
                Handles.matrix = startScaleMatrix;

                if (newRotation != pointsHandle.rotationAnchor)
                {
                    Undo.RegisterCompleteObjectUndo(pathBehaviour, "Modified_segment_Anchor_ROT: " + i);
                    Undo.FlushUndoRecordObjects();
                    pointsHandle.RotateAnchor(newRotation);
                }
            }

            Vector3 pointRotated = pointsHandle.rotationAnchor * (Vector3.up*2) + pointsHandle.anchorPoint;
            Handles.DrawLine(pointsHandle.anchorPoint, pointRotated);     
            path.SetPointHandle(pointsHandle,i);
            EditorUtility.SetDirty(pathBehaviour);
            
        }
        
        HandleAddRemoveInputs(mousePos);
    }

    private Vector3 GetMousePos()
    {
        Vector3 mousePos = Vector3.zero;
        
        if (pathBehaviour.is2D)
            mousePos = (Vector2)HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
        else
            mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;
        return mousePos;
    }

    private void HandleAddRemoveInputs(Vector3 mousePos)
    {

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift && !guiEvent.control)
        {
            Undo.RegisterCompleteObjectUndo(pathBehaviour, "Add_segment");
            Undo.FlushUndoRecordObjects();
            path.AddPointHandle(mousePos);
            EditorUtility.SetDirty(pathBehaviour);
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.shift)
        {
            
            //simple algorithm of searching the closest handle point in the path
            
            float dstPoint = int.MaxValue;
            int targetIndex = 0;
            for (int i = 0; i < path.pointsHandleCount; i++)
            {
                float currentDstPoint = (path.GetPointHandle(i).anchorPoint - mousePos).sqrMagnitude;
                if (currentDstPoint < dstPoint)
                {   
                    targetIndex = i;
                    dstPoint = currentDstPoint;
                }
            }
        
            Undo.RegisterCompleteObjectUndo(pathBehaviour, "Remove_segment");
            Undo.FlushUndoRecordObjects();
            path.RemovePointHandle(targetIndex);
            EditorUtility.SetDirty(pathBehaviour);
        }
   
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift && guiEvent.control)
        {

            //simple algorithm to find the nearest segment to the mousePos
            float dstToPrevious = float.MaxValue;
            int indexTarget = 0;
            for (int i = 0; i < path.segmentsCount; i++)
            {
                Path.SegmentHandlers segment = new Path.SegmentHandlers(path.GetPointHandle(i), path.GetPointHandle(i+1));

                float currentDst = (segment.mediumPoint - mousePos).sqrMagnitude;
                if (currentDst < dstToPrevious)
                {
                    indexTarget = i;
                    dstToPrevious = currentDst;
                } 
            }

            Undo.RegisterCompleteObjectUndo(pathBehaviour, "Add_segment_Split");
            Undo.FlushUndoRecordObjects();
            path.AddPointHandle(mousePos, indexTarget+1);
            EditorUtility.SetDirty(pathBehaviour);
        }
    }
}
#endif