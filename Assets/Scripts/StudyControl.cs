
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StudyControl : Singleton<StudyControl>
{
    public ManipulationTechnique ManipulationBehavior;
    public ManipulatableObject GrabbedObject;

    public GameObject ManipulationBunny;
    public List<GameObject> DockingTargets;
    public TextMeshPro TechniqueText;

    private Vector3 _startingPosition;
    private Quaternion _startingRotation;

    void Start()
    {
        _startingPosition = ManipulationBunny.transform.position;
        _startingRotation = ManipulationBunny.transform.rotation;
        Reset();
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
            bunny.transform.position = new Vector3(
                Random.Range(-3f, 3f),
                Random.Range(0f, 2f),
                Random.Range(1f, 10f)
            );

            Vector3 randomAxis = Random.onUnitSphere;
            float randomAngle = Random.Range(-15f, 15f);
            Quaternion randomRotation = Quaternion.AngleAxis(randomAngle, randomAxis);
            bunny.transform.rotation = randomRotation * _startingRotation;
        }

        ManipulationBunny.transform.position = _startingPosition;
        ManipulationBunny.transform.rotation = _startingRotation;
    }

    public void SwitchToGazeNPinch()
    {
        ManipulationBehavior = GetComponent<GazeNPinchOrigin>();
        TechniqueText.text = "Current Technique: Gaze and Pinch";
    }

    public void SwitchToScaledHOMER()
    {
        ManipulationBehavior = GetComponent<ScaledHOMER>();
        TechniqueText.text = "Current Technique: Scaled HOMER";
    }

    public void SwitchToGazeNPinchEyeHead()
    {
        ManipulationBehavior = GetComponent<GazeNPinchEyeHead>();
        TechniqueText.text = "Current Technique: Gaze and Pinch Eye Head";
    }

    public void SwitchToHomerEyeHead()
    {
        ManipulationBehavior = GetComponent<HomerEyeHead>();
        TechniqueText.text = "Current Technique: HOMER Eye Head";
    }

}