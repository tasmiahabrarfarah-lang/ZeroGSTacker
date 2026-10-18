using UnityEngine;

namespace ZeroGStacker.Hazards
{
    public class SolarWindGust : MonoBehaviour
    {
        [SerializeField] private float windForce = 3.5f;
        [SerializeField] private float gustInterval = 4.0f;
        [SerializeField] private float gustDuration = 1.5f;

        private float timer = 0f;
        private bool isBlowing = false;
        private float direction = 1.0f;

        public bool IsBlowing => isBlowing;
        public float Direction => direction;
        public float CurrentWindForce => isBlowing ? windForce * direction : 0f;

        private void Update()
        {
            timer += Time.deltaTime;
            if (!isBlowing && timer >= gustInterval)
            {
                isBlowing = true;
                timer = 0f;
                direction = (Random.value > 0.5f) ? 1.0f : -1.0f;
            }
            else if (isBlowing && timer >= gustDuration)
            {
                isBlowing = false;
                timer = 0f;
            }
        }
    }
}
