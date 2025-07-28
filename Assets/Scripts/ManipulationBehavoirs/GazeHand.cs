using UnityEngine;

public class GazeHand : ManipulationTechnique
{
    float _armLength = 0.75f;
    float _depth = 2f;
    Vector3 _handInitPosition;

    public override void ApplyObjectFreeBehaviour()
    {
        // VirtualHandPosition = Vector3.zero;
    }

    public override void TriggerOnLookAtNewObjectBehavior()
    {
        float distanceOffset = 0.1f;
        BoxCollider box = GazingObject.GetComponent<BoxCollider>();

        if (box != null)
        {
            distanceOffset = box.size.x * 0.5f;
        }

        Vector3 gazeToObjectVector = GazingObject.transform.position - GazeOrigin;
        Vector3 gazeOriginToHandOffset = WristPosition - GazeOrigin;

        _depth = gazeToObjectVector.magnitude - distanceOffset;
        VirtualHandPosition = GazeOrigin + gazeToObjectVector.normalized * _depth + gazeOriginToHandOffset;
    }

    public override void ApplyGazingButNotGrabbingBehaviour()
    {
        VirtualHandPosition += WristPosition_delta;
    }

    public override void TriggerOnSingleHandGrabbed(ManipulatableObject obj, GrabbedState grabbedState)
    {
        //TODO: can not trigger direct grab here, because if (GazingObject.IsHand == false) conditon in l.209, it always first hit the hand.
        base.TriggerOnSingleHandGrabbed(obj, grabbedState);

        _handInitPosition = WristPosition;
    }

    public override void ApplyDirectGrabbedBehaviour()
    {
        Vector3 handOffsetVector = WristPosition - _handInitPosition;
        float handOffsetDistance = Vector3.Project(handOffsetVector, GazeDirection).magnitude;

        if (handOffsetDistance >= 0.05f) _depth += (handOffsetDistance - 0.05f) * 0.144f * (Vector3.Dot(handOffsetVector, GazeDirection) > 0 ? 1 : -1);

        VirtualHandPosition = GazeOrigin + GazeDirection * _depth +  WristPosition - GazeOrigin;
    }

}
