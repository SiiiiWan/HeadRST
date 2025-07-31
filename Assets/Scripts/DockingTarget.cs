
using UnityEngine;

public class DockingTarget : MonoBehaviour
{
    private bool _poseAligned;

    public float PositionDifference;
    public float OrientationDifference;
    void Update()
    {
        StudyControl studyControl = StudyControl.GetInstance();

        ManipulatableObject grabbedObject = studyControl.ManipulationBehavior.GrabbedObject;

        if (grabbedObject != null)
        {
            PositionDifference = Vector3.Distance(transform.position, grabbedObject.transform.position);
            OrientationDifference = Quaternion.Angle(transform.rotation, grabbedObject.transform.rotation);
            _poseAligned = PositionDifference < 0.1f && OrientationDifference < 10f;
            // _poseAligned = DistanceToGrabbedObj < 0.05f && IsAxisAligned(grabbedObject.transform, 5f);
            // _poseAligned = DistanceToGrabbedObj < 0.015f && IsAxisAligned(grabbedObject.transform, 3.5f);

        }

        // transform.GetComponent<Outline>().OutlineColor = _poseAligned? Color.green : Color.red;
        transform.GetComponent<Outline>().enabled = _poseAligned;

    }


    public bool IsPoseAligned()
    {
        return _poseAligned;
    }

}
