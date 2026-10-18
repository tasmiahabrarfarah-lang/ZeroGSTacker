using UnityEngine;

namespace ZeroGStacker.Hazards
{
    public class AsteroidDrifter : MonoBehaviour
    {
        [SerializeField] private float speed = 1.8f;
        [SerializeField] private float rotSpeed = 45f;
        private float direction = 1f;

        public void Initialize(Vector2 pos, float dir, float spd)
        {
            transform.position = pos;
            direction = dir;
            speed = spd;
        }

        private void Update()
        {
            transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);
            transform.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x) > 12f)
            {
                Destroy(gameObject);
            }
        }
    }
}
