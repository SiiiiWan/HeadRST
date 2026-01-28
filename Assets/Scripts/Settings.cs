using UnityEngine;


public enum Handedness_v
{
    Right,
    Left
}

public class Settings : Singleton<Settings>
{
    public Handedness_v DominantHand = Handedness_v.Right;
    public ManipulationTechnique ManipulationBehavior;
    public VirtualHandProvider VirtualHandProvider;
    public OVRBody BodyTracking;

    void Update()
    {
        if(VirtualHandProvider != null && ObjectManager.GetInstance().AllowDirectGrab)
        {
            VirtualHandProvider.UpdateVirtualHandPoses();
        }
    }

    public Pose GetVirtualHandPose(Handedness_v handedness)
    {
        // if direct grab
        if(VirtualHandProvider != null && ObjectManager.GetInstance().AllowDirectGrab)
        {
            return VirtualHandProvider.VirtualHandPoses.GetVirtualHandPose(handedness);
        }

        // if indirect interactions, use real hand pose
        if (handedness == Handedness_v.Right)
        {
            return new Pose(HandData.GetInstance().RightHandPosition, HandData.GetInstance().RightHandRotation);
        }
        else
        {
            return new Pose(HandData.GetInstance().LeftHandPosition, HandData.GetInstance().LeftHandRotation);
        }
    }

    public float GetVirtualHandScalingFactor()
    {
        if(VirtualHandProvider != null && ObjectManager.GetInstance().AllowDirectGrab)
        {
            return VirtualHandProvider.GetVirtualHandScalingFactor();
        }

        return 1.0f;
    }    
}



/// <summary>
/// Defines the contract for objects that react to entering or exiting the user's gaze cone.
/// </summary>
public interface IInGazeConeHandler
{
    bool IsInGazeCone { get; }
    /// <summary>
    /// Called once when the object enters the gaze cone.
    /// </summary>
    void OnGazeConeEnter();

    /// <summary>
    /// Called once when the object exits the gaze cone.
    /// </summary>
    void OnGazeConeExit();
}

/// <summary>
/// Defines the contract for objects that can be hovered over by a cursor or focus point.
/// </summary>
public interface IHoverable
{
    bool IsHovering { get; }
    /// <summary>
    /// Called once when the hover state begins.
    /// </summary>
    void OnHoverEnter();

    /// <summary>
    /// Called once when the hover state ends.
    /// </summary>
    void OnHoverExit();


}

/// <summary>
/// Defines the contract for objects that can be picked up and dropped.
/// </summary>
public interface IPickupable
{
    /// <summary>
    /// Gets a value indicating whether the object is currently picked up.
    /// </summary>
    bool IsPickedUp { get; }

    /// <summary>
    /// Method called to initiate the pickup action.
    /// </summary>
    void OnPickup();

    void HandlePickup();

    /// <summary>
    /// Method called to release the object.
    /// </summary>
    void OnDrop();
}
