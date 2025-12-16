using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Outline))]
[RequireComponent(typeof(Rigidbody))]

public class TargetCube : MonoBehaviour
{
    public CubeColor CubeColor;
    public Material SolidColor, TransparentColor;
    private Coroutine _moveCoroutine;
    private float _moveDuration = 0.5f;

    private void Awake()
    {
        // Ensure the BoxCollider on this object is set as a trigger
        // to detect when other colliders enter it without a physical collision.
        GetComponent<BoxCollider>().isTrigger = true;
        GetComponent<Outline>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered has a BoxCollider.
        if (other is BoxCollider && other.gameObject.GetComponent<ManipulatableCube>() != null)
        {
            GetComponent<Outline>().enabled = true;
            SetTransparency(0.5f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        ManipulatableCube cube = other.gameObject.GetComponent<ManipulatableCube>();
        if (cube != null && !cube.IsPickedUp)
        {
            // If the cube is not picked up, destroy it (simulate trashing).
            if(CubeColor == cube.CubeColor && _moveCoroutine == null)
            {
                OnTriggerExit(other);

                Destroy(cube.gameObject);  
                AudioPlay.PlayClickSound();
                
                float newX = Random.Range(-2f, 2f);
                float newY = Random.Range(0.5f, 2f);
                float newZ = Random.Range(1f, 10f);
                _moveCoroutine = StartCoroutine(MoveToPosition(new Vector3(newX, newY, newZ)));
            } 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited has a BoxCollider.
        if (other is BoxCollider && other.gameObject.GetComponent<ManipulatableCube>() != null)
        {
            GetComponent<Outline>().enabled = false;
            SetTransparency(1.0f);
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float elapsedTime = 0;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < _moveDuration)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / _moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the final position is exactly the target position
        transform.position = targetPosition;
        _moveCoroutine = null;
    }

    public void SetTransparency(float alpha)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (alpha < 1.0f)
            {
                renderer.material = TransparentColor;
            }
            else
            {
                renderer.material = SolidColor;
            }

            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;

            // Debug.Log("TargetCube Renderer: set transparency to " + alpha);
        }
        else
        {
            // Debug.Log("TargetCube Renderer: component not found.");
        }
    }

}
