using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMade
{
    private Path path;

    //holds the current index of the segment where the transform currently is in
    private int progressSegment = 0;
    //holds the current point to which the transform is headed
    private int progressNextPoints = 0;

    private int progressOffSet = 1;

    private float lengthSegmentProgress = 0;
    private Vector3 dirToNextPoint = Vector3.zero;
    private Vector3 startPosPoint;
    private Vector3 endPosPoint;
    private Quaternion startRotPoint;
    private Quaternion endRotPoint;

    private Transform transform;
    private bool isPositiveSpeed = true;
    private bool finishedPath = false;
    //returns the previous segment based on the progressSegment index 
    private Path.SegmentPoints previousSegment
    {
        get
        {
            if (progressSegment-1 < 0 || progressSegment-1 >= path.segmentsCount) return null;
            return path.GetSegment(progressSegment-1);
        }
    }

    //returns the current segment based on the progressSegment index
    private Path.SegmentPoints currentSegment
    {
        get
        {
            if (progressSegment < 0 || progressSegment >= path.segmentsCount) return null;
            return path.GetSegment(progressSegment);
        }
    }

    //returns the next segment based on the progressSegment index
    private Path.SegmentPoints nextSegment
    {
        get
        {
            if (progressSegment+1 < 0 || progressSegment+1 >= path.segmentsCount) return null;
            return path.GetSegment(progressSegment+1);
        }
    }

    public PathMade(Transform transform, Path path)
    {
        this.path = path;
        this.transform = transform;

        SetStartPoint(0,0);
    }

    public void ProgressPath(float speed, float deltaTime)
    {
        if (speed > 0)
        {
            if (!isPositiveSpeed)
            {
               
                isPositiveSpeed = true;
                SetNextPointTarget(true);
            }
        }
        else
        {
            if (isPositiveSpeed)
            {
                 Debug.Log("Squirtaci");
                isPositiveSpeed = false;
                SetNextPointTarget(false);
            }
        }
        
        if (finishedPath) return;
        if (currentSegment == null) return;
        lengthSegmentProgress += speed * deltaTime;
        speed = Mathf.Abs(speed);
        transform.position += dirToNextPoint.normalized * speed * deltaTime;


        float progressSegment01 = Mathf.Abs(lengthSegmentProgress)/currentSegment.length;

        transform.rotation = Quaternion.Slerp(startRotPoint, endRotPoint, progressSegment01);

        float dstEnd = Vector3.Distance(transform.position, endPosPoint);
        float dstStart = Vector3.Distance(transform.position, startPosPoint);
        float dstEndStart = Vector3.Distance(startPosPoint,endPosPoint);

        if ((dstStart >= dstEndStart) || dstEnd == 0)
        {
            SetNextPointTarget(isPositiveSpeed);
        }
    }

    private void SetNextPointTarget(bool isPositive)
    {
        if (isPositive)
        {
            if (progressNextPoints+1 > currentSegment.pointsCount-1)
            {
                
                startPosPoint = transform.position;
                if (nextSegment == null)
                {
                    finishedPath = true;
                    dirToNextPoint = Vector3.zero;
                }
                else
                {
                    if (finishedPath)
                    {
                        finishedPath = false;
                    }

                    ChangeSegment(+1);
                    endPosPoint = currentSegment.GetPoint(0);
                }
            }else
            {
                if (finishedPath)
                {
                    finishedPath = false;
                }
                startPosPoint = transform.position;
                progressNextPoints++;
                endPosPoint = currentSegment.GetPoint(progressNextPoints);
            }
            dirToNextPoint = endPosPoint - startPosPoint;
        }
        else 
        {
            if (progressNextPoints-1< 0)
            {
                startPosPoint = transform.position;
                if (previousSegment == null)
                {
                    finishedPath = true;
                    dirToNextPoint = Vector3.zero;
                }
                else
                {
                    if (finishedPath)
                    {
                        finishedPath = false;
                    }

                    ChangeSegment(-1);
                    endPosPoint =  currentSegment.GetPoint(currentSegment.pointsCount-1);
                }
            }
            else
            {
                if (finishedPath)
                {
                    finishedPath = false;
                }
                startPosPoint = currentSegment.GetPoint(progressNextPoints);
                progressNextPoints--;
                endPosPoint = currentSegment.GetPoint(progressNextPoints);
            }
            dirToNextPoint = endPosPoint - startPosPoint;
        }
    }


    private void ChangeSegment(int offSet)
    {
        progressSegment+=offSet;
        if (currentSegment == null)
        {
            progressSegment -= offSet;
            return;
        }

        if (offSet > 0)
        {
            lengthSegmentProgress = 0;
            progressNextPoints = 0;
        }
        else if (offSet < 0)
        {
            lengthSegmentProgress = currentSegment.length;
            progressNextPoints = currentSegment.pointsCount-1;
        }

        
        startRotPoint = currentSegment.startRot;
        endRotPoint = currentSegment.endRot;
    }

    public void SetStartPoint(int indexSegment, int indexPoint)
    {
        
        Path.SegmentPoints segmentTarget = path.GetSegment(indexSegment); 
        if (segmentTarget == null) 
        {
            Debug.LogError("Segment not valid");
            return;
        }

        float dstStart = segmentTarget.GetDstTargetPoint(indexPoint, 0);
        float tSegment = dstStart / path.GetSegment(indexSegment).length;

        Quaternion rotation = Quaternion.Slerp(segmentTarget.startRot, segmentTarget.endRot, tSegment); 
        transform.rotation = rotation;
        transform.position = segmentTarget.GetPoint(indexPoint);

        progressSegment = indexSegment;
        progressNextPoints = indexPoint;

        startPosPoint = transform.position;
        startRotPoint = transform.rotation;
        endPosPoint = transform.position;
        endRotPoint = currentSegment.endRot;
    }
}
