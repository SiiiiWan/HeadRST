
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;

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

public class StudyControl : Singleton<StudyControl>
{
    [Header("Study Settings")]
    public string ParticipantID;
    public Handedness DominantHand = Handedness.right;
    public bool IsPractice;
    public ManipulationTechnique ManipulationBehavior;

    [Header("Study States")]
    public bool StudyFlag = false; // Indicates if the study is currently running
    public int TotalTrialCount;
    public bool IsAfterFirstPickUpInTrial = false;
    public CubePositionLabels StartPositionLabel;
    public float TaskMinDepth, TaskMaxDepth, TaskAmplitude;

    [Header("Bindings")]
    public TextMeshPro TaskText;
    public TextMeshPro TaskEndText;
    public GameObject TargetPrefab;
    public GameObject ObjectPrefab;
    public GameObject LeftHand_Virtual, RightHand_Virtual, LeftHandSynth_Virtual, RightHandSynth_Virtual;
    public Transform TaskButtonsFront;

    [HideInInspector] public GameObject ObjectToBeManipulated;
    [HideInInspector] public GameObject TargetIndicator;
    [HideInInspector] public Linescript TargetLine;
    [HideInInspector] public Linescript Circle_static, Circle_dynamic;

    public List<((float depth_min, float depth_max), float amplitude)> DepthAmplitudeCombinations = new List<((float, float), float)>();
    public List<(float depth, DockingDirections direction)> DepthDirectionCombinations = new List<(float, DockingDirections)>();

    [HideInInspector] public List<CubePositionLabels> StartPositionLabelsList = new List<CubePositionLabels>();


    public List<(float min, float max)> DepthPairs_within { get; private set; } = new List<(float, float)> { (2f, 4f), (2f, 6f), (2f, 10f)};
    public List<(float min, float max)> DepthPairs_practice { get; private set; } = new List<(float, float)> {(2f, 6f)};

    public List<float> Amplitudes_within { get; private set; } = new List<float> { 15f, 30f, 60f };
    public List<float> Amplitudes_practice { get; private set; } = new List<float> { 30f };

    private Vector3 _startButtonPosition, _startTaskEndTextPosition;

    protected override void Awake()
    {
        base.Awake();
        TargetLine = new Linescript();
        Circle_static = new Linescript(sampleNumberForCircle: 100, color: Color.gray);
        Circle_dynamic = new Linescript(sampleNumberForCircle: 100, color: Color.yellow);
        TargetLine.IsVisible = false;
    }

    void Start()
    {
        // UpdateHandVisuals();
        // SwitchToGazeNPinch();

        _startButtonPosition = TaskButtonsFront.position;
        _startTaskEndTextPosition = TaskEndText.transform.position;
        TaskEndText.transform.position = Vector3.down * 1000;
    }

    void Update()
    {
        // UpdateHandVisuals();
        TaskText.text = IsPractice ? "Start Practice" : "Start Formal Test";

        if (Input.GetKeyDown(KeyCode.Space)) ShowTrials_within();

        if (TargetIndicator == null || ObjectToBeManipulated == null)
        {
            TargetLine.IsVisible = false;
            Circle_static.IsVisible = false;
            Circle_dynamic.IsVisible = false;
            return; // No target indicator to check
        }

        UpdateTaskVisualFeedbacks();

        if (PinchDetector.GetInstance().PinchState == PinchState.NotPinching && ManipulationBehavior.GrabbedObject != null)
        {
            if (TargetIndicator.GetComponent<DockingTarget>().PoseAligned_200msAgo || TargetIndicator.GetComponent<DockingTarget>().IsPoseAligned())
            {
                Destroy(ObjectToBeManipulated);
                Destroy(TargetIndicator);

                ObjectToBeManipulated = null;
                TargetIndicator = null;
                AudioPlay.PlayClickSound();
            }
        }


        // if (ManipulationBehavior.GrabbedObject != null && TargetIndicator.GetComponent<DockingTarget>().IsPoseAligned())
        // {
        //     Destroy(ObjectToBeManipulated);
        //     Destroy(TargetIndicator);

        //     ObjectToBeManipulated = null;
        //     TargetIndicator = null;
        //     AudioPlay.PlayClickSound();
        // }
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

    void UpdateTaskVisualFeedbacks()
    {

        TargetLine.SetPosition(TargetIndicator.transform.position, ObjectToBeManipulated.transform.position);

        float staticCircleRadius = TargetIndicator.transform.localScale.x * 1.2f;
        Circle_static.DrawRing(TargetIndicator.transform.position, staticCircleRadius);

        Vector3 camToObjectVector = ObjectToBeManipulated.transform.position - HeadPosition_OnTrialStart;
        Vector3 camToTargetVector = TargetIndicator.transform.position - HeadPosition_OnTrialStart;
        float projectedDistanceOnDepthAxis = Vector3.Project(camToObjectVector, camToTargetVector).magnitude;

        float depthProgress = Mathf.Max(0, (projectedDistanceOnDepthAxis / camToTargetVector.magnitude - 0.5f) * 2);
        Circle_dynamic.DrawRing(TargetIndicator.transform.position, staticCircleRadius * depthProgress);

        float circleLineWidth = MathFunctions.Deg2Meter(0.1f, camToTargetVector.magnitude);

        Circle_dynamic.SetWidth(circleLineWidth);
        Circle_static.SetWidth(circleLineWidth);

        Circle_static.IsVisible = Mathf.Abs(camToObjectVector.magnitude - camToTargetVector.magnitude) > TargetIndicator.GetComponent<DockingTarget>().GetPositionAlignmentThreshold();
        Circle_dynamic.IsVisible = Mathf.Abs(camToObjectVector.magnitude - camToTargetVector.magnitude) > TargetIndicator.GetComponent<DockingTarget>().GetPositionAlignmentThreshold();

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
        // Vector3 scale_start = MathFunctions.Deg2Meter(TargetSize, Vector3.Distance(HeadPosition_OnTrialStart, startPos)) * Vector3.one;
        startObj = SpawnPrefab(ObjectPrefab, startPos, Quaternion.identity, ObjectPrefab.transform.localScale);

        // Vector3 scale_end = MathFunctions.Deg2Meter(TargetSize, Vector3.Distance(HeadPosition_OnTrialStart, endPos)) * Vector3.one;
        Quaternion randomRotationOffset = Quaternion.AngleAxis(20, Random.onUnitSphere);
        target = SpawnPrefab(TargetPrefab, endPos, randomRotationOffset * startObj.transform.rotation, TargetPrefab.transform.localScale);

        // print("Trial target size: " + MathFunctions.Meter2Deg(scale.x, Vector3.Distance(Camera.main.transform.position, endPos)) + " degrees");

        TotalTrialCount++;
        IsAfterFirstPickUpInTrial = false;
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

        TaskButtonsFront.position = Vector3.down * 1000;
        TaskEndText.transform.position = Vector3.down * 1000;

        // if (TaskMode == TaskMode.depth_only) StartCoroutine(RunTrials_between());
        // else
        StartCoroutine(RunTrials_within(OnStudyComplete));
    }

    public Vector3 TrialStartPosition { get; private set; } = Vector3.zero;
    public Vector3 TrialEndPosition { get; private set; } = Vector3.zero;
    public Vector3 HeadPosition_OnTrialStart { get; private set; } = Vector3.zero;
    public Vector3 HeadPosition_OnTaskStart { get; private set; } = Vector3.zero;

    public float TaskSpatialDistance { get { return Vector3.Distance(TrialStartPosition, TrialEndPosition); } }
    public float ProjectedDistanceOnTaskAxis
    {
        get
        {
            if (ObjectToBeManipulated == null) return 0;

            Vector3 taskVector = TrialEndPosition - TrialStartPosition;
            Vector3 objectVector = ObjectToBeManipulated.transform.position - TrialStartPosition;

            return Vector3.Project(objectVector, taskVector).magnitude * (Vector3.Dot(objectVector, taskVector) > 0 ? 1 : -1);
        }
    }
    public float TaskProgress { get { return ProjectedDistanceOnTaskAxis / TaskSpatialDistance; } }

    private IEnumerator RunTrials_within(System.Action onComplete = null)
    {
        StudyFlag = true;

        HeadPosition_OnTaskStart = Camera.main.transform.position;
        DepthAmplitudeCombinations = IsPractice ? GetShuffledDepth_Amplitude_Combinations(DepthPairs_practice, Amplitudes_practice) : GetShuffledDepth_Amplitude_Combinations(DepthPairs_within, Amplitudes_within);

        foreach (((float depth_min, float depth_max), float amplitude) depthAmpCondition in DepthAmplitudeCombinations)
        {
            var depthPair = depthAmpCondition.Item1;
            float amplitude = depthAmpCondition.Item2;

            TaskMinDepth = depthPair.depth_min;
            TaskMaxDepth = depthPair.depth_max;
            TaskAmplitude = amplitude;

            CubePositions = GetCubePositions_Visual(
                viewPoint: HeadPosition_OnTaskStart,
                forwardDir: Vector3.forward,
                minDepth: TaskMinDepth,
                maxDepth: TaskMaxDepth,
                angularDeviation_horizontal: TaskAmplitude,
                angularDeviation_vertical: TaskAmplitude);

            StartPositionLabelsList = GetShuffledStartPositionLabels();

            foreach (CubePositionLabels startPosition in StartPositionLabelsList)
            {
                StartPositionLabel = startPosition;
                TrialStartPosition = CubePositions[startPosition];
                TrialEndPosition = CubePositions[GetDiagonalPositionLabel(startPosition)];

                StartTrial(TrialStartPosition, TrialEndPosition, out ObjectToBeManipulated, out TargetIndicator);

                // Wait until TargetIndicator is null before continuing to the next trial
                yield return StartCoroutine(WaitForTargetIndicatorToBeNull(null));
            }
        }

        StudyFlag = false;
        onComplete?.Invoke();
    }

    public void OnStudyComplete()
    {
        if (IsPractice) TaskButtonsFront.position = _startButtonPosition;
        TaskEndText.transform.position = _startTaskEndTextPosition;
    }

    private void ShowTrials_within()
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

                if (depthPair.depth_min < 1f)
                {
                    TrialStartPosition += Vector3.down * 0.5f;
                    TrialEndPosition += Vector3.down * 0.5f;
                }

                // Vector3 scale_start = MathFunctions.Deg2Meter(TargetSize, Vector3.Distance(Camera.main.transform.position, TrialStartPosition)) * Vector3.one;
                // Vector3 scale_end = MathFunctions.Deg2Meter(TargetSize, Vector3.Distance(Camera.main.transform.position, TrialEndPosition)) * Vector3.one;
                GameObject obj = SpawnPrefab(ObjectPrefab, TrialStartPosition, Quaternion.identity, ObjectPrefab.transform.localScale);
                obj.GetComponent<ManipulatableObject>().enabled = false;
                // SpawnPrefab(TargetPrefab, TrialEndPosition, Quaternion.identity, scale_end);
                // print(Vector3.Angle(Vector3.forward, TrialStartPosition - Camera.main.transform.position) + " degrees");
            }
        }
    }

    // private IEnumerator RunTrials_between()
    // {
    //     DepthDirectionCombinations = GetShuffledDepth_Direction_Combinations(Depths_between);

    //     foreach ((float depth, DockingDirections direction) depthDirCondition in DepthDirectionCombinations)
    //     {
    //         Vector3 closePosition = GetRandomFrontPosition(height: 0.8f, depth: 0.5f, width: 0.6f);
    //         Vector3 farPosition = Camera.main.transform.position +
    //                 Quaternion.AngleAxis(Random.Range(-MaxAmplitude_between, MaxAmplitude_between), Vector3.right) *
    //                 Quaternion.AngleAxis(Random.Range(-MaxAmplitude_between, MaxAmplitude_between), Vector3.up) *
    //                 Vector3.forward.normalized * depthDirCondition.depth;

    //         if (depthDirCondition.direction == DockingDirections.forward)
    //         {
    //             TrialStartPosition = closePosition;
    //             TrialEndPosition = farPosition;
    //         }
    //         else
    //         {
    //             TrialStartPosition = farPosition;
    //             TrialEndPosition = closePosition;
    //         }

    //         StartTrial(TrialStartPosition, TrialEndPosition, out ObjectToBeManipulated, out TargetIndicator);

    //         // Wait until TargetIndicator is null before continuing to the next trial
    //         yield return StartCoroutine(WaitForTargetIndicatorToBeNull(null));
    //     }
    // }


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
        angularDeviation_vertical = angularDeviation_vertical / Mathf.Sqrt(2f); 
        angularDeviation_horizontal = angularDeviation_horizontal / Mathf.Sqrt(2f);

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

    public CubePositionLabels GetDiagonalPositionLabel(CubePositionLabels label)
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

    // public List<(float depth, DockingDirections direction)> GetShuffledDepth_Direction_Combinations(List<float> depths)
    // {
    //     // Create all unique combinations
    //     var combinations = new List<(float, DockingDirections)>();
    //     foreach (var depth in depths)
    //     {
    //         foreach (DockingDirections direction in System.Enum.GetValues(typeof(DockingDirections)))
    //         {
    //             combinations.Add((depth, direction));
    //         }
    //     }

    //     // Shuffle the list
    //     combinations = combinations.OrderBy(x => new System.Random().Next()).ToList();

    //     return combinations;
    // }

    // public Vector3 GetRandomFrontPosition(float height, float depth, float width)
    // {
    //     Vector3 camPos = Camera.main.transform.position;

    //     return new Vector3(camPos.x + Random.Range(-width / 2f, width / 2f), height, camPos.z + depth);
    // }

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

    // public void SwitchToAnywhereHandBase()
    // {
    //     var techniques = GetComponents<ManipulationTechnique>();
    //     foreach (var technique in techniques)
    //     {
    //         if (technique is AnywhereHand_Base)
    //         {
    //             technique.enabled = true;
    //             ManipulationBehavior = GetComponent<AnywhereHand_Base>();
    //         }
    //         else
    //         {
    //             technique.enabled = false;
    //         }
    //     }

    //     TechniqueText.text = "Current Technique: AnywhereHand- HeadPriority";
    // }

    // public void SwitchToAnywhereHandAttenuated()
    // {
    //     var techniques = GetComponents<ManipulationTechnique>();
    //     foreach (var technique in techniques)
    //     {
    //         if (technique is AnywhereHand_Att)
    //         {
    //             technique.enabled = true;
    //             ManipulationBehavior = GetComponent<AnywhereHand_Att>();
    //         }
    //         else
    //         {
    //             technique.enabled = false;
    //         }
    //     }
    //     TechniqueText.text = "Current Technique: AnywhereHand - HandPriority";
    // }

    // public void SwitchToContinuous2()
    // {
    //     ManipulationBehavior = GetComponent<Continuous2>();
    //     TechniqueText.text = "Current Technique: AnywhereHand 2";
    // }

    // public void SwitchToGazeHand()
    // {
    //     var techniques = GetComponents<ManipulationTechnique>();
    //     foreach (var technique in techniques)
    //     {
    //         if (technique is GazeHand)
    //         {
    //             technique.enabled = true;
    //             ManipulationBehavior = GetComponent<GazeHand>();
    //         }
    //         else
    //         {
    //             technique.enabled = false;
    //         }
    //     }
    //     TechniqueText.text = "Current Technique: GazeHand2";
    // }

    // public void SwitchToGazeNPinch()
    // {
    //     var techniques = GetComponents<ManipulationTechnique>();
    //     foreach (var technique in techniques)
    //     {
    //         if (technique is GazeNPinchOrigin)
    //         {
    //             technique.enabled = true;
    //             ManipulationBehavior = GetComponent<GazeNPinchOrigin>();
    //         }
    //         else
    //         {
    //             technique.enabled = false;
    //         }
    //     }
    //     TechniqueText.text = "Current Technique: Gaze+Pinch (Visual Gain)";
    // }

    // public void SwitchTask_DepthOnly()
    // {
    //     TaskMode = TaskMode.depth_only;
    //     TaskText.text = "Current Task: Far-Close Switching";
    //     StartTask();
    // }

    // public void SwitchTask_AmpAndDepth()
    // {
    //     TaskMode = TaskMode.amp_and_depth;
    //     TaskText.text = "Current Task: Distance Manipulation";
    //     StartTask();
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