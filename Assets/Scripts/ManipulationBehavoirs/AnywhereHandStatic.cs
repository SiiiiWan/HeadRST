using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnywhereHandStatic : ManipulationTechnique
{
    public StaticState CurrentState = StaticState.Hand;
    public TextMeshPro text;

    public override void ApplyIndirectGrabbedBehaviour()
    {
        if (IsGazeFixating == false)
        {

            CurrentState = StaticState.Gaze;
        }
        else CurrentState = StaticState.Hand;

        if (CurrentState == StaticState.Gaze)
        {
            float distance = Vector3.Distance(GazeOrigin, GrabbedObject.position);
            GrabbedObject.position = GazeOrigin + GazeDirection * distance;
        }
        else
        {
            Vector3 objectDirection = (GrabbedObject.position - GazeOrigin).normalized;
            GrabbedObject.position += objectDirection * GetHeadDepthOffset();
            GrabbedObject.position = GazeOrigin + objectDirection * Mathf.Clamp(Vector3.Distance(GrabbedObject.position, GazeOrigin), 1, 10);

            GrabbedObject.position += PinchPosition_delta * Vector3.Distance(GrabbedObject.position, GazeOrigin);
            GrabbedObject.rotation = PinchRotation_delta * GrabbedObject.rotation;
        }
    }
    

    public List<ManipulatableObject> ObjectsInGazeCone_OnGazeFixation { get; private set; } = new List<ManipulatableObject>();
    private Vector3 _accumulatedHandOffset = Vector3.zero;
    private ManipulatableObject _closestAnchor;   
    private Vector3 _anchorPosition; 

    public override Vector3 GetNewVirtualHandPosition()
    {
        text.transform.LookAt(Camera.main.transform);
        text.transform.Rotate(0, 180f, 0);

        if (GrabbedObject)
        {
            if (GrabbedObject.GetComponent<ManipulatableObject>().GrabbedState == GrabbedState.Grabbed_Indirect)
            {
                return Vector3.zero;
            }
        }

        Vector3 nextVirtualHandPosition = VirtualHandPosition;

        UpdateAndSortAnchorList();
        if (ObjectsInGazeCone_OnGazeFixation.Count > 0)
        {
            if (_closestAnchor != ObjectsInGazeCone_OnGazeFixation[0])
            {
                _accumulatedHandOffset = Vector3.zero;

                _closestAnchor = ObjectsInGazeCone_OnGazeFixation[0];

                // TODO: can be optimized when hit a shelf or plane, in this case should take the gaze hit point and aling the depth only. should be able to free with gaze pointnig.
                _anchorPosition = _closestAnchor.transform.position + (WristPosition - PinchPosition);
            }




            _accumulatedHandOffset += WristPosition_delta * Vector3.Distance(_closestAnchor.transform.position, GazeOrigin);
            nextVirtualHandPosition = _anchorPosition + _accumulatedHandOffset;

            //TODO: keep the relative position of the virtual hand to object when start and end the indirect grab
            // if (closestAnchor.IsRealHand) VirtualHandPosition = WristPosition;
            // else
            // {

            // }
        }
        else
        {
            // _accumulatedHandOffset = Vector3.zero;
            nextVirtualHandPosition = GetNewAnywhereHandPosition(VirtualHandPosition);
        }

        return nextVirtualHandPosition;
    }

    public Vector3 GetNewAnywhereHandPosition(Vector3 currentObjectPosition)
    {
        Vector3 nextObjectPosition = currentObjectPosition;

        text.text = IsGazeFixating.ToString();

        if (IsGazeFixating == false)
        {
            CurrentState = StaticState.Gaze;
        }
        else CurrentState = StaticState.Hand;

        // CurrentState = StaticState.Gaze;

        if (CurrentState == StaticState.Gaze)
        {
            float distance = Vector3.Distance(GazeOrigin, currentObjectPosition);
            nextObjectPosition = GazeOrigin + GazeDirection * distance;
        }
        else
        {
            Vector3 objectDirection = (currentObjectPosition - GazeOrigin).normalized;
            nextObjectPosition = currentObjectPosition + objectDirection * GetHeadDepthOffset();
            nextObjectPosition = GazeOrigin + objectDirection * Mathf.Clamp(Vector3.Distance(nextObjectPosition, GazeOrigin), 1, 10);

            nextObjectPosition += WristPosition_delta;
        }
        

        return nextObjectPosition;
    }

    public void UpdateAndSortAnchorList()
    {
        ManipulatableObject[] anchors = FindObjectsByType<ManipulatableObject>(FindObjectsSortMode.None);

        if (anchors.Length != 0)
        {
            var sortedAnchors = anchors
                .Where(anchor => Vector3.Angle(GazeDirection, anchor.transform.position - GazeOrigin) < 10f && anchor.GrabbedState == GrabbedState.NotGrabbed)
                .OrderBy(anchor => Vector3.Angle(GazeDirection, anchor.transform.position - GazeOrigin))
                .ToList();

            ObjectsInGazeCone_OnGazeFixation.Clear();
            ObjectsInGazeCone_OnGazeFixation.AddRange(sortedAnchors);
        }
    }


    public float minHeadSpd = 0.2f;
    public float maxHeadSpd = 0.6f;

    public float minGainDeg = 30;
    public float maxGainDeg = 10;

    float GetHeadDepthOffset()
    {
        float depthOffset;

        float max_gain = (10f - 1f) / maxGainDeg;
        float min_gain = (10f - 1f) / minGainDeg;

        depthOffset = DeltaHeadY * VitLerp(Math.Abs(HeadSpeed), min_gain, max_gain, minHeadSpd, maxHeadSpd);

        depthOffset = depthOffset * EyeHeadGain();

        return depthOffset;
    }

    float EyeHeadGain()
    {
        float eyeRange = GetEyeRange(EyeInHeadXAngle, EyeInHeadYAngle);
        float k = 3;
        float boostStartDeg = eyeRange / k;

        float gain = 1;

        float gazeAngleFromHead = Vector3.Angle(GazeDirection, HeadForward);

        if(gazeAngleFromHead >= boostStartDeg & Filtered_EyeInHeadAngle > Filtered_EyeInHeadAngle_Pre) gain = linearDepthFunction_TwoPoints(gazeAngleFromHead, new Vector2(boostStartDeg, 1), new Vector2(eyeRange, k));

        return gain;       
    }

    float GetEyeRange(float x, float y, float up_lim = 15, float down_lim = 30, float side_lim = 30)
    {
        if(y >= 0) return (1 - (1-(up_lim/side_lim)) * Mathf.Sin(Mathf.Atan2(y, x))) * side_lim;
        else return (1 + (1-(down_lim/side_lim)) * Mathf.Sin(Mathf.Atan2(y, x))) * side_lim;
    }


    protected float linearDepthFunction_TwoPoints(float x, Vector2 left, Vector2 right)
    {
        float k = (right.y - left.y) / (right.x - left.x);

        float b = right.y - k*right.x;

        return k * x + b;
    }
}