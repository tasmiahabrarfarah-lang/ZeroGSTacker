using System.Collections;
using UnityEngine;

namespace ZeroGStacker.Visuals
{
    public class PulseVFX : MonoBehaviour
    {
        [Header("Beam Line Renderer")]
        [SerializeField] private LineRenderer pulseBeam;

        [Header("Burst Particle System")]
        [SerializeField] private ParticleSystem burstParticles;

        private void Awake()
        {
            if (pulseBeam == null)
            {
                var beamGo = new GameObject("PulseBeamRenderer");
                beamGo.transform.SetParent(transform, false);
                pulseBeam = beamGo.AddComponent<LineRenderer>();
                pulseBeam.positionCount = 2;
                pulseBeam.startWidth = 0.18f;
                pulseBeam.endWidth = 0.04f;
                pulseBeam.useWorldSpace = true;
                pulseBeam.enabled = false;

                // Additive-like glowing material
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                var mat = new Material(shader);
                pulseBeam.material = mat;
            }

            if (burstParticles == null)
            {
                var burstGo = new GameObject("ImpactBurstParticles");
                burstGo.transform.SetParent(transform, false);
                burstParticles = burstGo.AddComponent<ParticleSystem>();

                var main = burstParticles.main;
                main.loop = false;
                main.playOnAwake = false;
                main.startLifetime = 0.45f;
                main.startSpeed = 5.0f;
                main.startSize = 0.16f;
                main.maxParticles = 80;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var emission = burstParticles.emission;
                emission.rateOverTime = 0f;

                var shape = burstParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.2f;

                var colorOverLife = burstParticles.colorOverLifetime;
                colorOverLife.enabled = true;
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.cyan, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                colorOverLife.color = grad;

                var renderer = burstGo.GetComponent<ParticleSystemRenderer>();
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.sortingOrder = 10;
            }
        }

        public void ShowBeam(Vector2 startPos, Vector2 endPos, Color beamColor)
        {
            if (pulseBeam != null)
            {
                pulseBeam.enabled = true;
                pulseBeam.startColor = beamColor;
                pulseBeam.endColor = Color.white;
                pulseBeam.SetPosition(0, startPos);
                pulseBeam.SetPosition(1, endPos);
            }
        }

        public void HideBeam()
        {
            if (pulseBeam != null)
            {
                pulseBeam.enabled = false;
            }
        }

        public void PlayImpactBurst(Vector2 position, Color color, bool isPerfect = false)
        {
            if (burstParticles != null)
            {
                burstParticles.transform.position = position;
                var main = burstParticles.main;
                main.startColor = isPerfect ? Color.white : color;
                main.startSpeed = isPerfect ? 7.5f : 4.5f;

                int burstCount = isPerfect ? 45 : 20;
                burstParticles.Emit(burstCount);
            }
        }
    }
}
