using UnityEngine;

public class Fireball : MonoBehaviour
{
    public PositionRotationProvider PositionRotationProvider;
    private Vector3 _lastPosition;

    void Start()
    {
        // Initialize last position to the starting position
        _lastPosition = transform.position;
    }
    
    void Update()
    {
        if(PositionRotationProvider != null)
        {
            Vector3 newPosition = PositionRotationProvider.GetPositionOutput(transform.position);
            
            // Calculate the direction of movement for this frame
            Vector3 movementDirection = newPosition - _lastPosition;

            // Update the position
            transform.position = newPosition;

            // Check if there was any significant movement to avoid errors with zero vectors
            if (movementDirection.sqrMagnitude > 0.0001f)
            {
                // Set the rotation so that the forward vector points opposite to the movement direction.
                // This makes the backward vector align with the movement direction.
                transform.rotation = Quaternion.LookRotation(-movementDirection);
            }

            // Update the last position for the next frame
            _lastPosition = newPosition;
        }
    }

}
