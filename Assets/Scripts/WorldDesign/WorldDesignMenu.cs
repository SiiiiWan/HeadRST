using UnityEngine;
using System.Collections.Generic;

public class WorldDesignMenu : MonoBehaviour
{
    public Transform MenuAnchor;
    public string prefabFolderPath = "Prefabs";
    public float menuRadius = 0.15f;

    [Tooltip("The scale to apply to the instantiated prefabs.")]
    public float itemScale = 1f;

    private List<GameObject> _menuItems = new List<GameObject>();

    void Start()
    {
        if (MenuAnchor == null)
        {
            Debug.LogError("MenuAnchor is not assigned in the WorldDesignMenu script.");
            return;
        }

        GenerateMenuItems();
    }

    void Update()
    {
        transform.position = MenuAnchor.position;
        transform.rotation = MenuAnchor.rotation;
    }

    /// <summary>
    /// Loads prefabs, instantiates them, and arranges them in a circle around the hand.
    /// </summary>
    public void GenerateMenuItems()
    {
        // Clear any existing menu items
        foreach (var item in _menuItems)
        {
            Destroy(item);
        }
        _menuItems.Clear();

        // Load all GameObjects from the specified folder within any Resources folder.
        var prefabs = Resources.LoadAll<GameObject>(prefabFolderPath);

        if (prefabs.Length == 0)
        {
            Debug.LogWarning($"No prefabs found in 'Resources/{prefabFolderPath}'.");
            return;
        }

        // Arrange the items in a circle
        float angleStep = 360f / prefabs.Length;
        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = prefabs[i];

            // Calculate the position in a circle around the hand
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 localPosition = new Vector3(Mathf.Cos(angle) * menuRadius, 0, Mathf.Sin(angle) * menuRadius);

            // Instantiate the prefab
            GameObject newItem = Instantiate(prefab, MenuAnchor);

            // Set local position and scale
            newItem.transform.localPosition = localPosition;
            newItem.transform.localRotation = Quaternion.identity;
            newItem.transform.localScale = Vector3.one * itemScale;

            newItem.transform.parent = transform;

            // It's good practice to remove or disable components that might interfere
            // with the menu's behavior, like Rigidbody physics.
            if (newItem.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            _menuItems.Add(newItem);
        }
    }
}