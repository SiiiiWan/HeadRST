using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TrashBin : MonoBehaviour
{
    public Transform TrashLidTransform;
    private void Awake()
    {
        // Ensure the BoxCollider on this object is set as a trigger
        // to detect when other colliders enter it without a physical collision.
        GetComponent<BoxCollider>().isTrigger = true;
    }

    /// <summary>
    /// This method is called by Unity's physics engine when another collider enters this object's trigger zone.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered has a BoxCollider.
        if (other is BoxCollider && other.gameObject.GetComponent<ManipulatableCube>() != null)
        {
            print("Trash Bin Lid Open");
            TrashLidOpenAnimation();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Continuously check if the object inside has been dropped.
        ManipulatableCube cube = other.gameObject.GetComponent<ManipulatableCube>();
        if (cube != null && !cube.IsPickedUp)
        {
            // If the cube is not picked up, destroy it (simulate trashing).
            Destroy(cube.gameObject);
            TrashLidCloseAnimation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited has a BoxCollider.
        if (other is BoxCollider && other.gameObject.GetComponent<ManipulatableCube>() != null)
        {
            TrashLidCloseAnimation();
        }
    }

    private void TrashLidOpenAnimation()
    {
        TrashLidTransform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
    }

    private void TrashLidCloseAnimation()
    {
        print("Trash Bin Lid Close");
        TrashLidTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
