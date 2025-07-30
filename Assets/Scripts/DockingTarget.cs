
using UnityEngine;

public class DockingTarget : MonoBehaviour
{
    private bool _poseAligned;
    private float _offset = 1.5f;

    public float DistanceToGrabbedObj;
    public float OrientationDifference;
    void Update()
    {
        StudyControl studyControl = StudyControl.GetInstance();

        ManipulatableObject grabbedObject = studyControl.ManipulationBehavior.GrabbedObject;

        if (grabbedObject != null)
        {
            DistanceToGrabbedObj = Vector3.Distance(transform.position, grabbedObject.transform.position);
            OrientationDifference = Quaternion.Angle(transform.rotation, grabbedObject.transform.rotation);
            // _poseAligned = DistanceToGrabbedObj < 0.1f && OrientationDifference < 10f;
            _poseAligned = DistanceToGrabbedObj < 0.05f && IsAxisAligned(grabbedObject.transform, 5f);
            // _poseAligned = DistanceToGrabbedObj < 0.015f && IsAxisAligned(grabbedObject.transform, 3.5f);

        }

        // transform.GetComponent<Outline>().OutlineColor = _poseAligned? Color.green : Color.red;
        transform.GetComponent<Outline>().enabled = _poseAligned;

    }


    public bool IsPoseAligned()
    {
        return _poseAligned;
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

}
