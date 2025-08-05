using UnityEngine;

public class SelectionTaskLogging : Singleton<SelectionTaskLogging>
{
    public DataLogger DataLogger;
    public bool LogData;

    string pid, technique;


    protected override void Awake()
    {
        base.Awake();
        SetupDataLogging();
    }
    
    void SetupDataLogging()
    {
        StudyControl studyControl = StudyControl.GetInstance();
        ManipulationTechnique technique = studyControl.ManipulationBehavior;
        GameObject objectToBeManipulated = studyControl.ObjectToBeManipulated;
        GameObject targetIndicator = studyControl.TargetIndicator;

        HandData handData = HandData.GetInstance();

        (string, System.Func<string>)[] _dataLoggerInput = {
            ("Participant", () => studyControl.ParticipantID.ToString()),
            ("Technique", () => technique.ToString()),
            ("TotalTrialCount", () => studyControl.TotalTrialCount.ToString()),


            ("ObjectPosition", () => objectToBeManipulated == null ? Vector3.zero.ToString() : objectToBeManipulated.transform.position.ToString()),
            ("ObjectRotation", () => objectToBeManipulated == null ? Quaternion.identity.ToString() : objectToBeManipulated.transform.rotation.ToString()),
            ("TargetPosition", () => targetIndicator == null ? Vector3.zero.ToString() : targetIndicator.transform.position.ToString()),
            ("TargetRotation", () => targetIndicator == null ? Quaternion.identity.ToString() : targetIndicator.transform.rotation.ToString()),
            ("PositionDifference", () => targetIndicator == null ? (-1f).ToString() : targetIndicator.GetComponent<DockingTarget>().PositionDifference.ToString()),
            ("OrientationDifference", () => targetIndicator == null ? (-1f).ToString() : targetIndicator.GetComponent<DockingTarget>().OrientationDifference.ToString()),
            ("IsAligned", () => targetIndicator == null ? "false" : targetIndicator.GetComponent<DockingTarget>().IsPoseAligned().ToString()),
            ("TrialStartPosition", () => studyControl.TrialStartPosition.ToString()),
            ("TrialEndPosition", () => studyControl.TrialEndPosition.ToString()),
            ("HeadPosition_OnTrialStart", () => studyControl.HeadPosition_OnTrialStart.ToString()),

            // technique used hand data
            ("VirtualHandPosition", () => technique.VirtualHandPosition.ToString()),
            ("PinchPosition", () => technique.PinchPosition.ToString()),
            ("PinchPosition_delta", () => technique.PinchPosition_delta.ToString()),
            ("WristPosition", () => technique.WristPosition.ToString()),
            ("WristPosition_delta", () => technique.WristPosition_delta.ToString()),
            ("PinchRotation_delta", () => technique.PinchRotation_delta.ToString()),
            ("HandTranslationSpeed", () => technique.HandTranslationSpeed.ToString()),
            ("HandRotationSpeed", () => technique.HandRotationSpeed.ToString()),
            ("IsHandStablized", () => technique.IsHandStablized.ToString()),
            ("VirtualHandPosition_OnGrab", () => technique.VirtualHandPosition_OnGrab.ToString()),
            ("ObjectPosition_OnGrab", () => technique.ObjectPosition_OnGrab.ToString()),

            // technique used gaze data
            ("GazeOrigin", () => technique.GazeOrigin.ToString()),
            ("GazeDirection", () => technique.GazeDirection.ToString()),
            ("IsGazeFixating", () => technique.IsGazeFixating.ToString()),
            ("IsGazeFixating_pre", () => technique.IsGazeFixating_pre.ToString()),
            ("GazeFixationCentroid", () => technique.GazeFixationCentroid.ToString()),
            ("IsGazeSaccading", () => technique.IsGazeSaccading.ToString()),
            ("GazeDirection_OnGazeFixation", () => technique.GazeDirection_OnGazeFixation.ToString()),
            ("HeadDirection_OnGazeFixation", () => technique.HeadDirection_OnGazeFixation.ToString()),
            ("EyeInHeadYAngle", () => technique.EyeInHeadYAngle.ToString()),
            ("Filtered_EyeInHeadAngle", () => technique.Filtered_EyeInHeadAngle.ToString()),
            ("Filtered_EyeInHeadAngle_Pre", () => technique.Filtered_EyeInHeadAngle_Pre.ToString()),
            ("EyeInHeadXAngle", () => technique.EyeInHeadXAngle.ToString()),
            ("EyeInHeadYAngle_OnGazeFixation", () => technique.EyeInHeadYAngle_OnGazeFixation.ToString()),
            ("Filtered_HandMovementVector", () => technique.Filtered_HandMovementVector.ToString()),

            // technique used head data
            ("HeadForward", () => technique.HeadForward.ToString()),
            ("HeadRight", () => technique.HeadRight.ToString()),
            ("HeadPosition", () => technique.HeadPosition.ToString()),
            ("IsHeadFixating", () => technique.IsHeadFixating.ToString()),
            ("IsHeadFixating_pre", () => technique.IsHeadFixating_pre.ToString()),
            ("HeadFixationCentroid", () => technique.HeadFixationCentroid.ToString()),
            ("HeadSpeed", () => technique.HeadSpeed.ToString()),
            ("HeadYAngle", () => technique.HeadYAngle.ToString()),
            ("DeltaHeadY", () => technique.DeltaHeadY.ToString()),
            ("Limit_HeadY_Up", () => technique.Limit_HeadY_Up.ToString()),
            ("Limit_HeadY_Down", () => technique.Limit_HeadY_Down.ToString()),
            ("HeadYAngle_OnGazeFixation", () => technique.HeadYAngle_OnGazeFixation.ToString()),
            

            // raw hand data
            ("RightHandPosition", () => handData.RightHandPosition.ToString()),
            ("LeftHandPosition", () => handData.LeftHandPosition.ToString()),
            ("RightPinchTipPosition", () => handData.RightPinchTipPosition.ToString()),
            ("LeftPinchTipPosition", () => handData.LeftPinchTipPosition.ToString()),
            ("RightHandRotation", () => handData.RightHandRotation.ToString()),
            ("LeftHandRotation", () => handData.LeftHandRotation.ToString()),
            ("RightPinchTipRotation", () => handData.RightPinchTipRotation.ToString()),
            ("LeftPinchTipRotation", () => handData.LeftPinchTipRotation.ToString()),
            ("RightHandPosition_delta", () => handData.RightHandPosition_delta.ToString()),
            ("LeftHandPosition_delta", () => handData.LeftHandPosition_delta.ToString()),
            ("RightPinchTipPosition_delta", () => handData.RightPinchTipPosition_delta.ToString()),
            ("LeftPinchTipPosition_delta", () => handData.LeftPinchTipPosition_delta.ToString()),
            ("RightHandRotation_delta", () => handData.RightHandRotation_delta.ToString()),
            ("LeftHandRotation_delta", () => handData.LeftHandRotation_delta.ToString()),
            ("RightPinchTipRotation_delta", () => handData.RightPinchTipRotation_delta.ToString()),
            ("LeftPinchTipRotation_delta", () => handData.LeftPinchTipRotation_delta.ToString()),
            ("RightHandDirection", () => handData.RightHandDirection.ToString()),
            ("LeftHandDirection", () => handData.LeftHandDirection.ToString()),
            ("RightHandDirection_delta", () => handData.RightHandDirection_delta.ToString()),
            ("LeftHandDirection_delta", () => handData.LeftHandDirection_delta.ToString()),
            ("RightHandSpeed_wrist", () => handData.RightHandSpeed_wrist.ToString()),
            ("LeftHandSpeed_wrist", () => handData.LeftHandSpeed_wrist.ToString()),
            ("RightHandSpeed_pinch", () => handData.RightHandSpeed_pinch.ToString()),
            ("LeftHandSpeed_pinch", () => handData.LeftHandSpeed_pinch.ToString()),
            ("HandDistance", () => handData.HandDistance.ToString()),
            ("HandDistance_delta", () => handData.HandDistance_delta.ToString()),
            ("HandMidPosition", () => handData.HandMidPosition.ToString()),
            ("HandMidPosition_delta", () => handData.HandMidPosition_delta.ToString()),

            // raw gaze data

            // position, oritenation aligned, double aligned

            ("Time", () => Time.realtimeSinceStartup.ToString()),

        };
        
            //TODO: start positionlabel, forward or backward
        // _dataLoggerInput = _dataLoggerInput.Concat(technique.GetTechniqueData()).ToArray();

        DataLogger = new DataLogger(_dataLoggerInput);
    }

    void Update()
    {
        if(LogData && StudyControl.GetInstance().IsPractice == false)
        {
            DataLogger.LogData();
        }
    }    

    void OnDisable()
    {
        SaveCSV();
    }


    void SaveCSV()
    {
        DataLogger.ExportDataToCSV(pid + "-" + technique);
        DataLogger.ClearData();
    }

}
