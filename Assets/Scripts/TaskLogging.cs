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
            ("IsPractice", () => StudyControl.GetInstance().IsPractice.ToString()),

            // study status
            ("TotalTrialCount", () => StudyControl.GetInstance().TotalTrialCount.ToString()),
            ("StudyFlag", () => StudyControl.GetInstance().StudyFlag.ToString()),
            ("TaskMinDepth", () => StudyControl.GetInstance().TaskMinDepth.ToString()),
            ("TaskMaxDepth", () => StudyControl.GetInstance().TaskMaxDepth.ToString()),
            ("TaskDepthSet", () => StudyControl.GetInstance().TaskMinDepth.ToString() + "_" + StudyControl.GetInstance().TaskMaxDepth.ToString()),
            ("TaskDepthDistance", () => (StudyControl.GetInstance().TaskMaxDepth - StudyControl.GetInstance().TaskMinDepth).ToString()),
            ("TaskAmplitude", () => StudyControl.GetInstance().TaskAmplitude.ToString()),
            ("StartPositionLabel", () => StudyControl.GetInstance().StartPositionLabel.ToString()),
            ("EndPositionLabel", () => StudyControl.GetInstance().GetDiagonalPositionLabel(StudyControl.GetInstance().StartPositionLabel).ToString()),
            ("IsPickedUpOnce", () => StudyControl.GetInstance().IsAfterFirstPickUpInTrial.ToString()),
            
            ("TaskProgress", () => StudyControl.GetInstance().TaskProgress.ToString("F10")),
            ("ProjectedDistanceOnTaskAxis", () => StudyControl.GetInstance().ProjectedDistanceOnTaskAxis.ToString("F10")),
            ("TaskSpatialDistance", () => StudyControl.GetInstance().TaskSpatialDistance.ToString("F10")),

            ("ObjectPosition", () => StudyControl.GetInstance().ObjectToBeManipulated == null ? Vector3.zero.ToString("F10") : StudyControl.GetInstance().ObjectToBeManipulated.transform.position.ToString("F10")),
            ("ObjectRotation", () => StudyControl.GetInstance().ObjectToBeManipulated == null ? Quaternion.identity.ToString("F10") : StudyControl.GetInstance().ObjectToBeManipulated.transform.rotation.ToString("F10")),
            ("TargetPosition", () => StudyControl.GetInstance().TargetIndicator == null ? Vector3.zero.ToString("F10") : StudyControl.GetInstance().TargetIndicator.transform.position.ToString("F10")),
            ("TargetRotation", () => StudyControl.GetInstance().TargetIndicator == null ? Quaternion.identity.ToString("F10") : StudyControl.GetInstance().TargetIndicator.transform.rotation.ToString("F10")),
            ("PositionDifference", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().PositionDifference.ToString("F10")),
            ("OrientationDifference", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().OrientationDifference.ToString("F10")),
            ("IsPoseAligned", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsPoseAligned().ToString()),
            ("IsPositionAligned", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsPositionAligned().ToString()),
            ("IsPositionAligned_Double", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsPositionAligned_Double().ToString()),
            ("IsOrientationAligned", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsOrientationAligned().ToString()),
            ("IsOrientationAligned_Double", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>().IsOrientationAligned_Double().ToString()),
            ("TrialStartPosition", () => StudyControl.GetInstance().TrialStartPosition.ToString("F10")),
            ("TrialEndPosition", () => StudyControl.GetInstance().TrialEndPosition.ToString("F10")),
            ("HeadPosition_OnTrialStart", () => StudyControl.GetInstance().HeadPosition_OnTrialStart.ToString("F10")),

            ("IsObjectHitbyGaze", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().ObjectToBeManipulated.GetComponent<ManipulatableObject>().IsHitbyGaze.ToString()),
            ("ObjectAngleToGaze", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().ObjectToBeManipulated.GetComponent<ManipulatableObject>().AngleToGaze.ToString("F10")),
            ("IsObjectPinched", () => (StudyControl.GetInstance().TargetIndicator == null || StudyControl.GetInstance().TargetIndicator.GetComponent<DockingTarget>() == null) ? "Null Value" : StudyControl.GetInstance().ObjectToBeManipulated.GetComponent<ManipulatableObject>().GrabbedState.ToString()),

            // planned metrics
            ("PinchTipTranslationDistance", () => StudyControl.GetInstance().ManipulationBehavior.PinchPosition_delta.magnitude.ToString("F10")),
            ("WristTranslationDistance", () => StudyControl.GetInstance().ManipulationBehavior.WristPosition_delta.magnitude.ToString("F10")),
            ("PinchTipRotationAngle", () => Quaternion.Angle(Quaternion.identity, StudyControl.GetInstance().ManipulationBehavior.PinchRotation_delta).ToString("F10")),

            // technique used hand data
            ("TechInput_VirtualHandPosition", () => StudyControl.GetInstance().ManipulationBehavior.VirtualHandPosition.ToString("F10")),
            ("TechInput_PinchPosition", () => StudyControl.GetInstance().ManipulationBehavior.PinchPosition.ToString("F10")),
            ("TechInput_PinchPosition_delta", () => StudyControl.GetInstance().ManipulationBehavior.PinchPosition_delta.ToString("F10")),
            ("TechInput_WristPosition", () => StudyControl.GetInstance().ManipulationBehavior.WristPosition.ToString("F10")),
            ("TechInput_WristPosition_delta", () => StudyControl.GetInstance().ManipulationBehavior.WristPosition_delta.ToString("F10")),
            ("TechInput_PinchRotation_delta", () => StudyControl.GetInstance().ManipulationBehavior.PinchRotation_delta.ToString("F10")),
            ("TechInput_HandTranslationSpeed", () => StudyControl.GetInstance().ManipulationBehavior.HandTranslationSpeed.ToString("F10")),
            ("TechInput_HandRotationSpeed", () => StudyControl.GetInstance().ManipulationBehavior.HandRotationSpeed.ToString("F10")),
            ("TechInput_IsHandStablized", () => StudyControl.GetInstance().ManipulationBehavior.IsHandStablized.ToString()),
            ("TechInput_VirtualHandPosition_OnGrab", () => StudyControl.GetInstance().ManipulationBehavior.VirtualHandPosition_OnGrab.ToString("F10")),
            ("TechInput_ObjectPosition_OnGrab", () => StudyControl.GetInstance().ManipulationBehavior.ObjectPosition_OnGrab.ToString("F10")),

            // technique used gaze data
            ("TechInput_GazeOrigin", () => StudyControl.GetInstance().ManipulationBehavior.GazeOrigin.ToString("F10")),
            ("TechInput_GazeDirection", () => StudyControl.GetInstance().ManipulationBehavior.GazeDirection.ToString("F10")),
            ("TechInput_IsGazeFixating", () => StudyControl.GetInstance().ManipulationBehavior.IsGazeFixating.ToString()),
            ("TechInput_IsGazeFixating_pre", () => StudyControl.GetInstance().ManipulationBehavior.IsGazeFixating_pre.ToString()),
            ("TechInput_GazeFixationCentroid", () => StudyControl.GetInstance().ManipulationBehavior.GazeFixationCentroid.ToString()),
            ("TechInput_IsGazeSaccading", () => StudyControl.GetInstance().ManipulationBehavior.IsGazeSaccading.ToString()),
            ("TechInput_GazeDirection_OnGazeFixation", () => StudyControl.GetInstance().ManipulationBehavior.GazeDirection_OnGazeFixation.ToString("F10")),
            ("TechInput_HeadDirection_OnGazeFixation", () => StudyControl.GetInstance().ManipulationBehavior.HeadDirection_OnGazeFixation.ToString("F10")),
            ("TechInput_EyeInHeadYAngle", () => StudyControl.GetInstance().ManipulationBehavior.EyeInHeadYAngle.ToString("F10")),
            ("TechInput_Filtered_EyeInHeadAngle", () => StudyControl.GetInstance().ManipulationBehavior.Filtered_EyeInHeadAngle.ToString("F10")),
            ("TechInput_Filtered_EyeInHeadAngle_Pre", () => StudyControl.GetInstance().ManipulationBehavior.Filtered_EyeInHeadAngle_Pre.ToString("F10")),
            ("TechInput_EyeInHeadXAngle", () => StudyControl.GetInstance().ManipulationBehavior.EyeInHeadXAngle.ToString("F10")),
            ("TechInput_EyeInHeadYAngle_OnGazeFixation", () => StudyControl.GetInstance().ManipulationBehavior.EyeInHeadYAngle_OnGazeFixation.ToString("F10")),
            ("TechInput_Filtered_HandMovementVector", () => StudyControl.GetInstance().ManipulationBehavior.Filtered_HandMovementVector.ToString("F10")),

            // technique used head data
            ("TechInput_HeadForward", () => StudyControl.GetInstance().ManipulationBehavior.HeadForward.ToString("F10")),
            ("TechInput_HeadRight", () => StudyControl.GetInstance().ManipulationBehavior.HeadRight.ToString("F10")),
            ("TechInput_HeadPosition", () => StudyControl.GetInstance().ManipulationBehavior.HeadPosition.ToString("F10")),
            ("TechInput_IsHeadFixating", () => StudyControl.GetInstance().ManipulationBehavior.IsHeadFixating.ToString()),
            ("TechInput_IsHeadFixating_pre", () => StudyControl.GetInstance().ManipulationBehavior.IsHeadFixating_pre.ToString()),
            ("TechInput_HeadFixationCentroid", () => StudyControl.GetInstance().ManipulationBehavior.HeadFixationCentroid.ToString("F10")),
            ("TechInput_HeadSpeed", () => StudyControl.GetInstance().ManipulationBehavior.HeadSpeed.ToString("F10")),
            ("TechInput_HeadYAngle", () => StudyControl.GetInstance().ManipulationBehavior.HeadYAngle.ToString("F10")),
            ("TechInput_DeltaHeadY", () => StudyControl.GetInstance().ManipulationBehavior.DeltaHeadY.ToString("F10")),
            ("TechInput_Limit_HeadY_Up", () => StudyControl.GetInstance().ManipulationBehavior.Limit_HeadY_Up.ToString("F10")),
            ("TechInput_Limit_HeadY_Down", () => StudyControl.GetInstance().ManipulationBehavior.Limit_HeadY_Down.ToString("F10")),
            ("TechInput_HeadYAngle_OnGazeFixation", () => StudyControl.GetInstance().ManipulationBehavior.HeadYAngle_OnGazeFixation.ToString("F10")),

            // raw hand data
            ("RightHandPosition", () => HandData.GetInstance().RightHandPosition.ToString("F10")),
            ("LeftHandPosition", () => HandData.GetInstance().LeftHandPosition.ToString("F10")),
            ("RightPinchTipPosition", () => HandData.GetInstance().RightPinchTipPosition.ToString("F10")),
            ("LeftPinchTipPosition", () => HandData.GetInstance().LeftPinchTipPosition.ToString("F10")),
            ("RightHandRotation", () => HandData.GetInstance().RightHandRotation.ToString("F10")),
            ("LeftHandRotation", () => HandData.GetInstance().LeftHandRotation.ToString("F10")),
            ("RightPinchTipRotation", () => HandData.GetInstance().RightPinchTipRotation.ToString("F10")),
            ("LeftPinchTipRotation", () => HandData.GetInstance().LeftPinchTipRotation.ToString("F10")),
            ("RightHandPosition_delta", () => HandData.GetInstance().RightHandPosition_delta.ToString("F10")),
            ("LeftHandPosition_delta", () => HandData.GetInstance().LeftHandPosition_delta.ToString("F10")),
            ("RightPinchTipPosition_delta", () => HandData.GetInstance().RightPinchTipPosition_delta.ToString("F10")),
            ("LeftPinchTipPosition_delta", () => HandData.GetInstance().LeftPinchTipPosition_delta.ToString("F10")),
            ("RightHandRotation_delta", () => HandData.GetInstance().RightHandRotation_delta.ToString("F10")),
            ("LeftHandRotation_delta", () => HandData.GetInstance().LeftHandRotation_delta.ToString("F10")),
            ("RightPinchTipRotation_delta", () => HandData.GetInstance().RightPinchTipRotation_delta.ToString("F10")),
            ("LeftPinchTipRotation_delta", () => HandData.GetInstance().LeftPinchTipRotation_delta.ToString("F10")),
            ("RightHandDirection", () => HandData.GetInstance().RightHandDirection.ToString("F10")),
            ("LeftHandDirection", () => HandData.GetInstance().LeftHandDirection.ToString("F10")),
            ("RightHandDirection_delta", () => HandData.GetInstance().RightHandDirection_delta.ToString("F10")),
            ("LeftHandDirection_delta", () => HandData.GetInstance().LeftHandDirection_delta.ToString("F10")),
            ("RightHandSpeed_wrist", () => HandData.GetInstance().RightHandSpeed_wrist.ToString("F10")),
            ("LeftHandSpeed_wrist", () => HandData.GetInstance().LeftHandSpeed_wrist.ToString("F10")),
            ("RightHandSpeed_pinch", () => HandData.GetInstance().RightHandSpeed_pinch.ToString("F10")),
            ("LeftHandSpeed_pinch", () => HandData.GetInstance().LeftHandSpeed_pinch.ToString("F10")),
            ("HandDistance", () => HandData.GetInstance().HandDistance.ToString("F10")),
            ("HandDistance_delta", () => HandData.GetInstance().HandDistance_delta.ToString("F10")),
            ("HandMidPosition", () => HandData.GetInstance().HandMidPosition.ToString("F10")),
            ("HandMidPosition_delta", () => HandData.GetInstance().HandMidPosition_delta.ToString("F10")),

            ("IsRightPinching", () => PinchDetector.GetInstance().IsRightPinching.ToString()),
            ("IsLeftPinching", () => PinchDetector.GetInstance().IsLeftPinching.ToString()),
            ("IsBothHandsPinching", () => PinchDetector.GetInstance().IsBothHandsPinching.ToString()),
            ("IsOneHandPinching", () => PinchDetector.GetInstance().IsOneHandPinching.ToString()),
            ("IsNoHandPinching", () => PinchDetector.GetInstance().IsNoHandPinching.ToString()),
            ("IsNoHandPinching_LastFrame", () => PinchDetector.GetInstance().IsNoHandPinching_LastFrame.ToString()),
            ("PinchState", () => PinchDetector.GetInstance().PinchState.ToString()),

            // raw gaze data
            ("GazeOrigin", () => EyeGaze.GetInstance().GetGazeRay().origin.ToString("F10")),
            ("GazeDirection", () => EyeGaze.GetInstance().GetGazeRay().direction.ToString("F10")),
            ("RawGazeOrigin", () => EyeGaze.GetInstance().GetRawGazeOrigin().ToString("F10")),
            ("RawGazeDirection", () => EyeGaze.GetInstance().GetRawGazeDirection().ToString("F10")),
            ("RawEyeInHeadAngle", () => EyeGaze.GetInstance().EyeInHeadAngle.ToString("F10")),
            ("FilteredEyeInHeadAngle", () => EyeGaze.GetInstance().FilteredEyeInHeadAngle.ToString("F10")),
            ("FilteredEyeInHeadAngle_Pre", () => EyeGaze.GetInstance().FilteredEyeInHeadAngle_Pre.ToString("F10")),
            ("EyeInHeadYAngle", () => EyeGaze.GetInstance().EyeInHeadYAngle.ToString("F10")),
            ("EyeInHeadXAngle", () => EyeGaze.GetInstance().EyeInHeadXAngle.ToString("F10")),
            
            // Special Technique Variables (from ManipulationTechnique.cs lines 320-340)
            ("CurrentState", () => StudyControl.GetInstance().ManipulationBehavior.CurrentState.ToString()),
            ("VisualGainValue", () => StudyControl.GetInstance().ManipulationBehavior.VisualGainValue.ToString("F10")),
            ("OffsetAddedByHand", () => StudyControl.GetInstance().ManipulationBehavior.OffsetAddedByHand.ToString("F10")),
            ("OffsetAddedByHand_Distance", () => StudyControl.GetInstance().ManipulationBehavior.OffsetAddedByHand.magnitude.ToString("F10")),
            ("AngleRotatedByHand", () => StudyControl.GetInstance().ManipulationBehavior.AngleRotatedByHand.ToString("F10")),
            ("CurrentDistanceToGaze", () => StudyControl.GetInstance().ManipulationBehavior.CurrentDistanceToGaze.ToString("F10")),
            ("HeadDepthOffset", () => StudyControl.GetInstance().ManipulationBehavior.HeadDepthOffset.ToString("F10")),
            ("HeadDepthOffset_magnitude", () => StudyControl.GetInstance().ManipulationBehavior.HeadDepthOffset.magnitude.ToString("F10")),
            ("DistanceToGazeAfterAddingHeadDepth", () => StudyControl.GetInstance().ManipulationBehavior.DistanceToGazeAfterAddingHeadDepth.ToString("F10")),
            ("AngleGazeDirectionToObject", () => StudyControl.GetInstance().ManipulationBehavior.AngleGazeDirectionToObject.ToString("F10")),
            ("MinHeadSpeed", () => StudyControl.GetInstance().ManipulationBehavior.MinHeadSpeed.ToString("F10")),
            ("MaxHeadSpeed", () => StudyControl.GetInstance().ManipulationBehavior.MaxHeadSpeed.ToString("F10")),
            ("MinGainDeg", () => StudyControl.GetInstance().ManipulationBehavior.MinGainDeg.ToString("F10")),
            ("MaxGainDeg", () => StudyControl.GetInstance().ManipulationBehavior.MaxGainDeg.ToString("F10")),
            ("BaseGain", () => StudyControl.GetInstance().ManipulationBehavior.BaseGain.ToString("F10")),
            ("EdgeGain", () => StudyControl.GetInstance().ManipulationBehavior.EdgeGain.ToString("F10")),
            ("HeadDepthOffset_base", () => StudyControl.GetInstance().ManipulationBehavior.HeadDepthOffset_base.ToString("F10")),
            ("HeadDepthOffset_base_magnitude", () => StudyControl.GetInstance().ManipulationBehavior.HeadDepthOffset_base.magnitude.ToString("F10")),
            ("Attenuation", () => StudyControl.GetInstance().ManipulationBehavior.Attenuation.ToString("F10")),

            ("Time", () => Time.realtimeSinceStartup.ToString("F10")),

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
