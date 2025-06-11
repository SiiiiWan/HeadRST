using System;
using UnityEngine;

public class GazeScaledHOMER : ManipulationTechnique
{

    public float TorsoOffset = 0.35f; // Offset from the camera to the torso

    private Vector3 lastHandPosition;
    private float scalingConstant = 0.15f; //TODO: fine parameters from papers
    private float minVelocityThreshold = 0.01f;

    public CentricType CentricType;
    private Linescript _handRayLine;

    public override void OnSingleHandGrabbed(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();
        Vector3 handPos = handData.GetHandPosition(usePinchTip: false);

        Vector3 torsoPosition = Camera.main.transform.position + Vector3.down * TorsoOffset;

        StartManipulation_HOMER(torsoPosition, handPos, target.position);

        lastHandPosition = handPos;
    }

    public override void ApplySingleHandGrabbedBehaviour(Transform target)
    {
        HandPosition handData = HandPosition.GetInstance();

        Vector3 currentHandPosition = handData.GetHandPosition(usePinchTip: false);

        //TODO: update hand position tracking

        Vector3 handVelocity = (currentHandPosition - lastHandPosition) / Time.deltaTime;
        float handSpeed = handVelocity.magnitude;

        float scaleFactor = Mathf.Min(1.2f, handSpeed / scalingConstant);
        if (handSpeed < minVelocityThreshold)
        {
            scaleFactor = 0f; // Freeze motion for precision
        }

        Vector3 scaledHandMovement = handVelocity.normalized * handSpeed * scaleFactor * Time.deltaTime;
        Vector3 scaledHandPosition = lastHandPosition + scaledHandMovement;

        lastHandPosition = currentHandPosition;

        _handRayLine.IsVisible = false;

        if (EyeGaze.GetInstance().IsSaccading())
        {
            Ray gazeRay = EyeGaze.GetInstance().GetGazeRay();
            Vector3 torsoPosition = Camera.main.transform.position + Vector3.down * TorsoOffset;
            float targetToGazeDistance = Vector3.Distance(gazeRay.origin, target.position);
            StartManipulation_HOMER(torsoPosition, currentHandPosition, gazeRay.origin + gazeRay.direction.normalized * targetToGazeDistance);
        }
        else if ((HeadMovement.GetInstance().HeadSpeed >= 0.2f || Math.Abs(HeadMovement.GetInstance().HeadAcc) >= 1f) && handData.GetHandSpeed() <= 0.5f)
        {
            Vector3 startPoint = CentricType == CentricType.HandCentric ? handData.GetHandPosition(usePinchTip: true) : EyeGaze.GetInstance().GetGazeRay().origin;
            Vector3 movementDirection = (target.position - startPoint).normalized;

            Vector3 headDepthPosOffset = movementDirection * HeadMovement.GetInstance().DeltaHeadY * 0.2f;

            offsetVector += headDepthPosOffset;
            //TODO: can add a eye head angle exiding just warp to the depth limits

            // float nextTargetDistToHand = Vector3.Distance(nextTargetPosition, handData.GetHandPosition(usePinchTip: true));
            // if (nextTargetDistToHand >= 0.05f & nextTargetDistToHand <= 10f)
            // {
            //     target.position = nextTargetPosition;
            // }
            _handRayLine.IsVisible = true;
        }


        target.position = UpdateObjectPosition_HOMER(scaledHandPosition);


        Quaternion deltaRot = HandPosition.GetInstance().GetDeltaHandRotation(usePinchTip: true);
        target.rotation = deltaRot * target.rotation;

        _handRayLine.SetPostion(handData.GetHandPosition(usePinchTip: true), target.position);

    }

    private Vector3 torsoPosition;
    private Vector3 initialHandPosition;
    private Vector3 initialObjectPosition;
    private float distanceHandToTorso;
    private float distanceObjectToTorso;
    private Vector3 offsetVector;

    public void StartManipulation_HOMER(Vector3 torso, Vector3 hand, Vector3 objectPosition)
    {
        torsoPosition = torso;
        initialHandPosition = hand;
        initialObjectPosition = objectPosition;

        distanceHandToTorso = Vector3.Distance(torsoPosition, initialHandPosition);
        distanceObjectToTorso = Vector3.Distance(torsoPosition, initialObjectPosition);

        Vector3 rayDirection = (initialHandPosition - torsoPosition).normalized;
        Vector3 expectedObjectPosition = torsoPosition + rayDirection * distanceObjectToTorso;
        offsetVector = initialObjectPosition - expectedObjectPosition;
    }

    public Vector3 UpdateObjectPosition_HOMER(Vector3 currentHandPosition)
    {
        float currentHandDistance = Vector3.Distance(torsoPosition, currentHandPosition);
        float scaledDistance = (distanceObjectToTorso / distanceHandToTorso) * currentHandDistance;

        Vector3 bodyToHand = (currentHandPosition - torsoPosition).normalized;
        Vector3 objectPosition = torsoPosition + bodyToHand * scaledDistance + offsetVector;
        return objectPosition;
    }
    
    void Awake()
    {
        _handRayLine = new Linescript();
    }
    
}
