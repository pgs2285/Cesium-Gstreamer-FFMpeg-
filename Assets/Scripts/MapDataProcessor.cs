using UnityEngine;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Collections;
using System.Threading.Tasks;

public class MapDataProcessor : MonoBehaviour
{
    private SqliteConnection _connection;
    public string databasePath = "/seoul.sqlite";
    public string tableName = "idx_al_d010_11_20240706_GEOMETRY";

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
        string dbPath = "URI=file:" + Application.dataPath + databasePath;
        _connection = new SqliteConnection(dbPath);
        _connection.Open();

        // 데이터 읽기 및 처리
        StartCoroutine(ProcessData(0, batchSize));
    }

    IEnumerator ProcessData(int offset, int batchSize)
    {
        // 첫 번째 패스를 통해 최소/최대 좌표를 계산합니다.
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
            yield return null;
        }

        // 중심점을 계산합니다.
        centerX = (minX + maxX) / 2;
        centerY = (minY + maxY) / 2;

        // 두 번째 패스를 통해 데이터를 처리합니다.
        offset = 0;
        while (true)
        {
            List<MyDataClass> coordinates = ReadData(offset, batchSize);
            if (coordinates.Count == 0) break;

            // Task를 통해 병렬 작업 수행
            Task<MeshData> meshDataTask = Task.Run(() => PrepareMeshData(coordinates));

            // 완료될 때까지 기다림
            while (!meshDataTask.IsCompleted)
            {
                yield return null; // 프레임을 양보하여 UI가 멈추지 않도록 함
            }

            // 메인 스레드에서 메쉬 생성
            CreateMesh(meshDataTask.Result, offset / batchSize);

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

    MeshData PrepareMeshData(List<MyDataClass> coordinates)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float scale = 10f;

        foreach (var coord in coordinates)
        {
            Vector2 convertedMin = new Vector2(coord.xmin, coord.ymin) * 1000;
            Vector2 convertedMax = new Vector2(coord.xmax, coord.ymax) * 1000;

            convertedMin -= new Vector2(centerX, centerY) * 1000;
            convertedMax -= new Vector2(centerX, centerY) * 1000;

            Vector2 normalizedMin = NormalizeCoordinates(convertedMin);
            Vector2 normalizedMax = NormalizeCoordinates(convertedMax);

            normalizedMin *= scale;
            normalizedMax *= scale;

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

        return new MeshData
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
    }

    void CreateMesh(MeshData meshData, int batchIndex)
    {
        GameObject mapBatch = new GameObject($"MapBatch_{batchIndex}", typeof(MeshFilter), typeof(MeshRenderer));
        MeshFilter meshFilter = mapBatch.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = mapBatch.GetComponent<MeshRenderer>();

        
        meshRenderer.material = material;

        Mesh mesh = new Mesh();
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        MeshCollider meshCollider = mapBatch.AddComponent<MeshCollider>();
        meshCollider.convex = false;
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    Vector2 NormalizeCoordinates(Vector2 coordinates)
    {
        float normalizedX = (coordinates.x - minX * 1000) / ((maxX - minX) * 1000);
        float normalizedY = (coordinates.y - minY * 1000) / ((maxY - minY) * 1000);

        float scale = 10f;
        return new Vector2(normalizedX * scale, normalizedY * scale);
    }

    public class MyDataClass
    {
        public float xmin { get; set; }
        public float ymin { get; set; }
        public float xmax { get; set; }
        public float ymax { get; set; }
    }

    public class MeshData
    {
        public Vector3[] vertices { get; set; }
        public int[] triangles { get; set; }
    }

    void OnDestroy()
    {
        if (_connection != null)
        {
            _connection.Close();
        }
    }
}
