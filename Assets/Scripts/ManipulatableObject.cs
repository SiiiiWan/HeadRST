using UnityEngine;

public class ManipulatableObject : MonoBehaviour
{
    // public bool UseGlobalManipulationBehavior;
    private bool _isHitbyGaze;
    private bool _isGrabed;

    private System.Action<Transform> _manipulationBehavior;

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
            _manipulationBehavior?.Invoke(transform);
        }

        // transform.GetComponent<Outline>().enabled = _isHitbyGaze;
    }
    
    public void SetManipulationBehavior(ManipulationBehaviorNames behaviorName)
    {
        switch (behaviorName)
        {
            case ManipulationBehaviorNames.OneToOne:
                _manipulationBehavior = ManipulationBehaviors.OneToOne;
                break;
            case ManipulationBehaviorNames.ImplicitGaze:
                _manipulationBehavior = ManipulationBehaviors.ImplicitGaze;
                break;
            // case "PositionOnly":
            //     _manipulationBehavior = ManipulationBehaviors.PositionOnly;
            //     break;
            default:
                Debug.LogWarning($"Unknown manipulation behavior: {behaviorName}");
                break;
        }
    }

}
