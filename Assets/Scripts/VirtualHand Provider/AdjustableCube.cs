using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdjustableCube : MonoBehaviour
{
    public GameObject CornerPrefab;
    public float InitialSize = 1.0f;
    public Material EdgeMaterial;

    private Transform[] _corners = new Transform[8];
    private Mesh _mesh;
    private Vector3[] _vertices = new Vector3[8];
    private LineRenderer _edgeLines;

    void Start()
    {
        // Initialize the mesh
        _mesh = new Mesh();
        _mesh.name = "AdjustableCubeMesh";
        GetComponent<MeshFilter>().mesh = _mesh;
        GetComponent<MeshCollider>().sharedMesh = _mesh;

        // Initialize the LineRenderer for edges
        _edgeLines = new GameObject("EdgeLines").AddComponent<LineRenderer>();
        _edgeLines.transform.SetParent(transform, false);
        _edgeLines.useWorldSpace = false;
        _edgeLines.positionCount = 24;
        _edgeLines.startWidth = 0.01f;
        _edgeLines.endWidth = 0.01f;
        _edgeLines.material = EdgeMaterial != null ? EdgeMaterial : new Material(Shader.Find("Sprites/Default"));


        if (CornerPrefab == null)
        {
            Debug.LogError("CornerPrefab is not assigned in the AdjustableCube script.");
            return;
        }

        // Create the 8 corner handles
        for (int i = 0; i < 8; i++)
        {
            _corners[i] = Instantiate(CornerPrefab, transform).transform;
            _corners[i].name = $"Corner_{i}";
        }

        // Set initial positions of the corners to form a cube
        SetInitialCornerPositions();
        UpdateCube();
    }

    void Update()
    {
        // Continuously update the cube's mesh based on corner positions
        UpdateCube();
    }

    private void SetInitialCornerPositions()
    {
        float halfSize = InitialSize / 2.0f;
        _corners[0].localPosition = new Vector3(-halfSize, -halfSize, -halfSize); // Bottom-Left-Back
        _corners[1].localPosition = new Vector3( halfSize, -halfSize, -halfSize); // Bottom-Right-Back
        _corners[2].localPosition = new Vector3(-halfSize,  halfSize, -halfSize); // Top-Left-Back
        _corners[3].localPosition = new Vector3( halfSize,  halfSize, -halfSize); // Top-Right-Back
        _corners[4].localPosition = new Vector3(-halfSize, -halfSize,  halfSize); // Bottom-Left-Front
        _corners[5].localPosition = new Vector3( halfSize, -halfSize,  halfSize); // Bottom-Right-Front
        _corners[6].localPosition = new Vector3(-halfSize,  halfSize,  halfSize); // Top-Left-Front
        _corners[7].localPosition = new Vector3( halfSize,  halfSize,  halfSize); // Top-Right-Front
    }

    private void UpdateCube()
    {
        // Update vertices based on corner positions
        for (int i = 0; i < 8; i++)
        {
            _vertices[i] = _corners[i].localPosition;
        }

        _mesh.Clear();
        _mesh.vertices = _vertices;

        // Define the 12 triangles (2 for each of the 6 faces)
        _mesh.triangles = new int[]
        {
            // Back face
            0, 2, 3, 0, 3, 1,
            // Front face
            4, 7, 6, 4, 5, 7,
            // Left face
            0, 6, 2, 0, 4, 6,
            // Right face
            1, 3, 7, 1, 7, 5,
            // Top face
            2, 7, 3, 2, 6, 7,
            // Bottom face
            0, 1, 5, 0, 5, 4
        };

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        UpdateEdgeLines();
    }

    private void UpdateEdgeLines()
    {
        if (_edgeLines == null) return;

        Vector3[] points = new Vector3[24];
        // Bottom face
        points[0] = _corners[0].localPosition; points[1] = _corners[1].localPosition;
        points[2] = _corners[1].localPosition; points[3] = _corners[5].localPosition;
        points[4] = _corners[5].localPosition; points[5] = _corners[4].localPosition;
        points[6] = _corners[4].localPosition; points[7] = _corners[0].localPosition;

        // Top face
        points[8] = _corners[2].localPosition; points[9] = _corners[3].localPosition;
        points[10] = _corners[3].localPosition; points[11] = _corners[7].localPosition;
        points[12] = _corners[7].localPosition; points[13] = _corners[6].localPosition;
        points[14] = _corners[6].localPosition; points[15] = _corners[2].localPosition;

        // Vertical edges
        points[16] = _corners[0].localPosition; points[17] = _corners[2].localPosition;
        points[18] = _corners[1].localPosition; points[19] = _corners[3].localPosition;
        points[20] = _corners[4].localPosition; points[21] = _corners[6].localPosition;
        points[22] = _corners[5].localPosition; points[23] = _corners[7].localPosition;

        _edgeLines.SetPositions(points);
    }
}
