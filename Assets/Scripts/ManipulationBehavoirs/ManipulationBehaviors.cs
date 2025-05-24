using UnityEngine;


public enum ManipulationBehaviorNames
{
    OneToOne,
    ImplicitGaze
}

public interface IManipulationBehavior
{
    void Apply(Transform target);
}
