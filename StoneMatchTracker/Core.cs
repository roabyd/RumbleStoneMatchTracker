using Il2CppRUMBLE.Managers;
using RumbleModdingAPI;
using MelonLoader;
using UnityEngine;
using MatchCounterFiles;
using System.Collections;
using System.Reflection;

namespace StoneMatchTracker
{
    public class Core : MelonMod
    {
        public StoneSummoner StoneSummoner;
        public GameObject SummoningStoneInstance;
        public GameObject StoneAttachPoint;
        public GameObject StoneSplitterObject;
        public StoneSplitter StoneSplitterScript;
        private string currentScene = "Loader";
        private int localPlayerRoundScore = 0;
        private int remotePlayerRoundScore = 0;
        private int localPlayerHealth = 0;
        private int remotePlayerHealth = 0;

        private MelonPreferences_Category modCategory;
        private MelonPreferences_Entry<bool> enableDebugging;

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
            // Load the resources if not loaded.
            

            if (Input.GetKeyDown(KeyCode.I)) // Detects when the I is pressed
            {
                GenerateStone();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                MelonCoroutines.Start(StoneSplitterScript.SplitStone(6));

            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                MelonCoroutines.Start(StoneSummoner.SummonStone());
            }
        }

        public override void OnLateInitializeMelon()
        {
            Calls.onRoundEnded += roundEnded;
            Calls.onMatchStarted += matchStarted;
            Calls.onMatchEnded += matchEnded;
        }


        private void matchEnded()
        {
            Logger.Msg("Match ended, may be able to stop animation here...");
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
                    Logger.Msg("Round ended, generating stone");
                    GenerateStone();
                    yield return new WaitForSeconds(2f);
                    yield return MelonCoroutines.Start(StoneSummoner.SummonStone());

                    if (localPlayerRoundScore == 0)
                    {
                        Logger.Msg("You Lost");
                        yield return new WaitForSeconds(0.5f);
                        MelonCoroutines.Start(StoneSplitterScript.SplitStone(UnityEngine.Random.Range(StoneSplitterScript.minPieces, StoneSplitterScript.maxPieces)));
                    }
                }
                else if (localPlayerRoundScore == remotePlayerRoundScore)
                {
                    yield return new WaitForSeconds(1.5f);
                    if (StoneSplitterScript.objectIsSplit)
                    {
                        Logger.Msg("You Won! Clawing it back...");
                        yield return MelonCoroutines.Start(StoneSplitterScript.ReturnShardsToOriginal());
                        yield return new WaitForSeconds(0.5f);
                        
                    }
                    yield return MelonCoroutines.Start(StoneSplitterScript.SplitStone(2));
                }
                else
                {
                    //Could animate this
                    Logger.Msg("Match Over");
                    if (StoneSplitterScript.objectIsSplit)
                    {
                        yield return MelonCoroutines.Start(StoneSplitterScript.ReturnShardsToOriginal());
                    }
                    yield return new WaitForSeconds(5f);
                    yield return MelonCoroutines.Start(StoneSplitterScript.DestoryCouterStone());
                }
            }
            
            yield break;
        }

        private void GenerateStone()
        {
            
            Vector3 stonePos;
            if (currentScene.Equals("Gym"))
            {
                
                stonePos = new Vector3(0, 0.18f, 0);
            }
            else
            {
                stonePos = new Vector3(UnityEngine.Random.Range(0f, 3f), 0.01f, UnityEngine.Random.Range(0f, 3f));
            }
            SummoningStoneInstance = ModResources.InstantiateStone(stonePos, Quaternion.identity);
           
            StoneSummoner = SummoningStoneInstance.GetComponent<StoneSummoner>();
            StoneSummoner.endPosition = CreateStoneAttachPoint();
            CreateStoneSpliiterObject();
        }

        private GameObject CreateStoneAttachPoint()
        {
            Transform playerRightHand = PlayerManager.instance.localPlayer.Controller.transform.GetChild(0)
                .GetChild(1).GetChild(0).GetChild(4).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0);

            StoneAttachPoint = new GameObject("StoneAttachPoint");
            StoneAttachPoint.transform.SetParent(playerRightHand, false);
            StoneAttachPoint.transform.localPosition = new Vector3(-0.03f, 0.08f, 0f);
            StoneAttachPoint.transform.localRotation = Quaternion.Euler(0, 0, 90);
            return StoneAttachPoint;
        }

        private void CreateStoneSpliiterObject()
        {
            if (StoneSplitterObject == null)
            {
                StoneSplitterObject = new GameObject("StoneSplitterObject");
                StoneSplitterScript = StoneSplitterObject.AddComponent<StoneSplitter>();
                StoneSplitterScript.objectToSlice = SummoningStoneInstance;
                StoneSplitterScript.cutMaterial = SummoningStoneInstance.GetComponent<Renderer>().material;
            }
        }

        private void UpdateMatchScore()
        {
            localPlayerHealth = Calls.Players.GetLocalPlayer().Data.HealthPoints;
            foreach (var remotePlayer in Calls.Players.GetEnemyPlayers())
            {
                remotePlayerHealth = remotePlayer.Data.HealthPoints;
            }
            Logger.Msg($"Local Player Health: {localPlayerHealth}, enemy {remotePlayerHealth}");
            if (PlayerIsInMatch())
            {
                if (localPlayerHealth > remotePlayerHealth)
                {
                    localPlayerRoundScore++;
                    Logger.Msg("You won a round!");
                }
                else if (localPlayerHealth < remotePlayerHealth)
                {
                    remotePlayerRoundScore++;
                    Logger.Msg("You lost a round...");
                }
                Logger.Msg($"Local Player score: {localPlayerRoundScore}, enemy {remotePlayerRoundScore}");
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
