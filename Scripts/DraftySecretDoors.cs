using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace DraftySecretDoors
{
    public class DraftySecretDoors : MonoBehaviour
    {
        public static Mod mod;

        float volume = 1.0f;
        float minDist = 2.5f;
        float maxDist = 12.0f;
        float pitch = 1.0f;

        void Start()
        {
            ModSettings settings = mod.GetSettings();

            volume = settings.GetValue<float>("Settings", "Volume");
            minDist = settings.GetValue<float>("Settings", "MinVolumeDistance");
            maxDist = settings.GetValue<float>("Settings", "MaxVolumeDistance");
            pitch = settings.GetValue<float>("Settings", "Pitch");

            PlayerEnterExit.OnTransitionDungeonInterior += PlayerEnterExit_OnTransitionDungeonInterior;
        }

        void PlayerEnterExit_OnTransitionDungeonInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            CreateAudio();
        }

        void CreateAudio()
        {
            DaggerfallActionDoor[] actionDoors = FindObjectsOfType<DaggerfallActionDoor>();

            if (actionDoors != null)
            {
                foreach (DaggerfallActionDoor actionDoor in actionDoors)
                {
                    string meshFilterName = actionDoor.GetComponent<MeshFilter>().name;

                    if (!meshFilterName.Contains("55000") && !meshFilterName.Contains("55001") && !meshFilterName.Contains("55002") && !meshFilterName.Contains("55003") &&
                        !meshFilterName.Contains("55004") && !meshFilterName.Contains("55005"))
                    {
                        // Secret door

                        DaggerfallAudioSource daggerfallAudioSource = new GameObject("SecretDoorAudio").AddComponent<DaggerfallAudioSource>();
                        daggerfallAudioSource.transform.position = actionDoor.transform.position;
                        daggerfallAudioSource.transform.parent = actionDoor.transform;

                        daggerfallAudioSource.SetSound(SoundClips.AmbientWindBlow1b, AudioPresets.LoopIfPlayerNear);
                        daggerfallAudioSource.AudioSource.rolloffMode = AudioRolloffMode.Linear;
                        daggerfallAudioSource.AudioSource.minDistance = minDist;
                        daggerfallAudioSource.AudioSource.maxDistance = maxDist;
                        daggerfallAudioSource.AudioSource.volume = volume;
                        daggerfallAudioSource.AudioSource.pitch = pitch;
                    }
                }
            }
        }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            new GameObject("DraftySecretDoors").AddComponent<DraftySecretDoors>();

            ModManager.Instance.GetMod(initParams.ModTitle).IsReady = true;
        }
    }
}