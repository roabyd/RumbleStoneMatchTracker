using MelonLoader;
using UnityEngine;

namespace StoneMatchTracker
{
    [RegisterTypeInIl2Cpp]
    public class RumbleEffect : MonoBehaviour
    {
        public float rumbleDuration = 2f;
        public float maxIntensity = 0.5f;
        public AnimationCurve intensityOverTime;

        private Vector3 originalPosition;
        private float timer = 0f;
        private bool isRumbling = false;

        public System.Action onExplode; // Hook to plug in your own explosion logic

        public void StartRumble()
        {
            if (!isRumbling)
            {
                originalPosition = transform.localPosition;
                timer = 0f;
                isRumbling = true;
            }
        }

        private void Update()
        {
            if (!isRumbling) return;

            timer += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(timer / rumbleDuration);
            float currentIntensity = intensityOverTime.Evaluate(normalizedTime) * maxIntensity;

            transform.localPosition = originalPosition + UnityEngine.Random.insideUnitSphere * currentIntensity;

            if (timer >= rumbleDuration)
            {
                isRumbling = false;
                transform.localPosition = originalPosition;

                onExplode?.Invoke(); // Trigger your custom explosion logic
            }
        }


    }

}
