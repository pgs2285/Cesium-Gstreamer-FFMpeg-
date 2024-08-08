using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraCapture : DependencyRoot
{
    [SerializeField] bool _setResolution = true;
    [SerializeField] int _width = 1280;
    [SerializeField] int _height = 720;
    [SerializeField] bool _allowSlowDown = true;

    public GstUnityImageGrabber _grabber;

    [SerializeField, HideInInspector] Shader _shader;
    Material _material;

    bool _prepared = false;

    RenderTexture _tempTarget;
    public Texture2D _tempTex;

    static int _activePipeCount;

    public bool HasData = false;

    public int Width
    {
        get { return _width; }
    }

    public int Height
    {
        get { return _height; }
    }

    void OnValidate()
    {
    }

    void OnEnable()
    {
        PrepareTexture();
    }

    void OnDisable()
    {
        DestroyTexture();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _grabber.Destroy();
        DestroyTexture();
    }

    protected override void Start()
    {
        _grabber = new GstUnityImageGrabber();
        base.Start();
    }

    void Update()
    {
        if (_prepared)
        {
            CaptureFrame();
        }
    }

    void PrepareTexture()
    {
        if (_prepared) return;

        var camera = GetComponent<Camera>();
        var width = _width;
        var height = _height;

        // Apply the screen resolution settings.
        if (_setResolution)
        {
            _tempTarget = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
            _tempTarget.wrapMode = TextureWrapMode.Clamp;
            _tempTarget.filterMode = FilterMode.Bilinear;
            camera.targetTexture = _tempTarget;
        }
        else
        {
            width = camera.pixelWidth;
            height = camera.pixelHeight;
        }

        _tempTex = new Texture2D(width, height, TextureFormat.RGB24, false);
        _prepared = true;
    }

    void DestroyTexture()
    {
        var camera = GetComponent<Camera>();

        // Release the temporary render target.
        if (_tempTarget != null && _tempTarget == camera.targetTexture)
        {
            camera.targetTexture = null;
            _tempTarget.Release();
            Destroy(_tempTarget);
            _tempTarget = null;
        }

        if (_tempTex != null)
        {
            Destroy(_tempTex);
            _tempTex = null;
        }

        _prepared = false;
    }

    void CaptureFrame()
    {
        var camera = GetComponent<Camera>();

        // Ensure the camera is rendering to the correct target
        camera.targetTexture = _tempTarget;

        // Render the camera's view to the RenderTexture
        camera.Render();

        // Reset the camera's target texture to null to render to the main display
        camera.targetTexture = null;

        // Read the RenderTexture to the Texture2D
        RenderTexture.active = _tempTarget;

        // Read pixels from RenderTexture to Texture2D
        _tempTex.ReadPixels(new Rect(0, 0, _tempTarget.width, _tempTarget.height), 0, 0, false);
        _tempTex.Apply();
        RenderTexture.active = null;

        // Pass the Texture2D to the grabber
        _grabber.SetTexture2D(_tempTex);
        _grabber.Update();
        HasData = true;
    }
}
