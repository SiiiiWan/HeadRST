using UnityEngine;

public class ManipulatableObject : MonoBehaviour
{
    // public bool UseGlobalManipulationBehavior;
    private bool _isHitbyGaze;
    private bool _isGrabed;

    private IManipulationBehavior _manipulationBehavior;

    void Update()
    {
        //TODO: issue of target hard to hit by gaze at a distance for multiple manipulations
        // _isHitbyGaze = EyeGaze.GetInstance().GetGazeHitTrans() == transform;
        _isHitbyGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin) <= 10f;


        if (_isHitbyGaze && PinchDetector.GetInstance().IsOneHandPinching)
        {
            if (_isGrabed == false) _manipulationBehavior?.OnSingleHandGrabbed(transform);
            _isGrabed = true;
        }

        if (!PinchDetector.GetInstance().IsOneHandPinching)
        {
            _isGrabed = false;
        }


        if (_isGrabed) _manipulationBehavior?.ApplySingleHandGrabbedBehaviour(transform);
        else _manipulationBehavior?.ApplySingleHandReleasedBehaviour(transform);

        transform.GetComponent<Outline>().enabled = _isHitbyGaze && !_isGrabed;
    }

    public void SetManipulationBehavior(IManipulationBehavior behavior)
    {
        _manipulationBehavior = behavior;
    }

}
