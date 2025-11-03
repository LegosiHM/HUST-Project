using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Cinemachine;

public class PhotoCapture : MonoBehaviour
{
    [Header("Photo Taker")]
    [SerializeField] private Image photoDisplayArea;
    [SerializeField] private GameObject photoFrame;
    [SerializeField] private float showPhotoDuration = 0.75f;

    [Header("FlashEffect")]
    [SerializeField] private GameObject cameraFlash;
    [SerializeField] private float flashTime = 0.05f;

    [Header("Photo Fader Effect")]
    [SerializeField] private Animator fadingAnimation;

    [Header("Photo Scan (Box ‘raycast’ in front of camera)")]
    [SerializeField] private Camera cam;
    [SerializeField] private float nearDistance = 0.3f;
    [SerializeField] private float captureRange = 20f;
    [SerializeField] private LayerMask physicsMask = ~0;
    [SerializeField] private bool countUniqueGameObjects = true;

    [System.Serializable]
    public struct TagScore
    {
        public string tag;
        public int score;
    }

    [Header("Scoring by Tag")]
    [Tooltip("Add tag→score mappings here. Only these tags add score.")]
    [SerializeField] private List<TagScore> tagScores = new List<TagScore>();

    private Texture2D screenCapture;
    private bool viewingPhoto;
    private Dictionary<string, int> tagScoreMap;

    private void Awake()
    {
        if (cam != null) return;

        // 1️⃣ Try to get Cinemachine's active output camera
        CinemachineBrain brain = FindFirstObjectByType<CinemachineBrain>();
        if (brain != null && brain.OutputCamera != null)
        {
            cam = brain.OutputCamera;
            Debug.Log($"✅ PhotoCapture bound to Cinemachine camera: {cam.name}");
            return;
        }

        // 2️⃣ Fallback to the MainCamera tag
        cam = Camera.main;
        if (cam != null)
        {
            Debug.Log($"✅ PhotoCapture bound to MainCamera: {cam.name}");
        }
        else
        {
            Debug.LogError("❌ No Camera found for PhotoCapture! Add a MainCamera or CinemachineBrain.");
        }
    }

    private void Start()
    {
        screenCapture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (!viewingPhoto)
            {
                StartCoroutine(CapturePhoto());
            }
            else
            {
                RemovePhoto();
            }
        }
    }

    IEnumerator CapturePhoto()
    {
        viewingPhoto = true;

        // Wait for end of frame so the screen read matches what was rendered.
        yield return new WaitForEndOfFrame();

        // 1) Grab the screen
        Rect regionToRead = new Rect(0, 0, Screen.width, Screen.height);
        screenCapture.ReadPixels(regionToRead, 0, 0, false);
        screenCapture.Apply();
        ShowPhoto();

        // 2) Score what’s inside the “photo” using an oriented OverlapBox that approximates the camera frustum slice
        int totalScore = ComputePhotoScore();

        // 3) Output the total
        Debug.Log($"[PhotoCapture] Total Photo Score = {totalScore}");

        yield return new WaitForSeconds(showPhotoDuration);
        photoFrame.SetActive(false);
    }

    void ShowPhoto()
    {
        Sprite photoSprite = Sprite.Create(
            screenCapture,
            new Rect(0.0f, 0.0f, screenCapture.width, screenCapture.height),
            new Vector2(0.5f, 0.5f),
            100.0f
        );

        photoDisplayArea.sprite = photoSprite;
        photoFrame.SetActive(true);
        StartCoroutine(CameraFlashEffect());
        if (fadingAnimation != null) fadingAnimation.Play("PhotoFade");
    }

    IEnumerator CameraFlashEffect()
    {
        if (cameraFlash != null)
        {
            cameraFlash.SetActive(true);
            yield return new WaitForSeconds(flashTime);
            cameraFlash.SetActive(false);
        }
    }

    void RemovePhoto()
    {
        viewingPhoto = false;
        photoFrame.SetActive(false);
    }

    // --- Core scoring logic ---
    int ComputePhotoScore()
    {
        // --- Ensure tag score mapping exists ---
        if (tagScoreMap == null || tagScoreMap.Count == 0)
        {
            tagScoreMap = new Dictionary<string, int>();
            foreach (var ts in tagScores)
            {
                if (!string.IsNullOrEmpty(ts.tag))
                    tagScoreMap[ts.tag] = ts.score;
            }
        }

        // --- Calculate OverlapBox volume based on camera view ---
        float near = Mathf.Max(0.01f, nearDistance);
        float depth = Mathf.Max(0.01f, captureRange - near);
        float mid = near + depth * 0.5f;

        float halfHeight = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * mid;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 center = cam.transform.position + cam.transform.forward * mid;
        Vector3 halfExtents = new Vector3(halfWidth, halfHeight, depth * 0.5f);

        // --- Detect all colliders inside the camera’s photo zone ---
        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            cam.transform.rotation,
            physicsMask,
            QueryTriggerInteraction.Collide
        );

        // --- Score calculation ---
        int totalScore = 0;
        HashSet<GameObject> countedObjects = countUniqueGameObjects ? new HashSet<GameObject>() : null;

        foreach (Collider col in hits)
        {
            if (col == null) continue;

            GameObject go = col.gameObject;

            if (countUniqueGameObjects && countedObjects.Contains(go))
                continue;

            countedObjects?.Add(go);

            if (tagScoreMap.TryGetValue(go.tag, out int value))
                totalScore += value;
        }

        return totalScore;
    }


#if UNITY_EDITOR
    // Helpful gizmo to see the scan volume in the Scene view while selected
    private void OnDrawGizmosSelected()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        float near = Mathf.Max(0.01f, nearDistance);
        float depth = Mathf.Max(0.01f, captureRange - near);
        float mid = near + depth * 0.5f;

        float halfHeight = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * mid;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 center = cam.transform.position + cam.transform.forward * mid;
        Vector3 size = new Vector3(halfWidth * 2f, halfHeight * 2f, depth);

        Gizmos.color = Color.cyan;
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, cam.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.matrix = old;
    }
#endif
}
