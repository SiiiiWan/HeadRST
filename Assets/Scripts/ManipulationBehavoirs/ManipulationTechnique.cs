using UnityEngine;


public enum ManipulationBehaviorNames
{
    OneToOne,
    ImplicitGaze
}

public interface IManipulationBehavior
{
    void ApplySingleHandGrabbedBehaviour(Transform target);
    void ApplyHandReleasedBehaviour(Transform target);
    void OnSingleHandGrabbed(Transform target);


    void ApplyBothHandGrabbedBehaviour(Transform target);
}

public class ManipulationTechnique : MonoBehaviour, IManipulationBehavior
{
    public virtual void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        Debug.LogWarning("Apply method not implemented in " + this.GetType().Name);
    }

    public virtual void OnSingleHandGrabbed(Transform target)
    {
        // Debug.LogWarning("Apply method not implemented in " + this.GetType().Name);
    }
    public virtual void ApplyHandReleasedBehaviour(Transform target)
    {
        // Debug.LogWarning("Apply method not implemented in " + this.GetType().Name);
    }

    public virtual void ApplyBothHandGrabbedBehaviour(Transform target)
    {

    }
}