
using System.Collections.Generic;
using UnityEngine;

public class DockingTarget : MonoBehaviour
{
    private bool _poseAligned;

    public float PositionDifference;
    public float OrientationDifference;
    public List<Transform> Wedges = new List<Transform>();
    public float PositionAlignmentThreshold { get; private set; } = 1.5f;
    public float OrientationAlignmentThreshold { get; private set; } = 10f;

    void Update()
    {
        StudyControl studyControl = StudyControl.GetInstance();

        GameObject taskObject = studyControl.ObjectToBeManipulated;

        if (taskObject != null)
        {
            _poseAligned = IsOrientationAligned() && IsPositionAligned();

            UpdateAlignmentHistory();

            studyControl.TargetLine.IsVisible = !IsPositionAligned();
            SetActiveWedges(!IsOrientationAligned());
            taskObject.GetComponent<ManipulatableObject>().SetActiveWedges(!IsOrientationAligned());
        }

        transform.GetComponent<Outline>().enabled = _poseAligned;

    }


    public bool IsPoseAligned()
    {
        return _poseAligned;
    }

    public float GetPositionAlignmentThreshold()
    {
        return MathFunctions.Deg2Meter(PositionAlignmentThreshold, Vector3.Distance(StudyControl.GetInstance().HeadPosition_OnTrialStart, transform.position));
    }

    public bool IsPositionAligned()
    {
        GameObject taskObject = StudyControl.GetInstance().ObjectToBeManipulated;
        PositionDifference = Vector3.Distance(transform.position, taskObject.transform.position);
        return PositionDifference < GetPositionAlignmentThreshold();
    }

    public bool IsOrientationAligned()
    {
        GameObject taskObject = StudyControl.GetInstance().ObjectToBeManipulated;
        OrientationDifference = Quaternion.Angle(transform.rotation, taskObject.transform.rotation);
        return OrientationDifference < OrientationAlignmentThreshold;
    }


    public void SetActiveWedges(bool isActive)
    {
        foreach (Transform wedge in Wedges)
        {
            if (wedge != null)
            {
                wedge.gameObject.SetActive(isActive);
            }
        }
    }
    
    public bool PoseAligned_200msAgo { get; private set; }
    public bool PositionAligned_200msAgo { get; private set; }
    public bool OrientationAligned_200msAgo { get; private set; }
    
    private Queue<(float timestamp, bool poseAligned, bool positionAligned, bool orientationAligned)> _alignmentHistory = new Queue<(float, bool, bool, bool)>();
    private const float HISTORY_DURATION = 0.2f; // 200ms
    private void UpdateAlignmentHistory()
    {
        _alignmentHistory.Enqueue((Time.time, IsPoseAligned(), IsPositionAligned(), IsOrientationAligned()));

        while (_alignmentHistory.Count > 0 && Time.time - _alignmentHistory.Peek().timestamp > HISTORY_DURATION)
        {
            _alignmentHistory.Dequeue();
        }

        if (_alignmentHistory.Count > 0)
        {
            var oldestEntry = _alignmentHistory.Peek();
            PoseAligned_200msAgo = oldestEntry.poseAligned;
            PositionAligned_200msAgo = oldestEntry.positionAligned;
            OrientationAligned_200msAgo = oldestEntry.orientationAligned;
        }
        else
        {
            PoseAligned_200msAgo = IsPoseAligned();
            PositionAligned_200msAgo = IsPositionAligned();
            OrientationAligned_200msAgo = IsOrientationAligned();
        }
    }
}
