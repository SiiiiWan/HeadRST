using UnityEngine;

public class Monster : MonoBehaviour
{
    private Animator _monsterAnimator;
    public GameObject VanishingEffect;
    public float Speed = 0.5f;
    private bool isDead = false;

    void Awake()
    {
        _monsterAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // Do nothing if the monster is dead or the camera isn't found
        if (isDead)
        {
            return;
        }

        Move();

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

            _monsterAnimator.SetTrigger("TrDie");
            
            // Destroy the monster GameObject after the death animation has finished
            Destroy(gameObject, 1.0f);
        }
    }

    public virtual void Move()
    {

    }
}
