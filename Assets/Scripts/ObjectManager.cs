using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    public TaskCursor TaskCursor;

    public List<ManipulatableObject> CurrentFocusedObjects = new List<ManipulatableObject>();
    public ManipulatableObject ClosestFocusedObject, ClosestFocusedObject_prev;

    void Update()
    {
        ClosestFocusedObject = UpdateAndGetClosestFocusedObject();

        if (ClosestFocusedObject_prev != ClosestFocusedObject)
        {
            if (ClosestFocusedObject != null)
            {
                ClosestFocusedObject.OnHoverEnter();
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

    public ManipulatableObject UpdateAndGetClosestFocusedObject()
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
