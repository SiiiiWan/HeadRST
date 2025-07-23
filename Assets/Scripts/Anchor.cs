using Oculus.Interaction;
using UnityEngine;

public class Anchor : MonoBehaviour
{
    public GrabbedState GrabbedState = GrabbedState.NotGrabbed;
    public Grabbable Grabbable;
    public bool IsRealHand = false;
    void Update()
    {
        if (Grabbable == null) return;

        if (Grabbable.SelectingPointsCount > 0)
        {
            GrabbedState = GrabbedState.Grabbed_Indirect;
        }
        else
        {
            GrabbedState = GrabbedState.NotGrabbed;
        }

        // ManipulationTechnique technique = StudyControl.GetInstance().ManipulationBehavior;

        // if (technique.ObjectsInGazeCone_OnGazeFixation.Count > 0)
        // {
        //     if (technique.ObjectsInGazeCone_OnGazeFixation[0] == this)
        //     {
        //         if (PinchDetector.GetInstance().IsOneHandPinching && GrabbedState == GrabbedState.NotGrabbed)
        //         {
        //             print("switch to grabbed state");
        //             GrabbedState = GrabbedState.OneHandGrabbed;
        //             return;
        //         }
        //     }
        // }

        // if (PinchDetector.GetInstance().IsOneHandPinching == false)
        // {
        //     if (GrabbedState == GrabbedState.OneHandGrabbed)
        //     {
        //         print("switch to not grabbed state");
        //         GrabbedState = GrabbedState.NotGrabbed;
        //     }
        // }
    }
}
