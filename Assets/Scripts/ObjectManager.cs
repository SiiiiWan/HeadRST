using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    public TaskCursor TaskCursor;
    public PositionRotationProvider PositionRotationProvider_Global;
    public bool AllowDirectGrab = true;
    public bool AllowIndirectGrab = true;

    [HideInInspector] public List<ManipulatableObject> CurrentFocusedObjects = new List<ManipulatableObject>();
    [HideInInspector] public ManipulatableObject ClosestFocusedObject, ClosestFocusedObject_prev;
    [HideInInspector] public ManipulatableObject PickedUpObject;

    void Update()
    {
        if(PickedUpObject != null)
        {
            return; // Skip updating focused objects when an object is picked up
        }
        // ClosestFocusedObject = UpdateAndGetClosestFocusedObject_Cursor();
        ClosestFocusedObject = UpdateAndGetClosestFocusedObject_Ray(EyeGaze.GetInstance().GetGazeRay());

        if (ClosestFocusedObject_prev != ClosestFocusedObject)
        {
            if (ClosestFocusedObject != null)
            {
                if(AllowIndirectGrab) ClosestFocusedObject.OnHoverEnter();
            }
                
            if (ClosestFocusedObject_prev != null)
            {
                ClosestFocusedObject_prev.OnHoverExit();
            }
        }
        
        ClosestFocusedObject_prev = ClosestFocusedObject;
    }

    public void RegisterFocusedObj(ManipulatableObject obj)
    {
        if (!CurrentFocusedObjects.Contains(obj))
        {
            CurrentFocusedObjects.Add(obj);
        }
    }

    public void UnregisterFocusedObj(ManipulatableObject obj)
    {
        if (CurrentFocusedObjects.Contains(obj))
        {
            CurrentFocusedObjects.Remove(obj);
        }
    }

    public void RegisterPickedUpObject(ManipulatableObject obj)
    {
        PickedUpObject = obj;
    }

    public void UnregisterPickedUpObject(ManipulatableObject obj)
    {
        if(PickedUpObject == obj) PickedUpObject = null;
    }

    public ManipulatableObject UpdateAndGetClosestFocusedObject_Cursor()
    {
        if (CurrentFocusedObjects == null || CurrentFocusedObjects.Count == 0)
        {
            return null;
        }

        ManipulatableObject closestObject = null;
        float minDistance = float.MaxValue;
        Vector3 cursorPosition = TaskCursor.transform.position;

        foreach (var obj in CurrentFocusedObjects)
        {
            if (obj == null) continue;

            float distance = Vector3.Distance(obj.transform.position, cursorPosition);
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
        if (CurrentFocusedObjects == null || CurrentFocusedObjects.Count == 0)
        {
            return null;
        }

        ManipulatableObject closestObject = null;
        float minDistance = float.MaxValue;

        foreach (var obj in CurrentFocusedObjects)
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

    public Vector3 GetCenterOfFocusedObjects()
    {
        if (CurrentFocusedObjects == null || CurrentFocusedObjects.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 center = Vector3.zero;
        int validObjectCount = 0;

        foreach (var obj in CurrentFocusedObjects)
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
