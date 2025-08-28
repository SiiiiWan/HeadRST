using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;

public enum GrabbedState
{
    NotGrabbed,
    Grabbed_Indirect,
    Grabbed_Direct
}

public class ManipulatableObject : MonoBehaviour
{

    public bool IsHitbyGaze { get; private set; }
    public float AngleToGaze { get; private set; }
    public GrabbedState GrabbedState { get; private set; } = GrabbedState.NotGrabbed;
    public Grabbable Grabbable;
    public HandGrabInteractable HandGrabInteractable;
    public ManipulationTechnique ManipulationBehavior { get; private set; }

    // public bool IsHand = false;

    void Update()
    {
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsHitbyGaze = AngleToGaze <= 10f || EyeGaze.GetInstance().GetGazeHitTrans() == transform;
        ManipulationBehavior = StudyControl.GetInstance().ManipulationBehavior;

        SetOutlineVisibility(IsHitbyGaze && GrabbedState == GrabbedState.NotGrabbed);
        
        // if (IsHand)
        // {
        //     AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, HandData.GetInstance().GetHandPosition(usePinchTip: true) - EyeGaze.GetInstance().GetGazeRay().origin);
        //     IsHitbyGaze = AngleToGaze <= 20f;
        // } 

    }

    public void SetGrabbedState(GrabbedState state)
    {
        GrabbedState = state;
    }

    public void SetOutlineVisibility(bool isVisible)
    {
        if (transform.TryGetComponent<Outline>(out Outline outline))
        {
            outline.enabled = isVisible;
        }
    }

    public void DisableDirectGrab()
    {
        if (Grabbable != null)
        {
            Grabbable.enabled = false;
        }
        if (HandGrabInteractable != null)
        {
            HandGrabInteractable.enabled = false;
        }
    }

    public bool DistanceToClosestPoint(Vector3 position, out float distance)
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            distance = -1f;
            return false;
        }
        Vector3 closest = col.ClosestPoint(position);
        distance = Vector3.Distance(closest, position);
        return true;
    }
}
