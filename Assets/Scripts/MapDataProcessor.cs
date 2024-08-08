using UnityEngine;
using System.Collections.Generic;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Mono.Data.Sqlite;
using System.Collections;

public class MapDataProcessor : MonoBehaviour
{
    private SqliteConnection _connection;
    public string databasePath = "/seoul.sqlite";
    public string tableName = "idx_al_d010_11_20240706_GEOMETRY";

    // �ε��� �������� �ִ� ����
    public int batchSize = 1000;

    private float minX = float.MaxValue;
    private float maxX = float.MinValue;
    private float minY = float.MaxValue;
    private float maxY = float.MinValue;
    private float centerX;
    private float centerY;

    public Material material;

    void Start()
    {
        // SQLite �����ͺ��̽� ����
        string dbPath = "URI=file:" + Application.dataPath + databasePath;
        _connection = new SqliteConnection(dbPath);
        _connection.Open(); // SQLite ���� ����

        // ������ �б� �� ó��
        StartCoroutine(ProcessData(0, batchSize));
    }

    IEnumerator ProcessData(int offset, int batchSize)
    {
        // ù ��° �н��� ���� �ּ�/�ִ� ��ǥ�� ����մϴ�.
        while (true)
        {
            List<MyDataClass> coordinates = ReadData(offset, batchSize);
            if (coordinates.Count == 0) break;

            foreach (var coord in coordinates)
            {
                minX = Mathf.Min(minX, coord.xmin, coord.xmax);
                maxX = Mathf.Max(maxX, coord.xmin, coord.xmax);
                minY = Mathf.Min(minY, coord.ymin, coord.ymax);
                maxY = Mathf.Max(maxY, coord.ymin, coord.ymax);
            }

            offset += batchSize;
            yield return null; // �������� �纸�Ͽ� UI�� ������ �ʵ��� ��
        }

        // �߽����� ����մϴ�.
        centerX = (minX + maxX) / 2;
        centerY = (minY + maxY) / 2;

        // �� ��° �н��� ���� �����͸� ó���մϴ�.
        offset = 0;
        while (true)
        {
            List<MyDataClass> coordinates = ReadData(offset, batchSize);
            if (coordinates.Count == 0) break;

            // ���� ��ġ�� ����Ͽ� �޽� ����
            CreateMesh(coordinates, offset / batchSize);

            offset += batchSize;
            yield return null;
        }
    }

    List<MyDataClass> ReadData(int offset, int limit)
    {
        string query = $"SELECT xmin, ymin, xmax, ymax FROM {tableName} LIMIT {limit} OFFSET {offset}";
        SqliteCommand command = new SqliteCommand(query, _connection);
        SqliteDataReader reader = command.ExecuteReader();

        List<MyDataClass> result = new List<MyDataClass>();

        while (reader.Read())
        {
            MyDataClass data = new MyDataClass
            {
                xmin = reader.GetFloat(0),
                ymin = reader.GetFloat(1),
                xmax = reader.GetFloat(2),
                ymax = reader.GetFloat(3)
            };

            result.Add(data);
        }

        reader.Close();
        return result;
    }

    void CreateMesh(List<MyDataClass> coordinates, int batchIndex)
    {
        GameObject mapBatch = new GameObject($"MapBatch_{batchIndex}", typeof(MeshFilter), typeof(MeshRenderer));
        MeshFilter meshFilter = mapBatch.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = mapBatch.GetComponent<MeshRenderer>();

        meshRenderer.material = material;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float scale = 10f; // Ȯ�� ������ ����

        foreach (var coord in coordinates)
        {
            Vector2 convertedMin = new Vector2(coord.xmin, coord.ymin) * 1000;
            Vector2 convertedMax = new Vector2(coord.xmax, coord.ymax) * 1000;

            // �߽��� �������� ��ǥ �̵�
            convertedMin -= new Vector2(centerX, centerY) * 1000;
            convertedMax -= new Vector2(centerX, centerY) * 1000;

            Vector2 normalizedMin = NormalizeCoordinates(convertedMin);
            Vector2 normalizedMax = NormalizeCoordinates(convertedMax);

            normalizedMin *= scale; // �����ϸ� ����
            normalizedMax *= scale; // �����ϸ� ����

            int vertexIndex = vertices.Count;

            vertices.Add(new Vector3(normalizedMin.x, normalizedMin.y, 0));
            vertices.Add(new Vector3(normalizedMin.x, normalizedMax.y, 0));
            vertices.Add(new Vector3(normalizedMax.x, normalizedMax.y, 0));
            vertices.Add(new Vector3(normalizedMax.x, normalizedMin.y, 0));

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }


    Vector2 NormalizeCoordinates(Vector2 coordinates)
    {
        float normalizedX = (coordinates.x - minX * 1000) / ((maxX - minX) * 1000);
        float normalizedY = (coordinates.y - minY * 1000) / ((maxY - minY) * 1000);

        float scale = 10f; // Ȯ�� ������ ���� (1000 -> 10000)
        return new Vector2(normalizedX * scale, normalizedY * scale);
    }


    public class MyDataClass
    {
        public float xmin { get; set; }
        public float ymin { get; set; }
        public float xmax { get; set; }
        public float ymax { get; set; }
    }

    void OnDestroy()
    {
        if (_connection != null)
        {
            _connection.Close();
        }
    }
}
