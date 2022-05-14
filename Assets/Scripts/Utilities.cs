using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UtilityPack
{
    //TO DO: replace all the repetitive code with delegates
    public static class Utilities
    {
        ///<summary>
        ///It returns the component T from a parent
       ///<para> It checks through all the parents and returns when it finds the required component or when the parent is null</para>
        ///</summary>
        public static T GetComponentFromParents<T>(this GameObject obj){
            Transform parent = obj.transform;
            while (parent != null)
            {
                if (parent.gameObject.GetComponent<T>() != null) 
                    return parent.gameObject.GetComponent<T>() ;
                parent = parent.parent;
            }
            return default;
        }

        ///<summary>
        ///It returns the component T from the parent with the tag given as argument
        ///<para> It checks through all the parents and returns when it finds the required component or when the parent is null</para>
        ///</summary>
        public static T GetComponentFromParentsTag<T>(this GameObject obj, string tag){
            Transform parent = obj.transform;
            while (parent != null)
            {
                if (parent.gameObject.GetComponent<T>() != null && parent.CompareTag(tag)) 
                    return parent.gameObject.GetComponent<T>() ;
                parent = parent.parent;
            }
            return default;
        }

        ///<summary>
        ///It returns the component from the parent with the name given as argument
       ///<para> It checks through all the parents and returns when it finds the required component or when the parent is null</para>
        ///</summary>
        public static T GetComponentFromParentsName<T>(this GameObject obj, string name){
            Transform parent = obj.transform;
            while (parent != null)
            {
                if (parent.gameObject.GetComponent<T>() != null && parent.name == name) 
                    return parent.gameObject.GetComponent<T>() ;
                parent = parent.parent;
            }
            return default;
        }

        ///<summary>
        ///It returns an array of rectTransform in the given screen position
        ///</summary>
        public static T[] GetRectComponentsFromMousePos<T>(Vector2 mousePosScreen)
        {

            //create an pointerEventData and instantiate a raycast
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = mousePosScreen;

            List<RaycastResult> raycastResultsList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, raycastResultsList);
            
            //stores all the rectTransforms detected by the raycast
            List<T> rects = new List<T>();
            for(int i = 0; i < raycastResultsList.Count; i++)
            {
                if (raycastResultsList[i].gameObject.GetComponent<T>() != null)
                {
                    rects.Add(raycastResultsList[i].gameObject.GetComponent<T>());
                }
            }
            return rects.ToArray();
        }

        ///<summary>
        ///It converts a vector2 in vector3
        ///</summary>
        public static Vector3 ConvertToVec3(this Vector2 target){
            return new Vector3(target.x,target.y,0);
        }
        ///<summary>
        ///It swaps the values of x and y
        ///<para> EX: vec3(2,0,3) ===> vec3(0,2,3)</para>
        ///</summary>
        public static Vector3 Vec3SwapXY(this Vector3 target){
            return new Vector3(target.y,target.x,target.z);
        }

        ///<summary>
        ///It swaps the values of x and z
        ///<para> EX: vec3(2,0,3) ===> vec3(3,0,2)</para>
        ///</summary>
        public static Vector3 Vec3SwapXZ(this Vector3 target){
            return new Vector3(target.z,target.y,target.x);
        }

        ///<summary>
        ///It swaps the values of y and z
        ///<para> EX: vec3(2,0,3) ===> vec3(2,3,0)</para>
        ///</summary>
        public static Vector3 Vec3SwapYZ(this Vector3 target){
            return new Vector3(target.x,target.z,target.y);
        }

        ///<summary>
        /// Checks if the number of the layer is in the layermask
        ///</summary>
        public static bool IsInLayer(int layer, LayerMask layerMask)
        {
            return ((layerMask.value & (1 << layer)) > 0);
        }

        ///<summary>
        ///Clamps the vector between a min and max magnitude if its length is != 0
        ///</summary>
        public static Vector2 ClampMagnitude(this Vector2 vector, float min, float max)
        {
            if (vector.magnitude == 0) return vector;

            if (vector.magnitude < min)
            {
                vector = vector.normalized * min;
            }else if (vector.magnitude > max)
            {
                vector = vector.normalized * max;
            }

            return vector;
        }
        
        ///<summary>
        ///Clamps the vector between a min and max magnitude if its length is != 0
        ///</summary>
        public static Vector3 ClampMagnitude(this Vector3 vector, float min, float max)
        {
            if (vector.magnitude == 0) return vector;

            if (vector.magnitude < min)
            {
                vector = vector.normalized * min;
            }
            else if (vector.magnitude > max)
            {
                vector = vector.normalized * max;
            }

            return vector;
        }
    
        public static Vector3 Lerp(Vector3 pointA, Vector3 pointB, Vector3 pointC, float t)
        {
            Vector3 pointAToPointB = Vector3.Lerp(pointA, pointB, t);
            Vector3 pointBToPointC =  Vector3.Lerp(pointB, pointC, t);

            return Vector3.Lerp(pointAToPointB,pointBToPointC,t);
        }

        public static Vector3 Lerp(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, float t)
        {
            Vector3 pointAToPointB = Vector3.Lerp(pointA,pointB,t);
            Vector3 pointBToPointC = Vector3.Lerp(pointB, pointC,t);
            Vector3 pointCToPointD =  Vector3.Lerp(pointC,pointD,t);
            
            return Lerp(pointAToPointB,pointBToPointC,pointCToPointD,t);
        }
    
        public static bool CompareTagParents(this GameObject gmObj, string tag)
        {
            Transform transf = gmObj.transform;
            do
            {
                if(transf.CompareTag(tag))
                {
                    return true;
                }
                transf = transf.parent;
            }while(transf != null);
            return false;
        }

        public static bool CompareNameParents(this GameObject gmObj, string name)
        {
            Transform transf = gmObj.transform;
            do
            {
                if(transf.name == name)
                {
                    return true;
                }
                transf = transf.parent;
            }while(transf != null);
            return false;
        }
    }
}