using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic;
using System.Linq;

public enum ManipulationState
{
    Idle,
    Hovered,
    Transformation,
    Scaling
}
    
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Outline))]
public class ManipulatableObject : MonoBehaviour
{
    public bool UseGravity;
    public ManipulationMode ManipulationMode { get; set; } = ManipulationMode.Direct;
    public ManipulationState ManipulationState { get; private set; } = ManipulationState.Idle;
    
    private readonly List<(Vector3 position, Quaternion rotation, float time)> _movementHistory = new List<(Vector3, Quaternion, float)>();
    private const float ThrowVelocityTimeWindow = 0.15f; // Use 150ms of history for calculation

    private List<Handedness_v> _grabbedHands = new List<Handedness_v>();
    
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
        RefreshSelfInGazeConeState();
        ManipulationMode = ObjectManager.GetInstance().ManipulationMode;

        // Mode Switching
        if(ManipulationMode == ManipulationMode.Direct)
        {
            switch (HandGrabInteractable.State)
            {
                case InteractableState.Select:
                    ManipulationState = ManipulationState.Transformation;
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
            switch (_grabbedHands.Count)
            {
                case 2:
                    ApplyIndirectScalingBehaviour();
                    break;
                case 1: 
                    ApplyIndirectTransformationBehaviour();
                    break;

                case 0: break;
                default: break;
            }
        }

        // Update Visuals
        SetOutlineActive(ManipulationState == ManipulationState.Hovered);
    }

    void RightHandPinch_Close_Handler()
    {
        if(ObjectManager.GetInstance().IsObjectClosestFocused(this))
        {
            _grabbedHands.Add(Handedness_v.Right);
        }
    }

    void RightHandPinch_Release_Handler()
    {
        if(_grabbedHands.Contains(Handedness_v.Right))
        {
            _grabbedHands.Remove(Handedness_v.Right);
        }
    }

    void LeftHandPinch_Close_Handler()
    {
        if(ObjectManager.GetInstance().IsObjectClosestFocused(this))
        {
            _grabbedHands.Add(Handedness_v.Left);
        }        
    }

    void LeftHandPinch_Release_Handler()
    {
        if(_grabbedHands.Contains(Handedness_v.Left))
        {
            _grabbedHands.Remove(Handedness_v.Left);
        }
    }

    public void SetManipulationState(ManipulationState state)
    {
        ManipulationState = state;
    }


    public virtual void OnPickedUp()
    {
        // ObjectManager.GetInstance().RegisterPickedUpObject(this);
    }

    public virtual void OnDrop()
    {
        _movementHistory.Clear();
        // ObjectManager.GetInstance().UnregisterPickedUpObject(this);
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

    public virtual void ApplyIndirectTransformationBehaviour()
    {
        IObjectPositionRotationProvider objectPositionRotationProvider = ObjectManager.GetInstance().ObjectPositionRotationProvider;
        if(objectPositionRotationProvider == null) return;

        transform.position = objectPositionRotationProvider.GetPositionOutput(transform.position, _grabbedHands[0]);
        transform.rotation = objectPositionRotationProvider.GetRotationOutput(transform.rotation, _grabbedHands[0]);

        UpdateMovementHistory();
    }

    public virtual void ApplyIndirectScalingBehaviour()
    {
        IObjectPositionRotationProvider objectPositionRotationProvider = ObjectManager.GetInstance().ObjectPositionRotationProvider;
        if(objectPositionRotationProvider == null) return;

        transform.localScale = objectPositionRotationProvider.GetScaleOutput(transform.localScale);
        
        UpdateMovementHistory();
    }

    void UpdateMovementHistory()
    {
        _movementHistory.Add((transform.position, transform.rotation, Time.time));
        _movementHistory.RemoveAll(p => Time.time - p.time > ThrowVelocityTimeWindow);
    }

    public void RefreshSelfInGazeConeState()
    {
        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsInGazeCone = AngleToGaze <= ObjectManager.GazeConeSize || EyeGaze.GetInstance().GetGazeHitTrans() == transform;
    }

    public void SetOutlineActive(bool isEnabled)
    {
        if(GetComponent<Outline>() == null) return;
        GetComponent<Outline>().enabled = isEnabled;
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

    protected virtual void OnEnable() 
    {
        PinchDetector.OnRightHandPinch_Close += RightHandPinch_Close_Handler;
        PinchDetector.OnRightHandPinch_Release += RightHandPinch_Release_Handler;
        PinchDetector.OnLeftHandPinch_Close += LeftHandPinch_Close_Handler;
        PinchDetector.OnLeftHandPinch_Release += LeftHandPinch_Release_Handler;
        
    }

    protected virtual void OnDisable()
    {
        PinchDetector.OnRightHandPinch_Close -= RightHandPinch_Close_Handler;
        PinchDetector.OnRightHandPinch_Release -= RightHandPinch_Release_Handler;
        PinchDetector.OnLeftHandPinch_Close -= LeftHandPinch_Close_Handler;
        PinchDetector.OnLeftHandPinch_Release -= LeftHandPinch_Release_Handler;        
    }
}

