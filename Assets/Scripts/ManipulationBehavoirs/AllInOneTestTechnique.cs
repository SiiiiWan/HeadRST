using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class AllInOneTestTechnique : ManipulationTechnique
{
    public CentricType CentricType;
    public HandGainFunction HandGainFunction;
    public bool AddGaze;
    public bool AddHead;

    [Header("Text Bindings")]
    public TextMeshPro HandFunctionText;
    public TextMeshPro GazeFunctionText;
    public TextMeshPro HeadFunctionText;
    public TextMeshPro HeadCentricText;


    private Linescript _handRayLine;


    [Header("PRISM Parameters")]
    public float scalingConstant = 0.15f; //TODO: fine parameters from papers
    public float minVelocityThreshold = 0.01f;


    [Header("HOMER Parameters")]
    public float TorsoOffset = 0.35f;


    public override void OnSingleHandGrabbed(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        Vector3 handPos = handData.GetHandPosition(usePinchTip: true);
        Vector3 handDirection = handData.GetHandDirection();
        Vector3 torsoPosition = Camera.main.transform.position + Vector3.down * TorsoOffset;

        HOMER_OnGrabbed_Init(torsoPosition, handPos, target.position);
        HandRaycast_OnGrabbed_Init(handPos, handDirection, target.position);
    }

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        EyeGaze gazeData = EyeGaze.GetInstance();
        HeadMovement headData = HeadMovement.GetInstance();

        Vector3 gazeOrigin = gazeData.GetGazeRay().origin;
        Vector3 gazeDirection = gazeData.GetGazeRay().direction;


        Vector3 handPos_delta = handData.GetDeltaHandPosition(usePinchTip: true);
        Vector3 handPos = handData.GetHandPosition(usePinchTip: true);

        bool updateObjectPosToGazePoint = gazeData.IsSaccading() && AddGaze;

        bool isBallisticHeadMovement = headData.HeadSpeed >= 0.2f || Math.Abs(headData.HeadAcc) >= 1f;
        // bool isEyeHeadAngleExceededLimit = Vector3.Angle(gazeDirection, Camera.main.transform.forward) > 10f;
        bool HandNotFastMoving = handData.GetHandSpeed() <= 0.5f;
        bool addDepthOffsetWithHead = isBallisticHeadMovement && HandNotFastMoving && AddHead;
        // bool addDepthOffsetWithHead = (isBallisticHeadMovement || isEyeHeadAngleExceededLimit) && HandNotFastMoving && AddHead;



        bool isHOMER = HandGainFunction == HandGainFunction.HOMER || HandGainFunction == HandGainFunction.ScaledHOMER;

        _handRayLine.IsVisible = false;

        if (updateObjectPosToGazePoint)
        {
            if (isHOMER)
            {
                Ray gazeRay = EyeGaze.GetInstance().GetGazeRay();
                Vector3 torsoPosition = Camera.main.transform.position + Vector3.down * TorsoOffset;
                float targetToGazeDistance = Vector3.Distance(gazeRay.origin, target.position);
                HOMER_OnGrabbed_Init(torsoPosition, handPos, gazeRay.origin + gazeRay.direction.normalized * targetToGazeDistance);
            }
            else if (HandGainFunction == HandGainFunction.HandRaycast)
            {
                Ray gazeRay = EyeGaze.GetInstance().GetGazeRay();
                float targetToGazeDistance = Vector3.Distance(gazeRay.origin, target.position);
                HandRaycast_OnGrabbed_Init(handPos, handData.GetHandDirection(), gazeRay.origin + gazeRay.direction.normalized * targetToGazeDistance);
            }
            else {
                float distance = Vector3.Distance(gazeOrigin, target.position);
                target.position = gazeOrigin + gazeDirection * distance;                
            }
        }
        else
        {
            target.position = GetNextTargetPosbyHand(target, handPos, handPos_delta);

            if (addDepthOffsetWithHead)
            {

                Vector3 startPoint = CentricType == CentricType.HandCentric ? handPos : gazeOrigin;
                Vector3 movementDirection = (target.position - startPoint).normalized;
                float DeltaHeadY = HeadMovement.GetInstance().DeltaHeadY;
                float depthOffset = DeltaHeadY * 0.2f;

                // if (isEyeHeadAngleExceededLimit)
                // {
                //     if (gazeData.EyeInHeadYAngle > 0f && DeltaHeadY < 0) depthOffset = -2f * Time.deltaTime;

                //     if (gazeData.EyeInHeadYAngle < 0f && DeltaHeadY > 0) depthOffset = 2f * Time.deltaTime;

                // }

                Vector3 headDepthPosOffset = movementDirection * depthOffset;
                Vector3 nextTargetPosition = target.position + headDepthPosOffset;

                // Depth cap
                float nextTargetDistToHand = Vector3.Distance(nextTargetPosition, handPos);
                if (nextTargetDistToHand >= 0.05f & nextTargetDistToHand <= 10f)
                {
                    if (isHOMER)
                    {
                        _HOMER_offsetVector += headDepthPosOffset;
                    }
                    else if (HandGainFunction == HandGainFunction.HandRaycast)
                    {
                        _handRaycast_distanceOffset += depthOffset;
                    }
                    else
                    {
                        target.position = nextTargetPosition;
                    }
                }   

                // ray visual feedback
                _handRayLine.IsVisible = CentricType == CentricType.HandCentric;
            }

        }

        // Apply rotation
        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: false);
        target.rotation = deltaRot * target.rotation;

        // Update visual feedback
        if(HandGainFunction == HandGainFunction.HandRaycast) _handRayLine.IsVisible = true;

        _handRayLine.SetPostion(handPos, handPos + (target.position - handPos) * 0.9f);

        // Update UI text
        UpdateUIElements();
    }

    
    Vector3 GetNextTargetPosbyHand(Transform target, Vector3 handPos, Vector3 handPos_delta)
    {
        Vector3 currentTargetPosition = target.position;

        switch (HandGainFunction)
        {
            case HandGainFunction.isomophic:
                return currentTargetPosition + handPos_delta;
            case HandGainFunction.visual:
                return currentTargetPosition + handPos_delta * GetVisualGain(target);
            case HandGainFunction.prism:
                return currentTargetPosition + handPos_delta * GetPrismGain();
            case HandGainFunction.gazeNpinch:
                return currentTargetPosition + handPos_delta * GetGazePinchGain(target);
            case HandGainFunction.HOMER:
                return GetObjectPosition_HOMER(handPos);
            case HandGainFunction.ScaledHOMER:
                return GetObjectPosition_HOMER(handPos - handPos_delta + handPos_delta * GetPrismGain());
            case HandGainFunction.HandRaycast: 
                return GetHandRaycastObjectPosition(handPos);
            default:
                return currentTargetPosition + handPos_delta;
        }
    }

    float GetVisualGain(Transform target)
    {
        return Vector3.Distance(target.position, Camera.main.transform.position) / Vector3.Distance(HandPosition.GetInstance().GetHandPosition(usePinchTip: true), Camera.main.transform.position);
    }

    float GetPrismGain()
    {
        float handSpeed = HandPosition.GetInstance().GetHandSpeed();

        if (handSpeed < minVelocityThreshold)
        {
            return 0f;
        }

        return Mathf.Min(1.2f, handSpeed / scalingConstant);
    }

    float GetGazePinchGain(Transform target)
    {
        return Vector3.Distance(target.position, Camera.main.transform.position);
    }


    Quaternion _handRaycast_directionOffset;
    float _handRaycast_distanceOffset;
    void HandRaycast_OnGrabbed_Init(Vector3 handPos, Vector3 handDir, Vector3 objectPosition)
    {
        Vector3 handToObjectDir = (objectPosition - handPos).normalized;
        _handRaycast_directionOffset = Quaternion.FromToRotation(handDir, handToObjectDir);
        _handRaycast_distanceOffset = Vector3.Distance(handPos, objectPosition);
    }

    Vector3 GetHandRaycastObjectPosition(Vector3 handPos)
    {
        Vector3 handToObjectDir = _handRaycast_directionOffset * HandPosition.GetInstance().GetHandDirection();
        return handPos + handToObjectDir * _handRaycast_distanceOffset;
    }

    #region HOMER

    private Vector3 _torsoPos_init;
    private Vector3 _handPos_init;
    private Vector3 _objectPos_init;
    private float _distanceHandToTorso_init;
    private float _distanceObjectToTorso_init;
    private Vector3 _HOMER_offsetVector;
    void HOMER_OnGrabbed_Init(Vector3 torso, Vector3 hand, Vector3 objectPosition)
    {
        _torsoPos_init = torso;
        _handPos_init = hand;
        _objectPos_init = objectPosition;

        _distanceHandToTorso_init = Vector3.Distance(_torsoPos_init, _handPos_init);
        _distanceObjectToTorso_init = Vector3.Distance(_torsoPos_init, _objectPos_init);

        Vector3 rayDirection = (_handPos_init - _torsoPos_init).normalized;
        Vector3 expectedObjectPosition = _torsoPos_init + rayDirection * _distanceObjectToTorso_init;
        _HOMER_offsetVector = _objectPos_init - expectedObjectPosition;
    }

    Vector3 GetObjectPosition_HOMER(Vector3 currentHandPosition)
    {
        Vector3 currentTorsoPosition = Camera.main.transform.position + Vector3.down * TorsoOffset;

        float currentHandDistance = Vector3.Distance(currentTorsoPosition, currentHandPosition);
        float scaledDistance = _distanceObjectToTorso_init / _distanceHandToTorso_init * currentHandDistance;

        Vector3 bodyToHand = (currentHandPosition - currentTorsoPosition).normalized;
        Vector3 objectPosition = currentTorsoPosition + bodyToHand * scaledDistance + _HOMER_offsetVector;
        return objectPosition;
    }

    #endregion

    #region Switching Functions

    public void SwitchToIsomorphic()
    {
        HandGainFunction = HandGainFunction.isomophic;
        UpdateUIElements();
    }
    public void SwitchToVisual()
    {
        HandGainFunction = HandGainFunction.visual;
        UpdateUIElements();
    }

    public void SwitchToPrism()
    {
        HandGainFunction = HandGainFunction.prism;
        UpdateUIElements();
    }
    public void SwitchToGazeNPinch()
    {
        HandGainFunction = HandGainFunction.gazeNpinch;
        UpdateUIElements();
    }
    public void SwitchToHOMER()
    {
        HandGainFunction = HandGainFunction.HOMER;
        UpdateUIElements();
    }
    public void SwitchToScaledHOMER()
    {
        HandGainFunction = HandGainFunction.ScaledHOMER;
        UpdateUIElements();
    }
    public void SwitchToHandRaycast()
    {
        HandGainFunction = HandGainFunction.HandRaycast;
        UpdateUIElements();
    }

    public void SwitchGaze()
    {
        AddGaze = !AddGaze;
        UpdateUIElements();
    }
    public void SwitchHead()
    {
        AddHead = !AddHead;
        UpdateUIElements();
    }
    public void SwitchCentricType()
    {
        CentricType = CentricType == CentricType.HandCentric ? CentricType.HeadCentric : CentricType.HandCentric;
        UpdateUIElements();
    }

    public void Reset()
    {
        AddGaze = false;
        AddHead = false;
        HandGainFunction = HandGainFunction.isomophic;

        UpdateUIElements();
    }

    void UpdateUIElements()
    {
        HandFunctionText.text = "Hand Function: " + HandGainFunction.ToString();
        GazeFunctionText.text = AddGaze ? "ON" : "OFF";
        HeadFunctionText.text = AddHead ? "ON" : "OFF";
        HeadCentricText.text = CentricType == CentricType.HandCentric ? "Hand Centric" : "Head Centric";

        HeadCentricText.transform.parent.gameObject.SetActive(AddHead);
    }

    #endregion

    void Awake()
    {
        _handRayLine = new Linescript();
    }

}