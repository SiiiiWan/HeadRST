
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public enum PositionLabels
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

    public TextMeshPro TechniqueText;

    public GameObject TargetPrefab;
    public GameObject ObjectPrefab;

    public GameObject ObjectToBeManipulated;
    public GameObject TargetIndicator;

    public List<((float depth_min, float depth_max), float amplitude)> DepthAmplitudeCombinations = new List<((float, float), float)>();
    public List<PositionLabels> StartPositionLabelsList = new List<PositionLabels>();


    public List<(float min, float max)> DepthPairs_within = new List<(float, float)> { (1f, 5f), (5f, 9f) };
    public List<(float min, float max)> DepthPairs_between = new List<(float, float)> { (-1f, 2f), (-1f, 4f), (-1f, 8f) };
    public List<float> Amplitudes_within = new List<float> { 15f, 30f };
    public float MaxAmplitude_between = 20f;

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

        if (PinchDetector.GetInstance().PinchState == PinchState.NotPinching && ManipulationBehavior.GrabbedObject != null)
        {
            if (TargetIndicator.GetComponent<DockingTarget>().IsPoseAligned())
            {
                Destroy(ObjectToBeManipulated);
                Destroy(TargetIndicator);

                ObjectToBeManipulated = null;
                TargetIndicator = null;
            }
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

        DepthAmplitudeCombinations = GetShuffledDepthAmplitudeCombinations(DepthPairs_within, Amplitudes_within);
        // DepthAmplitudeCombinations = GetShuffledDepth_Random_AmplitudeCombinations(DepthPairs_between, MaxAmplitude_between);

        StartCoroutine(RunTrials());
    }

    public Vector3 TrialStartPosition {get; private set;} = Vector3.zero;
    public Vector3 TrialEndPosition {get; private set;} = Vector3.zero;

    private IEnumerator RunTrials()
    {
        foreach (((float depth_min, float depth_max), float amplitude) depthAmpCondition in DepthAmplitudeCombinations)
        {
            var depthPair = depthAmpCondition.Item1;
            float amplitude = depthAmpCondition.Item2;

            CubePositions = GetCubePositions_Visual(
                viewPoint: Camera.main.transform.position,
                forwardDir: new Vector3(0, 0, 1),
                minDepth: depthPair.depth_min,
                maxDepth: depthPair.depth_max,
                angularDeviation_horizontal: amplitude,
                angularDeviation_vertical: amplitude);

            StartPositionLabelsList = GetShuffledStartPositionLabels();

            foreach (PositionLabels startPosition in StartPositionLabelsList)
            {
                TrialStartPosition = CubePositions[startPosition];
                TrialEndPosition = CubePositions[GetDiagonalPositionLabel(startPosition)];

                StartTrial(TrialStartPosition, TrialEndPosition, out ObjectToBeManipulated, out TargetIndicator);

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

    public Dictionary<PositionLabels, Vector3> CubePositions { get; private set; } = new Dictionary<PositionLabels, Vector3>();

    Dictionary<PositionLabels, Vector3> GetCubePositions_Visual(
        Vector3 viewPoint, Vector3 forwardDir, float minDepth, float maxDepth, float angularDeviation_horizontal, float angularDeviation_vertical)
    {
        Dictionary<PositionLabels, Vector3> positions = new Dictionary<PositionLabels, Vector3>
        {
            {PositionLabels.FrontUpperLeft,
            viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth},

            {PositionLabels.FrontUpperRight,
            viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth},

            {PositionLabels.FrontLowerLeft,
            viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth},

            {PositionLabels.FrontLowerRight,
            viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth},

            {PositionLabels.BackUpperLeft,
            viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth},

            {PositionLabels.BackUpperRight,
            viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth},

            {PositionLabels.BackLowerLeft,
            viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth},

            {PositionLabels.BackLowerRight,
            viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth}
        };
        

        return positions;
    }

    PositionLabels GetDiagonalPositionLabel(PositionLabels label)
    {
        switch (label)
        {
            case PositionLabels.FrontUpperLeft: return PositionLabels.BackLowerRight;
            case PositionLabels.FrontUpperRight: return PositionLabels.BackLowerLeft;
            case PositionLabels.FrontLowerLeft: return PositionLabels.BackUpperRight;
            case PositionLabels.FrontLowerRight: return PositionLabels.BackUpperLeft;
            case PositionLabels.BackUpperLeft: return PositionLabels.FrontLowerRight;
            case PositionLabels.BackUpperRight: return PositionLabels.FrontLowerLeft;
            case PositionLabels.BackLowerLeft: return PositionLabels.FrontUpperRight;
            case PositionLabels.BackLowerRight: return PositionLabels.FrontUpperLeft;
            default: throw new System.ArgumentException("Invalid position label");
        }
    }


    public List<((float depth_min, float depth_max), float amplitude)> GetShuffledDepthAmplitudeCombinations(List<(float min, float max)> depthPairs, List<float> amplitudes)
    {
        // Create all unique combinations
        var combinations = new List<((float, float), float)>();
        foreach (var depth in depthPairs)
        {
            foreach (float amplitude in amplitudes)
            {
                combinations.Add((depth, amplitude));
            }
        }

        // Shuffle the list
        combinations = combinations.OrderBy(x => new System.Random().Next()).ToList();

        return combinations;
    }

    public List<((float depth_min, float depth_max), float amplitude)> GetShuffledDepth_Random_AmplitudeCombinations(List<(float min, float max)> depthPairs, float maxAmplitude)
    {
        // Create all unique combinations
        var combinations = new List<((float, float), float)>();
        foreach (var depth in depthPairs)
        {   
            float randomAmplitude = Random.Range(0f, maxAmplitude);
            combinations.Add((depth, randomAmplitude));
        }

        // Shuffle the list
        combinations = combinations.OrderBy(x => new System.Random().Next()).ToList();

        return combinations;
    }


    public List<PositionLabels> GetShuffledStartPositionLabels()
    {
        var positions = System.Enum.GetValues(typeof(PositionLabels)).Cast<PositionLabels>().ToList();
        System.Random rng = new System.Random();
        positions = positions.OrderBy(x => rng.Next()).ToList();
        return positions;
    }

    public void SwitchToVisualGain()
    {
        ManipulationBehavior = GetComponent<GazeNPinchOrigin>();
        TechniqueText.text = "Current Technique: Visual Gain";
    }

    // public void SwitchToAnywhereHandContinuous()
    // {
    //     ManipulationBehavior = GetComponent<AnywhereHandContinuous>();
    //     TechniqueText.text = "Current Technique: AnywhereHand 1";
    // }

    // public void SwitchToAnywhereHandDiscrete()
    // {
    //     ManipulationBehavior = GetComponent<AnywhereHandDiscrete>();
    //     TechniqueText.text = "Current Technique: Anywhere Hand Discrete";
    // }

    // public void SwitchToContinuous2()
    // {
    //     ManipulationBehavior = GetComponent<Continuous2>();
    //     TechniqueText.text = "Current Technique: AnywhereHand 2";
    // }

    // public void SwitchToGazeHand()
    // {
    //     ManipulationBehavior = GetComponent<GazeHand>();
    //     TechniqueText.text = "Current Technique: Gaze Hand";
    // }

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