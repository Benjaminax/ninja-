using UnityEngine;

[DefaultExecutionOrder(10000)]
[RequireComponent(typeof(Camera))]
public sealed class MobileCameraView : MonoBehaviour
{
    [SerializeField] float referenceAspect = 380f / 180f;
    [SerializeField] float referenceOrthographicSize = 4f;
    [SerializeField] float minimumOrthographicSize = 4f;
    [SerializeField] float maximumOrthographicSize = 8f;

    Camera targetCamera;
    float lastAspect;

    void Awake()
    {
        targetCamera = GetComponent<Camera>();
        if (Application.isMobilePlatform)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null || !targetCamera.orthographic || Screen.height <= 0)
            return;

        float aspect = (float)Screen.width / Screen.height;
        if (Mathf.Approximately(aspect, lastAspect))
            return;

        lastAspect = aspect;
        float size = referenceOrthographicSize;
        if (aspect < referenceAspect)
            size *= referenceAspect / aspect;

        targetCamera.orthographicSize = Mathf.Clamp(size, minimumOrthographicSize, maximumOrthographicSize);
    }
}
