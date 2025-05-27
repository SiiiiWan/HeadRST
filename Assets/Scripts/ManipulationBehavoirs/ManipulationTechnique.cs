using UnityEngine;

public class ManipulationTechnique : MonoBehaviour, IManipulationBehavior
{
    public virtual void Apply(Transform target)
    { 
        Debug.LogWarning("Apply method not implemented in " + this.GetType().Name);
    }

    public virtual void OnGrabbed(Transform target)
    {
        // Debug.LogWarning("Apply method not implemented in " + this.GetType().Name);
    }
}