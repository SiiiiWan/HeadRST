using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ManipulationMode
{
    Direct,
    Indirect
}

public class ObjectManager : Singleton<ObjectManager>
{
    [HideInInspector] public ManipulationMode ManipulationMode = ManipulationMode.Direct;

    public IObjectPositionRotationProvider ObjectPositionRotationProvider;

    public const float GazeConeSize = 10f; // Use 150ms of history for calculation
    [HideInInspector] public List<ManipulatableObject> ObjectsInGazeCone = new List<ManipulatableObject>();
    [HideInInspector] public ManipulatableObject ClosestFocusedObject_current, ClosestFocusedObject_prev;

    void Update()
    {
        if(ManipulationMode == ManipulationMode.Indirect)
        {
            // get closest focused object that is not picked up
            ClosestFocusedObject_current = UpdateAndGetClosestFocusedObject_Ray(EyeGaze.GetInstance().GetGazeRay());

            // if focus shifted...
            if (ClosestFocusedObject_current != ClosestFocusedObject_prev)
            {
                // ..to a new object, set the new object to Hovered
                if (ClosestFocusedObject_current != null && ClosestFocusedObject_current.ManipulationState != ManipulationState.Transformation)
                {
                    ClosestFocusedObject_current.SetManipulationState(ManipulationState.Hovered);
                }
                    
                // set the previous object to Idle
                if (ClosestFocusedObject_prev != null && ClosestFocusedObject_prev.ManipulationState != ManipulationState.Transformation)
                {
                    ClosestFocusedObject_prev.SetManipulationState(ManipulationState.Idle);
                }
            }
            
            ClosestFocusedObject_prev = ClosestFocusedObject_current;            
        }
    }

    public void RegisterFocusedObj(ManipulatableObject obj)
    {
        if (!ObjectsInGazeCone.Contains(obj))
        {
            ObjectsInGazeCone.Add(obj);
        }
    }

    public void UnregisterFocusedObj(ManipulatableObject obj)
    {
        if (ObjectsInGazeCone.Contains(obj))
        {
            ObjectsInGazeCone.Remove(obj);
        }
    }

    public bool IsObjectClosestFocused(ManipulatableObject obj)
    {
        return ClosestFocusedObject_current == obj;
    }

    public ManipulatableObject UpdateAndGetClosestFocusedObject_Position(Vector3 pos)
    {
        if (ObjectsInGazeCone == null || ObjectsInGazeCone.Count == 0)
        {
            return null;
        }

        ManipulatableObject closestObject = null;
        float minDistance = float.MaxValue;

        foreach (var obj in ObjectsInGazeCone)
        {
            if (obj == null) continue;

            float distance = Vector3.Distance(obj.transform.position, pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObject = obj;
            }
        }

        return closestObject;
    }

    public ManipulatableObject UpdateAndGetClosestFocusedObject_Ray(Ray ray)
    {
        if (ObjectsInGazeCone == null || ObjectsInGazeCone.Count == 0)
        {
            return null;
        }

        ManipulatableObject closestObject = null;
        float minDistance = float.MaxValue;

        foreach (var obj in ObjectsInGazeCone)
        {
            if (obj == null) continue;

            float distance = Vector3.Angle(ray.direction, obj.transform.position - ray.origin);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObject = obj;
            }
        }

        return closestObject;
    }

    public Vector3 GetCentreOfFocusedObjects()
    {
        if (ObjectsInGazeCone == null || ObjectsInGazeCone.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 center = Vector3.zero;
        int validObjectCount = 0;

        foreach (var obj in ObjectsInGazeCone)
        {
            if (obj == null) continue;

            center += obj.transform.position;
            validObjectCount++;
        }

        if (validObjectCount > 0)
        {
            center /= validObjectCount;
        }

        return center;
    }

}
