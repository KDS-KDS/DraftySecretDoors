using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Serialization;

namespace DraftySecretDoors
{
    public class DraftySecretDoors : MonoBehaviour
    {
        public static Mod mod;

        private DaggerfallActionDoor[] actionDoors;
        private bool getDoors = false;

        private void Start()
        {
            SaveLoadManager.OnLoad += (saveData) => { getDoors = false; };
        }

        void Update()
        {
            if (!GameManager.Instance.IsPlayerInsideDungeon && getDoors)
            {
                actionDoors = null;
                getDoors = false;
            }
            else if (GameManager.Instance.IsPlayerInsideDungeon && !getDoors)
            {
                GetDoors();
                getDoors = true;
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
                        //Debug.Log("Normal Door");
                        continue;
                    }
                    else
                    {
                        //Debug.Log("Secret Door");

                        GameObject audioSource = Instantiate(mod.GetAsset<GameObject>("SecretDoorAudio.prefab"), actionDoors[i].transform);
                        DaggerfallAudioSource secretDoorAudio = audioSource.GetComponent<DaggerfallAudioSource>();
                        secretDoorAudio.SetSound(70, AudioPresets.LoopIfPlayerNear);
                        secretDoorAudio.AudioSource.minDistance = 1.0f;
                        secretDoorAudio.AudioSource.maxDistance = 12.0f;
                        secretDoorAudio.AudioSource.volume = 0.8f;  
                    }
                }
            }
        }

        //this method will be called automatically by the modmanager after the main game scene is loaded.
        //The following requirements must be met to be invoked automatically by the ModManager during setup for this to happen:
        //1. Marked with the [Invoke] custom attribute
        //2. Be public & static class method
        //3. Take in an InitParams struct as the only parameter
        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("main init");

            mod = initParams.Mod;

            //just an example of how to add a mono-behavior to a scene.
            GameObject dsd = new GameObject("DraftySecretDoors");
            DraftySecretDoors draftySecretDoors = dsd.AddComponent<DraftySecretDoors>();

            //after finishing, set the mod's IsReady flag to true.
            ModManager.Instance.GetMod(initParams.ModTitle).IsReady = true;
        }
    }
}