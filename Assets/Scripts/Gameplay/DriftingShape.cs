using System;
using UnityEngine;
using ZeroGStacker.Core;

namespace ZeroGStacker.Gameplay
{
    public class DriftingShape : MonoBehaviour
    {
        [Header("Shape Data")]
        [SerializeField] private ShapeType shapeType;
        [SerializeField] private ShapeColor colorType;
        [SerializeField] private float radius = 0.7f;
        [SerializeField] private float mass = 1.0f;
        [SerializeField] private float driftSpeed = 2.0f;
        [SerializeField] private float rotationSpeed = 20.0f;
        [SerializeField] private bool isUnstable = false;

        [Header("Path State")]
        private Vector2 spawnOrigin;
        private float pathFrequency = 1.2f;
        private float pathAmplitude = 0.6f;
        private float driftDirection = 1.0f; // 1 = right, -1 = left
        private float elapsed = 0f;
        private bool isBeingPulled = false;

        public ShapeType Type => shapeType;
        public ShapeColor ColorType => colorType;
        public float Radius => radius;
        public float Mass => mass;
        public bool IsBeingPulled => isBeingPulled;
        public bool IsUnstable => isUnstable;

        public void Initialize(ShapeType type, ShapeColor color, float rad, float m, float speed, float rotSpeed, bool unstable, Vector2 origin, float dir)
        {
            shapeType = type;
            colorType = color;
            radius = rad;
            mass = m;
            driftSpeed = speed;
            rotationSpeed = rotSpeed;
            isUnstable = unstable;
            spawnOrigin = origin;
            driftDirection = dir;
            elapsed = UnityEngine.Random.Range(0f, 10f);
            isBeingPulled = false;
        }

        public void SetBeingPulled(bool pulled)
        {
            isBeingPulled = pulled;
        }

        private void Update()
        {
            if (isBeingPulled) return;

            elapsed += Time.deltaTime;

            if (isUnstable)
            {
                // Wobble in place with high frequency jitter
                float wobbleX = Mathf.Sin(elapsed * 12.0f) * 0.15f;
                float wobbleY = Mathf.Cos(elapsed * 10.0f) * 0.15f;
                transform.position = (Vector3)(spawnOrigin + new Vector2(wobbleX, wobbleY));

                // Pulsate scale slightly
                float scalePulse = 1.0f + Mathf.Sin(elapsed * 14.0f) * 0.08f;
                transform.localScale = Vector3.one * scalePulse;

                transform.Rotate(0f, 0f, rotationSpeed * 1.5f * Time.deltaTime);
                return;
            }

            // Spline / sinusoidal wave drift across the screen
            float currentX = transform.position.x + (driftDirection * driftSpeed * Time.deltaTime);
            float currentY = spawnOrigin.y + Mathf.Sin(elapsed * pathFrequency) * pathAmplitude;

            transform.position = new Vector3(currentX, currentY, 0f);
            transform.Rotate(0f, 0f, rotationSpeed * driftDirection * Time.deltaTime);
        }

        public void DetachAndTumble(Vector2 velocity, float torque)
        {
            isBeingPulled = false;
            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0.8f;
            rb.linearVelocity = velocity;
            rb.angularVelocity = torque;

            // Destroy after falling off screen
            Destroy(gameObject, 3.5f);
        }
    }
}
