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

    void Update()
    {
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsHitbyGaze = AngleToGaze <= 10f;

        SetOutlineVisibility(IsHitbyGaze && GrabbedState == GrabbedState.NotGrabbed);
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


}
