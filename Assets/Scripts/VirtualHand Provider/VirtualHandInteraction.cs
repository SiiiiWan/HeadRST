using UnityEngine;

public class VirtualHandInteraction : MonoBehaviour
{
    public GameObject SelectionCubePrefab;

    private GameObject _selectionCubeInstance;
    private Vector3 _pinchStartPosition;

    void Update()
    {
        Vector3 HandToPinchOffset = HandData.GetInstance().GetHandPosition(usePinchTip: true) - HandData.GetInstance().GetHandPosition(usePinchTip: false);

        if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame)
        {
            if (SelectionCubePrefab != null)
            {
                // Store the starting position of the pinch
                _pinchStartPosition = Settings.GetInstance().GetVirtualHandPosition(isRightHand: true) + HandToPinchOffset;

                // Instantiate the prefab
                _selectionCubeInstance = Instantiate(SelectionCubePrefab);
                _selectionCubeInstance.name = "SelectionCubeInstance";
            }
            else
            {
                Debug.LogWarning("SelectionCubePrefab is not assigned in the VirtualHandInteraction component.");
            }
        }

        // While pinching
        if (PinchDetector.GetInstance().IsOneHandPinching)
        {
            if (_selectionCubeInstance != null)
            {
                // Get the current position of the pinch
                Vector3 currentPinchPosition = Settings.GetInstance().GetVirtualHandPosition(isRightHand: true) + HandToPinchOffset;

                // The center of the cube is the midpoint between the start and current positions
                Vector3 center = (_pinchStartPosition + currentPinchPosition) / 2f;

                // The size of the cube is the absolute difference between the positions
                Vector3 size = new Vector3(
                    Mathf.Abs(_pinchStartPosition.x - currentPinchPosition.x),
                    Mathf.Abs(_pinchStartPosition.y - currentPinchPosition.y),
                    Mathf.Abs(_pinchStartPosition.z - currentPinchPosition.z)
                );

                // Update the cube's position and scale
                _selectionCubeInstance.transform.position = center;
                _selectionCubeInstance.transform.localScale = size;
            }
        }

        if (PinchDetector.GetInstance().IsNoHandPinching && _selectionCubeInstance != null)
        {
            Destroy(_selectionCubeInstance);
            _selectionCubeInstance = null;
            
        }
    }
    
}
