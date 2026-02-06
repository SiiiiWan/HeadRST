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
    public ManipulatableObject PickedUpObject_rightHand, PickedUpObject_leftHand;

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
                if (ClosestFocusedObject_current != null && ClosestFocusedObject_current.ManipulationState != ManipulationState.PickedUp)
                {
                    ClosestFocusedObject_current.SetManipulationState(ManipulationState.Hovered);
                }
                    
                // set the previous object to Idle
                if (ClosestFocusedObject_prev != null && ClosestFocusedObject_prev.ManipulationState != ManipulationState.PickedUp)
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

    public bool IsFullHand {get => PickedUpObject_rightHand != null && PickedUpObject_leftHand != null; }
    public bool IsOneHandPickedUp {get => !IsFullHand && (PickedUpObject_rightHand != null || PickedUpObject_leftHand != null); }
    public void RegisterPickedUpObject(ManipulatableObject obj)
    {
        if(PickedUpObject_rightHand == null && PickedUpObject_leftHand == null)
        {
            if(PinchDetector.GetInstance().IsRightPinching)
            {
                PickedUpObject_rightHand = obj;
                return;
            }
            else if(PinchDetector.GetInstance().IsLeftPinching)
            {
                PickedUpObject_leftHand = obj;
                return;
            }
        }
        
        if (PickedUpObject_rightHand == null)
        {
            PickedUpObject_rightHand = obj;
        }
        else if (PickedUpObject_leftHand == null)
        {
            PickedUpObject_leftHand = obj;
        }
    }
    public bool GetPitckedUpObjectHandedness(ManipulatableObject obj, out Handedness_v hand)
    {
        hand = Settings.GetInstance().DominantHand;
        if(PickedUpObject_rightHand == obj) 
        {
            hand = Handedness_v.Right; 
            return true; 
        }
        
        if(PickedUpObject_leftHand == obj)
        { 
            hand = Handedness_v.Left; 
            return true; 
        }

        return false;
    }


    public void UnregisterPickedUpObject(ManipulatableObject obj)
    {
        if(PickedUpObject_rightHand == obj) PickedUpObject_rightHand = null;
        if(PickedUpObject_leftHand == obj) PickedUpObject_leftHand = null;
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
            if (obj == null || obj.ManipulationState == ManipulationState.PickedUp) continue;

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
            if (obj == null || obj.ManipulationState == ManipulationState.PickedUp) continue;

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
