using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

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

public enum Handedness { left, right }

public class StudyControl : Singleton<StudyControl>
{
    [Header("Study Settings")]
    [SerializeField] private TechniqueControl techniqueControl;
    public bool IsPractice;

    public TechniqueControl TechniqueControl
    {
        get
        {
            if (techniqueControl == null) techniqueControl = GetComponent<TechniqueControl>();
            return techniqueControl;
        }
    }

    public Handedness DominantHand => TechniqueControl != null ? TechniqueControl.DominantHand : Handedness.right;
    public ManipulationTechnique ManipulationBehavior => TechniqueControl != null ? TechniqueControl.ManipulationBehavior : null;

    [Header("Task State")]
    public float TaskMinDepth;
    public float TaskMaxDepth;
    public float TaskAmplitude;

    [Header("Bindings")]
    public TextMeshPro TaskText;
    public TextMeshPro TaskEndText;
    public GameObject TargetPrefab;
    public GameObject ObjectPrefab;
    public Transform TaskButtonsFront;

    [HideInInspector] public GameObject ObjectToBeManipulated;
    [HideInInspector] public GameObject TargetIndicator;
    [HideInInspector] public Linescript TargetLine;
    [HideInInspector] public Linescript Circle_static, Circle_dynamic;

    public List<(float min, float max)> DepthPairs_within { get; private set; } = new List<(float, float)> { (2f, 4f), (2f, 6f), (2f, 10f) };
    public List<(float min, float max)> DepthPairs_practice { get; private set; } = new List<(float, float)> { (2f, 6f) };
    public List<float> Amplitudes_within { get; private set; } = new List<float> { 15f, 30f, 60f };
    public List<float> Amplitudes_practice { get; private set; } = new List<float> { 30f };

    private Vector3 _taskButtonPosition, _taskEndTextPosition;
    private GameObject _practiceDemoObject;

    public Vector3 TrialStartPosition { get; private set; } = Vector3.zero;
    public Vector3 TrialEndPosition { get; private set; } = Vector3.zero;
    public Vector3 HeadPosition_OnTrialStart { get; private set; } = Vector3.zero;
    public Vector3 HeadPosition_OnTaskStart { get; private set; } = Vector3.zero;
    public Dictionary<CubePositionLabels, Vector3> CubePositions { get; private set; } = new Dictionary<CubePositionLabels, Vector3>();

    public float TaskSpatialDistance => Vector3.Distance(TrialStartPosition, TrialEndPosition);

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

    public float TaskProgress => ProjectedDistanceOnTaskAxis / TaskSpatialDistance;

    protected override void Awake()
    {
        base.Awake();
        TargetLine = new Linescript();
        Circle_static = new Linescript(sampleNumberForCircle: 100, color: Color.gray);
        Circle_dynamic = new Linescript(sampleNumberForCircle: 100, color: Color.yellow);
        TargetLine.IsVisible = false;
    }

    private void Start()
    {
        _taskButtonPosition = TaskButtonsFront.position;
        _taskEndTextPosition = TaskEndText.transform.position;
        TaskEndText.transform.position = Vector3.down * 1000;

        if (IsPractice)
        {
            StartCoroutine(WaitAndSpawnPracticeObject());
        }
    }

    private IEnumerator WaitAndSpawnPracticeObject()
    {
        while (Camera.main.transform.position == Vector3.zero)
        {
            yield return null;
        }

        Vector3 spawnPosition = Camera.main.transform.position + 5 * Vector3.forward;
        _practiceDemoObject = SpawnPrefab(ObjectPrefab, spawnPosition, Quaternion.identity, ObjectPrefab.transform.localScale);
    }

    private void Update()
    {
        TaskText.text = IsPractice ? "Start Practice" : "Start Formal Test";
        if (TargetIndicator == null || ObjectToBeManipulated == null)
        {
            TargetLine.IsVisible = false;
            Circle_static.IsVisible = false;
            Circle_dynamic.IsVisible = false;
            return;
        }

        UpdateTaskVisualFeedbacks();

        TechniqueInputProvider inputProvider = TechniqueInputProvider.GetInstance();
        inputProvider.Refresh();

        if (ManipulationBehavior != null && inputProvider.Current.PinchState == PinchState.NotPinching && ManipulationBehavior.GrabbedObject != null)
        {
            DockingTarget dockingTarget = TargetIndicator.GetComponent<DockingTarget>();
            if (dockingTarget.PoseAligned_200msAgo || dockingTarget.IsPoseAligned())
            {
                Destroy(ObjectToBeManipulated);
                Destroy(TargetIndicator);

                ObjectToBeManipulated = null;
                TargetIndicator = null;
                AudioPlay.PlayClickSound();
            }
        }
    }

    private void UpdateTaskVisualFeedbacks()
    {
        TargetLine.SetPosition(TargetIndicator.transform.position, ObjectToBeManipulated.transform.position);

        float staticCircleRadius = TargetIndicator.transform.localScale.x * 1.2f;
        Circle_static.DrawRing(TargetIndicator.transform.position, staticCircleRadius);

        Vector3 camToObjectVector = ObjectToBeManipulated.transform.position - HeadPosition_OnTrialStart;
        Vector3 camToTargetVector = TargetIndicator.transform.position - HeadPosition_OnTrialStart;
        float projectedDistanceOnDepthAxis = Vector3.Project(camToObjectVector, camToTargetVector).magnitude;
        float depthProgress = Mathf.Max(0, 3 * (projectedDistanceOnDepthAxis / camToTargetVector.magnitude) - 2);

        Circle_dynamic.DrawRing(TargetIndicator.transform.position, staticCircleRadius * depthProgress);

        float circleLineWidth = MathFunctions.Deg2Meter(0.1f, camToTargetVector.magnitude);
        Circle_dynamic.SetWidth(circleLineWidth);
        Circle_static.SetWidth(circleLineWidth);

        float positionThreshold = TargetIndicator.GetComponent<DockingTarget>().GetPositionAlignmentThreshold();
        bool showRings = Mathf.Abs(camToObjectVector.magnitude - camToTargetVector.magnitude) > positionThreshold &&
                         Mathf.Abs(camToObjectVector.magnitude - camToTargetVector.magnitude) < positionThreshold * 4;

        Circle_static.IsVisible = showRings;
        Circle_dynamic.IsVisible = showRings;
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
        startObj = SpawnPrefab(ObjectPrefab, startPos, Quaternion.identity, ObjectPrefab.transform.localScale);
        target = SpawnPrefab(TargetPrefab, endPos, startObj.transform.rotation, TargetPrefab.transform.localScale);
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
        TaskButtonsFront.position = Vector3.down * 1000;
        TaskEndText.transform.position = Vector3.down * 1000;

        if (_practiceDemoObject) Destroy(_practiceDemoObject);
        StartCoroutine(RunTrials_within(OnStudyComplete));
    }

    private IEnumerator RunTrials_within(System.Action onComplete = null)
    {
        HeadPosition_OnTaskStart = Camera.main.transform.position;
        List<((float depth_min, float depth_max), float amplitude)> depthAmplitudeCombinations = IsPractice
            ? GetShuffledDepth_Amplitude_Combinations(DepthPairs_practice, Amplitudes_practice)
            : GetShuffledDepth_Amplitude_Combinations(DepthPairs_within, Amplitudes_within);

        foreach (((float depth_min, float depth_max), float amplitude) depthAmpCondition in depthAmplitudeCombinations)
        {
            var depthPair = depthAmpCondition.Item1;
            TaskMinDepth = depthPair.depth_min;
            TaskMaxDepth = depthPair.depth_max;
            TaskAmplitude = depthAmpCondition.Item2;

            CubePositions = GetCubePositions_Visual(
                viewPoint: HeadPosition_OnTaskStart,
                forwardDir: Vector3.forward,
                minDepth: TaskMinDepth,
                maxDepth: TaskMaxDepth,
                angularDeviation_horizontal: TaskAmplitude,
                angularDeviation_vertical: TaskAmplitude);

            List<CubePositionLabels> startPositionLabelsList = GetShuffledStartPositionLabels();

            foreach (CubePositionLabels startPosition in startPositionLabelsList)
            {
                TrialStartPosition = CubePositions[startPosition];
                TrialEndPosition = CubePositions[GetDiagonalPositionLabel(startPosition)];

                StartTrial(TrialStartPosition, TrialEndPosition, out ObjectToBeManipulated, out TargetIndicator);
                yield return StartCoroutine(WaitForTargetIndicatorToBeNull(null));
            }
        }

        onComplete?.Invoke();
    }

    public void OnStudyComplete()
    {
        if (IsPractice) TaskButtonsFront.position = _taskButtonPosition;
        TaskEndText.transform.position = _taskEndTextPosition;
    }

    private IEnumerator WaitForTargetIndicatorToBeNull(System.Action onComplete)
    {
        while (TargetIndicator != null)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }

    private Dictionary<CubePositionLabels, Vector3> GetCubePositions_Visual(
        Vector3 viewPoint,
        Vector3 forwardDir,
        float minDepth,
        float maxDepth,
        float angularDeviation_horizontal,
        float angularDeviation_vertical)
    {
        angularDeviation_vertical /= Mathf.Sqrt(2f);
        angularDeviation_horizontal /= Mathf.Sqrt(2f);

        return new Dictionary<CubePositionLabels, Vector3>
        {
            { CubePositionLabels.FrontUpperLeft, viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth },
            { CubePositionLabels.FrontUpperRight, viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth },
            { CubePositionLabels.FrontLowerLeft, viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth },
            { CubePositionLabels.FrontLowerRight, viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * minDepth },
            { CubePositionLabels.BackUpperLeft, viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth },
            { CubePositionLabels.BackUpperRight, viewPoint + Quaternion.AngleAxis(-angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth },
            { CubePositionLabels.BackLowerLeft, viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(-angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth },
            { CubePositionLabels.BackLowerRight, viewPoint + Quaternion.AngleAxis(angularDeviation_vertical, Vector3.right) * Quaternion.AngleAxis(angularDeviation_horizontal, Vector3.up) * forwardDir.normalized * maxDepth }
        };
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
        var combinations = new List<((float, float), float)>();
        foreach (var depth in depthPairs)
        {
            foreach (float amplitude in amplitudes)
            {
                combinations.Add((depth, amplitude));
            }
        }

        return combinations.OrderBy(_ => new System.Random().Next()).ToList();
    }

    public List<CubePositionLabels> GetShuffledStartPositionLabels()
    {
        var positions = System.Enum.GetValues(typeof(CubePositionLabels)).Cast<CubePositionLabels>().ToList();
        System.Random rng = new System.Random();
        return positions.OrderBy(_ => rng.Next()).ToList();
    }
}

