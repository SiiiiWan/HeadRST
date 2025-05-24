using UnityEngine;

public class ManipulatableObject : MonoBehaviour
{
    // public bool UseGlobalManipulationBehavior;
    private bool _isHitbyGaze;
    private bool _isGrabed;

    private IManipulationBehavior _manipulationBehavior;

    void Update()
    {
        _isHitbyGaze = EyeGaze.GetInstance().GetGazeHitTrans() == transform;


        if (_isHitbyGaze && PinchDetector.GetInstance().IsPinching)
        {
            _isGrabed = true;
        }

        if (!PinchDetector.GetInstance().IsPinching)
        {
            _isGrabed = false;
        }

        if (_isGrabed && PinchDetector.GetInstance().IsPinching)
        {
            _manipulationBehavior?.Apply(transform);
        }

        // transform.GetComponent<Outline>().enabled = _isHitbyGaze;
    }
    
    public void SetManipulationBehavior(IManipulationBehavior behavior)
    {
        _manipulationBehavior = behavior;
    }

}
