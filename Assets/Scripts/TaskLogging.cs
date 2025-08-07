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

        pid = StudyControl.GetInstance().ParticipantID.ToString();
        technique = StudyControl.GetInstance().ManipulationBehavior.GetType().Name.ToString();
    }
    
    void SetupDataLogging()
    {
        (string, System.Func<string>)[] _dataLoggerInput = {

            // study settings
            ("Participant", () => StudyControl.GetInstance().ParticipantID.ToString()),
            ("DominantHand", () => StudyControl.GetInstance().DominantHand.ToString()),
            ("Technique", () => StudyControl.GetInstance().ManipulationBehavior.GetType().Name.ToString()),
            ("TaskMode", () => StudyControl.GetInstance().TaskMode.ToString()),
            ("IsPractice", () => StudyControl.GetInstance().IsPractice.ToString()),

            // study status
            ("TotalTrialCount", () => StudyControl.GetInstance().TotalTrialCount.ToString()),
            ("StudyFlag", () => StudyControl.GetInstance().StudyFlag.ToString()),
            ("TaskMinDepth", () => StudyControl.GetInstance().TaskMinDepth.ToString()),
            ("TaskMaxDepth", () => StudyControl.GetInstance().TaskMaxDepth.ToString()),
            ("TaskAmplitude", () => StudyControl.GetInstance().TaskAmplitude.ToString()),
            ("StartPositionLabel", () => StudyControl.GetInstance().StartPositionLabel.ToString()),
            ("EndPositionLabel", () => StudyControl.GetInstance().GetDiagonalPositionLabel(StudyControl.GetInstance().StartPositionLabel).ToString()),

            ("ObjectPosition", () => StudyControl.GetInstance().ObjectToBeManipulated == null ? Vector3.zero.ToString() : StudyControl.GetInstance().ObjectToBeManipulated.transform.position.ToString()),
            ("ObjectRotation", () => StudyControl.GetInstance().ObjectToBeManipulated == null ? Quaternion.identity.ToString() : StudyControl.GetInstance().ObjectToBeManipulated.transform.rotation.ToString()),
            ("TargetPosition", () => StudyControl.GetInstance().TargetIndicator == null ? Vector3.zero.ToString() : StudyControl.GetInstance().TargetIndicator.transform.position.ToString()),
            ("TargetRotation", () => StudyControl.GetInstance().TargetIndicator == null ? Quaternion.identity.ToString() : StudyControl.GetInstance().TargetIndicator.transform.rotation.ToString()),
            ("PositionDifference", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().PositionDifference.ToString()),
            ("OrientationDifference", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().OrientationDifference.ToString()),
            ("IsPoseAligned", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsPoseAligned().ToString()),
            ("IsPositionAligned", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsPositionAligned().ToString()),
            ("IsPositionAligned_Double", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsPositionAligned_Double().ToString()),
            ("IsOrientationAligned", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsOrientationAligned().ToString()),
            ("IsOrientationAligned_Double", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsOrientationAligned_Double().ToString()),
            ("TrialStartPosition", () => StudyControl.GetInstance().TrialStartPosition.ToString()),
            ("TrialEndPosition", () => StudyControl.GetInstance().TrialEndPosition.ToString()),
            ("HeadPosition_OnTrialStart", () => StudyControl.GetInstance().HeadPosition_OnTrialStart.ToString()),

            ("IsObjectHitbyGaze", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().ObjectToBeManipulated.GetComponent<ManipulatableObject>().IsHitbyGaze.ToString()),
            ("ObjectAngleToGaze", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().ObjectToBeManipulated.GetComponent<ManipulatableObject>().AngleToGaze.ToString()),
            ("IsObjectPinched", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().ObjectToBeManipulated.GetComponent<ManipulatableObject>().GrabbedState.ToString()),


            // technique used hand data
            ("TechInput_VirtualHandPosition", () => StudyControl.GetInstance().ManipulationBehavior.VirtualHandPosition.ToString()),
            ("TechInput_PinchPosition", () => StudyControl.GetInstance().ManipulationBehavior.PinchPosition.ToString()),
            ("TechInput_PinchPosition_delta", () => StudyControl.GetInstance().ManipulationBehavior.PinchPosition_delta.ToString()),
            ("TechInput_WristPosition", () => StudyControl.GetInstance().ManipulationBehavior.WristPosition.ToString()),
            ("TechInput_WristPosition_delta", () => StudyControl.GetInstance().ManipulationBehavior.WristPosition_delta.ToString()),
            ("TechInput_PinchRotation_delta", () => StudyControl.GetInstance().ManipulationBehavior.PinchRotation_delta.ToString()),
            ("TechInput_HandTranslationSpeed", () => StudyControl.GetInstance().ManipulationBehavior.HandTranslationSpeed.ToString()),
            ("TechInput_HandRotationSpeed", () => StudyControl.GetInstance().ManipulationBehavior.HandRotationSpeed.ToString()),
            ("TechInput_IsHandStablized", () => StudyControl.GetInstance().ManipulationBehavior.IsHandStablized.ToString()),
            ("TechInput_VirtualHandPosition_OnGrab", () => StudyControl.GetInstance().ManipulationBehavior.VirtualHandPosition_OnGrab.ToString()),
            ("TechInput_ObjectPosition_OnGrab", () => StudyControl.GetInstance().ManipulationBehavior.ObjectPosition_OnGrab.ToString()),

            // technique used gaze data
            ("TechInput_GazeOrigin", () => StudyControl.GetInstance().ManipulationBehavior.GazeOrigin.ToString()),
            ("TechInput_GazeDirection", () => StudyControl.GetInstance().ManipulationBehavior.GazeDirection.ToString()),
            ("TechInput_IsGazeFixating", () => StudyControl.GetInstance().ManipulationBehavior.IsGazeFixating.ToString()),
            ("TechInput_IsGazeFixating_pre", () => StudyControl.GetInstance().ManipulationBehavior.IsGazeFixating_pre.ToString()),
            ("TechInput_GazeFixationCentroid", () => StudyControl.GetInstance().ManipulationBehavior.GazeFixationCentroid.ToString()),
            ("TechInput_IsGazeSaccading", () => StudyControl.GetInstance().ManipulationBehavior.IsGazeSaccading.ToString()),
            ("TechInput_GazeDirection_OnGazeFixation", () => StudyControl.GetInstance().ManipulationBehavior.GazeDirection_OnGazeFixation.ToString()),
            ("TechInput_HeadDirection_OnGazeFixation", () => StudyControl.GetInstance().ManipulationBehavior.HeadDirection_OnGazeFixation.ToString()),
            ("TechInput_EyeInHeadYAngle", () => StudyControl.GetInstance().ManipulationBehavior.EyeInHeadYAngle.ToString()),
            ("TechInput_Filtered_EyeInHeadAngle", () => StudyControl.GetInstance().ManipulationBehavior.Filtered_EyeInHeadAngle.ToString()),
            ("TechInput_Filtered_EyeInHeadAngle_Pre", () => StudyControl.GetInstance().ManipulationBehavior.Filtered_EyeInHeadAngle_Pre.ToString()),
            ("TechInput_EyeInHeadXAngle", () => StudyControl.GetInstance().ManipulationBehavior.EyeInHeadXAngle.ToString()),
            ("TechInput_EyeInHeadYAngle_OnGazeFixation", () => StudyControl.GetInstance().ManipulationBehavior.EyeInHeadYAngle_OnGazeFixation.ToString()),
            ("TechInput_Filtered_HandMovementVector", () => StudyControl.GetInstance().ManipulationBehavior.Filtered_HandMovementVector.ToString()),

            // technique used head data
            ("TechInput_HeadForward", () => StudyControl.GetInstance().ManipulationBehavior.HeadForward.ToString()),
            ("TechInput_HeadRight", () => StudyControl.GetInstance().ManipulationBehavior.HeadRight.ToString()),
            ("TechInput_HeadPosition", () => StudyControl.GetInstance().ManipulationBehavior.HeadPosition.ToString()),
            ("TechInput_IsHeadFixating", () => StudyControl.GetInstance().ManipulationBehavior.IsHeadFixating.ToString()),
            ("TechInput_IsHeadFixating_pre", () => StudyControl.GetInstance().ManipulationBehavior.IsHeadFixating_pre.ToString()),
            ("TechInput_HeadFixationCentroid", () => StudyControl.GetInstance().ManipulationBehavior.HeadFixationCentroid.ToString()),
            ("TechInput_HeadSpeed", () => StudyControl.GetInstance().ManipulationBehavior.HeadSpeed.ToString()),
            ("TechInput_HeadYAngle", () => StudyControl.GetInstance().ManipulationBehavior.HeadYAngle.ToString()),
            ("TechInput_DeltaHeadY", () => StudyControl.GetInstance().ManipulationBehavior.DeltaHeadY.ToString()),
            ("TechInput_Limit_HeadY_Up", () => StudyControl.GetInstance().ManipulationBehavior.Limit_HeadY_Up.ToString()),
            ("TechInput_Limit_HeadY_Down", () => StudyControl.GetInstance().ManipulationBehavior.Limit_HeadY_Down.ToString()),
            ("TechInput_HeadYAngle_OnGazeFixation", () => StudyControl.GetInstance().ManipulationBehavior.HeadYAngle_OnGazeFixation.ToString()),

            // raw hand data
            ("RightHandPosition", () => HandData.GetInstance().RightHandPosition.ToString()),
            ("LeftHandPosition", () => HandData.GetInstance().LeftHandPosition.ToString()),
            ("RightPinchTipPosition", () => HandData.GetInstance().RightPinchTipPosition.ToString()),
            ("LeftPinchTipPosition", () => HandData.GetInstance().LeftPinchTipPosition.ToString()),
            ("RightHandRotation", () => HandData.GetInstance().RightHandRotation.ToString()),
            ("LeftHandRotation", () => HandData.GetInstance().LeftHandRotation.ToString()),
            ("RightPinchTipRotation", () => HandData.GetInstance().RightPinchTipRotation.ToString()),
            ("LeftPinchTipRotation", () => HandData.GetInstance().LeftPinchTipRotation.ToString()),
            ("RightHandPosition_delta", () => HandData.GetInstance().RightHandPosition_delta.ToString()),
            ("LeftHandPosition_delta", () => HandData.GetInstance().LeftHandPosition_delta.ToString()),
            ("RightPinchTipPosition_delta", () => HandData.GetInstance().RightPinchTipPosition_delta.ToString()),
            ("LeftPinchTipPosition_delta", () => HandData.GetInstance().LeftPinchTipPosition_delta.ToString()),
            ("RightHandRotation_delta", () => HandData.GetInstance().RightHandRotation_delta.ToString()),
            ("LeftHandRotation_delta", () => HandData.GetInstance().LeftHandRotation_delta.ToString()),
            ("RightPinchTipRotation_delta", () => HandData.GetInstance().RightPinchTipRotation_delta.ToString()),
            ("LeftPinchTipRotation_delta", () => HandData.GetInstance().LeftPinchTipRotation_delta.ToString()),
            ("RightHandDirection", () => HandData.GetInstance().RightHandDirection.ToString()),
            ("LeftHandDirection", () => HandData.GetInstance().LeftHandDirection.ToString()),
            ("RightHandDirection_delta", () => HandData.GetInstance().RightHandDirection_delta.ToString()),
            ("LeftHandDirection_delta", () => HandData.GetInstance().LeftHandDirection_delta.ToString()),
            ("RightHandSpeed_wrist", () => HandData.GetInstance().RightHandSpeed_wrist.ToString()),
            ("LeftHandSpeed_wrist", () => HandData.GetInstance().LeftHandSpeed_wrist.ToString()),
            ("RightHandSpeed_pinch", () => HandData.GetInstance().RightHandSpeed_pinch.ToString()),
            ("LeftHandSpeed_pinch", () => HandData.GetInstance().LeftHandSpeed_pinch.ToString()),
            ("HandDistance", () => HandData.GetInstance().HandDistance.ToString()),
            ("HandDistance_delta", () => HandData.GetInstance().HandDistance_delta.ToString()),
            ("HandMidPosition", () => HandData.GetInstance().HandMidPosition.ToString()),
            ("HandMidPosition_delta", () => HandData.GetInstance().HandMidPosition_delta.ToString()),

            ("IsRightPinching", () => PinchDetector.GetInstance().IsRightPinching.ToString()),
            ("IsLeftPinching", () => PinchDetector.GetInstance().IsLeftPinching.ToString()),
            ("IsBothHandsPinching", () => PinchDetector.GetInstance().IsBothHandsPinching.ToString()),
            ("IsOneHandPinching", () => PinchDetector.GetInstance().IsOneHandPinching.ToString()),
            ("IsNoHandPinching", () => PinchDetector.GetInstance().IsNoHandPinching.ToString()),
            ("IsNoHandPinching_LastFrame", () => PinchDetector.GetInstance().IsNoHandPinching_LastFrame.ToString()),
            ("PinchState", () => PinchDetector.GetInstance().PinchState.ToString()),

            // raw gaze data
            ("GazeOrigin", () => EyeGaze.GetInstance().GetGazeRay().origin.ToString()),
            ("GazeDirection", () => EyeGaze.GetInstance().GetGazeRay().direction.ToString()),
            ("RawGazeOrigin", () => EyeGaze.GetInstance().GetRawGazeOrigin().ToString()),
            ("RawGazeDirection", () => EyeGaze.GetInstance().GetRawGazeDirection().ToString()),
            ("FilteredEyeInHeadAngle", () => EyeGaze.GetInstance().FilteredEyeInHeadAngle.ToString()),
            ("FilteredEyeInHeadAngle_Pre", () => EyeGaze.GetInstance().FilteredEyeInHeadAngle_Pre.ToString()),
            ("EyeInHeadYAngle", () => EyeGaze.GetInstance().EyeInHeadYAngle.ToString()),
            ("EyeInHeadXAngle", () => EyeGaze.GetInstance().EyeInHeadXAngle.ToString()),
            


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
