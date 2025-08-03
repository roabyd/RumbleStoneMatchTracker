using MatchCounterFiles;
using MelonLoader;
using RumbleModdingAPI;
using UnityEngine;


namespace StoneMatchTracker
{
    internal class ModResources
    {
        public static AssetBundle Bundle;

        public static AudioClip Sound1;
        public static AudioClip Sound2;
        public static AudioClip Sound3;
        public static AudioClip Sound4;
        public static AudioClip ReverseStone;
        public static AudioClip ExplodeStone;
        private static List<string> stones;

        private static bool initialized = false;
        public static bool Initialized { get { return initialized; } }

        public static void LoadResources(bool reload = false)
        { 
            if (initialized && !reload) return;

            Bundle = Calls.LoadAssetBundleFromFile(@"UserData/stonetrackerbundle");

            LoadSoundFiles();

            stones = new List<string> { "Stone1", "Stone2", "Stone3", "Stone4", "Stone5" };
            initialized = true;
        }

        public static void LoadSoundFiles()
        {
            Sound1 = Bundle.LoadAsset<AudioClip>("assets/bundledassests/stonetrackerbundle/lighthit.wav");
            Sound2 = Bundle.LoadAsset<AudioClip>("assets/bundledassests/stonetrackerbundle/ricochet.wav");
            Sound3 = Bundle.LoadAsset<AudioClip>("assets/bundledassests/stonetrackerbundle/stonestep_big01.wav");
            Sound4 = Bundle.LoadAsset<AudioClip>("assets/bundledassests/stonetrackerbundle/structure_unground.wav");
            ReverseStone = Bundle.LoadAsset<AudioClip>("assets/bundledassests/stonetrackerbundle/ReverseStone.wav");
            ExplodeStone = Bundle.LoadAsset<AudioClip>("assets/bundledassests/stonetrackerbundle/ExplodeStone.wav");
        }

        public static GameObject InstantiateStone(Vector3 position)
        {           
            int randomStoneIndex = UnityEngine.Random.Range(0, stones.Count);
            GameObject stoneCounterObject = GameObject.Instantiate(Bundle.LoadAsset<GameObject>(stones[randomStoneIndex]));
            stoneCounterObject.transform.position = position;
            if (!stoneCounterObject.TryGetComponent<StoneSummoner>(out var ss))
            {
                stoneCounterObject.AddComponent<StoneSummoner>();
            }
            stoneCounterObject.AddComponent<VictoryAnimator>();
            stoneCounterObject.name = "TrackerStone";
            return stoneCounterObject;
        }

        public static GameObject InstantiatePebble(Transform parent)
        {
            int randomStoneIndex = UnityEngine.Random.Range(0, stones.Count);
            GameObject pebble = GameObject.Instantiate(Bundle.LoadAsset<GameObject>(stones[randomStoneIndex]), parent);
            if (pebble.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                GameObject.Destroy(rb);
            }
            if (pebble.TryGetComponent<StoneSummoner>(out StoneSummoner ss))
            {
                GameObject.Destroy(ss);
            }
            return pebble;
        }
    }

}
