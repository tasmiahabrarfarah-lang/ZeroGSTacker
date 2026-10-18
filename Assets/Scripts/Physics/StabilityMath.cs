using System.Collections.Generic;
using UnityEngine;
using ZeroGStacker.Core;

namespace ZeroGStacker.Physics
{
    public static class StabilityMath
    {
        /// <summary>
        /// Computes individual block torque:
        /// torque_i = mass_i * horizontalOffset_i * gravityConstant
        /// </summary>
        public static float ComputeBlockTorque(float mass, float horizontalOffset, float gravityConstant)
        {
            return mass * horizontalOffset * gravityConstant;
        }

        /// <summary>
        /// Computes total lean across the stack:
        /// totalLean = sum(torque_i) / stack.height
        /// (taller stacks are more sensitive, creating natural difficulty ramp)
        /// </summary>
        public static float ComputeTotalLean(IReadOnlyList<PlacedShape> blocks, float stackHeight, float gravityConstant)
        {
            if (blocks == null || blocks.Count == 0) return 0f;

            float sumTorque = 0f;
            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                sumTorque += ComputeBlockTorque(block.mass, block.horizontalOffset, gravityConstant);
            }

            // Normalizing by stack height creates the natural leverage effect
            float effectiveHeight = Mathf.Max(stackHeight, 1.0f);
            return sumTorque / effectiveHeight;
        }

        /// <summary>
        /// Calculates target lean angle (degrees) derived from totalLean, clamped to max angle.
        /// </summary>
        public static float CalculateTargetAngle(float totalLean, float leanSensitivity, float maxLeanAngle)
        {
            float target = totalLean * leanSensitivity;
            return Mathf.Clamp(target, -maxLeanAngle, maxLeanAngle);
        }

        /// <summary>
        /// Interpolates current lean angle toward target angle using a critically damped spring
        /// </summary>
        public static float StepSpringDamp(float currentAngle, float targetAngle, ref float currentVelocity, float smoothTime, float deltaTime)
        {
            return Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentVelocity, smoothTime, Mathf.Infinity, deltaTime);
        }

        /// <summary>
        /// Calculates stack Center of Mass
        /// </summary>
        public static Vector2 ComputeCenterOfMass(IReadOnlyList<PlacedShape> blocks)
        {
            if (blocks == null || blocks.Count == 0) return Vector2.zero;

            Vector2 weightedPosSum = Vector2.zero;
            float totalMass = 0f;

            for (int i = 0; i < blocks.Count; i++)
            {
                var b = blocks[i];
                weightedPosSum += b.localPosition * b.mass;
                totalMass += b.mass;
            }

            return totalMass > 0f ? weightedPosSum / totalMass : Vector2.zero;
        }
    }
}
