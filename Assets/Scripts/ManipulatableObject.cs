using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public enum GrabbedState
{
    NotGrabbed,
    Grabbed_Indirect,
    Grabbed_Direct
}

public class ManipulatableObject : MonoBehaviour, IHoverable, IInGazeConeHandler, IPickupable
{

    public bool IsPickedUp { get; private set; }
    public virtual void OnPickup()
    {
        IsPickedUp = true;
        SetCancelObjectGravity(true);
        GetComponent<Outline>().enabled = false;
    }
    public virtual void HandlePickup()
    {
        // Update object position to follow the cursor
        ApplyPickedUpBehaviour();

        // Check for drop condition
        if (PinchDetector.GetInstance().IsNoHandPinching)
        {
            OnDrop();
        }
    }
    public virtual void OnDrop()
    {
        IsPickedUp = false;
        SetCancelObjectGravity(false);
    }

    public bool IsHovering { get; private set; }
    public virtual void OnHoverEnter()
    {
        IsHovering = true;
        // Handle hover enter logic
    }

    public virtual void OnHoverExit()
    {
        IsHovering = false;
        // Handle hover exit logic
    }

    public float AngleToGaze { get; private set; }
    public bool IsInGazeCone { get; private set; }
    public virtual void OnGazeConeEnter()
    {
        IsInGazeCone = true;
    }
    public virtual void OnGazeConeExit()
    {
        IsInGazeCone = false;
    }



    public bool UseGravity;
    public bool IsObjectFrozen { get; private set; }


    void Update()
    {
        if (IsPickedUp)
        {
            HandlePickup();
        }
        else
        {
            RefreshInGazeConeState();

            if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame)
            {
                if(IsHovering) OnPickup();
            }
        }
    }

    public virtual void ApplyPickedUpBehaviour()
    {
        UpdatePositionTo(CubeManager.GetInstance().CubeStackingCursor.transform.position);
    }

    public void RefreshInGazeConeState()
    {
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        if (AngleToGaze <= 10f || EyeGaze.GetInstance().GetGazeHitTrans() == transform)
        {
            OnGazeConeEnter();
        }
        else
        {
            OnGazeConeExit();
        }
    }


    public void UpdatePositionTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void SetCancelObjectGravity(bool isFreeze)
    {
        if(UseGravity == false) return;
        
        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        Collider collider = transform.GetComponent<Collider>();
        if (rigidbody != null && collider != null)
        {
            if (isFreeze == true)
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                // collider.enabled = false;
            }
            else
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                // collider.enabled = true;
            }
        }
    }




    public GrabbedState GrabbedState { get; protected set; }
    public Grabbable Grabbable;
    public HandGrabInteractable HandGrabInteractable;

    public void SetGrabbedState(GrabbedState state)
    {
        GrabbedState = state;
        ApplyPickedUpBehaviour();
    }
}
