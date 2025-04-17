using System.Collections;
using UnityEngine;
using EzySlice;
using MelonLoader;

namespace StoneMatchTracker
{
    [RegisterTypeInIl2Cpp]
    public class StoneSplitter : MonoBehaviour
    {
        public GameObject objectToSlice; // The object to slice
        public Material cutMaterial; // Material for sliced surfaces
        public int minPieces = 5;
        public int maxPieces = 9;
        public float separationDistance = 0.005f; // Separation in meters (5 mm)
        public float splitDuration = 1f; // Duration of outward and return movement in seconds
        public bool objectIsSplit = false;

        private string backupObjectName;
        private List<GameObject> pieces = new List<GameObject>();
        private Vector3 objectCenter;
        private GameObject backupObject; // Backup of the original object
        private Dictionary<GameObject, Vector3> initialShardPositions = new Dictionary<GameObject, Vector3>();
        private Transform originalParent;

        public IEnumerator SplitStone(int numberOfSlices)
        {
            if (objectToSlice != null)
            {
                pieces = new List<GameObject> { objectToSlice };
                objectCenter = objectToSlice.GetComponent<MeshRenderer>().bounds.center;

                // Create a backup copy of the original object
                originalParent = objectToSlice.transform.parent;
                backupObject = Instantiate(objectToSlice, originalParent);
                backupObject.transform.localPosition = objectToSlice.transform.localPosition;
                backupObject.transform.localRotation = objectToSlice.transform.localRotation;
                backupObject.SetActive(false); // Hide the backup copy
                backupObjectName = objectToSlice.name;

                // Disable the original object for visual purposes
                objectToSlice.SetActive(false);

                for (int i = 0; i < numberOfSlices - 1; i++)
                {
                    GameObject pieceToSlice = pieces[0];
                    float baseAngle = (360f / numberOfSlices) * i;

                    float finalAngle = baseAngle + UnityEngine.Random.Range(-15f, 15f);

                    // Create the direction vector in local space
                    Vector3 randomDirectionLocal = Quaternion.AngleAxis(finalAngle, Vector3.up) * Vector3.forward;

                    // Transform the direction vector into world space
                    Vector3 randomDirectionWorld = pieceToSlice.transform.TransformDirection(randomDirectionLocal);

                    // Use the object's world position as the center
                    Vector3 worldCenter = pieceToSlice.transform.position;
                    SlicedHull slicedHull = pieceToSlice.Slice(worldCenter, worldCenter + randomDirectionWorld.normalized * 5f, cutMaterial);

                    if (slicedHull != null)
                    {
                        GameObject upperHull = slicedHull.CreateUpperHull(pieceToSlice, cutMaterial);
                        upperHull.transform.SetParent(pieceToSlice.transform.parent, false);
                        GameObject lowerHull = slicedHull.CreateLowerHull(pieceToSlice, cutMaterial);
                        lowerHull.transform.SetParent(pieceToSlice.transform.parent, false);

                        pieces.Add(upperHull);
                        pieces.Add(lowerHull);
                        pieces.Remove(pieceToSlice);
                        Destroy(pieceToSlice);
                    }
                }

                yield return MelonCoroutines.Start(MovePiecesOutward());
            }
            else
            {
                Core.Logger.Msg("Object to slice is not assigned!");
            }
        }

        public IEnumerator ReturnShardsToOriginal()
        {
            yield return MelonCoroutines.Start(MovePiecesInwardAndReplace());
        }

        public IEnumerator DestoryCouterStone()
        {
            GameObject.Destroy(objectToSlice);
            yield return null;
        }

        private IEnumerator MovePiecesOutward()
        {
            float elapsedTime = 0f;
            Dictionary<GameObject, Vector3> initialPositions = new Dictionary<GameObject, Vector3>();
            Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();

            // Calculate initial and target positions in local space
            foreach (GameObject piece in pieces)
            {
                // Local center of the piece
                Vector3 pieceCenter = piece.GetComponent<MeshRenderer>().bounds.center;
                Vector3 outwardDirection = (pieceCenter - objectCenter).normalized;

                // Convert to local space relative to the parent
                outwardDirection = piece.transform.parent.InverseTransformDirection(outwardDirection);

                // Store initial and target positions in local space
                initialPositions[piece] = piece.transform.localPosition;
                initialShardPositions[piece] = piece.transform.localPosition;
                targetPositions[piece] = piece.transform.localPosition + outwardDirection * separationDistance;
            }

            // Animate movement
            while (elapsedTime < splitDuration)
            {
                float t = elapsedTime / splitDuration;
                float easedT = 1 - Mathf.Pow(1 - t, 25);

                foreach (GameObject piece in pieces)
                {
                    if (piece != null)
                    {
                        // Lerp between initial and target positions in local space
                        Vector3 initialPosition = initialPositions[piece];
                        Vector3 targetPosition = targetPositions[piece];
                        piece.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, easedT);
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Set final positions
            foreach (GameObject piece in pieces)
            {
                if (piece != null)
                {
                    piece.transform.localPosition = targetPositions[piece];
                }
            }
            objectIsSplit = true; // Mark the object as split
        }

        private IEnumerator MovePiecesInwardAndReplace()
        {
            float elapsedTime = 0f;

            // Save the initial positions of all shards in local space
            Dictionary<GameObject, Vector3> splitPositions = new Dictionary<GameObject, Vector3>();
            foreach (GameObject piece in pieces)
            {
                if (piece != null)
                {
                    splitPositions[piece] = piece.transform.localPosition; // Use local position
                }
            }

            // Animate shards moving inward
            while (elapsedTime < splitDuration)
            {
                float t = elapsedTime / splitDuration;
                float easedT = 1 - Mathf.Pow(1 - t, 2); // Ease-out animation curve

                foreach (GameObject piece in pieces)
                {
                    if (piece != null)
                    {
                        Vector3 initialPosition = splitPositions[piece];
                        Vector3 targetPosition = initialShardPositions[piece]; // Target local position
                        piece.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, easedT); // Use local position
                    }
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Snap shards precisely back into place and destroy them
            foreach (GameObject piece in pieces)
            {
                if (piece != null)
                {
                    Destroy(piece); // Safely destroy the shard
                }
            }
            pieces.Clear(); // Clear the list of shards

            // Restore the backup object to its exact original transform
            if (backupObject != null)
            {
                Core.Logger.Msg("Restoring backup to parent: " + originalParent.name);
                Core.Logger.Msg("Backup object local pos BEFORE restore: " + backupObject.transform.localPosition);
                objectToSlice = backupObject; // Restore the reference to the original object
                objectToSlice.transform.SetParent(originalParent, false);
                objectToSlice.transform.localPosition = Vector3.zero;
                objectToSlice.transform.localRotation = Quaternion.identity;
                objectToSlice.SetActive(true); // Re-enable the backup object
                Core.Logger.Msg("objectToSlice local pos AFTER restore: " + objectToSlice.transform.localPosition);
                objectToSlice.name = backupObjectName; // Restore the original name
            }
            else
            {
                Core.Logger.Msg("Backup object is missing!");
            }
        }
    }
}
