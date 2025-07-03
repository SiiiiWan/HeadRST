
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
                Random.Range(-2.5f, 2.5f),
                Random.Range(0.1f, 2f),
                Random.Range(1.5f, 9.5f)
            );

            Vector3 randomAxis = Random.onUnitSphere;
            float randomAngle = Random.Range(-30f, 30f);
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