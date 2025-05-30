using UnityEngine;


public enum ManipulationBehaviorNames
{
    OneToOne,
    ImplicitGaze
}

public interface IManipulationBehavior
{
    void ApplySingleHandGrabbedBehaviour(Transform target);
    void ApplySingleHandReleasedBehaviour(Transform target);
    void OnSingleHandGrabbed(Transform target);


    void ApplyBothHandGrabbedBehaviour(Transform target);
    void ApplyBothHandReleasedBehaviour(Transform target);
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
    public virtual void ApplySingleHandReleasedBehaviour(Transform target)
    {
        // Debug.LogWarning("Apply method not implemented in " + this.GetType().Name);
    }

    public void ApplyBothHandGrabbedBehaviour(Transform target)
    {
        // throw new System.NotImplementedException();
    }

    public void ApplyBothHandReleasedBehaviour(Transform target)
    {
        // throw new System.NotImplementedException();
    }
}