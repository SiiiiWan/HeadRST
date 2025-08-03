
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections;

public enum CubePositionLabels
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

public enum DockingDirections { forward, backward }
public enum Handedness { left, right }
public enum TaskMode { amp_and_depth, depth_only }

public class StudyControl : Singleton<StudyControl>
{
    [Header("Study Settings")]
    public string ParticipantID;
    public Handedness DominantHand = Handedness.right;
    public TaskMode TaskMode = TaskMode.amp_and_depth;
    public bool IsPractice;
    public ManipulationTechnique ManipulationBehavior;

    [Header("Study States")]
    public int TotalTrialCount;

    [Header("Bindings")]
    public TextMeshPro TechniqueText;
    public TextMeshPro TaskText;
    public GameObject TargetPrefab;
    public GameObject ObjectPrefab;
    public GameObject LeftHand_Virtual, RightHand_Virtual, LeftHandSynth_Virtual, RightHandSynth_Virtual;

    [HideInInspector] public GameObject ObjectToBeManipulated;
    [HideInInspector] public GameObject TargetIndicator;

    public List<((float depth_min, float depth_max), float amplitude)> DepthAmplitudeCombinations = new List<((float, float), float)>();
    public List<(float depth, DockingDirections direction)> DepthDirectionCombinations = new List<(float, DockingDirections)>();

    [HideInInspector] public List<CubePositionLabels> StartPositionLabelsList = new List<CubePositionLabels>();


    public List<(float min, float max)> DepthPairs_within = new List<(float, float)> { (1f, 5f), (1f, 9f) };
    [HideInInspector] public List<float> Depths_between = new List<float> { 2f, 4f, 8f };
    [HideInInspector] public List<float> Amplitudes_within = new List<float> { 15f, 30f };
    [HideInInspector] public float MaxAmplitude_between = 20f;
    [HideInInspector] public float TargetSize = 4f; // in degrees

    void Start()
    {
        UpdateHandVisuals();
        SwitchTask_AmpAndDepth();
        SwitchToGazeNPinch();
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

        UpdateHandVisuals();

    }

    void UpdateHandVisuals()
    {
        if (DominantHand == Handedness.right)
        {
            RightHand_Virtual.SetActive(true);
            LeftHand_Virtual.SetActive(false);
            RightHandSynth_Virtual.SetActive(true);
            LeftHandSynth_Virtual.SetActive(false);
        }
        else
        {
            RightHand_Virtual.SetActive(false);
            LeftHand_Virtual.SetActive(true);
            RightHandSynth_Virtual.SetActive(false);
            LeftHandSynth_Virtual.SetActive(true);
        }
    }


    public GameObject SpawnPrefab(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject obj = Instantiate(prefab, position, rotation);
        obj.transform.SetParent(transform);
        obj.transform.localScale = scale;
        return obj;
    }

    public void StartTrial(Vector3 startPos, Vector3 endPos, out GameObject startObj, out GameObject target)
    {
        HeadPosition_OnTrialStart = Camera.main.transform.position;
        Vector3 scale = MathFunctions.Deg2Meter(TargetSize, Vector3.Distance(HeadPosition_OnTrialStart, endPos)) * Vector3.one;
        startObj = SpawnPrefab(ObjectPrefab, startPos, Quaternion.identity, scale);

        Quaternion randomRotationOffset = Quaternion.AngleAxis(Random.Range(-30f, 30f), Random.onUnitSphere);
        target = SpawnPrefab(TargetPrefab, endPos, randomRotationOffset * startObj.transform.rotation, scale);

        // print("Trial target size: " + MathFunctions.Meter2Deg(scale.x, Vector3.Distance(Camera.main.transform.position, endPos)) + " degrees");

        TotalTrialCount++;
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

        TotalTrialCount = 0;

        if (TaskMode == TaskMode.depth_only) StartCoroutine(RunTrials_between());
        else StartCoroutine(RunTrials_within());
    }

    public Vector3 TrialStartPosition { get; private set; } = Vector3.zero;
    public Vector3 TrialEndPosition { get; private set; } = Vector3.zero;
    public Vector3 HeadPosition_OnTrialStart { get; private set; } = Vector3.zero;

    private IEnumerator RunTrials_within()
    {
        DepthAmplitudeCombinations = GetShuffledDepth_Amplitude_Combinations(DepthPairs_within, Amplitudes_within);

        foreach (((float depth_min, float depth_max), float amplitude) depthAmpCondition in DepthAmplitudeCombinations)
        {
            var depthPair = depthAmpCondition.Item1;
            float amplitude = depthAmpCondition.Item2;

            CubePositions = GetCubePositions_Visual(
                viewPoint: Camera.main.transform.position,
                forwardDir: Vector3.forward,
                minDepth: depthPair.depth_min,
                maxDepth: depthPair.depth_max,
                angularDeviation_horizontal: amplitude,
                angularDeviation_vertical: amplitude);

            StartPositionLabelsList = GetShuffledStartPositionLabels();

            foreach (CubePositionLabels startPosition in StartPositionLabelsList)
            {
                TrialStartPosition = CubePositions[startPosition];
                TrialEndPosition = CubePositions[GetDiagonalPositionLabel(startPosition)];

                StartTrial(TrialStartPosition, TrialEndPosition, out ObjectToBeManipulated, out TargetIndicator);

                // Wait until TargetIndicator is null before continuing to the next trial
                yield return StartCoroutine(WaitForTargetIndicatorToBeNull(null));
            }
        }
    }

    private IEnumerator RunTrials_between()
    {
        DepthDirectionCombinations = GetShuffledDepth_Direction_Combinations(Depths_between);

        foreach ((float depth, DockingDirections direction) depthDirCondition in DepthDirectionCombinations)
        {
            Vector3 closePosition = GetRandomFrontPosition(height: 0.8f, depth: 0.5f, width: 0.6f);
            Vector3 farPosition = Camera.main.transform.position +
                    Quaternion.AngleAxis(Random.Range(-MaxAmplitude_between, MaxAmplitude_between), Vector3.right) *
                    Quaternion.AngleAxis(Random.Range(-MaxAmplitude_between, MaxAmplitude_between), Vector3.up) *
                    Vector3.forward.normalized * depthDirCondition.depth;

            if (depthDirCondition.direction == DockingDirections.forward)
            {
                TrialStartPosition = closePosition;
                TrialEndPosition = farPosition;
            }
            else
            {
                TrialStartPosition = farPosition;
                TrialEndPosition = closePosition;
            }

            StartTrial(TrialStartPosition, TrialEndPosition, out ObjectToBeManipulated, out TargetIndicator);

            // Wait until TargetIndicator is null before continuing to the next trial
            yield return StartCoroutine(WaitForTargetIndicatorToBeNull(null));
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

    public Dictionary<CubePositionLabels, Vector3> CubePositions { get; private set; } = new Dictionary<CubePositionLabels, Vector3>();

    Dictionary<CubePositionLabels, Vector3> GetCubePositions_Visual(
        Vector3 viewPoint, Vector3 forwardDir, float minDepth, float maxDepth, float angularDeviation_horizontal, float angularDeviation_vertical)
    {
        Dictionary<CubePositionLabels, Vector3> positions = new Dictionary<CubePositionLabels, Vector3>
        {
            {CubePositionLabels.FrontUpperLeft,
            viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth},

            {CubePositionLabels.FrontUpperRight,
            viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth},

            {CubePositionLabels.FrontLowerLeft,
            viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth},

            {CubePositionLabels.FrontLowerRight,
            viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth},

            {CubePositionLabels.BackUpperLeft,
            viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth},

            {CubePositionLabels.BackUpperRight,
            viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth},

            {CubePositionLabels.BackLowerLeft,
            viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth},

            {CubePositionLabels.BackLowerRight,
            viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth}
        };


        return positions;
    }

    CubePositionLabels GetDiagonalPositionLabel(CubePositionLabels label)
    {
        switch (label)
        {
            case CubePositionLabels.FrontUpperLeft: return CubePositionLabels.BackLowerRight;
            case CubePositionLabels.FrontUpperRight: return CubePositionLabels.BackLowerLeft;
            case CubePositionLabels.FrontLowerLeft: return CubePositionLabels.BackUpperRight;
            case CubePositionLabels.FrontLowerRight: return CubePositionLabels.BackUpperLeft;
            case CubePositionLabels.BackUpperLeft: return CubePositionLabels.FrontLowerRight;
            case CubePositionLabels.BackUpperRight: return CubePositionLabels.FrontLowerLeft;
            case CubePositionLabels.BackLowerLeft: return CubePositionLabels.FrontUpperRight;
            case CubePositionLabels.BackLowerRight: return CubePositionLabels.FrontUpperLeft;
            default: throw new System.ArgumentException("Invalid position label");
        }
    }


    public List<((float depth_min, float depth_max), float amplitude)> GetShuffledDepth_Amplitude_Combinations(List<(float min, float max)> depthPairs, List<float> amplitudes)
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

    public List<(float depth, DockingDirections direction)> GetShuffledDepth_Direction_Combinations(List<float> depths)
    {
        // Create all unique combinations
        var combinations = new List<(float, DockingDirections)>();
        foreach (var depth in depths)
        {
            foreach (DockingDirections direction in System.Enum.GetValues(typeof(DockingDirections)))
            {
                combinations.Add((depth, direction));
            }
        }

        // Shuffle the list
        combinations = combinations.OrderBy(x => new System.Random().Next()).ToList();

        return combinations;
    }

    public Vector3 GetRandomFrontPosition(float height, float depth, float width)
    {
        Vector3 camPos = Camera.main.transform.position;

        return new Vector3(camPos.x + Random.Range(-width / 2f, width / 2f), height, camPos.z + depth);
    }

    public List<CubePositionLabels> GetShuffledStartPositionLabels()
    {
        var positions = System.Enum.GetValues(typeof(CubePositionLabels)).Cast<CubePositionLabels>().ToList();
        System.Random rng = new System.Random();
        positions = positions.OrderBy(x => rng.Next()).ToList();
        return positions;
    }

    // public void SwitchToVisualGain()
    // {
    //     ManipulationBehavior = GetComponent<GazeNPinchOrigin>();
    //     TechniqueText.text = "Current Technique: Visual Gain";
    // }

    public Vector3 GetVirtualHandPosition(bool isRightHand)
    {
        if (isRightHand)
        {
            if (DominantHand == Handedness.right)
            {
                return ManipulationBehavior.VirtualHandPosition;
            }
            else
            {
                return HandData.GetInstance().RightHandPosition;
            }
        }
        else
        {
            if (DominantHand == Handedness.left)
            {
                return ManipulationBehavior.VirtualHandPosition;
            }
            else
            {
                return HandData.GetInstance().LeftHandPosition;
            }
        }
    }

    public void SwitchToAnywhereHandBase()
    {
        var techniques = GetComponents<ManipulationTechnique>();
        foreach (var technique in techniques)
        {
            if (technique is AnywhereHand_Base)
            {
                technique.enabled = true;
                ManipulationBehavior = GetComponent<AnywhereHand_Base>();
            }
            else
            {
                technique.enabled = false;
            }
        }

        TechniqueText.text = "Current Technique: AnywhereHand- HeadPriority";
    }

    public void SwitchToAnywhereHandAttenuated()
    {
        var techniques = GetComponents<ManipulationTechnique>();
        foreach (var technique in techniques)
        {
            if (technique is AnywhereHand_Att)
            {
                technique.enabled = true;
                ManipulationBehavior = GetComponent<AnywhereHand_Att>();
            }
            else
            {
                technique.enabled = false;
            }
        }
        TechniqueText.text = "Current Technique: AnywhereHand - HandPriority";
    }

    // public void SwitchToContinuous2()
    // {
    //     ManipulationBehavior = GetComponent<Continuous2>();
    //     TechniqueText.text = "Current Technique: AnywhereHand 2";
    // }

    public void SwitchToGazeHand()
    {
        var techniques = GetComponents<ManipulationTechnique>();
        foreach (var technique in techniques)
        {
            if (technique is GazeHand)
            {
                technique.enabled = true;
                ManipulationBehavior = GetComponent<GazeHand>();
            }
            else
            {
                technique.enabled = false;
            }
        }
        TechniqueText.text = "Current Technique: GazeHand2";
    }

    public void SwitchToGazeNPinch()
    {
        var techniques = GetComponents<ManipulationTechnique>();
        foreach (var technique in techniques)
        {
            if (technique is GazeNPinchOrigin)
            {
                technique.enabled = true;
                ManipulationBehavior = GetComponent<GazeNPinchOrigin>();
            }
            else
            {
                technique.enabled = false;
            }
        }
        TechniqueText.text = "Current Technique: Gaze+Pinch (Visual Gain)";
    }

    public void SwitchTask_DepthOnly()
    {
        TaskMode = TaskMode.depth_only;
        TaskText.text = "Current Task: Far-Close Switching";
        StartTask();
    }

    public void SwitchTask_AmpAndDepth()
    {
        TaskMode = TaskMode.amp_and_depth;
        TaskText.text = "Current Task: Distance Manipulation";
        StartTask();
    }

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