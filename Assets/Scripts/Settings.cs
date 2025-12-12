using UnityEngine;


public enum DominantHand
{
    right,
    left
}


public class Settings : Singleton<Settings>
{
    public DominantHand DominantHand = DominantHand.right;
    public ManipulationTechnique ManipulationBehavior;


    public Vector3 GetVirtualHandPosition(bool isRightHand)
    {
        if (isRightHand)
        {
            if (DominantHand == DominantHand.right)
            {
                // return ObjectManager.GetInstance().TaskCursor.transform.position;

                return HandData.GetInstance().RightHandPosition;
            }
            else
            {
                return HandData.GetInstance().RightHandPosition;
            }
        }
        else
        {
            if (DominantHand == DominantHand.left)
            {
                return ManipulationBehavior.VirtualHandPosition;
            }
            else
            {
                return HandData.GetInstance().LeftHandPosition;
            }
        }
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
