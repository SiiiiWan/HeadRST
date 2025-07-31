using UnityEngine;
using System.Linq;

namespace Oculus.Interaction.Input.Filter
{
    public class ModifiedHand : Hand
    {
#if !ISDK_OPENXR_HAND
        #region Oculus Library Methods and Constants
        [DllImport("InteractionSdk")]
        private static extern int isdk_DataSource_Create(int id);
        [DllImport("InteractionSdk")]
        private static extern int isdk_DataSource_Destroy(int handle);
        [DllImport("InteractionSdk")]
        private static extern int isdk_DataModifier_Create(int id, int handle);
        [DllImport("InteractionSdk")]
        private static extern int isdk_DataSource_Update(int handle);
        [DllImport("InteractionSdk")]
        private static extern int isdk_DataSource_GetData(int handle, ref HandData data);
        [DllImport("InteractionSdk")]
        private static extern int isdk_ExternalHandSource_SetData(int handle, in HandData data);
        [DllImport("InteractionSdk")]
        private static extern int isdk_DataSource_SetAttributeFloat(int handle, int attrId, float value);

        enum AttributeId
        {
            Unknown = 0,
            WristPosBeta,
            WristPosMinCutOff,
            WristRotBeta,
            WristRotMinCutOff,
            FingerRotBeta,
            FingerRotMinCutOff,
            Frequency,
            WristPosDeltaCutOff,
            WristRotDeltaCutOff,
            FingerRotDeltaCutOff,
        };

        private const int _isdkExternalHandSourceId = 2;
        private const int _isdkOneEuroHandModifierId = 1;
        private const int _isdkSuccess = 0;
        #endregion Oculus Library Methods and Constants
#endif
        #region Tuneable Values
        [Header("Settings", order = -1)]
        [Tooltip("Applies a One Euro Filter when filter parameters are provided")]
        [SerializeField, Optional]
        private HandFilterParameterBlock _filterParameters = null;
        public bool isRightHand = true;
        #endregion Tuneable Values

#if ISDK_OPENXR_HAND
        private readonly IOneEuroFilter<Quaternion> _rootRotFilter = OneEuroFilter.CreateQuaternion();
        private readonly IOneEuroFilter<Vector3> _rootPosFilter = OneEuroFilter.CreateVector3();
        private readonly IOneEuroFilter<Vector3>[] _jointPosFilter = new IOneEuroFilter<Vector3>[Constants.NUM_HAND_JOINTS];
        private readonly IOneEuroFilter<Quaternion>[] _jointRotFilter = new IOneEuroFilter<Quaternion>[Constants.NUM_HAND_JOINTS];
#else
        private int _dataSourceHandle = -1;
        private int _handModifierHandle = -1;
        private const string _logPrefix = "[Oculus.Interaction]";
        private bool _hasFlaggedError = false;
        private HandData _handData = new HandData();
#endif

        protected virtual void Awake()
        {
#if ISDK_OPENXR_HAND
            for (var i = 0; i < Constants.NUM_HAND_JOINTS; i++)
            {
                _jointPosFilter[i] = OneEuroFilter.CreateVector3();
                _jointRotFilter[i] = OneEuroFilter.CreateQuaternion();

            }
#else
            _handData.Init();
            _dataSourceHandle = isdk_DataSource_Create(_isdkExternalHandSourceId);
            this.AssertIsTrue(_dataSourceHandle >= 0, $"{_logPrefix} Unable to allocate external hand data source!");

            _handModifierHandle = isdk_DataModifier_Create(_isdkOneEuroHandModifierId, _dataSourceHandle);
            this.AssertIsTrue(_handModifierHandle >= 0, $"{_logPrefix} Unable to allocate one euro hand data modifier!");
#endif
        }


#if !ISDK_OPENXR_HAND
        protected virtual void OnDestroy()
        {
            int result = -1;

            //Release the filter and source
            result = isdk_DataSource_Destroy(_handModifierHandle);
            this.AssertIsTrue(_isdkSuccess == result, $"{nameof(_handModifierHandle)} destroy was unsuccessful. ");
            result = isdk_DataSource_Destroy(_dataSourceHandle);
            this.AssertIsTrue(_isdkSuccess == result, $"{nameof(_dataSourceHandle)} destroy was unsuccessful. ");
        }
#endif

        protected override void Apply(HandDataAsset handDataAsset)
        {
            base.Apply(handDataAsset);

            if (!handDataAsset.IsTracked)
            {
                return;
            }

            if (UpdateHandData(handDataAsset))
            {
                return;
            }
#if !ISDK_OPENXR_HAND

            if (_hasFlaggedError)
                return;

            _hasFlaggedError = true;
            Debug.LogError("Unable to send value to filter, InteractionSDK plugin may be missing or corrupted");
#endif
        }

        private ShadowHand _shadowHand = new ShadowHand();

        protected bool UpdateHandData(HandDataAsset handDataAsset)
        {
#if ISDK_OPENXR_HAND
            var pose = handDataAsset.Root;


            _shadowHand.FromJoints(handDataAsset.JointPoses.ToList(), false);

            handDataAsset.Root = new Pose(StudyControl.GetInstance().GetVirtualHandPosition(isRightHand), pose.rotation);

            handDataAsset.JointPoses = _shadowHand.GetWorldPoses();


            // Legacy local rotations
            for (int i = 0; i < Constants.NUM_HAND_JOINTS; i++)
            {
                int parent = (int)HandJointUtils.JointParentList[i];
#pragma warning disable 0618
                handDataAsset.Joints[i] = parent < 0 ? Quaternion.identity :
                    Quaternion.Inverse(handDataAsset.JointPoses[parent].rotation) *
                    handDataAsset.JointPoses[i].rotation;
#pragma warning restore 0618
            }
#else
            // pipe data asset into temp struct
            _handData.SetData(handDataAsset.Joints, handDataAsset.Root);

            // Send it
            int result = isdk_ExternalHandSource_SetData(_dataSourceHandle, _handData);
            if (result != _isdkSuccess)
            {
                return false;
            }

            // Update
            result = isdk_DataSource_Update(_handModifierHandle);
            if (result != _isdkSuccess)
            {
                return false;
            }

            // Get result
            result = isdk_DataSource_GetData(_handModifierHandle, ref _handData);
            if (result != _isdkSuccess)
            {
                return false;
            }

            // Copy results into our hand data asset
            _handData.GetData(ref handDataAsset.Joints, out handDataAsset.Root);
#endif
            return true;
        }
    }
}