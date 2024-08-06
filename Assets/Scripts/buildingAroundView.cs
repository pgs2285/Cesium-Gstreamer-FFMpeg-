using UnityEngine;
using UnityEngine.UI;

public class buildingAroundView : MonoBehaviour
{
    public GameObject rotatingCameraPrefab; // ȸ�� ī�޶� ������
    public Vector3 offset = new Vector3(0, 5, -10); // ī�޶��� ������
    public RawImage cameraOutput; // ȸ�� ī�޶��� �並 ǥ���� UI RawImage
    public float rotationSpeed = 10f; // ȸ�� �ӵ�

    private Camera rotatingCamera;
    private RenderTexture renderTexture;
    private GameObject rotatingCameraObject;
    private Vector3 target;
    private bool isStart = false;
    private GameObject viewPanel;
    public Canvas canvas;

    void Update()
    {
        // ���콺 Ŭ���� ����
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Ŭ���� ������Ʈ�� �ִٸ� startViewing ȣ��
                startViewing(hit.transform.position);
            }
        }

        // ī�޶� ȸ�� ����
        if (isStart)
        {
            rotatingCameraObject.transform.RotateAround(target, Vector3.up, rotationSpeed * Time.deltaTime);
            rotatingCameraObject.transform.LookAt(target);
        }
    }

    public void startViewing(Vector3 newTarget)
    {
        // ȸ�� ī�޶� �ν��Ͻ�ȭ �� ����
        if (!isStart)
        {
            viewPanel = canvas.transform.Find("ViewPanel").gameObject;
            viewPanel.SetActive(true);

            rotatingCameraObject = Instantiate(rotatingCameraPrefab);
            rotatingCamera = rotatingCameraObject.GetComponent<Camera>();

            // RenderTexture ���� �� ����
            renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
            rotatingCamera.targetTexture = renderTexture;
            cameraOutput.texture = renderTexture;

            isStart = true;
        }

        // ���ο� Ÿ�� ����
        target = newTarget;

        // �ʱ� ī�޶� ��ġ ����
        rotatingCameraObject.transform.position = target + offset;
        rotatingCameraObject.transform.LookAt(newTarget);
    }
}
