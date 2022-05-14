using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityPack;
using MEC;

[System.Serializable]
public class Path
{
    ///<summary>
    ///Handles the handler points respectevely the rearHandler, the anchor and the frontHandler
    ///<para> It has an "interface" of utility functions</para>
    ///</summary>
    [System.Serializable]
    public struct PointPathHandle
    {
        [System.Serializable]
        public struct PointsPath
        {
            [HideInInspector]
            public Vector3 rearhandle;
            [HideInInspector]
            public Vector3 anchorPoint;
            [HideInInspector]
            public Vector3 frontHandle;

            [HideInInspector]
            public Quaternion rotation;
        }

        [SerializeField,HideInInspector]
        private PointsPath pointsPath;
        [SerializeField,HideInInspector]
        private Transform localTransform;

        ///<summary>
        ///world position of the rear handle
        ///</summary>
        public Vector3 rearhandle 
        {
            get
            {
                return GetToWorldSpace(pointsPath.rearhandle);
            }
        }

        ///<summary>
        ///world position of the anchor
        ///</summary>
        public Vector3 anchorPoint 
        {
            get
            {
                return GetToWorldSpace(pointsPath.anchorPoint);
            }
        }

        ///<summary>
        ///world position of the front handle
        ///</summary>
        public Vector3 frontHandle 
        {
            get
            {
                return GetToWorldSpace(pointsPath.frontHandle);
            }
        }

        ///<summary>
        ///rotation of the anchor
        ///</summary>
        public Quaternion rotationAnchor
        {
            get
            {
                return pointsPath.rotation;
            }
        }
        
        public enum HandleType { FRONT, REAR }

        public PointPathHandle(Transform localTransform, PointsPath pointsPath)
        {
            this.pointsPath = pointsPath;
            this.localTransform = localTransform;
        }

        ///<summary>
        ///Moves the position of the anchor along with its handles
        ///</summary>
        public void MoveAnchorPoint(Vector3 newPos)
        {
            //take the directions in world space (it doesn't change in localspace)
            Vector3 dirToRearHandle = GetToLocal(rearhandle - anchorPoint + localTransform.position);
            Vector3 dirToFrontHandle = GetToLocal(frontHandle - anchorPoint + localTransform.position);
            
            //set the anchor point to local space
            pointsPath.anchorPoint = GetToLocal(newPos);
            pointsPath.rearhandle = pointsPath.anchorPoint + dirToRearHandle;
            pointsPath.frontHandle = pointsPath.anchorPoint + dirToFrontHandle;
        }

        ///<summary>
        ///Correctly moves the handles around the anchor
        ///</summary>
        public void MoveHandles(Vector3 newPos, HandleType typeHandle)
        {
            //convert to local space
            newPos = GetToLocal(newPos);
            switch(typeHandle)
            {
                case HandleType.FRONT:
                    //set the new position of the frontHandle
                    pointsPath.frontHandle = newPos;

                    //repositioning the rearHandle as opposite to the frontHandle while mantaining the same length as before
                    pointsPath.rearhandle = GetOppositeHandle(pointsPath.frontHandle, pointsPath.rearhandle, pointsPath.anchorPoint);
                    break;

                case HandleType.REAR:
                    //set the new position of the rearHandle
                    pointsPath.rearhandle = newPos;

                    //repositioning the frontHandle as opposite to the rearHandle while mantaining the same length as before
                    pointsPath.frontHandle = GetOppositeHandle(pointsPath.rearhandle, pointsPath.frontHandle, pointsPath.anchorPoint);
                    break;
            }
        }

        public void RotateAnchor(Quaternion newRotation)
        {
            pointsPath.rotation = newRotation;
        }

        private Vector3 GetOppositeHandle( Vector3 handleNewPos, Vector3 oppositeHandle, Vector3 anchorPointTarget)
        {
            float lengthRearHandle = Vector3.Distance(anchorPointTarget,oppositeHandle);
            Vector3 dirToRear = anchorPointTarget - handleNewPos;
            return oppositeHandle = anchorPointTarget + dirToRear.normalized * lengthRearHandle;
        }

        private Vector3 GetToLocal(Vector3 posWS)
        {
            return Quaternion.Inverse(localTransform.rotation) * (posWS - localTransform.position);
        }
        private Vector3 GetToWorldSpace(Vector3 posOS)
        {
            return localTransform.rotation * posOS + localTransform.position;
        }
    }

    ///<summary>
    ///Convient way of storing the handlers of a segment
    ///</summary>
    public struct SegmentHandlers
    {
        public Point anchorPointA;
        public Vector3 frontHandlePointA;
        public Vector3 rearHandlePointB;
        public Point anchorPointB;

        public float estimatedLengthCurve
        {
            get
            {
                return Vector3.Distance(anchorPointA.pos, anchorPointB.pos) + 
                (
                    Vector3.Distance(anchorPointA.pos, frontHandlePointA) 
                    + Vector3.Distance(frontHandlePointA, rearHandlePointB) 
                    + Vector3.Distance(rearHandlePointB,anchorPointB.pos)
                )*0.5f;
            }
        }

        public Vector3 mediumPoint
        {
            get
            {
                return (anchorPointA.pos + frontHandlePointA + rearHandlePointB + anchorPointB.pos)*0.25f;
            }
        }
        
        public SegmentHandlers(PointPathHandle a, PointPathHandle b)
        {
            anchorPointA = new Point { pos =a.anchorPoint, rotation = a.rotationAnchor};
            frontHandlePointA = a.frontHandle;
            rearHandlePointB = b.rearhandle;
            anchorPointB = new Point { pos =b.anchorPoint, rotation = b.rotationAnchor};
        }
    }

    [System.Serializable]
    public class SegmentPoints
    {
        [SerializeField,HideInInspector]
        private List<Vector3> points = new List<Vector3>();

        public int pointsCount
        {
            get
            {
                return points.Count;
            }
        }

        public Vector3 startPoint
        {
            get
            {
                return points[0];
            }
        }

        public Vector3 endPoint
        {
            get
            {
                return points[points.Count-1];
            }
        }

        [SerializeField,HideInInspector]
        private Quaternion _startRot;
        [SerializeField,HideInInspector]
        private Quaternion _endRot;

        public Quaternion startRot { get { return _startRot; } }
        public Quaternion endRot {get { return _endRot; } }

        public float length 
        {
            get
            {
                return _length; 
            }
        }

        [HideInInspector]
        private float _length = 0;
        
        public SegmentPoints(PointPathHandle A, PointPathHandle B)
        {
            _startRot = A.rotationAnchor;
            _endRot = B.rotationAnchor;
        }

        public void AddPoint(Vector3 pos)
        {
            if (points.Count > 0)
            {
                _length += (endPoint - pos).magnitude;
            }
            points.Add(pos);
        } 

        ///<summary>
        /// Returns the distance between two points in the segment along the path
        ///</summary>
        public float GetDstTargetPoint(int indexTarget, int indexPoint)
        {
            int minIndex = Mathf.Min(indexTarget, indexPoint);
            int maxIndex = Mathf.Max(indexTarget, indexPoint);
            float dst = 0;
            for (int i = minIndex; i < maxIndex; i++)
            {
                dst += Vector3.Distance(points[i], points[i+1]);
            }
            return dst;
        }

        public Vector3 GetPoint(int index)
        {
            if (index < 0 || index > pointsCount-1)
            {
                Debug.LogWarning("Out of bound! " + index);
                return Vector3.zero;
            }
            return points[index];
        }
    }

    [System.Serializable]
    public struct Point
    {
        [HideInInspector]
        public Vector3 pos;
        [HideInInspector]
        public Quaternion rotation;
    }

    [SerializeField, HideInInspector]
    private List<PointPathHandle> pointsHandler = new List<PointPathHandle>();
    
    [SerializeField,HideInInspector]
    private List<SegmentPoints> segments = new List<SegmentPoints>();

    [HideInInspector]
    public Transform localTransform;

    ///<summary>
    ///Number of point handlers
    ///</summary>
    public int pointsHandleCount
    {
        get
        {
            if (pointsHandler == null) return 0;
            return pointsHandler.Count;
        }
    }

    ///<summary>
    ///Number of segments
    ///</summary>
    public int segmentsCount
    {
        get
        {
            return pointsHandleCount-1;
        }
    }

    ///<summary>
    ///Intializes the path, which will be constrained to the given transform
    ///</summary>
    public Path(Transform localTransform, Vector3 A, Vector3 B)
    {
        this.localTransform = localTransform;

        //initializes the handler first points
        PointPathHandle.PointsPath pathA = new PointPathHandle.PointsPath
        {
            rearhandle = A-Vector3.up,
            anchorPoint = A,
            frontHandle = A+Vector3.up,
            rotation = Quaternion.identity
        };

        PointPathHandle pointHandle = new PointPathHandle(localTransform,pathA);

        PointPathHandle.PointsPath pathB = new PointPathHandle.PointsPath
        {
            rearhandle = B-Vector3.up,
            anchorPoint = B,
            frontHandle = B+Vector3.up,
            rotation = Quaternion.identity
        };

        PointPathHandle pointHandle1 = new PointPathHandle(localTransform,pathB);

        pointsHandler.Add(pointHandle);
        pointsHandler.Add(pointHandle1);
    }

    ///<summary>
    /// It draws the points along the path at a fixed distance
    ///</summary>
    ///<param name="sampleDistance"> fixed distance between each placed point </param>
    ///<param name="resolution"> The precision of the point calculation</param>
    public void Draw(float sampleDistance, float resolution)
    {
        if (pointsHandleCount < 2) return;

        Clear();

        float dstPreviousPoint = 0;
        Vector3 previousPoint = pointsHandler[0].anchorPoint;
        Vector3 previousSamplingPoint = previousPoint;

        //draws each segment
        for (int i = 0; i < segmentsCount; i++)
        {
            //create a segment handler to store the handler points of the targeted segment
            SegmentHandlers segmentHandlers = new SegmentHandlers(pointsHandler[i], pointsHandler[i+1]);
            
            //creates the segment points holder
            SegmentPoints segmentPoints = new SegmentPoints(pointsHandler[i], pointsHandler[i+1]);

            //initialize work variables
            float t = 0;
            float estimatedLengthCurve = segmentHandlers.estimatedLengthCurve;
            
            //the greater the resolution the lower is the offset for each sampling.
            //to compensate for the length, the greater the latter is the lower the offset is so that
            //at each sampling point the distance is the same no matter the length of the segment
            float offSet = 1f/estimatedLengthCurve/resolution;
            
            //t ranges from 0 to 1 to indicate the progress along the segment
            while(t < 1)
            {
                t += offSet;

                //calculating through a bezier curve algorithm the point with t progress
                Vector3 pointPos = 
                    Utilities.Lerp
                    (
                        segmentHandlers.anchorPointA.pos,
                        segmentHandlers.frontHandlePointA,
                        segmentHandlers.rearHandlePointB,
                        segmentHandlers.anchorPointB.pos, 
                        t
                    );

                float dstToPreviousPoint = Vector3.Distance(pointPos,previousPoint);

                if (dstToPreviousPoint > sampleDistance)
                {

                    Vector3 dirToPreviousPoint = previousSamplingPoint-pointPos;

                    //to fix the marginal error, the point must be turned back with the same distance as the error's length
                    pointPos += dirToPreviousPoint.normalized * (dstPreviousPoint-sampleDistance);
                    
                    segmentPoints.AddPoint(pointPos);
                    previousPoint = pointPos;
                }
                previousSamplingPoint = pointPos;
            }
            segments.Add(segmentPoints);
        }
    }

    ///<summary>
    /// Removes a handler at the given index
    ///</summary>
    public void RemovePointHandle(int index)
    {
        pointsHandler.RemoveAt(index);
    }

    ///<summary>
    ///It adds a handler somewhere between other handlers at the given index
    ///</summary>
    public void AddPointHandle(Vector3 pointWS, int index)
    {
        //if the index is beyond the limit return
        if (index >= pointsHandleCount || pointsHandleCount <= 0) return;

        //list in which to store the handlers to push a positive index off
        List<PointPathHandle> pointsHandleToPush = new List<PointPathHandle>();
        int length = pointsHandleCount;


        for (int i = index; i < length; i++)
        {
            pointsHandleToPush.Add(pointsHandler[index]);
            pointsHandler.RemoveAt(index);
        }
        AddPointHandle(pointWS);
        pointsHandler.AddRange(pointsHandleToPush);
    }

    ///<summary>
    ///It adds a handler to the path
    ///</summary>
    ///<param name="pointWS"> The point where to place the anchor point</param>
    public void AddPointHandle(Vector3 pointWS)
    {
        //calculate the dir to the previous handle
        Vector3 dirToPreviousHandle = pointsHandler[pointsHandleCount-1].frontHandle - pointWS;
        
        //convert to localspace
        Vector3 pointOS = Quaternion.Inverse(localTransform.rotation)* (pointWS - localTransform.position);

        //create the handler points
        PointPathHandle.PointsPath path = new PointPathHandle.PointsPath
        {
            //the rear handle point heads towards the previous front handle
            rearhandle = pointOS + dirToPreviousHandle.normalized,      

            anchorPoint = pointOS,
            
            //the front handle has an opposite direction of the rear handle
            frontHandle = pointOS - dirToPreviousHandle.normalized,     
            
            //default rotation
            rotation = Quaternion.identity                              
        };

        PointPathHandle pointHandle = new PointPathHandle(localTransform,path);
        pointsHandler.Add(pointHandle);
    }

    public PointPathHandle GetPointHandle(int index)
    {
        return pointsHandler[index];
    }

    public SegmentPoints GetSegment(int index)
    {
        if (index > segments.Count-1 || index < 0) return null;
        return segments[index];
    }

    ///<summary>
    ///Set a handler in the list
    ///</summary>
    public void SetPointHandle(PointPathHandle pointHandle, int index)
    {
        pointsHandler[index] = pointHandle;
    }

    ///<summary>
    ///Clears the segments of the path
    ///</summary>
    public void Clear()
    {
        segments.Clear();
    }
}
