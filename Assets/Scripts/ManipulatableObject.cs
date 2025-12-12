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

    private float _pinchStartTime;
    private const float PinchTapThreshold = 0.3f; // A pinch shorter than this is considered a "tap"

    public virtual void OnPickup()
    {
        IsPickedUp = true;
        // SetCancelObjectGravity(true);
        UpdateOutlineState(false);
        ObjectManager.GetInstance().TaskCursor.transform.position = transform.position;
        ObjectManager.GetInstance().RegisterPickedUpObject(this);
        _pinchStartTime = Time.time; // Record the time when the pinch starts
    }
    public virtual void HandlePickup()
    {
        // Update object position to follow the cursor
        ApplyPickedUpBehaviour();

        // Check for drop condition
        if (PinchDetector.GetInstance().IsNoHandPinching)
        {
            // OnDrop();

            float pinchDuration = Time.time - _pinchStartTime;
            if (pinchDuration > PinchTapThreshold)
            {
                print("Dropping object after pinch duration: " + pinchDuration);
                OnDrop();
            }
            else
            {
                _pinchStartTime = Time.time; // Record the time when the pinch starts
            }
        }
    }
    public virtual void OnDrop()
    {
        IsPickedUp = false;
        // SetCancelObjectGravity(false);
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

    void Awake()
    {
        UpdateOutlineState(false);
    }

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
        UpdatePositionTo(ObjectManager.GetInstance().TaskCursor.transform.position);
        UpdateRotationTo(ObjectManager.GetInstance().TaskCursor.transform.rotation);
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

    // public void SetCancelObjectGravity(bool isFreeze)
    // {
    //     if(UseGravity == false) return;
        
    //     Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
    //     Collider collider = transform.GetComponent<Collider>();
    //     if (rigidbody != null && collider != null)
    //     {
    //         if (isFreeze == true)
    //         {
    //             rigidbody.isKinematic = true;
    //             rigidbody.useGravity = false;
    //             // collider.enabled = false;
    //         }
    //         else
    //         {
    //             rigidbody.isKinematic = false;
    //             rigidbody.useGravity = true;
    //             // collider.enabled = true;
    //         }
    //     }
    // }




    public GrabbedState GrabbedState { get; protected set; }
    public Grabbable Grabbable { get; protected set; }
    public HandGrabInteractable HandGrabInteractable { get; protected set; }

    public void SetGrabbedState(GrabbedState state)
    {
        GrabbedState = state;
        ApplyPickedUpBehaviour();
    }
}
