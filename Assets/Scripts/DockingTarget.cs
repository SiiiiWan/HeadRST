
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

        ManipulatableObject grabbedObject = studyControl.GrabbedObject;

        if (grabbedObject != null)
        {
            DistanceToGrabbedObj = Vector3.Distance(transform.position, grabbedObject.transform.position);
            OrientationDifference = Quaternion.Angle(transform.rotation, grabbedObject.transform.rotation);    
            _poseAligned = DistanceToGrabbedObj < 0.1f && OrientationDifference < 30f;
        }

        // transform.GetComponent<Outline>().OutlineColor = _poseAligned? Color.green : Color.red;
        transform.GetComponent<Outline>().enabled = _poseAligned;

    }


    public bool IsPoseAligned()
    {
        return _poseAligned;
    }

}
