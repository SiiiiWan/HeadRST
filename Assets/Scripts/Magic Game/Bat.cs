using UnityEngine;
using System.Collections;
public class Bat : MonoBehaviour
{
    public Animator BatAnimator;
    public GameObject VanishingEffect;
    float speed = 0.5f; // Speed at which the bat moves towards the camera
    private bool isDead = false;

    void Update()
    {
        // Do nothing if the bat is dead or the camera isn't found
        if (isDead)
        {
            return;
        }

        // Make the bat look at the camera's position
        transform.LookAt(Camera.main.transform.position);

        // Move the bat forward (in the direction it is looking)
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent the bat from being triggered multiple times
        if (isDead) return;

        if (other is BoxCollider && other.gameObject.tag == "Fireball")
        {
            isDead = true;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            // Instantiate the vanishing effect at the bat's current position and rotation
            if (VanishingEffect != null)
            {
                // Get a reference to the created effect instance
                GameObject effectInstance = Instantiate(VanishingEffect, hitPoint, Quaternion.identity);
                // Schedule the destruction of the effect after 0.05 seconds
                Destroy(effectInstance, 0.1f);
            }

            // Instantly destroy the bat GameObject
            Destroy(gameObject);
        }
    }


}


