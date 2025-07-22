
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections;

public enum StartPositionLabels
{
    FrontUpperLeft,
    FrontUpperRight,
    FrontLowerLeft,
    FrontLowerRight,
    BackUpperLeft,
    BackUpperRight,
    BackLowerLeft,
    BackLowerRight
}

public class StudyControl : Singleton<StudyControl>
{
    public ManipulationTechnique ManipulationBehavior;
    public ManipulatableObject GrabbedObject;

    public TextMeshPro TechniqueText;

    public GameObject TargetPrefab;
    public GameObject ObjectPrefab;

    public GameObject ObjectToBeManipulated;
    public GameObject TargetIndicator;

    public List<(float depth, float amplitude)> DepthAmplitudeCombinations = new List<(float, float)>();
    public List<StartPositionLabels> StartPositionLabelsList = new List<StartPositionLabels>();

    public float MinDepth = 2f; // in meters
    public float VerticalAmpDev = 20f;

    public List<float> Depths = new List<float>();

    public List<float> Amplitudes = new List<float>();

    public

    void Start()
    {
        // StartTask();
    }

    void Update()
    {
        if (TargetIndicator == null || ObjectToBeManipulated == null)
        {
            return; // No target indicator to check
        }

        if (PinchDetector.GetInstance().PinchState == PinchState.NotPinching && GrabbedObject != null)
        {
            if (TargetIndicator.GetComponent<DockingTarget>().IsPoseAligned())
            {
                Destroy(ObjectToBeManipulated);
                Destroy(TargetIndicator);

                ObjectToBeManipulated = null;
                TargetIndicator = null;
            }

            GrabbedObject = null;
        }

    }




    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject obj = Instantiate(prefab, position, rotation);
        obj.transform.SetParent(transform);

        return obj;
    }

    public void StartTrial(Vector3 startPos, Vector3 endPos, out GameObject startObj, out GameObject target)
    {
        startObj = SpawnPrefab(ObjectPrefab, startPos, Quaternion.identity);

        Quaternion randomRotationOffset = Quaternion.AngleAxis(Random.Range(-30f, 30f), Random.onUnitSphere);
        target = SpawnPrefab(TargetPrefab, endPos, randomRotationOffset * startObj.transform.rotation);
    }

    public void StartTask()
    {
        if (ObjectToBeManipulated != null)
        {
            Destroy(ObjectToBeManipulated);
            ObjectToBeManipulated = null;
        }

        if (TargetIndicator != null)
        {
            Destroy(TargetIndicator);
            TargetIndicator = null;
        }

        DepthAmplitudeCombinations = GetShuffledDepthAmplitudeCombinations();

        StartCoroutine(RunTrials());
    }

    private IEnumerator RunTrials()
    {
        foreach ((float depth, float amplitude) depthAmpCondition in DepthAmplitudeCombinations)
        {
            StartPositionLabelsList = GetShuffledStartPositionLabels();

            foreach (StartPositionLabels startPosition in StartPositionLabelsList)
            {
                Vector3 forwardDir = new Vector3(0, 0, 1); // Forward direction in world space

                Vector3 origin = Camera.main.transform.position;

                GetStartEndPosition(startPosition, out Vector3 startPos, out Vector3 endPos, depthAmpCondition.amplitude, depthAmpCondition.depth, forwardDir, origin);

                StartTrial(startPos, endPos, out ObjectToBeManipulated, out TargetIndicator);

                // Wait until TargetIndicator is null before continuing to the next trial
                yield return StartCoroutine(WaitForTargetIndicatorToBeNull(null));
            }
        }
    }

    IEnumerator WaitForTargetIndicatorToBeNull(System.Action onComplete)
    {
        while (TargetIndicator != null)
        {
            yield return null; // wait for next frame
        }
        onComplete?.Invoke();
    }

    public void GetStartEndPosition(StartPositionLabels start, out Vector3 startPos, out Vector3 endPos, float amp, float depth, Vector3 forwardDir, Vector3 origin)
    {
        switch (start)
        {
            case StartPositionLabels.FrontUpperLeft:
                startPos = origin + Quaternion.AngleAxis(VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(-amp, Vector3.up) * forwardDir.normalized * MinDepth;
                endPos = origin + Quaternion.AngleAxis(-VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(amp, Vector3.up) * forwardDir.normalized * depth;
                break;
            case StartPositionLabels.FrontUpperRight:
                startPos = origin + Quaternion.AngleAxis(VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(amp, Vector3.up) * forwardDir.normalized * MinDepth;
                endPos = origin + Quaternion.AngleAxis(-VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(-amp, Vector3.up) * forwardDir.normalized * depth;
                break;
            case StartPositionLabels.FrontLowerLeft:
                startPos = origin + Quaternion.AngleAxis(-VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(-amp, Vector3.up) * forwardDir.normalized * MinDepth;
                endPos = origin + Quaternion.AngleAxis(VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(amp, Vector3.up) * forwardDir.normalized * depth;
                break;
            case StartPositionLabels.FrontLowerRight:
                startPos = origin + Quaternion.AngleAxis(-VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(amp, Vector3.up) * forwardDir.normalized * MinDepth;
                endPos = origin + Quaternion.AngleAxis(VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(-amp, Vector3.up) * forwardDir.normalized * depth;
                break;
            case StartPositionLabels.BackUpperLeft:
                startPos = origin + Quaternion.AngleAxis(VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(-amp, Vector3.up) * forwardDir.normalized * depth;
                endPos = origin + Quaternion.AngleAxis(-VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(amp, Vector3.up) * forwardDir.normalized * MinDepth;
                break;
            case StartPositionLabels.BackUpperRight:
                startPos = origin + Quaternion.AngleAxis(VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(amp, Vector3.up) * forwardDir.normalized * depth;
                endPos = origin + Quaternion.AngleAxis(-VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(-amp, Vector3.up) * forwardDir.normalized * MinDepth;
                break;
            case StartPositionLabels.BackLowerLeft:
                startPos = origin + Quaternion.AngleAxis(-VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(-amp, Vector3.up) * forwardDir.normalized * depth;
                endPos = origin + Quaternion.AngleAxis(VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(amp, Vector3.up) * forwardDir.normalized * MinDepth;
                break;
            case StartPositionLabels.BackLowerRight:
                startPos = origin + Quaternion.AngleAxis(-VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(amp, Vector3.up) * forwardDir.normalized * depth;
                endPos = origin + Quaternion.AngleAxis(VerticalAmpDev, Vector3.right) * Quaternion.AngleAxis(-amp, Vector3.up) * forwardDir.normalized * MinDepth;
                break;
            default:
                startPos = Vector3.zero;
                endPos = Vector3.zero;
                Debug.LogError("Invalid Start Position");
                break;
        }
    }

    public List<(float depth, float amplitude)> GetShuffledDepthAmplitudeCombinations()
    {
        // Create all unique combinations
        var combinations = new List<(float, float)>();
        foreach (float depth in Depths)
        {
            foreach (float amplitude in Amplitudes)
            {
                combinations.Add((depth, amplitude));
            }
        }

        // Shuffle the list
        System.Random rng = new System.Random();
        combinations = combinations.OrderBy(x => rng.Next()).ToList();

        return combinations;
    }

    public List<StartPositionLabels> GetShuffledStartPositionLabels()
    {
        var positions = System.Enum.GetValues(typeof(StartPositionLabels)).Cast<StartPositionLabels>().ToList();
        System.Random rng = new System.Random();
        positions = positions.OrderBy(x => rng.Next()).ToList();
        return positions;
    }

    public void SwitchToVisualGain()
    {
        ManipulationBehavior = GetComponent<GazeNPinchOrigin>();
        TechniqueText.text = "Current Technique: Visual Gain";
    }

    public void SwitchToAnywhereHandContinuous()
    {
        ManipulationBehavior = GetComponent<AnywhereHandContinuous>();
        TechniqueText.text = "Current Technique: AnywhereHand 1";
    }

    public void SwitchToAnywhereHandDiscrete()
    {
        ManipulationBehavior = GetComponent<AnywhereHandDiscrete>();
        TechniqueText.text = "Current Technique: Anywhere Hand Discrete";
    }

    public void SwitchToContinuous2()
    {
        ManipulationBehavior = GetComponent<Continuous2>();
        TechniqueText.text = "Current Technique: AnywhereHand 2";
    }

    public void SwitchToGazeHand()
    {
        ManipulationBehavior = GetComponent<GazeHand>();
        TechniqueText.text = "Current Technique: Gaze Hand";
    }

    // public void SwitchToGazeNPinch()
    // {
    //     ManipulationBehavior = GetComponent<GazeNPinchOrigin>();
    //     TechniqueText.text = "Current Technique: Gaze and Pinch";
    // }

    // public void SwitchToScaledHOMER()
    // {
    //     ManipulationBehavior = GetComponent<ScaledHOMER>();
    //     TechniqueText.text = "Current Technique: Scaled HOMER";
    // }

    // public void SwitchToGazeNPinchEyeHead()
    // {
    //     ManipulationBehavior = GetComponent<GazeNPinchEyeHead>();
    //     TechniqueText.text = "Current Technique: Gaze and Pinch Eye Head";
    // }

    // public void SwitchToHomerEyeHead()
    // {
    //     ManipulationBehavior = GetComponent<HomerEyeHead>();
    //     TechniqueText.text = "Current Technique: HOMER Eye Head";
    // }

}