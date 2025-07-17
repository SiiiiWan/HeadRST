using UnityEngine;

public enum GrabbedState
{
    NotGrabbed,
    OneHandGrabbed,
    BothHandsGrabbed
}

public class ManipulatableObject : MonoBehaviour
{
    // public bool UseGlobalManipulationBehavior;
    private bool _isHitbyGaze;
    private GrabbedState _grabbedState = GrabbedState.NotGrabbed;

    private IManipulationBehavior _manipulationBehavior;

    void Update()
    {
        //TODO: issue of target hard to hit by gaze at a distance for multiple manipulations
        // _isHitbyGaze = EyeGaze.GetInstance().GetGazeHitTrans() == transform;
        _isHitbyGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin) <= 10f;
        _manipulationBehavior = StudyControl.GetInstance().ManipulationBehavior;


        if (_isHitbyGaze && PinchDetector.GetInstance().IsOneHandPinching && _grabbedState == GrabbedState.NotGrabbed)
        {
            StudyControl.GetInstance().GrabbedObject = this;

            _manipulationBehavior?.OnSingleHandGrabbed(transform);
            _grabbedState = GrabbedState.OneHandGrabbed;
        }


        // if( PinchDetector.GetInstance().IsBothHandsPinching && _grabbedState == GrabbedState.OneHandGrabbed)
        // {
        //     _grabbedState = GrabbedState.BothHandsGrabbed;
        // }

        if (PinchDetector.GetInstance().IsNoHandPinching)
        {
            if (_grabbedState == GrabbedState.OneHandGrabbed)
            {
                _grabbedState = GrabbedState.NotGrabbed;
                _manipulationBehavior?.ApplyHandReleasedBehaviour();
            }
        }

        if(_grabbedState == GrabbedState.OneHandGrabbed) _manipulationBehavior?.ApplySingleHandGrabbedBehaviour();
        // switch (_grabbedState) 
        // {
        //     case GrabbedState.OneHandGrabbed:
        //         _manipulationBehavior?.ApplySingleHandGrabbedBehaviour();
        //         break;
        //     case GrabbedState.BothHandsGrabbed:
        //         _manipulationBehavior?.ApplyBothHandGrabbedBehaviour();
        //         break;
        // }

        transform.GetComponent<Outline>().enabled = _isHitbyGaze && _grabbedState == GrabbedState.NotGrabbed;
    }

    public void SetManipulationBehavior(IManipulationBehavior behavior)
    {
        _manipulationBehavior = behavior;
    }

}
