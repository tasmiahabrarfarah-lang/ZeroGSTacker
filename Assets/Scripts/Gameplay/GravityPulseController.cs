using System;
using System.Collections;
using UnityEngine;
using ZeroGStacker.Audio;
using ZeroGStacker.Core;
using ZeroGStacker.Physics;
using ZeroGStacker.Visuals;

namespace ZeroGStacker.Gameplay
{
    public class GravityPulseController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TowerStack towerStack;
        [SerializeField] private ShapeSpawner shapeSpawner;
        [SerializeField] private PulseVFX pulseVFX;

        [Header("Tuning")]
        [SerializeField] private float pulseDuration = GameConstants.DefaultPulseDuration;
        [SerializeField] private float perfectThreshold = GameConstants.DefaultPerfectThreshold;
        [SerializeField] private float stackMaxBounds = 1.6f;

        [Header("State")]
        private bool isPulseInFlight = false;
        private Camera mainCamera;

        public event Action<PlacedShape, bool> OnBlockPlaced; // block, isPerfect
        public event Action OnBlockMissed; // soft fail

        public bool IsPulseInFlight => isPulseInFlight;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        public void ApplyRemoteConfig(float duration, float perfectThresh)
        {
            pulseDuration = duration;
            perfectThreshold = perfectThresh;
        }

        public bool TryFirePulse(Vector2? screenPos = null)
        {
            if (isPulseInFlight) return false;
            if (towerStack == null || towerStack.IsCollapsed) return false;

            DriftingShape targetShape = SelectTargetShape(screenPos);
            if (targetShape == null) return false;

            StartCoroutine(ExecutePulseRoutine(targetShape));
            return true;
        }

        private DriftingShape SelectTargetShape(Vector2? screenPos)
        {
            var activeShapes = shapeSpawner.ActiveShapes;
            if (activeShapes == null || activeShapes.Count == 0) return null;

            // If screen position is provided, attempt 2D raycast / point overlap
            if (screenPos.HasValue && mainCamera != null)
            {
                Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPos.Value);
                RaycastHit2D hit = UnityEngine.Physics2D.Raycast(worldPoint, Vector2.zero);
                if (hit.collider != null)
                {
                    var clickedShape = hit.collider.GetComponent<DriftingShape>();
                    if (clickedShape != null && !clickedShape.IsBeingPulled)
                    {
                        return clickedShape;
                    }
                }
            }

            // Fallback: pick the lowest altitude or closest drifting shape to the stack top anchor
            Vector2 anchor = towerStack.GetTopAnchorPoint();
            DriftingShape best = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < activeShapes.Count; i++)
            {
                var s = activeShapes[i];
                if (s == null || s.IsBeingPulled) continue;

                float dist = Vector2.Distance(s.transform.position, anchor);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = s;
                }
            }

            return best;
        }

        private IEnumerator ExecutePulseRoutine(DriftingShape target)
        {
            isPulseInFlight = true;
            target.SetBeingPulled(true);
            shapeSpawner.RemoveShape(target);

            // Audio: pulse fire sound
            ProceduralSoundSynthesizer.Instance?.PlayPulseFire();

            Vector2 startPos = target.transform.position;
            Color shapeColor = GameConstants.GetColor(target.ColorType);

            float elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / pulseDuration);

                // Easing curve: EaseInQuad (t * t) for gravitational acceleration
                float easeT = t * t;

                Vector2 currentAnchor = towerStack.GetTopAnchorPoint();
                Vector2 targetArrivalPos = currentAnchor + Vector2.up * (target.Radius * 0.9f);

                // Interpolate position
                target.transform.position = Vector2.Lerp(startPos, targetArrivalPos, easeT);

                // Render glowing beam between anchor and shape
                if (pulseVFX != null)
                {
                    pulseVFX.ShowBeam(currentAnchor, target.transform.position, shapeColor);
                }

                yield return null;
            }

            // Hide pulse beam
            if (pulseVFX != null)
            {
                pulseVFX.HideBeam();
            }

            // On Arrival: Compute horizontal offset from ideal center (stack-top center X)
            Vector2 finalAnchor = towerStack.GetTopAnchorPoint();
            float arrivalX = target.transform.position.x;
            float idealCenterX = finalAnchor.x;
            float horizontalOffset = arrivalX - idealCenterX;
            float placementError = Mathf.Abs(horizontalOffset);

            // Evaluate placement outcome
            if (placementError < perfectThreshold)
            {
                // PERFECT PLACEMENT
                HandlePerfectPlacement(target, finalAnchor, shapeColor);
            }
            else if (placementError <= stackMaxBounds)
            {
                // NORMAL PLACEMENT
                HandleNormalPlacement(target, finalAnchor, horizontalOffset, shapeColor);
            }
            else
            {
                // SOFT FAIL: Falls off tower bounds
                HandleMissPlacement(target);
            }

            isPulseInFlight = false;
        }

        private void HandlePerfectPlacement(DriftingShape target, Vector2 anchor, Color color)
        {
            // Auto-correct nudge toward exact center
            Vector2 correctedPos = new Vector2(anchor.x, anchor.y + (target.Radius * 0.9f));
            target.transform.position = correctedPos;
            target.transform.rotation = Quaternion.identity;

            // Visuals: Radial particle burst + Screen Shake
            pulseVFX?.PlayImpactBurst(correctedPos, color, isPerfect: true);
            ScreenShaker.Instance?.Shake(0.16f, 0.09f);

            // Register into tower stack
            var placed = new PlacedShape(
                target.Type,
                target.ColorType,
                color,
                correctedPos,
                correctedPos,
                0f, // 0 offset because auto-corrected
                target.Radius,
                target.Mass,
                target.gameObject
            );

            towerStack.AddBlock(placed);
            Destroy(target); // remove DriftingShape component, keep GameObject

            OnBlockPlaced?.Invoke(placed, true);
        }

        private void HandleNormalPlacement(DriftingShape target, Vector2 anchor, float horizontalOffset, Color color)
        {
            Vector2 arrivalPos = new Vector2(anchor.x + horizontalOffset, anchor.y + (target.Radius * 0.9f));
            target.transform.position = arrivalPos;

            pulseVFX?.PlayImpactBurst(arrivalPos, color, isPerfect: false);
            ProceduralSoundSynthesizer.Instance?.PlayNormalPlacement();

            var placed = new PlacedShape(
                target.Type,
                target.ColorType,
                color,
                arrivalPos,
                arrivalPos,
                horizontalOffset,
                target.Radius,
                target.Mass,
                target.gameObject
            );

            towerStack.AddBlock(placed);
            Destroy(target);

            OnBlockPlaced?.Invoke(placed, false);
        }

        private void HandleMissPlacement(DriftingShape target)
        {
            // Soft fail: falls off, streak resets, no game over
            float flingDir = Mathf.Sign(target.transform.position.x - towerStack.GetTopAnchorPoint().x);
            Vector2 tumbleVelocity = new Vector2(flingDir * UnityEngine.Random.Range(3f, 5f), -UnityEngine.Random.Range(2f, 4f));
            float torque = -flingDir * UnityEngine.Random.Range(180f, 360f);

            target.DetachAndTumble(tumbleVelocity, torque);

            OnBlockMissed?.Invoke();
        }
    }
}
