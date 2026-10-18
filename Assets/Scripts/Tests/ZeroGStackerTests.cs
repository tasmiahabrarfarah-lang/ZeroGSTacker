using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ZeroGStacker.Backend;
using ZeroGStacker.Core;
using ZeroGStacker.Gameplay;
using ZeroGStacker.Physics;

namespace ZeroGStacker.Tests
{
    public class ZeroGStackerTests
    {
        [Test]
        public void StabilityMath_ComputeBlockTorque_CalculatesExpectedTorque()
        {
            float mass = 2.0f;
            float offset = 0.5f;
            float gravity = 9.81f;

            float torque = StabilityMath.ComputeBlockTorque(mass, offset, gravity);

            Assert.AreEqual(2.0f * 0.5f * 9.81f, torque, 0.001f);
        }

        [Test]
        public void StabilityMath_ComputeTotalLean_NormalizesByHeight()
        {
            var blocks = new List<PlacedShape>
            {
                new PlacedShape(ShapeType.Cube, ShapeColor.Cyan, Color.cyan, Vector2.zero, Vector2.zero, 0.4f, 0.5f, 1.0f, null),
                new PlacedShape(ShapeType.Cube, ShapeColor.Magenta, Color.magenta, Vector2.zero, Vector2.zero, 0.6f, 0.5f, 1.0f, null)
            };

            float stackHeight = 5.0f;
            float gravity = 10.0f;

            // Torque 1: 1.0 * 0.4 * 10 = 4.0
            // Torque 2: 1.0 * 0.6 * 10 = 6.0
            // Sum = 10.0
            // TotalLean = 10.0 / 5.0 = 2.0
            float totalLean = StabilityMath.ComputeTotalLean(blocks, stackHeight, gravity);

            Assert.AreEqual(2.0f, totalLean, 0.001f);
        }

        [Test]
        public void StabilityMath_ComputeCenterOfMass_CalculatesWeightedAverage()
        {
            var blocks = new List<PlacedShape>
            {
                new PlacedShape(ShapeType.Cube, ShapeColor.Cyan, Color.cyan, new Vector2(-1f, 2f), Vector2.zero, -1f, 0.5f, 1.0f, null),
                new PlacedShape(ShapeType.Cube, ShapeColor.Magenta, Color.magenta, new Vector2(3f, 4f), Vector2.zero, 3f, 0.5f, 1.0f, null)
            };

            Vector2 com = StabilityMath.ComputeCenterOfMass(blocks);

            // X = (-1 * 1 + 3 * 1) / 2 = 1.0
            // Y = (2 * 1 + 4 * 1) / 2 = 3.0
            Assert.AreEqual(1.0f, com.x, 0.001f);
            Assert.AreEqual(3.0f, com.y, 0.001f);
        }

        [Test]
        public void EndlessDifficulty_SpeedScalingBands_MatchSpec()
        {
            var go = new GameObject();
            var spawner = go.AddComponent<ShapeSpawner>();
            spawner.ApplyRemoteConfig(2.0f, 1.15f, 1.30f, 1.50f, 1.70f, 0.05f);

            // Band 0-10: 2.0
            Assert.AreEqual(2.0f, spawner.CalculateDriftSpeed(5.0f), 0.001f);

            // Band 10-25: 2.0 * 1.15 = 2.30
            Assert.AreEqual(2.30f, spawner.CalculateDriftSpeed(15.0f), 0.001f);

            // Band 25-50: 2.0 * 1.30 = 2.60
            Assert.AreEqual(2.60f, spawner.CalculateDriftSpeed(35.0f), 0.001f);

            // Band 50-100: 2.0 * 1.50 = 3.00
            Assert.AreEqual(3.00f, spawner.CalculateDriftSpeed(75.0f), 0.001f);

            // Band 100+ at 100m: 2.0 * 1.70 = 3.40
            Assert.AreEqual(3.40f, spawner.CalculateDriftSpeed(100.0f), 0.001f);

            // Band 100+ at 120m (20 units beyond 100 = 2 tens = +0.10): 2.0 * 1.80 = 3.60
            Assert.AreEqual(3.60f, spawner.CalculateDriftSpeed(120.0f), 0.001f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void DailyChallengeService_DeterministicSeed_IsIdenticalForSameDate()
        {
            var go = new GameObject();
            var daily = go.AddComponent<DailyChallengeService>();

            int seed1 = daily.GetDailySeed();
            int seed2 = daily.GetDailySeed();

            Assert.AreEqual(seed1, seed2);
            Assert.IsTrue(seed1 > 0);

            Object.DestroyImmediate(go);
        }
    }
}
