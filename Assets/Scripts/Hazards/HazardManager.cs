using System.Collections.Generic;
using UnityEngine;
using ZeroGStacker.Core;
using ZeroGStacker.Physics;

namespace ZeroGStacker.Hazards
{
    public class HazardManager : MonoBehaviour
    {
        [Header("Height Thresholds (Remote Configurable)")]
        [SerializeField] private float asteroidStartHeight = 25.0f;
        [SerializeField] private float gravityFlipStartHeight = 50.0f;
        [SerializeField] private float solarWindStartHeight = 100.0f;

        [Header("References")]
        [SerializeField] private TowerStack towerStack;

        private List<HazardType> activeHazards = new List<HazardType>();
        private float asteroidSpawnTimer = 0f;
        private SolarWindGust solarWind;
        private GravityFlipZone flipZone;

        public IReadOnlyList<HazardType> ActiveHazards => activeHazards;

        public void ApplyRemoteConfig(float asteroidH, float flipH, float windH)
        {
            asteroidStartHeight = asteroidH;
            gravityFlipStartHeight = flipH;
            solarWindStartHeight = windH;
        }

        public void ResetHazards()
        {
            activeHazards.Clear();
            asteroidSpawnTimer = 0f;
            if (solarWind != null) Destroy(solarWind.gameObject);
            if (flipZone != null) Destroy(flipZone.gameObject);
        }

        private void Update()
        {
            if (towerStack == null || towerStack.IsCollapsed) return;

            float currentHeight = towerStack.Height;
            activeHazards.Clear();

            // Asteroids (25-50+)
            if (currentHeight >= asteroidStartHeight)
            {
                activeHazards.Add(HazardType.Asteroids);
                asteroidSpawnTimer += Time.deltaTime;
                if (asteroidSpawnTimer > 6.0f)
                {
                    asteroidSpawnTimer = 0f;
                    SpawnDriftingAsteroid(towerStack.GetTopAnchorPoint());
                }
            }

            // Gravity Flip (50-100+)
            if (currentHeight >= gravityFlipStartHeight)
            {
                activeHazards.Add(HazardType.GravityFlip);
                if (flipZone == null)
                {
                    var zoneGo = new GameObject("GravityFlipZone");
                    flipZone = zoneGo.AddComponent<GravityFlipZone>();
                }
                flipZone.transform.position = towerStack.GetTopAnchorPoint() + Vector2.up * 2.0f;
            }

            // Solar Wind (100+)
            if (currentHeight >= solarWindStartHeight)
            {
                activeHazards.Add(HazardType.SolarWind);
                if (solarWind == null)
                {
                    var windGo = new GameObject("SolarWindGust");
                    solarWind = windGo.AddComponent<SolarWindGust>();
                }
            }
        }

        private void SpawnDriftingAsteroid(Vector2 topPos)
        {
            var astGo = new GameObject("AsteroidDrifter");
            float dir = (Random.value > 0.5f) ? 1.0f : -1.0f;
            Vector2 spawnPos = new Vector2(-dir * 9.0f, topPos.y + Random.Range(2.0f, 4.5f));

            var drifter = astGo.AddComponent<AsteroidDrifter>();
            drifter.Initialize(spawnPos, dir, Random.Range(1.8f, 2.8f));

            // Visual representation
            var mf = astGo.AddComponent<MeshFilter>();
            var mr = astGo.AddComponent<MeshRenderer>();
            mf.sharedMesh = Visuals.ProceduralShapeGenerator.GetOrCreateMesh(ShapeType.Sphere, 0.45f);

            var mat = new Material(Visuals.ProceduralShapeGenerator.GetNeonShader());
            mat.SetColor("_BaseColor", new Color(0.6f, 0.2f, 0.2f, 0.8f));
            mat.SetColor("_RimColor", new Color(1.2f, 0.3f, 0.3f, 1f));
            mr.material = mat;
        }
    }
}
