using UnityEngine;
using Oculus.Interaction;
using Unity.VisualScripting;
using NUnit.Framework;
using Oculus.Interaction.HandGrab;

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
    public bool IsPinchTipWithinCube { get; private set; }
    // public bool IsHand = false;

    void Update()
    {
        //TODO: issue of target hard to hit by gaze at a distance for multiple manipulations
        // _isHitbyGaze = EyeGaze.GetInstance().GetGazeHitTrans() == transform;
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsHitbyGaze = AngleToGaze <= 10f;
        ManipulationBehavior = StudyControl.GetInstance().ManipulationBehavior;

        IsPinchTipWithinCube = IsPointWithinCube(ManipulationBehavior.VirtualHandPosition + (HandData.GetInstance().GetHandPosition(usePinchTip: true) - HandData.GetInstance().GetHandPosition(usePinchTip: false)));
        // if(ManipulationBehavior == GazeHand)
        if (ManipulationBehavior is GazeHand)
        {
            SetOutlineVisibility(GrabbedState == GrabbedState.NotGrabbed && IsPinchTipWithinCube);
        }
        else
        {
            SetOutlineVisibility(IsHitbyGaze && GrabbedState == GrabbedState.NotGrabbed);
        }
        // print(ManipulationBehavior.GetType().Name);
        //TODO: bug: outline feedback and direct grab not aligned; probably because the direct grab detection allows a little bit more outsied of the cube

        // if (IsHand)
        // {
        //     AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, HandData.GetInstance().GetHandPosition(usePinchTip: true) - EyeGaze.GetInstance().GetGazeRay().origin);
        //     IsHitbyGaze = AngleToGaze <= 20f;
        // } 

        transform.localScale = MathFunctions.Deg2Meter(StudyControl.GetInstance().TargetSize, Vector3.Distance(StudyControl.GetInstance().HeadPosition_OnTrialStart, transform.position)) * Vector3.one;
    }

    public void SetGrabbedState(GrabbedState state)
    {
        GrabbedState = state;
    }

    public bool IsPointWithinCube(Vector3 point, float tolerancePercentage = 0.9f)
    {
        Vector3 center = transform.position;
        Vector3 halfSize = transform.localScale * 0.5f;

        // Calculate tolerance based on percentage of each dimension
        Vector3 tolerance = new Vector3(
            halfSize.x * (1 - tolerancePercentage),
            halfSize.y * (1 - tolerancePercentage),
            halfSize.z * (1 - tolerancePercentage)
        );

        return
            (point.x >= center.x - halfSize.x + tolerance.x && point.x <= center.x + halfSize.x - tolerance.x) &&
            (point.y >= center.y - halfSize.y + tolerance.y && point.y <= center.y + halfSize.y - tolerance.y) &&
            (point.z >= center.z - halfSize.z + tolerance.z && point.z <= center.z + halfSize.z - tolerance.z);
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

}
