using System.Collections;
using UnityEngine;

namespace ZeroGStacker.Visuals
{
    public class ScreenShaker : MonoBehaviour
    {
        private static ScreenShaker instance;
        public static ScreenShaker Instance => instance;

        private Transform targetCamera;
        private Vector3 initialLocalPos;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (instance != null && instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (Camera.main != null)
            {
                targetCamera = Camera.main.transform;
                initialLocalPos = targetCamera.localPosition;
            }
        }

        public void Shake(float duration = 0.15f, float magnitude = 0.08f)
        {
            if (targetCamera == null)
            {
                if (Camera.main != null) targetCamera = Camera.main.transform;
                else return;
            }

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            float elapsed = 0f;
            Vector3 basePos = targetCamera.localPosition;

            while (elapsed < duration)
            {
                float factor = 1f - (elapsed / duration);
                float x = (Random.value * 2f - 1f) * magnitude * factor;
                float y = (Random.value * 2f - 1f) * magnitude * factor;

                targetCamera.localPosition = basePos + new Vector3(x, y, 0f);

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            targetCamera.localPosition = basePos;
            shakeCoroutine = null;
        }
    }
}
