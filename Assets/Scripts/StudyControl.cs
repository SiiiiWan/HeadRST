
using System.Collections.Generic;
using UnityEngine;

public class StudyControl : Singleton<StudyControl>
{
    public ManipulationTechnique ManipulationBehavior;
    public ManipulatableObject GrabbedObject;

    public GameObject ManipulationBunny;
    public List<GameObject> DockingTargets;

    private Vector3 _startingPosition;
    private Quaternion _startingRotation;

    void Start()
    {
        _startingPosition = ManipulationBunny.transform.position;
        _startingRotation = ManipulationBunny.transform.rotation;

        
    }

    void Update()
    {
        if (PinchDetector.GetInstance().PinchState == PinchState.NotPinching && GrabbedObject != null)
        {
            GrabbedObject = null;
        }

    }

    public void Reset()
    {
        foreach (GameObject bunny in DockingTargets)
        {
            bunny.SetActive(true);
        }
        
        ManipulationBunny.transform.position = _startingPosition;
        ManipulationBunny.transform.rotation = _startingRotation;
    }

}