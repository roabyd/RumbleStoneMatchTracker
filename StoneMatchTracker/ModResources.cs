using MatchCounterFiles;
using UnityEngine;

namespace StoneMatchTracker
{
    internal class ModResources
    {
        public static Il2CppAssetBundle Bundle;

        public static GameObject StoneCounterObject;
        public static Material StoneMat;

        private static bool initialized = false;
        public static bool Initialized { get { return initialized; } }


        //public static readonly Color HeartLowColor = new Color(164f / 255, 245f / 255, 130f / 255);
        //public static readonly Color HeartMediumColor = new Color(255f / 255, 180f / 255, 0f / 255);
        //public static readonly Color HeartHighColor = new Color(255f / 255, 0f / 255, 0f / 255);
        //public static readonly Color HeartDefaultColor = new Color(255f / 255, 255f / 255, 255f / 255);

        public static void LoadResources(bool reload = false)
        { 
            if (initialized && !reload) return;

            Bundle = Il2CppAssetBundleManager.LoadFromFile(@"UserData/stonetrackerbundle");
            initialized = true;
        }

        public static GameObject InstantiateStone(Vector3 position, Quaternion rotation)
        {
            if (StoneCounterObject != null)
            {
                GameObject.Destroy(StoneCounterObject);
            }
            StoneCounterObject = GameObject.Instantiate(Bundle.LoadAsset<GameObject>("Stone1"), position, rotation);
            if (StoneCounterObject.GetComponent<StoneSummoner>() == null)
            {
                StoneCounterObject.AddComponent<StoneSummoner>();
                Core.Logger.Msg("StoneSummoner component added to StoneCounterObject.");
            }
            return StoneCounterObject;
        }
    }

}
