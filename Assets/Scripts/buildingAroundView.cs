using UnityEngine;
using UnityEngine.UI;

public class buildingAroundView : MonoBehaviour
{
    public GameObject rotatingCameraPrefab; // 회전 카메라 프리팹
    public Vector3 offset = new Vector3(0, 5, -10); // 카메라의 오프셋
    public RawImage cameraOutput; // 회전 카메라의 뷰를 표시할 UI RawImage
    public float rotationSpeed = 10f; // 회전 속도

    private Camera rotatingCamera;
    private RenderTexture renderTexture;
    private GameObject rotatingCameraObject;
    private Vector3 target;
    private bool isStart = false;
    private GameObject viewPanel;
    public Canvas canvas;

    void Update()
    {
        // 마우스 클릭을 감지
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // 클릭한 오브젝트가 있다면 startViewing 호출
                startViewing(hit.transform.position);
            }
        }

        // 카메라 회전 로직
        if (isStart)
        {
            rotatingCameraObject.transform.RotateAround(target, Vector3.up, rotationSpeed * Time.deltaTime);
            rotatingCameraObject.transform.LookAt(target);
        }
    }

    public void startViewing(Vector3 newTarget)
    {
        // 회전 카메라 인스턴스화 및 설정
        if (!isStart)
        {
            viewPanel = canvas.transform.Find("ViewPanel").gameObject;
            viewPanel.SetActive(true);

            rotatingCameraObject = Instantiate(rotatingCameraPrefab);
            rotatingCamera = rotatingCameraObject.GetComponent<Camera>();

            // RenderTexture 생성 및 설정
            renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
            rotatingCamera.targetTexture = renderTexture;
            cameraOutput.texture = renderTexture;

            isStart = true;
        }

        // 새로운 타겟 설정
        target = newTarget;

        // 초기 카메라 위치 설정
        rotatingCameraObject.transform.position = target + offset;
        rotatingCameraObject.transform.LookAt(newTarget);
    }
}
