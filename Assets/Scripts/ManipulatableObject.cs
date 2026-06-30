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
    public List<Transform> Wedges = new List<Transform>();

    void Update()
    {
        //TODO: issue of target hard to hit by gaze at a distance for multiple manipulations
        // _isHitbyGaze = EyeGaze.GetInstance().GetGazeHitTrans() == transform;
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsHitbyGaze = AngleToGaze <= 10f || EyeGaze.GetInstance().GetGazeHitTrans() == transform;
        ManipulationBehavior = StudyControl.GetInstance().ManipulationBehavior;

        SetOutlineVisibility(IsHitbyGaze && GrabbedState == GrabbedState.NotGrabbed);
        //TODO: bug: outline feedback and direct grab not aligned; probably because the direct grab detection allows a little bit more outsied of the cube

        // if (IsHand)
        // {
        //     AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, HandData.GetInstance().GetHandPosition(usePinchTip: true) - EyeGaze.GetInstance().GetGazeRay().origin);
        //     IsHitbyGaze = AngleToGaze <= 20f;
        // } 

        // transform.localScale = MathFunctions.Deg2Meter(StudyControl.GetInstance().TargetSize, Vector3.Distance(StudyControl.GetInstance().HeadPosition_OnTrialStart, transform.position)) * Vector3.one;
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

    public void SetActiveWedges(bool isActive)
    {
        foreach (Transform wedge in Wedges)
        {
            if (wedge != null)
            {
                wedge.gameObject.SetActive(isActive);
            }
        }
    }
}
