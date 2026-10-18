using System.Collections.Generic;
using UnityEngine;
using ZeroGStacker.Core;
using ZeroGStacker.Visuals;

namespace ZeroGStacker.Gameplay
{
    public class ShapeSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] private float spawnWidth = 7.0f;
        [SerializeField] private float spawnHeightOffsetMin = 3.2f;
        [SerializeField] private float spawnHeightOffsetMax = 5.2f;
        [SerializeField] private int maxActiveShapes = 3;

        [Header("Tuning (Remote Configurable)")]
        [SerializeField] private float baseSpeed = 2.0f;
        [SerializeField] private float mult10_25 = 1.15f;
        [SerializeField] private float mult25_50 = 1.30f;
        [SerializeField] private float mult50_100 = 1.50f;
        [SerializeField] private float mult100Plus = 1.70f;
        [SerializeField] private float scalingPer10Units = 0.05f;

        private List<DriftingShape> activeShapes = new List<DriftingShape>();
        private System.Random seededRng;
        private bool isUsingSeed = false;

        public IReadOnlyList<DriftingShape> ActiveShapes => activeShapes;

        public void ApplyRemoteConfig(float baseSpd, float m1, float m2, float m3, float m4, float scalePer10)
        {
            baseSpeed = baseSpd;
            mult10_25 = m1;
            mult25_50 = m2;
            mult50_100 = m3;
            mult100Plus = m4;
            scalingPer10Units = scalePer10;
        }

        public void SetDailySeed(int seed)
        {
            isUsingSeed = true;
            seededRng = new System.Random(seed);
        }

        public void ClearAll()
        {
            for (int i = activeShapes.Count - 1; i >= 0; i--)
            {
                if (activeShapes[i] != null)
                {
                    Destroy(activeShapes[i].gameObject);
                }
            }
            activeShapes.Clear();
            isUsingSeed = false;
        }

        public void RemoveShape(DriftingShape shape)
        {
            activeShapes.Remove(shape);
        }

        public void MaintainShapeCount(Vector2 stackTopPos, float currentHeight)
        {
            // Clean nulls or out of bounds
            for (int i = activeShapes.Count - 1; i >= 0; i--)
            {
                var s = activeShapes[i];
                if (s == null)
                {
                    activeShapes.RemoveAt(i);
                    continue;
                }

                // If drifting shape drifted too far past opposite side
                if (!s.IsBeingPulled && Mathf.Abs(s.transform.position.x) > spawnWidth * 1.6f)
                {
                    Destroy(s.gameObject);
                    activeShapes.RemoveAt(i);
                }
            }

            while (activeShapes.Count < maxActiveShapes)
            {
                SpawnShape(stackTopPos, currentHeight);
            }
        }

        public DriftingShape SpawnShape(Vector2 stackTopPos, float currentHeight)
        {
            ShapeType shapeType = PickShapeType(currentHeight);
            ShapeColor colorType = PickShapeColor();

            float speed = CalculateDriftSpeed(currentHeight);
            float rotSpeed = (currentHeight >= 50f) ? UnityEngine.Random.Range(45f, 90f) : UnityEngine.Random.Range(15f, 35f);

            bool unstable = false;
            if (currentHeight >= 100f)
            {
                // Rare unstable shape at 100+ height (approx 20% chance)
                float roll = NextFloat();
                if (roll < 0.22f)
                {
                    unstable = true;
                }
            }

            float radius = GameConstants.DefaultBaseShapeRadius;
            float mass = GameConstants.DefaultBaseShapeMass;

            // Spawn from left or right edge
            float dir = (NextFloat() > 0.5f) ? 1.0f : -1.0f;
            float spawnX = -dir * spawnWidth;
            float altitude = stackTopPos.y + Mathf.Lerp(spawnHeightOffsetMin, spawnHeightOffsetMax, NextFloat());
            Vector2 spawnPos = new Vector2(spawnX, altitude);

            if (unstable)
            {
                // Unstable shapes spawn right near central corridor and wobble in place
                spawnPos = new Vector2(UnityEngine.Random.Range(-1.5f, 1.5f), altitude);
            }

            GameObject shapeGo = ProceduralShapeGenerator.CreateShapeGameObject(shapeType, colorType, radius, mass, out Collider2D col);
            shapeGo.transform.position = spawnPos;

            var driftingComp = shapeGo.AddComponent<DriftingShape>();
            driftingComp.Initialize(shapeType, colorType, radius, mass, speed, rotSpeed, unstable, spawnPos, dir);

            activeShapes.Add(driftingComp);
            return driftingComp;
        }

        private ShapeType PickShapeType(float height)
        {
            if (height < 10f)
            {
                return ShapeType.Cube;
            }
            else if (height < 25f)
            {
                return (NextFloat() < 0.5f) ? ShapeType.Cube : ShapeType.Sphere;
            }
            else
            {
                float roll = NextFloat();
                if (roll < 0.35f) return ShapeType.Cube;
                if (roll < 0.70f) return ShapeType.Sphere;
                return ShapeType.Prism;
            }
        }

        private ShapeColor PickShapeColor()
        {
            int count = System.Enum.GetValues(typeof(ShapeColor)).Length;
            int idx = isUsingSeed ? seededRng.Next(0, count) : UnityEngine.Random.Range(0, count);
            return (ShapeColor)idx;
        }

        public float CalculateDriftSpeed(float height)
        {
            if (height < 10f)
            {
                return baseSpeed;
            }
            if (height < 25f)
            {
                return baseSpeed * mult10_25;
            }
            if (height < 50f)
            {
                return baseSpeed * mult25_50;
            }
            if (height < 100f)
            {
                return baseSpeed * mult50_100;
            }

            // 100+ : base * mult100Plus, scaling +5%/10 units beyond 100
            float unitsOver100 = Mathf.Max(0f, height - 100f);
            int tens = Mathf.FloorToInt(unitsOver100 / 10f);
            float scaleMult = mult100Plus + (tens * scalingPer10Units);
            return baseSpeed * scaleMult;
        }

        private float NextFloat()
        {
            return isUsingSeed ? (float)seededRng.NextDouble() : UnityEngine.Random.value;
        }
    }
}
