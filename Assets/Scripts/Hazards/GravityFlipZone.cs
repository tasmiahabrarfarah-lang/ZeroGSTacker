using UnityEngine;

namespace ZeroGStacker.Hazards
{
    public class GravityFlipZone : MonoBehaviour
    {
        [SerializeField] private float zoneHeight = 4.0f;
        [SerializeField] private float zoneWidth = 14.0f;
        [SerializeField] private float verticalForce = 2.5f;

        public float ZoneHeight => zoneHeight;
        public float VerticalForce => verticalForce;

        public bool Contains(Vector2 pos)
        {
            Vector2 local = (Vector2)transform.position;
            return Mathf.Abs(pos.x - local.x) <= zoneWidth * 0.5f &&
                   Mathf.Abs(pos.y - local.y) <= zoneHeight * 0.5f;
        }
    }
}
