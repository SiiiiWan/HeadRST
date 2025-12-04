using UnityEngine;

public class Decorator : ManipulatableObject
{
    public GameObject TouchingFingerTip { get; private set; }
    private Transform _originalParent;

    private Vector3 _grabPositionOffset;
    private Quaternion _grabRotationOffset;
    
    private void Awake()
    {
        // Ensure the BoxCollider on this object is set as a trigger
        // to detect when other colliders enter it without a physical collision.
        GetComponent<BoxCollider>().isTrigger = true;
        _originalParent = transform.parent;
    }


    /// <summary>
    /// This method is called by Unity's physics engine when another collider enters this object's trigger zone.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered has a BoxCollider.
        if (other.gameObject.tag == "PinchBall")
        {
            TouchingFingerTip = other.gameObject;
            OnHoverEnter();
        }

        if (other.gameObject.GetComponent<ManipulatableCube>() != null)
        {
            transform.parent = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited has a BoxCollider.
        if (other.gameObject.tag == "PinchBall")
        {
            TouchingFingerTip = other.gameObject;
            OnHoverExit();
        }

        if (other.gameObject.GetComponent<ManipulatableCube>() != null)
        {
            transform.parent = _originalParent;
        }
    }

    public override void OnPickup()
    {
        base.OnPickup();
        if (TouchingFingerTip != null)
        {
            // Calculate the offset from the finger's transform to this object's transform
            _grabPositionOffset = TouchingFingerTip.transform.InverseTransformPoint(transform.position);
            _grabRotationOffset = Quaternion.Inverse(TouchingFingerTip.transform.rotation) * transform.rotation;
        }
    }

    /// <summary>
    /// Called every frame while the object is picked up.
    /// Updates the object's position and rotation to follow the hand, maintaining the initial grab offset.
    /// </summary>
    public override void ApplyPickedUpBehaviour()
    {
        if (TouchingFingerTip != null)
        {
            // Apply the offset to the current finger transform to get the new object transform
            transform.position = TouchingFingerTip.transform.TransformPoint(_grabPositionOffset);
            transform.rotation = TouchingFingerTip.transform.rotation * _grabRotationOffset;
        }
    }

    public override void OnHoverEnter()
    {
        base.OnHoverEnter();

        GetComponent<Outline>().enabled = true;
    }

    public override void OnHoverExit()
    {
        base.OnHoverExit();
        GetComponent<Outline>().enabled = false;
    }
}
