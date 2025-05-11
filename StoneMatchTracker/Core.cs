using Il2CppRUMBLE.Managers;
using RumbleModdingAPI;
using MelonLoader;
using UnityEngine;
using MatchCounterFiles;
using System.Collections;

namespace StoneMatchTracker
{
    public class Core : MelonMod
    {
        public static GameObject TrackerStone;
        public static GameObject StoneAttachPoint;
        public GameObject StoneSplitterObject;
        public StoneSplitter StoneSplitterScript;
        private string currentScene = "Loader";
        private int localPlayerRoundScore = 0;
        private int remotePlayerRoundScore = 0;
        private int localPlayerHealth = 0;
        private int remotePlayerHealth = 0;

        // Gauntlet offsets for different gauntlet types. These ensure that the tracker stone
        // is positioned correctly on the player's gauntlet.
        private static Vector3 shiftStoneStrapOffset = new Vector3(-0.03f, 0.1f, -0.008f);
        private static Vector3 allRounderOffset = new Vector3(-0.042f, 0.11f, -0.008f);
        private static Vector3 devotedBracerOffset = new Vector3(-0.035f, 0.09f, -0.008f);
        private static Vector3 allRounderAltOffset = new Vector3(-0.042f, 0.11f, -0.008f);
        private static Vector3 wristbandOffset = new Vector3(-0.03f, 0.08f, -0.008f);
        private static List<Vector3> gauntletOffsets = new List<Vector3>
        {
            shiftStoneStrapOffset,
            allRounderOffset,
            devotedBracerOffset,
            allRounderAltOffset,
            wristbandOffset
        };

        public static MelonLogger.Instance Logger { get; private set; }

        public override void OnInitializeMelon()
        {
            Logger = LoggerInstance;
            LoggerInstance.Msg("StoneMatchTracker: Initialized.");

        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;

            if (!ModResources.Initialized)
            {
                ModResources.LoadResources();
            }
        }

        public override void OnUpdate()
        {
            // Testing purposes only
            //if (Input.GetKeyDown(KeyCode.I)) 
            //{
            //    GenerateTrackerStone();
            //}

            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    MelonCoroutines.Start(StoneSplitterScript.SplitStone(6));
            //}

            //if (Input.GetKeyDown(KeyCode.H))
            //{
            //    MelonCoroutines.Start(StoneSplitterScript.SplitStone(2));
            //}

            //if (Input.GetKeyDown(KeyCode.J))
            //{
            //    MelonCoroutines.Start(StoneSplitterScript.ReturnShardsToOriginal());              
            //}

            //if (Input.GetKeyDown(KeyCode.M))
            //{
            //    MelonCoroutines.Start(TrackerStone.GetComponent<StoneSummoner>().SummonStone());
            //}

            //if (Input.GetKeyDown(KeyCode.W))
            //{
            //    TrackerStone = StoneSplitterScript.objectToSlice;
            //    if (TrackerStone == null)
            //    {
            //        Logger.Error("TrackerStone is null!");
            //        return;
            //    }
            //    else if (TrackerStone.TryGetComponent<VictoryAnimator>(out var anim))
            //    {
            //        MelonCoroutines.Start(anim.VictoryAnimation());
            //    }
            //    else
            //    { 
            //       Logger.Error("TrackerStone does not have a VictoryAnimator component!");
            //    }
            //}

            //if (Input.GetKeyDown(KeyCode.L))
            //{
            //    StoneSplitterScript.PlayLossAnimation();
            //}
        }

        public override void OnLateInitializeMelon()
        {
            Calls.onRoundEnded += roundEnded;
            Calls.onMatchStarted += matchStarted;
        }

        private void roundEnded()
        {          
            MelonCoroutines.Start(RoundEnded());
        }

        private IEnumerator RoundEnded()
        {
            UpdateMatchScore();
            if (localPlayerRoundScore > 0 || remotePlayerRoundScore > 0)
            {
                if (localPlayerRoundScore + remotePlayerRoundScore == 1)
                {
                    GenerateTrackerStone();
                    yield return new WaitForSeconds(1f);
                    yield return MelonCoroutines.Start(TrackerStone.GetComponent<StoneSummoner>().SummonStone());

                    if (localPlayerRoundScore == 0)
                    {
                        yield return new WaitForSeconds(0.5f);
                        yield return MelonCoroutines.Start(StoneSplitterScript.SplitStone(UnityEngine.Random.Range(StoneSplitterScript.minPieces, StoneSplitterScript.maxPieces)));
                    }
                }
                else if (localPlayerRoundScore == remotePlayerRoundScore)
                {
                    yield return new WaitForSeconds(4f);
                    if (StoneSplitterScript.objectIsSplit)
                    {
                        yield return MelonCoroutines.Start(StoneSplitterScript.ReturnShardsToOriginal());
                        TrackerStone = StoneSplitterScript.objectToSlice;
                        yield return new WaitForSeconds(0.5f);
                        
                    }
                    yield return MelonCoroutines.Start(StoneSplitterScript.SplitStone(2));
                }
                else
                {
                    yield return new WaitForSeconds(4f);
                    if (StoneSplitterScript.objectIsSplit)
                    {
                        yield return MelonCoroutines.Start(StoneSplitterScript.ReturnShardsToOriginal());
                        TrackerStone = StoneSplitterScript.objectToSlice;
                    }
                    yield return new WaitForSeconds(2f);
                    if (localPlayerRoundScore > remotePlayerRoundScore)
                    {
                        //Play victory animation
                        MelonCoroutines.Start(TrackerStone.GetComponent<VictoryAnimator>().VictoryAnimation());
                    }
                    else
                    {
                        //Explode stone for a loss
                        StoneSplitterScript.PlayLossAnimation();
                    }
                }
            }
        }

        private void GenerateTrackerStone()
        {         
            Vector3 stonePos;
            if (currentScene.Equals("Gym"))
            {
                
                stonePos = new Vector3(0, 0.18f, 0);
            }
            else
            {
                stonePos = new Vector3(UnityEngine.Random.Range(0f, 2f), 0.01f, UnityEngine.Random.Range(0f, 2f));
            }
            TrackerStone = ModResources.InstantiateStone(stonePos);
           
            TrackerStone.GetComponent<StoneSummoner>().endPosition = CreateStoneAttachPoint();
            CreateStoneSpliterObject();
        }

        private GameObject CreateStoneAttachPoint()
        {
            if (StoneAttachPoint != null)
            {
                GameObject.Destroy(StoneAttachPoint);
            }
            Transform playerRightHand = PlayerManager.instance.localPlayer.Controller.transform.GetChild(0)
                .GetChild(1).GetChild(0).GetChild(4).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0);

            int rightGauntlet = PlayerManager.instance.AllPlayers[0].Data.visualData.CustomizationPartIndexes[5];

            StoneAttachPoint = new GameObject("StoneAttachPoint");
            StoneAttachPoint.transform.SetParent(playerRightHand, false);
            StoneAttachPoint.transform.localPosition = gauntletOffsets[rightGauntlet];
            StoneAttachPoint.transform.localRotation = Quaternion.Euler(0, 0, 90);

            AudioSource audioSource = StoneAttachPoint.AddComponent<AudioSource>();
            if (ModResources.Sound1 == null)
            {
                ModResources.LoadSoundFiles();
            }
            audioSource.clip = ModResources.Sound1;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.minDistance = 0.1f;
            audioSource.maxDistance = 2f;
            audioSource.volume = 0.6f;
            audioSource.loop = false;

            return StoneAttachPoint;
        }

        private void CreateStoneSpliterObject()
        {
            if (StoneSplitterObject == null)
            {
                StoneSplitterObject = new GameObject("StoneSplitterObject");
                StoneSplitterScript = StoneSplitterObject.AddComponent<StoneSplitter>();
            }
            StoneSplitterScript.objectToSlice = TrackerStone;
            StoneSplitterScript.cutMaterial = TrackerStone.GetComponent<Renderer>().material;
            if (ModResources.ExplodeStone == null)
            {
                ModResources.LoadSoundFiles();
            }
            StoneSplitterScript.explodeSound = ModResources.ExplodeStone;
        }

        private void UpdateMatchScore()
        {
            localPlayerHealth = Calls.Players.GetLocalPlayer().Data.HealthPoints;
            foreach (var remotePlayer in Calls.Players.GetEnemyPlayers())
            {
                remotePlayerHealth = remotePlayer.Data.HealthPoints;
            }
            if (PlayerIsInMatch())
            {
                if (localPlayerHealth > remotePlayerHealth)
                {
                    localPlayerRoundScore++;
                }
                else if (localPlayerHealth < remotePlayerHealth)
                {
                    remotePlayerRoundScore++;
                }
            }
        }

        private void ResetMatchScore()
        {
            localPlayerRoundScore = 0;
            remotePlayerRoundScore = 0;
        }

        private void matchStarted()
        {
            ResetMatchScore();
        }

        private bool PlayerIsInMatch()
        {
            return (currentScene.Equals("Map1") || currentScene.Equals("Map0")) && Calls.Players.GetEnemyPlayers().Count == 1;
        }
    }
}
