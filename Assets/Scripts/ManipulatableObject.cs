using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;
using System.Linq;

public enum ManipulationState
{
    Idle,
    Hovered,
    PickedUp
}
    
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Outline))]
public class ManipulatableObject : MonoBehaviour
{
    public bool UseGravity = true;
    public ManipulationMode ManipulationMode { get; set; } = ManipulationMode.Direct;
    public ManipulationState ManipulationState { get; private set; } = ManipulationState.Idle;
    
    private readonly List<(Vector3 position, Quaternion rotation, float time)> _movementHistory = new List<(Vector3, Quaternion, float)>();
    private const float ThrowVelocityTimeWindow = 0.15f; // Use 150ms of history for calculation

    public Grabbable Grabbable;
    public HandGrabInteractable HandGrabInteractable;


    protected virtual void Awake()
    {
        SetOutlineActive(false);

        if(Grabbable == null) Grabbable = GetComponentInChildren<Grabbable>();
        if(HandGrabInteractable == null) HandGrabInteractable = GetComponentInChildren<HandGrabInteractable>();
    }


    protected virtual void Update()
    {
        RefreshInGazeConeState();
        ManipulationMode = ObjectManager.GetInstance().ManipulationMode;

        // Mode Switching
        if(ManipulationMode == ManipulationMode.Direct)
        {
            switch (HandGrabInteractable.State)
            {
                case InteractableState.Select:
                    ManipulationState = ManipulationState.PickedUp;
                    break;
                case InteractableState.Hover:
                    ManipulationState = ManipulationState.Hovered;
                    break;
                case InteractableState.Normal:
                    ManipulationState = ManipulationState.Idle;
                    break;
            }
        }

        if(ManipulationMode == ManipulationMode.Indirect)
        {
            switch (ManipulationState)
            {
                case ManipulationState.PickedUp:
                    if (PinchDetector.GetInstance().IsNoHandPinching)
                    {
                        ManipulationState = ManipulationState.Idle;
                        OnDrop();
                    } 
                    break;

                case ManipulationState.Hovered:
                    if(PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame)
                    {
                        ManipulationState = ManipulationState.PickedUp;  
                        OnPickedUp();
                    } 
                    break;

                case ManipulationState.Idle:
                    
                    break;
            }
        }

        // Update Visuals
        SetOutlineActive(ManipulationState == ManipulationState.Hovered);

        if(ManipulationMode == ManipulationMode.Indirect)
        {
            ApplyIndirectPickedUpBehaviour();
        }
    }

    public void SetManipulationState(ManipulationState state)
    {
        ManipulationState = state;
    }


    public virtual void OnPickedUp()
    {
        ObjectManager.GetInstance().RegisterPickedUpObject(this);
        PositionRotationProvider = ObjectManager.GetInstance().PositionRotationProvider_Global;
    }

    public virtual void OnDrop()
    {
        _movementHistory.Clear();
        ObjectManager.GetInstance().UnregisterPickedUpObject(this);
    }

    public float AngleToGaze { get; private set; }
    private bool _isInGazeCone;
    public bool IsInGazeCone 
    {   
        get { return _isInGazeCone; } 

        private set
            {
                if (_isInGazeCone == value) return;

                _isInGazeCone = value;

                if(value == true)
                {
                    ObjectManager.GetInstance().RegisterFocusedObj(this);
                }
                else
                {
                    ObjectManager.GetInstance().UnregisterFocusedObj(this);
                }
            }
    }


    public PositionRotationProvider PositionRotationProvider { get; set; }
    public virtual void ApplyIndirectPickedUpBehaviour()
    {
        if(PositionRotationProvider == null) return;

        UpdatePositionTo(PositionRotationProvider.GetPositionOutput(transform.position));
        UpdateRotationTo(PositionRotationProvider.GetRotationOutput(transform.rotation));

        _movementHistory.Add((transform.position, transform.rotation, Time.time));
        _movementHistory.RemoveAll(p => Time.time - p.time > ThrowVelocityTimeWindow);
    }

    public void RefreshInGazeConeState()
    {
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsInGazeCone = AngleToGaze <= ObjectManager.GazeConeSize || EyeGaze.GetInstance().GetGazeHitTrans() == transform;
    }

    public void SetOutlineActive(bool isEnabled)
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
            if (isFreeze == true || UseGravity == false)
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
}

