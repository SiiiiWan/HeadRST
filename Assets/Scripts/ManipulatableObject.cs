using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;
using System.Linq;

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
    
    private readonly List<(Vector3 position, Quaternion rotation, float time)> _movementHistory = new List<(Vector3, Quaternion, float)>();
    private const float ThrowVelocityTimeWindow = 0.15f; // Use 150ms of history for calculation

    public bool IsPickedUp { get; protected set; }
    public virtual void OnPickup()
    {
        IsPickedUp = true;
        if(Grabbable == null)
        {
            if(ObjectManager.GetInstance().AllowIndirectGrab) SetGrabbedState(GrabbedState.Grabbed_Indirect);
            else return;
        }
        else
        {
            if(Grabbable.SelectingPointsCount > 0)
            {
                if(ObjectManager.GetInstance().AllowDirectGrab) SetGrabbedState(GrabbedState.Grabbed_Direct);
                else return;
            }
            else
            {
                if(ObjectManager.GetInstance().AllowIndirectGrab) SetGrabbedState(GrabbedState.Grabbed_Indirect);
                else return;
            }
        }

        SetCancelObjectGravity(true);
        UpdateOutlineState(false);
        ObjectManager.GetInstance().RegisterPickedUpObject(this);
        PositionRotationProvider = ObjectManager.GetInstance().PositionRotationProvider_Global;
    }
    public virtual void HandlePickup()
    {
        // Update object position to follow the cursor
        if(GrabbedState == GrabbedState.Grabbed_Indirect) ApplyPickedUpBehaviour();

        // Check for drop condition
        if (PinchDetector.GetInstance().IsNoHandPinching)
        {
            OnDrop();
        }
    }
    public virtual void OnDrop()
    {
        IsPickedUp = false;
        SetGrabbedState(GrabbedState.NotGrabbed);

        SetCancelObjectGravity(false);
        _movementHistory.Clear();
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
        SetCancelObjectGravity(!ApplyGravityByDefault);

        if(Grabbable == null)
        {
            Grabbable = GetComponentInChildren<Grabbable>();
        }
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
                if(IsHovering || Grabbable.SelectingPointsCount > 0) OnPickup();
            }
        }
    }

    public PositionRotationProvider PositionRotationProvider {get; set; }
    public virtual void ApplyPickedUpBehaviour()
    {
        UpdatePositionTo(PositionRotationProvider.GetPositionOutput(transform.position));
        UpdateRotationTo(PositionRotationProvider.GetRotationOutput(transform.rotation));

        _movementHistory.Add((transform.position, transform.rotation, Time.time));
        _movementHistory.RemoveAll(p => Time.time - p.time > ThrowVelocityTimeWindow);
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
        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        Collider collider = transform.GetComponent<Collider>();
        if (rigidbody != null && collider != null)
        {
            if (isFreeze == true || ApplyGravityByDefault == false)
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                // rigidbody.linearVelocity = Vector3.zero;
                // collider.enabled = false;
            }
            else
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                rigidbody.linearVelocity = GetLinearVelocity() / 2;
                // collider.enabled = true;
            }
        }
    }

    public Vector3 GetLinearVelocity()
    {
            var first = _movementHistory.First();
            var last = _movementHistory.Last();
            float timeDelta = last.time - first.time;

            if (timeDelta > 0)
            {
                // Calculate linear velocity
                return (last.position - first.position) / timeDelta;
            }

            return Vector3.zero;
    }


    public GrabbedState GrabbedState { get; protected set; }
    public Grabbable Grabbable { get; protected set; }
    public HandGrabInteractable HandGrabInteractable { get; protected set; }

    public void SetGrabbedState(GrabbedState state)
    {
        GrabbedState = state;
    }
}

