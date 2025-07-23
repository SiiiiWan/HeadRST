using UnityEngine;

public class Anchor : MonoBehaviour
{
    public GrabbedState GrabbedState = GrabbedState.NotGrabbed;
    public bool IsRealHand = false;
    void Update()
    {

        ManipulationTechnique technique = StudyControl.GetInstance().ManipulationBehavior;

        if (technique.ObjectsInGazeCone_OnGazeFixation.Count > 0)
        {
            if (technique.ObjectsInGazeCone_OnGazeFixation[0] == transform)
            {
                if(PinchDetector.GetInstance().IsOneHandPinching && GrabbedState == GrabbedState.NotGrabbed)
                {
                    GrabbedState = GrabbedState.OneHandGrabbed;
                    gameObject.tag = "Untagged";
                }
            }
        }

        if (PinchDetector.GetInstance().IsNoHandPinching)
        {
            if (GrabbedState == GrabbedState.OneHandGrabbed)
            {
                GrabbedState = GrabbedState.NotGrabbed;
                gameObject.tag = "Anchor";
            }
        }
    }
}
