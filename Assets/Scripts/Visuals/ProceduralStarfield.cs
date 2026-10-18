using UnityEngine;

namespace ZeroGStacker.Visuals
{
    public class ProceduralStarfield : MonoBehaviour
    {
        [Header("Layers Configuration")]
        [SerializeField] private int layerCount = 3;
        [SerializeField] private int starsPerLayer = 100;
        [SerializeField] private float fieldWidth = 14f;
        [SerializeField] private float fieldHeight = 24f;
        [SerializeField] private float[] layerSpeeds = new float[] { 0.15f, 0.4f, 0.85f };
        [SerializeField] private float[] layerScales = new float[] { 0.04f, 0.08f, 0.14f };

        private Transform mainCamera;
        private Vector3 lastCamPos;
        private ParticleSystem[] starLayers;

        private void Start()
        {
            if (Camera.main != null)
            {
                mainCamera = Camera.main.transform;
                lastCamPos = mainCamera.position;
            }

            BuildStarfieldLayers();
        }

        private void BuildStarfieldLayers()
        {
            starLayers = new ParticleSystem[layerCount];

            // Clean existing children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < layerCount; i++)
            {
                var layerGo = new GameObject($"StarLayer_{i}");
                layerGo.transform.SetParent(transform, false);

                var ps = layerGo.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.loop = true;
                main.playOnAwake = false;
                main.maxParticles = starsPerLayer;
                main.startLifetime = Mathf.Infinity;
                main.startSpeed = 0f;
                main.startSize = layerScales[Mathf.Min(i, layerScales.Length - 1)];
                main.simulationSpace = ParticleSystemSimulationSpace.Local;

                // Color tint per layer
                Color starCol = (i == 0) ? new Color(0.6f, 0.8f, 1f, 0.6f) : (i == 1 ? Color.white : new Color(1f, 0.95f, 0.8f, 0.9f));
                main.startColor = starCol;

                var emission = ps.emission;
                emission.rateOverTime = 0f;

                var shape = ps.shape;
                shape.enabled = false;

                var renderer = layerGo.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.sortingOrder = -100 + i;

                // Initialize random particles
                var particles = new ParticleSystem.Particle[starsPerLayer];
                for (int p = 0; p < starsPerLayer; p++)
                {
                    particles[p].position = new Vector3(
                        Random.Range(-fieldWidth * 0.5f, fieldWidth * 0.5f),
                        Random.Range(-fieldHeight * 0.5f, fieldHeight * 0.5f),
                        0f
                    );
                    particles[p].startSize = layerScales[Mathf.Min(i, layerScales.Length - 1)] * Random.Range(0.8f, 1.3f);
                    particles[p].startColor = starCol;
                    particles[p].remainingLifetime = Mathf.Infinity;
                }
                ps.SetParticles(particles, starsPerLayer);
                starLayers[i] = ps;
            }
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;

            Vector3 camDelta = mainCamera.position - lastCamPos;

            // Reposition root to follow camera roughly
            transform.position = new Vector3(mainCamera.position.x, mainCamera.position.y, 0f);

            for (int i = 0; i < starLayers.Length; i++)
            {
                if (starLayers[i] == null) continue;

                float speed = (i < layerSpeeds.Length) ? layerSpeeds[i] : 0.2f;
                var particles = new ParticleSystem.Particle[starsPerLayer];
                int count = starLayers[i].GetParticles(particles);

                for (int p = 0; p < count; p++)
                {
                    // Parallax offset
                    Vector3 pos = particles[p].position;
                    pos.x -= camDelta.x * speed;
                    pos.y -= camDelta.y * speed;

                    // Wrap horizontally
                    if (pos.x < -fieldWidth * 0.5f) pos.x += fieldWidth;
                    else if (pos.x > fieldWidth * 0.5f) pos.x -= fieldWidth;

                    // Wrap vertically
                    if (pos.y < -fieldHeight * 0.5f) pos.y += fieldHeight;
                    else if (pos.y > fieldHeight * 0.5f) pos.y -= fieldHeight;

                    particles[p].position = pos;
                }

                starLayers[i].SetParticles(particles, count);
            }

            lastCamPos = mainCamera.position;
        }
    }
}
