using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public enum GrabbedState
{
    NotGrabbed,
    Grabbed_Indirect,
    Grabbed_Direct
}

public class ManipulatableObject : MonoBehaviour
{
    public bool IsInGazeCone { get; private set; }
    public float AngleToGaze { get; private set; }

    public bool IsPickedUp { get; protected set; }
    public bool UseGravity;
    public bool IsObjectFrozen { get; private set; }

    public GrabbedState GrabbedState { get; protected set; }
    public Grabbable Grabbable;
    public HandGrabInteractable HandGrabInteractable;

    // Update is called once per frame
    void Update()
    {

        if (IsPickedUp)
        {
            UpdatePosition(CubeManager.GetInstance().CubeStackingCursor.transform.position);

            if (PinchDetector.GetInstance().IsNoHandPinching)
            {
                IsPickedUp = false;
                SetCancelObjectGravity(false);
            }
            return;
        }

        AngleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        IsInGazeCone = AngleToGaze <= 10f || EyeGaze.GetInstance().GetGazeHitTrans() == transform;

        SpecialBehaviour();
    }

    public virtual void SpecialBehaviour()
    {

    }

    public void UpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void SetGrabbedState(GrabbedState state)
    {
        GrabbedState = state;
        SpecialBehaviour();
    }

    public void SetCancelObjectGravity(bool isFreeze)
    {
        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();
        Collider collider = transform.GetComponent<Collider>();
        if (rigidbody != null && collider != null)
        {
            if (isFreeze || UseGravity == false)
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                // rigidbody.linearVelocity = Vector3.zero;
                // rigidbody.angularVelocity = Vector3.zero;
                collider.enabled = false;
            }
            else
            {
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                collider.enabled = true;
                // rigidbody.linearVelocity = Vector3.zero; // or another initial value
            }
        }
    }
}
