using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System.Collections;
using DaggerfallWorkshop.Game.Serialization;
using System;

namespace DraftySecretDoors
{
    public class DraftySecretDoors : MonoBehaviour
    {
        public static Mod mod;

        float volume = 1.0f;
        float minDist = 2.5f;
        float maxDist = 12.0f;
        float pitch = 1.5f;

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

                        SecretDoorAudioParent parent = actionDoor.gameObject.AddComponent<SecretDoorAudioParent>();
                        parent.Initialize(minDist, maxDist, volume, pitch);   
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

    public class SecretDoorAudioParent : MonoBehaviour, IPlayerActivable
    {
        public bool coroutineRunning;

        DaggerfallActionDoor actionDoor;
        SecretDoorAudio secretDoorAudio;

        void Awake()
        {
            actionDoor = GetComponent<DaggerfallActionDoor>();

            GameObject go = new GameObject("SecretDoorAudio");
            go.transform.parent = actionDoor.transform;
            go.transform.localPosition = actionDoor.gameObject.GetComponent<BoxCollider>().center;
            go.transform.rotation = actionDoor.transform.rotation;
            secretDoorAudio = go.AddComponent<SecretDoorAudio>();
        }

        public void Initialize(float minDist, float maxDist, float volume, float pitch)
        {
            secretDoorAudio.windVolume = volume;

            secretDoorAudio.SetSound(SoundClips.AmbientWindBlow1b, AudioPresets.LoopIfPlayerNear);
            secretDoorAudio.AudioSource.rolloffMode = AudioRolloffMode.Linear;
            secretDoorAudio.AudioSource.minDistance = minDist;
            secretDoorAudio.AudioSource.maxDistance = maxDist;
            secretDoorAudio.AudioSource.volume = 0.0f;
            secretDoorAudio.AudioSource.pitch = pitch;
        }

        public IEnumerator ActivateCheck()
        {
            // Wait for door to finish sequence or actionDoor.IsClosed will be incorrect.
            yield return new WaitForSeconds(actionDoor.OpenDuration + 0.1f);

            if (actionDoor.IsClosed)
            {
                StartCoroutine(LerpVolume(secretDoorAudio.windVolume, actionDoor.OpenDuration));
            }
            else
            {
                StartCoroutine(LerpVolume(0.0f, actionDoor.OpenDuration));
            }
        }

        public IEnumerator LerpVolume(float endValue, float duration)
        {
            coroutineRunning = true;
            float time = 0;
            float startValue = secretDoorAudio.AudioSource.volume;

            while (time < duration)
            {
                secretDoorAudio.AudioSource.volume = Mathf.Lerp(startValue, endValue, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            secretDoorAudio.AudioSource.volume = endValue;
            coroutineRunning = false;
        }
        
        public void Activate(RaycastHit hit)
        {
            StartCoroutine(ActivateCheck());
        }
    }

    public class SecretDoorAudio : DaggerfallAudioSource
    {
        public float windVolume;

        DaggerfallActionDoor actionDoor;
        SecretDoorAudioParent parent;

        private void Awake()
        {
            actionDoor = GetComponentInParent<DaggerfallActionDoor>();
            parent = GetComponentInParent<SecretDoorAudioParent>();
        }

        void FixedUpdate()
        {
            bool playerInRange = (Vector3.Distance(transform.position, GameManager.Instance.PlayerObject.transform.position) <= AudioSource.maxDistance);

            AudioSource.enabled = playerInRange;

            if (AudioSource.enabled && !actionDoor.IsOpen && !actionDoor.IsMoving)
            {
                Ray ray = new Ray(transform.position, GameManager.Instance.PlayerObject.transform.position - transform.position);
                RaycastHit hit;

                //Debug.DrawRay(transform.position, GameManager.Instance.PlayerObject.transform.position - transform.position);

                if (Physics.Raycast(ray, out hit, AudioSource.maxDistance))
                {
                    if (hit.transform != GameManager.Instance.PlayerObject.transform)
                    {
                        if (!parent.coroutineRunning)
                        {
                            StartCoroutine(parent.LerpVolume(0.0f, actionDoor.OpenDuration));
                        }
                    }
                    else
                    {
                        if (!parent.coroutineRunning)
                        {
                            StartCoroutine(parent.LerpVolume(windVolume, actionDoor.OpenDuration));
                        }
                    }
                }
            }
        }
    }
}