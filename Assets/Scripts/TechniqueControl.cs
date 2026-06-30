using UnityEngine;

public enum MagicPitchTechnique
{
    [InspectorName("GAZE+PINCH")]
    GazePinch,
    MAGIC,
    MAGICPITCH,
    MAGMODPITCH
}

public class TechniqueControl : MonoBehaviour
{
    [Header("Technique Settings")]
    public Handedness DominantHand = Handedness.right;
    [SerializeField] private MagicPitchTechnique selectedTechnique = MagicPitchTechnique.GazePinch;
    [SerializeField] private ManipulationTechnique manipulationBehavior;

    public MagicPitchTechnique SelectedTechnique => selectedTechnique;

    public ManipulationTechnique ManipulationBehavior
    {
        get
        {
            if (manipulationBehavior == null) ApplySelectedTechnique();
            return manipulationBehavior;
        }
    }

    void Awake()
    {
        ApplySelectedTechnique();
    }

    void OnValidate()
    {
        ApplySelectedTechnique();
    }

    public void SetTechnique(MagicPitchTechnique technique)
    {
        selectedTechnique = technique;
        ApplySelectedTechnique();
    }

    public void ApplySelectedTechnique()
    {
        switch (selectedTechnique)
        {
            case MagicPitchTechnique.GazePinch:
                SwitchToGazePinch();
                break;
            case MagicPitchTechnique.MAGIC:
                SwitchToMAGIC();
                break;
            case MagicPitchTechnique.MAGICPITCH:
                SwitchToMAGICPITCH();
                break;
            case MagicPitchTechnique.MAGMODPITCH:
                SwitchToMAGMODPITCH();
                break;
        }
    }

    [ContextMenu("Use GAZE+PINCH")]
    public void SwitchToGazePinch()
    {
        SwitchTechnique<GazePinch>(MagicPitchTechnique.GazePinch);
    }

    [ContextMenu("Use MAGIC")]
    public void SwitchToMAGIC()
    {
        SwitchTechnique<Magic>(MagicPitchTechnique.MAGIC);
    }

    [ContextMenu("Use MAGICPITCH")]
    public void SwitchToMAGICPITCH()
    {
        SwitchTechnique<MagicPitch>(MagicPitchTechnique.MAGICPITCH);
    }

    [ContextMenu("Use MAGMODPITCH")]
    public void SwitchToMAGMODPITCH()
    {
        SwitchTechnique<MagicModPitch>(MagicPitchTechnique.MAGMODPITCH);
    }

    public Vector3 GetVirtualHandPosition(bool isRightHand)
    {
        if (isRightHand)
        {
            return DominantHand == Handedness.right
                ? ManipulationBehavior.VirtualHandPosition
                : HandData.GetInstance().RightHandPosition;
        }

        return DominantHand == Handedness.left
            ? ManipulationBehavior.VirtualHandPosition
            : HandData.GetInstance().LeftHandPosition;
    }

    private void SwitchTechnique<T>(MagicPitchTechnique technique) where T : ManipulationTechnique
    {
        var techniques = GetComponents<ManipulationTechnique>();
        ManipulationTechnique selected = null;

        foreach (var candidate in techniques)
        {
            if (candidate is T)
            {
                selected = candidate;
                break;
            }
        }

        if (selected == null)
        {
            Debug.LogWarning($"Technique {typeof(T).Name} is not attached to {name}.");
            return;
        }

        foreach (var candidate in techniques)
        {
            candidate.enabled = candidate == selected;
        }

        selectedTechnique = technique;
        manipulationBehavior = selected;
    }
}
