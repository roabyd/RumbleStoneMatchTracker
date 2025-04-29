using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace StoneMatchTracker
{
    [RegisterTypeInIl2Cpp]
    public class VictoryAnimator : MonoBehaviour
    {
        float floatSpinSpeed = 90f;
        float hoverAmplitude = 0.01f;
        float hoverSpeed = 3f;
        public GameObject[] pebblePrefabs; // Array of pebble prefabs to instantiate

        public IEnumerator VictoryAnimation()
        {
            Vector3 startPos = transform.localPosition;
            Vector3 peakPos = startPos + new Vector3(0f, hoverAmplitude * 3, 0f); // rise to this point

            // Smoothly float up to peak
            float riseDuration = 0.5f;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / riseDuration;
                transform.localPosition = Vector3.Lerp(startPos, peakPos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            MelonCoroutines.Start(SummonOrbitingStones());
            // Begin hover loop from the peak
            float localTime = 0f;
            while (true)
            {
                try
                {
                    localTime += Time.deltaTime * hoverSpeed;
                    float yOffset = Mathf.Sin(localTime) * hoverAmplitude;
                    transform.localPosition = peakPos + new Vector3(0f, yOffset, 0f);
                    transform.Rotate(transform.parent.up, floatSpinSpeed * Time.deltaTime, Space.World);
                }
                catch (Exception e)
                {
                    //Adding to catch the error when the match ends and stop the animation
                    break;
                }

                yield return null;
            }
        }

        private IEnumerator SummonOrbitingStones()
        {
            GameObject auraRoot = new GameObject("StoneAura");
            auraRoot.transform.SetParent(transform, false);

            for (int i = 0; i < 10; i++)
            {
                GameObject pebble = ModResources.InstantiatePebble(auraRoot.transform);
                OrbitingPebble flyer = pebble.AddComponent<OrbitingPebble>();
                flyer.center = auraRoot.transform;
                flyer.orbitRadius = UnityEngine.Random.Range(0.015f, 0.025f);
                flyer.orbitSpeed = UnityEngine.Random.Range(50f, 80f);
                flyer.approachDuration = UnityEngine.Random.Range(0.8f, 1.8f);
            }
            yield break;
        }
    }
}
