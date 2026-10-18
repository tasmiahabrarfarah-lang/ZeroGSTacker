using UnityEngine;
using ZeroGStacker.Physics;

namespace ZeroGStacker.Gameplay
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private TowerStack towerStack;
        [SerializeField] private float verticalOffset = 3.5f;
        [SerializeField] private float smoothTime = 0.25f;

        [Header("Zoom Scaling")]
        [SerializeField] private float baseOrthoSize = 7.0f;
        [SerializeField] private float zoomOutRate = 0.04f;
        [SerializeField] private float maxOrthoSize = 14.0f;

        private Camera cam;
        private Vector3 currentVelocity;
        private float initialY;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            initialY = transform.position.y;
        }

        private void LateUpdate()
        {
            if (towerStack == null) return;

            Vector2 topAnchor = towerStack.GetTopAnchorPoint();
            float targetY = Mathf.Max(initialY, topAnchor.y + verticalOffset);

            Vector3 targetPos = new Vector3(0f, targetY, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);

            // Dynamic orthographic zoom for tall towers
            if (cam != null && cam.orthographic)
            {
                float targetSize = Mathf.Clamp(baseOrthoSize + (towerStack.Height * zoomOutRate), baseOrthoSize, maxOrthoSize);
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * 2.0f);
            }
        }

        public void ResetCamera()
        {
            transform.position = new Vector3(0f, initialY, transform.position.z);
            currentVelocity = Vector3.zero;
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = baseOrthoSize;
            }
        }
    }
}
