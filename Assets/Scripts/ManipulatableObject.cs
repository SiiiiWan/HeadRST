using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public enum GrabbedState
{
    NotGrabbed,
    Grabbed_Indirect,
    Grabbed_Direct
}
    
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Outline))]
public class ManipulatableObject : MonoBehaviour, IHoverable, IInGazeConeHandler, IPickupable
{
    public bool ApplyGravityByDefault = true;
    
    public bool IsPickedUp { get; protected set; }
    public virtual void OnPickup()
    {
        IsPickedUp = true;
        SetCancelObjectGravity(true);
        UpdateOutlineState(false);
        ObjectManager.GetInstance().RegisterPickedUpObject(this);
        PositionRotationProvider = ObjectManager.GetInstance().PositionRotationProvider_Global;
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
        if(IsHovering)
        {
            UpdateOutlineState(true);
        }
        ObjectManager.GetInstance().UnregisterPickedUpObject(this);
    }

    public bool IsHovering { get; private set; }
    public virtual void OnHoverEnter()
    {
        IsHovering = true;
        UpdateOutlineState(true);
    }

    public virtual void OnHoverExit()
    {
        IsHovering = false;
        UpdateOutlineState(false);
    }

    public float AngleToGaze { get; private set; }
    public bool IsInGazeCone { get; private set; }
    public virtual void OnGazeConeEnter()
    {
        IsInGazeCone = true;
        ObjectManager.GetInstance().RegisterFocusedObj(this);
    }
    public virtual void OnGazeConeExit()
    {
        IsInGazeCone = false;
        ObjectManager.GetInstance().UnregisterFocusedObj(this);
    }

    protected virtual void Awake()
    {
        UpdateOutlineState(false);
    }

    protected virtual void Update()
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

    public PositionRotationProvider PositionRotationProvider {get; set; }
    public virtual void ApplyPickedUpBehaviour()
    {
        UpdatePositionTo(PositionRotationProvider.GetPositionOutput(transform.position));
        UpdateRotationTo(PositionRotationProvider.GetRotationOutput(transform.rotation));
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

    public void UpdateOutlineState(bool isEnabled)
    {
        if(GetComponent<Outline>() == null) return;
        GetComponent<Outline>().enabled = isEnabled;
    }


    public void UpdatePositionTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void UpdateRotationTo(Quaternion newRotation)
    {
        transform.rotation = newRotation;
    }

    public void SetCancelObjectGravity(bool isFreeze)
    {
        if(ApplyGravityByDefault == false) return;
        
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
    public Grabbable Grabbable { get; protected set; }
    public HandGrabInteractable HandGrabInteractable { get; protected set; }

    public void SetGrabbedState(GrabbedState state)
    {
        GrabbedState = state;
        ApplyPickedUpBehaviour();
    }
}

