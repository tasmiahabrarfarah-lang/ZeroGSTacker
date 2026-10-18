using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZeroGStacker.Core;

namespace ZeroGStacker.Physics
{
    public class TowerStack : MonoBehaviour
    {
        [Header("Tower Hierarchy")]
        [SerializeField] private Transform basePlatform;
        [SerializeField] private Transform pivotRoot;

        [Header("Stability Configuration")]
        [SerializeField] private float gravityConstant = GameConstants.DefaultGravityConstant;
        [SerializeField] private float leanSensitivity = GameConstants.DefaultLeanSensitivity;
        [SerializeField] private float maxLeanAngle = GameConstants.DefaultMaxLeanAngle;
        [SerializeField] private float collapseThreshold = GameConstants.DefaultCollapseThreshold;
        [SerializeField] private float graceDuration = GameConstants.DefaultGracePeriodDuration;
        [SerializeField] private float springDampingTime = GameConstants.DefaultSpringDampingTime;

        [Header("State")]
        private StackData stackData = new StackData();
        private float angularVelocity;
        private float graceTimer = 0f;
        private bool isWarning = false;
        private bool isCollapsed = false;

        public event Action<float> OnLeanChanged; // lean angle (-max to +max)
        public event Action<float, float> OnGraceTimerUpdated; // current, max
        public event Action OnCollapseWarningStarted;
        public event Action OnCollapseWarningRecovered;
        public event Action OnTowerCollapsed;
        public event Action OnStabilizerFired;

        public StackData Data => stackData;
        public Transform PivotRoot => pivotRoot != null ? pivotRoot : transform;
        public Transform BasePlatform => basePlatform;
        public float Height => stackData.height;
        public float LeanAngle => stackData.leanAngle;
        public float TotalLean => stackData.totalLean;
        public float CollapseThreshold => collapseThreshold;
        public float GraceProgress => graceDuration > 0f ? Mathf.Clamp01(graceTimer / graceDuration) : 0f;
        public bool IsInGracePeriod => isWarning;
        public bool IsCollapsed => isCollapsed;

        private void Awake()
        {
            if (pivotRoot == null)
            {
                var pivotGo = new GameObject("StackPivot");
                pivotGo.transform.SetParent(transform, false);
                pivotRoot = pivotGo.transform;
            }
        }

        public void ApplyRemoteConfig(float gravity, float threshold, float grace, float sensitivity)
        {
            gravityConstant = gravity;
            collapseThreshold = threshold;
            graceDuration = grace;
            leanSensitivity = sensitivity;
        }

        public void ResetTower()
        {
            isCollapsed = false;
            graceTimer = 0f;
            isWarning = false;
            angularVelocity = 0f;

            for (int i = 0; i < stackData.blocks.Count; i++)
            {
                if (stackData.blocks[i].instance != null)
                {
                    Destroy(stackData.blocks[i].instance);
                }
            }

            stackData.Clear();
            if (pivotRoot != null)
            {
                pivotRoot.localRotation = Quaternion.identity;
            }
            OnLeanChanged?.Invoke(0f);
        }

        public Vector2 GetTopAnchorPoint()
        {
            if (stackData.blocks.Count == 0)
            {
                return basePlatform != null ? (Vector2)basePlatform.position + Vector2.up * 0.5f : (Vector2)transform.position;
            }

            var topBlock = stackData.blocks[stackData.blocks.Count - 1];
            if (topBlock.instance != null)
            {
                return (Vector2)topBlock.instance.transform.position + Vector2.up * (topBlock.radius * 1.0f);
            }

            return (Vector2)transform.position + Vector2.up * stackData.height;
        }

        public void AddBlock(PlacedShape block)
        {
            if (isCollapsed) return;

            // Parent block to pivot so it sways with the tower
            block.instance.transform.SetParent(PivotRoot, true);
            block.localPosition = block.instance.transform.localPosition;
            block.worldPosition = block.instance.transform.position;

            // Horizontal offset is relative to the tower central axis
            block.horizontalOffset = block.localPosition.x;

            stackData.blocks.Add(block);

            // Update stack height
            float topY = block.localPosition.y + block.radius;
            if (topY > stackData.height)
            {
                stackData.height = topY;
            }

            // Recalculate stability metrics
            RecalculateStability();
        }

        private void RecalculateStability()
        {
            stackData.centerOfMass = StabilityMath.ComputeCenterOfMass(stackData.blocks);
            stackData.totalLean = StabilityMath.ComputeTotalLean(stackData.blocks, stackData.height, gravityConstant);
        }

        private void Update()
        {
            if (isCollapsed) return;

            float dt = Time.deltaTime;
            float targetAngle = StabilityMath.CalculateTargetAngle(stackData.totalLean, leanSensitivity, maxLeanAngle);

            // Critically damped spring interpolation
            stackData.leanAngle = StabilityMath.StepSpringDamp(
                stackData.leanAngle,
                targetAngle,
                ref angularVelocity,
                springDampingTime,
                dt
            );

            // Apply sway rotation to tower pivot
            if (pivotRoot != null)
            {
                pivotRoot.localRotation = Quaternion.Euler(0f, 0f, -stackData.leanAngle);
            }

            OnLeanChanged?.Invoke(stackData.leanAngle);

            // Collapse Condition Check
            float absLean = Mathf.Abs(stackData.leanAngle);
            if (absLean > collapseThreshold)
            {
                if (!isWarning)
                {
                    isWarning = true;
                    OnCollapseWarningStarted?.Invoke();
                }

                graceTimer += dt;
                OnGraceTimerUpdated?.Invoke(graceTimer, graceDuration);

                if (graceTimer >= graceDuration)
                {
                    TriggerCollapse();
                }
            }
            else
            {
                if (isWarning)
                {
                    isWarning = false;
                    graceTimer = 0f;
                    OnCollapseWarningRecovered?.Invoke();
                    OnGraceTimerUpdated?.Invoke(0f, graceDuration);
                }
            }
        }

        public void StabilizeTower()
        {
            if (isCollapsed) return;

            // Stabilizer instantly zeroes totalLean accumulated so far
            stackData.totalLean = 0f;
            angularVelocity = 0f;

            // Nudge blocks toward center line to preserve mechanical equilibrium
            for (int i = 0; i < stackData.blocks.Count; i++)
            {
                var block = stackData.blocks[i];
                if (block.instance != null)
                {
                    Vector3 localPos = block.instance.transform.localPosition;
                    localPos.x *= 0.15f; // Pull blocks close to vertical axis
                    block.instance.transform.localPosition = localPos;
                    block.localPosition = localPos;
                    block.horizontalOffset = localPos.x;
                }
            }

            isWarning = false;
            graceTimer = 0f;
            OnCollapseWarningRecovered?.Invoke();
            OnGraceTimerUpdated?.Invoke(0f, graceDuration);

            OnStabilizerFired?.Invoke();
        }

        public void TriggerCollapse()
        {
            if (isCollapsed) return;
            isCollapsed = true;

            StartCoroutine(CollapseRoutine());
        }

        private IEnumerator CollapseRoutine()
        {
            // Section 9: shapes detach with individual physics, slow-motion for 0.5s, then cut to game-over UI
            float fallSign = Mathf.Sign(stackData.leanAngle);
            if (fallSign == 0f) fallSign = 1f;

            for (int i = 0; i < stackData.blocks.Count; i++)
            {
                var block = stackData.blocks[i];
                if (block.instance != null)
                {
                    block.instance.transform.SetParent(null, true);

                    var rb = block.instance.GetComponent<Rigidbody2D>();
                    if (rb == null)
                    {
                        rb = block.instance.gameObject.AddComponent<Rigidbody2D>();
                    }
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = 1.4f;

                    // Apply outward burst and torque based on collapse direction
                    float heightFactor = (float)(i + 1) / Mathf.Max(1, stackData.blocks.Count);
                    Vector2 impulse = new Vector2(fallSign * UnityEngine.Random.Range(2.5f, 6.0f) * heightFactor, UnityEngine.Random.Range(0.5f, 3.0f));
                    rb.linearVelocity = impulse;
                    rb.angularVelocity = -fallSign * UnityEngine.Random.Range(180f, 400f);
                }
            }

            // Slow motion for 0.5 real seconds
            Time.timeScale = 0.25f;
            yield return new WaitForSecondsRealtime(0.5f);
            Time.timeScale = 1.0f;

            OnTowerCollapsed?.Invoke();
        }

        public void ReviveStack()
        {
            // Revive once per run: restabilizes stack
            isCollapsed = false;
            graceTimer = 0f;
            isWarning = false;
            angularVelocity = 0f;
            stackData.leanAngle = 0f;
            stackData.totalLean = 0f;

            if (pivotRoot != null)
            {
                pivotRoot.localRotation = Quaternion.identity;
            }

            // Re-parent active blocks if any
            for (int i = 0; i < stackData.blocks.Count; i++)
            {
                var block = stackData.blocks[i];
                if (block.instance != null)
                {
                    block.instance.transform.SetParent(PivotRoot, true);
                    var rb = block.instance.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                        rb.bodyType = RigidbodyType2D.Kinematic;
                    }
                    Vector3 lp = block.instance.transform.localPosition;
                    lp.x = 0f;
                    block.instance.transform.localPosition = lp;
                    block.localPosition = lp;
                    block.horizontalOffset = 0f;
                }
            }

            OnLeanChanged?.Invoke(0f);
            OnCollapseWarningRecovered?.Invoke();
        }
    }
}
