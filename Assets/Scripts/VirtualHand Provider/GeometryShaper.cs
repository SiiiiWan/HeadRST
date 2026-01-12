using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GeometryShaper : MonoBehaviour
{
    public GameObject CubePrefab;
    private Vector3 _pinchStartPosition;

    void Update()
    {
        Vector3 HandToPinchOffset = HandData.GetInstance().GetHandPosition(usePinchTip: true) - HandData.GetInstance().GetHandPosition(usePinchTip: false);

        if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame && ObjectManager.GetInstance().PickedUpObject == null)
        {
            if (CubePrefab != null)
            {
                // Store the starting position of the pinch
                _pinchStartPosition = Settings.GetInstance().GetVirtualHandPose(isRightHand: true).position + HandToPinchOffset;

                // Instantiate the prefab
                GameObject cubeInstance = Instantiate(CubePrefab);
                cubeInstance.name = "SelectionCubeInstance";
                cubeInstance.transform.parent = transform;
            }
            else
            {
                Debug.LogWarning("SelectionCubePrefab is not assigned in the VirtualHandInteraction component.");
            }
        }

        // While pinching
        // if (PinchDetector.GetInstance().IsOneHandPinching)
        // {
        //     if (cubeInstance != null)
        //     {
        //         // Get the current position of the pinch
        //         Vector3 currentPinchPosition = Settings.GetInstance().GetVirtualHandPosition(isRightHand: true) + HandToPinchOffset;

        //         // The center of the cube is the midpoint between the start and current positions
        //         Vector3 center = (_pinchStartPosition + currentPinchPosition) / 2f;

        //         // The size of the cube is the absolute difference between the positions
        //         Vector3 size = new Vector3(
        //             Mathf.Abs(_pinchStartPosition.x - currentPinchPosition.x),
        //             Mathf.Abs(_pinchStartPosition.y - currentPinchPosition.y),
        //             Mathf.Abs(_pinchStartPosition.z - currentPinchPosition.z)
        //         );

        //         // Update the cube's position and scale
        //         CubeInstance.transform.position = center;
        //         CubeInstance.transform.localScale = size;

        //     }
        // }
    }


}
