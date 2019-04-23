using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Entity;
using System.Collections.Generic;
using System;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallConnect;

namespace DraftySecretDoors
{
    public class DraftySecretDoors : MonoBehaviour
    {
        public static Mod mod;

        private PlayerEnterExit playerEnterExit;
        private DaggerfallActionDoor[] actionDoors;

        private string currentLocationName = null;

        private float volume = 1.0f;
		private float minDist = 2.5f;
		private float maxDist = 12.0f;

        private void Start()
        {
            ModSettings settings = mod.GetSettings();

            volume  = settings.GetValue<float>("Settings", "Volume");
            minDist = settings.GetValue<float>("Settings", "MinVolumeDistance");
            maxDist = settings.GetValue<float>("Settings", "MaxVolumeDistance");

            playerEnterExit = GameManager.Instance.PlayerEnterExit;

            SaveLoadManager.OnLoad += (saveData) => { currentLocationName = null; };
            StartGameBehaviour.OnNewGame += () => { currentLocationName = null; };
        }

        void Update()
        {
            if (!GameManager.Instance.IsPlayerInsideDungeon && currentLocationName != null)
            {
                actionDoors = null;
                currentLocationName = null;
            }
            else if (GameManager.Instance.IsPlayerInsideDungeon && playerEnterExit.Dungeon.Summary.LocationName != currentLocationName)
            {
                currentLocationName = playerEnterExit.Dungeon.Summary.LocationName;
                GetDoors();
            }
        }

        private void GetDoors()
        {
            actionDoors = FindObjectsOfType<DaggerfallActionDoor>();

            if (actionDoors != null)
            {
                for (int i = 0; i < actionDoors.Length; i++)
                {
                    string meshFilterName = actionDoors[i].GetComponent<MeshFilter>().name;

                    if (meshFilterName.Contains("55000") || meshFilterName.Contains("55001") || meshFilterName.Contains("55002") || meshFilterName.Contains("55003") ||
                        meshFilterName.Contains("55004") || meshFilterName.Contains("55005"))
                    {
                        // Normal door
                        continue;
                    }
                    else
                    {
                        // Secret door

                        GameObject audioSource = Instantiate(mod.GetAsset<GameObject>("SecretDoorAudio.prefab"), actionDoors[i].transform);
                        DaggerfallAudioSource secretDoorAudio = audioSource.GetComponent<DaggerfallAudioSource>();
                        secretDoorAudio.SetSound(72, AudioPresets.LoopIfPlayerNear);
                        secretDoorAudio.AudioSource.rolloffMode = AudioRolloffMode.Linear;
                        secretDoorAudio.AudioSource.minDistance = minDist;
                        secretDoorAudio.AudioSource.maxDistance = maxDist;
                        secretDoorAudio.AudioSource.volume = volume;                  
                    }
                }
            }
        }

        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            GameObject dsd = new GameObject("DraftySecretDoors");
            dsd.AddComponent<DraftySecretDoors>();

            //after finishing, set the mod's IsReady flag to true.
            ModManager.Instance.GetMod(initParams.ModTitle).IsReady = true;
        }
    }
}