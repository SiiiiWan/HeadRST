using UnityEngine;


public enum ManipulationBehaviorNames
{
    OneToOne,
    ImplicitGaze
}

public interface IManipulationBehavior
{
    void ApplyGrabbedBehaviour(Transform target);
    void ApplyReleasedBehaviour(Transform target);
    void OnGrabbed(Transform target);
}
