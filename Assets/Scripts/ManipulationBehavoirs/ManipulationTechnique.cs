using UnityEngine;

public class ManipulationTechnique : MonoBehaviour, IManipulationBehavior
{
    public virtual void Apply(Transform target)
    { 
        // Default implementation does nothing
        // Derived classes should override this method to provide specific manipulation behavior
        Debug.LogWarning("Apply method not implemented in " + this.GetType().Name);
    }
}