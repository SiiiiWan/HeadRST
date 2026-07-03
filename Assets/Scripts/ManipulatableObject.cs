using UnityEngine;
using System.Collections.Generic;

public enum GrabbedState
{
    NotGrabbed,
    Grabbed
}

public class ManipulatableObject : MonoBehaviour
{
    public bool IsHitbyGaze { get; private set; }
    public float AngleToGaze { get; private set; }
    public GrabbedState GrabbedState { get; private set; } = GrabbedState.NotGrabbed;
    public ManipulationTechnique ManipulationBehavior { get; private set; }
    public List<Transform> Wedges = new List<Transform>();

    void Update()
    {
        TechniqueInputProvider inputProvider = TechniqueInputProvider.GetInstance();
        inputProvider.Refresh();

        Ray gazeRay = inputProvider.Current.GazeRay;
        AngleToGaze = Vector3.Angle(gazeRay.direction, transform.position - gazeRay.origin);
        IsHitbyGaze = AngleToGaze <= 10f || IsHitByGazeRay(gazeRay);
        ManipulationBehavior = StudyControl.GetInstance().ManipulationBehavior;

        SetOutlineVisibility(IsHitbyGaze && GrabbedState == GrabbedState.NotGrabbed);
    }

    private bool IsHitByGazeRay(Ray gazeRay)
    {
        if (Physics.Raycast(gazeRay, out RaycastHit hit, 100f))
        {
            return hit.transform == transform;
        }

        return false;
    }

    public void SetGrabbedState(GrabbedState state)
    {
        GrabbedState = state;
    }

    public void SetOutlineVisibility(bool isVisible)
    {
        if (transform.TryGetComponent<Outline>(out Outline outline))
        {
            outline.enabled = isVisible;
        }
    }

    public void SetActiveWedges(bool isActive)
    {
        foreach (Transform wedge in Wedges)
        {
            if (wedge != null)
            {
                wedge.gameObject.SetActive(isActive);
            }
        }
    }
}
