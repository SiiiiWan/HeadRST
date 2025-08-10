
using System.Collections.Generic;
using UnityEngine;

public class DockingTarget : MonoBehaviour
{
    private bool _poseAligned;

    public float PositionDifference;
    public float OrientationDifference;
    public List<Transform> Wedges = new List<Transform>();
    public float PositionAlignmentThreshold {get; private set;} = 0.25f;
    public float OrientationAlignmentThreshold {get; private set;} = 10f;

    void Update()
    {
        StudyControl studyControl = StudyControl.GetInstance();

        GameObject taskObject = studyControl.ObjectToBeManipulated;

        if (taskObject != null)
        {
            // _poseAligned = PositionDifference < 0.1f && OrientationDifference < 10f;
            _poseAligned = IsOrientationAligned() && IsPositionAligned();

            // _poseAligned = DistanceToGrabbedObj < 0.05f && IsAxisAligned(grabbedObject.transform, 5f);
            // _poseAligned = DistanceToGrabbedObj < 0.015f && IsAxisAligned(grabbedObject.transform, 3.5f);

            studyControl.TargetLine.IsVisible = !IsPositionAligned();
            SetActiveWedges(!IsOrientationAligned());
            taskObject.GetComponent<ManipulatableObject>().SetActiveWedges(!IsOrientationAligned());
        }

        // transform.GetComponent<Outline>().OutlineColor = _poseAligned? Color.green : Color.red;
        transform.GetComponent<Outline>().enabled = _poseAligned;

    }


    public bool IsPoseAligned()
    {
        return _poseAligned;
    }

    public bool IsPositionAligned()
    {
        GameObject taskObject = StudyControl.GetInstance().ObjectToBeManipulated;
        PositionDifference = Vector3.Distance(transform.position, taskObject.transform.position);
        return PositionDifference < PositionAlignmentThreshold* transform.localScale.x;
    }
 
    public bool IsPositionAligned_Double()
    {
        GameObject taskObject = StudyControl.GetInstance().ObjectToBeManipulated;
        PositionDifference = Vector3.Distance(transform.position, taskObject.transform.position);
        return PositionDifference < PositionAlignmentThreshold * 2 * transform.localScale.x;
    }

    public bool IsOrientationAligned()
    {
        GameObject taskObject = StudyControl.GetInstance().ObjectToBeManipulated;
        OrientationDifference = Quaternion.Angle(transform.rotation, taskObject.transform.rotation);
        return OrientationDifference < OrientationAlignmentThreshold;
        // return IsAxisAligned(taskObject.transform, OrientationAlignmentThreshold);
    }

    public bool IsOrientationAligned_Double()
    {
        GameObject taskObject = StudyControl.GetInstance().ObjectToBeManipulated;
        OrientationDifference = Quaternion.Angle(transform.rotation, taskObject.transform.rotation);
        return OrientationDifference < OrientationAlignmentThreshold * 2;
        // return IsAxisAligned(taskObject.transform, OrientationAlignmentThreshold * 2);
    }

    public bool IsAxisAligned(Transform grabbedTransform, float angleThreshold)
    {
        // Get local axes for both objects
        Vector3[] targetAxes = { transform.right, transform.up, transform.forward };
        Vector3[] grabbedAxes = { grabbedTransform.right, grabbedTransform.up, grabbedTransform.forward };

        for (int i = 0; i < targetAxes.Length; i++)
        {
            float angle = Vector3.Angle(targetAxes[i], grabbedAxes[i]);
            if (angle >= angleThreshold && Mathf.Abs(angle - 180f) >= angleThreshold)
            {
                return false;
            }
        }

        return true;
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
}
