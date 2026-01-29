using System.Collections.Generic;
using UnityEngine;

public enum ManipulationMode
{
    Direct,
    Indirect
}

public class ObjectManager : Singleton<ObjectManager>
{
    public ManipulationMode ManipulationMode = ManipulationMode.Direct;

    public PositionRotationProvider PositionRotationProvider_Global;

    public const float GazeConeSize = 10f; // Use 150ms of history for calculation
    [HideInInspector] public List<ManipulatableObject> ObjectsInGazeCone = new List<ManipulatableObject>();
    [HideInInspector] public ManipulatableObject ClosestFocusedObject, ClosestFocusedObject_prev;
    [HideInInspector] public ManipulatableObject PickedUpObject_1, PickedUpObject_2;

    void Update()
    {
        if(ManipulationMode == ManipulationMode.Indirect)
        {
            ClosestFocusedObject = UpdateAndGetClosestFocusedObject_Ray(EyeGaze.GetInstance().GetGazeRay());

            if (ClosestFocusedObject != ClosestFocusedObject_prev)
            {
                if (ClosestFocusedObject != null)
                {
                    ClosestFocusedObject.SetManipulationState(ManipulationState.Hovered);
                }
                    
                if (ClosestFocusedObject_prev != null)
                {
                    ClosestFocusedObject_prev.SetManipulationState(ManipulationState.Idle);
                }
            }
            
            ClosestFocusedObject_prev = ClosestFocusedObject;            
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

    public void RegisterPickedUpObject(ManipulatableObject obj)
    {
        if (PickedUpObject_1 == null)
        {
            PickedUpObject_1 = obj;
        }
        else if (PickedUpObject_2 == null)
        {
            PickedUpObject_2 = obj;
        }
    }

    public void UnregisterPickedUpObject(ManipulatableObject obj)
    {
        if(PickedUpObject_1 == obj) PickedUpObject_1 = null;
        if(PickedUpObject_2 == obj) PickedUpObject_2 = null;
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
