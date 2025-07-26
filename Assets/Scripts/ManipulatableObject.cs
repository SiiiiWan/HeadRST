using UnityEngine;
using Oculus.Interaction;
using Unity.VisualScripting;
using NUnit.Framework;

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
    public ManipulationTechnique ManipulationBehavior { get; private set; }
    public bool IsPinchTipWithinCube { get; private set; }
    public bool IsHand = false;

    void Update()
    {
        //TODO: issue of target hard to hit by gaze at a distance for multiple manipulations
        // _isHitbyGaze = EyeGaze.GetInstance().GetGazeHitTrans() == transform;
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsHitbyGaze = AngleToGaze <= 10f;

        if (IsHand)
        {
            AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, HandData.GetInstance().GetHandPosition(usePinchTip: true) - EyeGaze.GetInstance().GetGazeRay().origin);
            IsHitbyGaze = AngleToGaze <= 20f;
            return;
        } 

        ManipulationBehavior = StudyControl.GetInstance().ManipulationBehavior;
        IsPinchTipWithinCube = IsPointWithinBoxCollider(GetComponent<BoxCollider>(), ManipulationBehavior.VirtualHandPosition + (HandData.GetInstance().GetHandPosition(usePinchTip: true) - HandData.GetInstance().GetHandPosition(usePinchTip: false)));

        if (IsHitbyGaze && (PinchDetector.GetInstance().IsOneHandPinching || Grabbable.SelectingPointsCount > 0) && GrabbedState == GrabbedState.NotGrabbed)
        {
            StudyControl.GetInstance().GrabbedObject = this;
            GrabbedState = Grabbable.SelectingPointsCount > 0 ? GrabbedState.Grabbed_Direct : GrabbedState.Grabbed_Indirect;

            ManipulationBehavior?.OnSingleHandGrabbed(transform, GrabbedState);
        }


        if (PinchDetector.GetInstance().IsNoHandPinching && Grabbable.SelectingPointsCount == 0 && GrabbedState != GrabbedState.NotGrabbed)
        {
            GrabbedState tempState = GrabbedState;
            GrabbedState = GrabbedState.NotGrabbed;

            ManipulationBehavior?.OnHandReleased(tempState);
        }

        if (GrabbedState == GrabbedState.Grabbed_Indirect) ManipulationBehavior?.ApplyIndirectGrabbedBehaviour();

        transform.GetComponent<Outline>().enabled = IsHitbyGaze && GrabbedState == GrabbedState.NotGrabbed && IsPinchTipWithinCube == false;
        //TODO: bug: outline feedback and direct grab not aligned; probably because the direct grab detection allows a little bit more outsied of the cube
    }
    
public static bool IsPointWithinBoxCollider(BoxCollider box, Vector3 point)
{
    // Transform the point into the local space of the collider
    Vector3 localPoint = box.transform.InverseTransformPoint(point);
    Vector3 halfSize = box.size * 0.5f;
    Vector3 center = box.center;

    return
        (localPoint.x >= center.x - halfSize.x && localPoint.x <= center.x + halfSize.x) &&
        (localPoint.y >= center.y - halfSize.y && localPoint.y <= center.y + halfSize.y) &&
        (localPoint.z >= center.z - halfSize.z && localPoint.z <= center.z + halfSize.z);
}


}
