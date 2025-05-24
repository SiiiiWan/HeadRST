
using UnityEngine;

public class StudyControl : MonoBehaviour
{

    public ManipulationBehaviorNames ManipulationBehavior;

    void Update()
    {
        if (PinchDetector.GetInstance().IsPinching)
        {
            // Get the manipulatable object under gaze
            Transform target = EyeGaze.GetInstance().GetGazeHitTrans();
            if (target != null)
            {
                // Set the manipulation behavior based on the selected type
                ManipulatableObject manipulatableObject = target.GetComponent<ManipulatableObject>();
                if (manipulatableObject != null)
                {
                    manipulatableObject.SetManipulationBehavior(ManipulationBehavior);
                }
            }
        }
        
    }

}