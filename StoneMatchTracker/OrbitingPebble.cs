using MelonLoader;
using UnityEngine;

namespace StoneMatchTracker
{
    [RegisterTypeInIl2Cpp]
    public class OrbitingPebble : MonoBehaviour
    {
        public Transform center;
        public float orbitRadius = 0.2f;
        public float orbitSpeed = 60f;
        public float approachDuration = 1.5f;

        private Vector3 startOffset;
        private float angleOffset;
        private float arrivalTime;
        private bool arrived = false;

        void Start()
        {
            // Set random radial direction and distance to start from
            Vector3 randomDir = UnityEngine.Random.onUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y); // bias to above ground
            startOffset = randomDir.normalized * UnityEngine.Random.Range(1.5f, 2.5f);

            transform.position = center.position + startOffset;
            angleOffset = UnityEngine.Random.Range(0f, 360f);
            arrivalTime = Time.time + approachDuration;
            transform.localScale = Vector3.one * UnityEngine.Random.Range(0.07f, 0.2f); // Randomize scale
        }

        void Update()
        {
            if (center == null) return;

            if (!arrived)
            {
                float t = 1f - Mathf.Clamp01((arrivalTime - Time.time) / approachDuration);
                transform.position = Vector3.Lerp(center.position + startOffset, center.position + GetOrbitOffset(), EaseOut(t));

                if (t >= 1f)
                {
                    arrived = true;
                }
            }
            else
            {
                transform.position = center.position + GetOrbitOffset();
                transform.rotation = Quaternion.LookRotation(center.position - transform.position);
            }
        }

        Vector3 GetOrbitOffset()
        {
            float angle = angleOffset + Time.time * orbitSpeed;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
            offset.y = Mathf.Sin(Time.time * 2f + angleOffset) * 0.01f;
            return offset;
        }

        float EaseOut(float t) => 1 - Mathf.Pow(1 - t, 3); // smooth deceleration
    }
}
