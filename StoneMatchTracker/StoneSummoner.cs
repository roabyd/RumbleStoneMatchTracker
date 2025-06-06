﻿using MelonLoader;
using StoneMatchTracker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatchCounterFiles
{
    [RegisterTypeInIl2Cpp]
    public class StoneSummoner : MonoBehaviour
    {
        public GameObject endPosition;
        public float upMoveDuration = 2f;
        private Rigidbody rb;
        public float stopDistance = 0.05f; // Distance threshold to stop pulling
        public float pullForce = 4f;   // Strength of the pull
        public bool Animating;
        private bool summoning;

        // Track this outside the function
        private float spinTime = 0f;
        private float totalSpinDuration = 1.5f; // How long it takes to spin down
        private int totalSpins = 3; // Number of full spins
        private Quaternion initialRotation;
        private Quaternion intendedRotation;
        private Vector3 initialLocalPosition;
        private float startTime;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            summoning = false;
            Animating = false;
            intendedRotation = transform.localRotation;
            startTime = Time.time; // mark the time when object is instantiated
        }

        public IEnumerator SummonStone()
        {
            Animating = true;
            // Start the summon coroutine and wait for it to finish the lift
            yield return MelonCoroutines.Start(LiftTheStone());

            // Then wait until the full summoning process (handled in Update) is done
            while (summoning)
            {
                yield return null;
            }
            Animating = false;
            Destroy(rb);
        }

        private IEnumerator LiftTheStone()
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            float elapsed = 0f;
            Vector3 startPosition = transform.position;
            Vector3 liftPosition = transform.position + new Vector3(0, 1.5f, 0); // Lift upwards by 1.5 units

            // Phase 1: Lift the object upwards
            while (elapsed < upMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / upMoveDuration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                transform.position = Vector3.Lerp(startPosition, liftPosition, smoothT);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f); // Pause before moving to the hand
            rb.isKinematic = false;
            // Phase 2: Gravitational pull toward the VR hand
            
            summoning = true;
        }

        void FixedUpdate()
        {
            float elapsed = Time.time - startTime;
            if (endPosition != null && summoning && !rb.isKinematic)
            {
                Vector3 directionToHand = (endPosition.transform.position + endPosition.transform.up * 0.1f) - transform.position;

                float distanceToHand = directionToHand.magnitude;

                if (distanceToHand > stopDistance && elapsed < 10)
                {
                    float dampingFactor = Mathf.Clamp(distanceToHand / 7f, 0.2f, 1f);
                    if (distanceToHand < 1f)
                    {
                        pullForce = 2.5f;
                    }
                    else
                    {
                        pullForce = 4;
                    }

                    rb.AddForce(directionToHand.normalized * pullForce * dampingFactor);
                }
                else
                {
                    rb.interpolation = RigidbodyInterpolation.None;
                    rb.isKinematic = true;
                    transform.parent = endPosition.transform;
                    initialLocalPosition = transform.localPosition;
                    initialRotation = transform.localRotation;
                    spinTime = 0f;
                }
            }
        }

        // In Update – smooth settling
        void Update()
        {
            if (summoning && rb.isKinematic)
            {               
                if (spinTime < totalSpinDuration)
                {
                    spinTime += Time.deltaTime;
                    float t = spinTime / totalSpinDuration;
                    float ease = Mathf.SmoothStep(0f, 1f, t);

                    transform.localPosition = Vector3.Lerp(initialLocalPosition, Vector3.zero, ease);
                    // Decreasing Y-axis spin
                    float currentSpinAngle = Mathf.Lerp(360f * totalSpins, 0f, ease);
                    Quaternion spin = Quaternion.Euler(0f, currentSpinAngle, 0f);

                    // Smooth rotation from initial rotation to identity
                    Quaternion alignment = Quaternion.Slerp(initialRotation, intendedRotation, ease);

                    // Combine spin (Y) with upright alignment
                    transform.localRotation = spin * alignment;
                }
                else
                {
                    transform.localRotation = intendedRotation;
                    transform.localPosition = Vector3.zero;
                    summoning = false;
                }
            }
        }
    }
}