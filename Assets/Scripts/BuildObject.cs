using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DelaunatorSharp;

public class BuildObject : MonoBehaviour
{
    private PlayerController playerController;
    private BuildingSystem buildingSystem;
    public bool isPlaced {  get; private set; }
    public Vector3Int size { get; private set; }
    private Vector3[] vertices;

    [SerializeField] BoxCollider col;

    [SerializeField] BuildObjectConnector leftConnector;
    [SerializeField] BuildObjectConnector rightConnector;

    public List<BuildObject> group;
    public BuildObject groupHolder;
    public GameObject groupBound;
    public bool isConnected = false;

    private void Awake()
    {
        playerController = PlayerController.instance;
        buildingSystem = BuildingSystem.instance;

        GetColliderVertexPositionLocal();
        CalculateSizeInCells();
    }

    private void GetColliderVertexPositionLocal()
    {
        vertices = new Vector3[4];
        vertices[0] = col.center + new Vector3(-col.size.x, -col.size.y, -col.size.z) * 0.5f;
        vertices[1] = col.center + new Vector3(col.size.x, -col.size.y, -col.size.z) * 0.5f;
        vertices[2] = col.center + new Vector3(col.size.x, -col.size.y, col.size.z) * 0.5f;
        vertices[3] = col.center + new Vector3(-col.size.x, -col.size.y, col.size.z) * 0.5f;
    }

    private void CalculateSizeInCells()
    {
        Vector3Int[] vertices = new Vector3Int[this.vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            vertices[i] = buildingSystem.gridLayout.WorldToCell(worldPos);
        }

        size = new Vector3Int(Mathf.Abs((vertices[0] - vertices[1]).x), Mathf.Abs((vertices[0] - vertices[3]).y), 1);
    }

    public Vector3 GetStartPosition()
    {
        return transform.TransformPoint(vertices[0]);
    }

    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 90f, 0));
        size = new Vector3Int(size.y, size.x, 1);

        Vector3[] vertices = new Vector3[this.vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = this.vertices[(i + 1) % this.vertices.Length];
        }

        this.vertices = vertices;
    }

    public virtual void Place()
    {
        Destroy(gameObject.GetComponent<BuildObjectDrag>());

        isPlaced = true;

        col.isTrigger = false;

        leftConnector.col.isTrigger = false;
        rightConnector.col.isTrigger = false;
        Rigidbody l = leftConnector.AddComponent<Rigidbody>();
        Rigidbody r = rightConnector.AddComponent<Rigidbody>();
        l.isKinematic = true;
        l.useGravity = false;
        l.freezeRotation = true;
        r.isKinematic = true;
        r.useGravity = false;
        r.freezeRotation = true;

        group = new List<BuildObject>();
        leftConnector.CheckConnection(this, group);
    }

    public void CreateGroupBounds()
    {
        IPoint[] pointsDouble = new IPoint[group.Count];
        Vector3[] pointsVector = new Vector3[group.Count];

        for (int i = 0; i < group.Count; i++)
        {
            Vector3 pos = buildingSystem.SnapCoordinateToGrid(group[i].transform.position);
            pointsDouble[i] = new Point(pos.x, pos.z);
            pointsVector[i] = new Vector3((float)pointsDouble[i].X, 0, (float)pointsDouble[i].Y);
        }

        Delaunator triangleDelaunay = new Delaunator(pointsDouble);

        int[] triangles = triangleDelaunay.Triangles;

        Mesh mesh = new Mesh();
        mesh.vertices = pointsVector;
        mesh.triangles = triangles;

        groupBound = new GameObject();
        groupBound.transform.position = buildingSystem.SnapCoordinateToGrid(transform.position);
        groupBound.transform.SetParent(transform, true);

        Vector3 offset = Vector3.zero;
        if (transform.rotation.eulerAngles.y == 0)
        {
            Debug.Log("000");
            offset = new Vector3(transform.position.x, 0, transform.position.z);
        }
        else if (transform.rotation.eulerAngles.y == 90)
        {
            Debug.Log("090");
            offset = new Vector3(-transform.position.z, 0, transform.position.x);
        }
        else if (transform.rotation.eulerAngles.y == 180)
        {
            Debug.Log("180");
            offset = new Vector3(-transform.position.x, 0, -transform.position.z);
        }
        else if (transform.rotation.eulerAngles.y == 270)
        {
            Debug.Log("270");
            offset = new Vector3(transform.position.x, 0, -transform.position.z);
        }
        groupBound.transform.localPosition -= offset;

        MeshFilter meshFilter = groupBound.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        MeshCollider collider = groupBound.AddComponent<MeshCollider>();
        collider.convex = true;
        collider.isTrigger = true;
        collider.sharedMesh = mesh;

    }
}
