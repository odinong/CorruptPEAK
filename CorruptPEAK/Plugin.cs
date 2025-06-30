using BepInEx;
using BepInEx;
using BepInEx.Configuration;
using CorruptPEAK;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Networking;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;
using Zorro.UI.Modal;

namespace CorruptPEAK
{
    [BepInPlugin("com.zbytek.CorruptPEAK", "CorruptPEAK", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool readyToPlay = false;
        public static ConfigEntry<bool> AntiCrashEnabled;
        public static float CorruptionIntensity = 0.0001f;
        public static ConfigEntry<float> CorruptionIncreaseRate;
        public static ConfigEntry<float> AntiCrashLimit;
        public static ConfigEntry<bool> CorruptRenderers;
        public static ConfigEntry<bool> CorruptAudio;
        public static ConfigEntry<bool> CorruptParticles;
        public static ConfigEntry<bool> CorruptTransforms;
        public static ConfigEntry<bool> CorruptMeshes;
        public static ConfigEntry<bool> ShouldCorruptLightmaps;
        public static ConfigEntry<bool> CorruptRandomValues;
        public static ConfigEntry<bool> WelcomeAccepted;
        public static bool corruptionStarted = false;
        public static bool litthecampfiresostopcorruption = false;
        private List<AudioClip> AllAudioClips = new List<AudioClip>();
        private bool Init;
        public bool alreadysent = false;

        private void Awake()
        {
            AntiCrashEnabled = Config.Bind("General", "AntiCrashEnabled", true, "makes ur game quit before it dies");
            AntiCrashLimit = Config.Bind("General", "AntiCrashLimit", 20f, "corruption intensity to quit your game at");
            CorruptionIncreaseRate = Config.Bind("Corruption", "CorruptionIncreaseRate", 0.0001f, "how fast the corruption increases");
            CorruptRenderers = Config.Bind("Corruption Options", "CorruptRenderers", true, "makes it so that colors break");
            CorruptAudio = Config.Bind("Corruption Options", "CorruptAudio", true, "makes it so that audio values change");
            CorruptParticles = Config.Bind("Corruption Options", "CorruptParticles", true, "changes particle system values");
            CorruptTransforms = Config.Bind("Corruption Options", "CorruptTransforms", false, "makes it so things randomly get bigger or smaller");
            CorruptMeshes = Config.Bind("Corruption Options", "CorruptMeshes", true, "makes it so any editable mesh can be edited randomly");
            ShouldCorruptLightmaps = Config.Bind("Corruption Options", "CorruptLightmaps", true, "makes it so the lighting breaks");
            CorruptRandomValues = Config.Bind("Dangerous", "CorruptRandomValues", false, "THIS IS DANGEROUS, PLEASE USE AT YOUR OWN WILL!!! randomizes values in basically any object in the scene, might go faster than the increase rate cuz i suck at coding");

        }
        private void Update()
        {
            if (readyToPlay && corruptionStarted)
            {
                CorruptionIntensity = Mathf.Clamp(CorruptionIntensity + CorruptionIncreaseRate.Value * Time.deltaTime, 0f, 500f);
            }
            // i think 20 should be fine, if it doesnt say 20 then i changed it so its a config
            if (CorruptionIntensity > AntiCrashLimit.Value && AntiCrashEnabled.Value)
            {
                MessageBox(IntPtr.Zero, "Hey your game is about to crash so its also about to quit so bye", "AntiCrash", 0);
                Application.Quit();
            }
        }

        public void LateUpdate()
        {
            if (PhotonNetwork.InRoom)
            {
                corruptionStarted = true;
                readyToPlay = true;
            }

            if (readyToPlay && corruptionStarted && PhotonNetwork.InRoom)
            {
                if (!alreadysent)
                {
                    ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
                    props["CRPTPEAK"] = true;
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                    Notifications.SendOnce("<color=blue>[ CorruptPEAK ] : Corruption Has Begun. </color>");
                    alreadysent = true;
                }

                if (!Init && Player.localPlayer != null)
                {
                    FindAllAudioClips();
                    Init = true;
                }

                if (UnityEngine.Random.value < CorruptionIntensity * 0.1f)
                {
                    CorruptScene();
                }
            }
        }
        private void FindAllAudioClips()
        {
            foreach (var clip in Resources.FindObjectsOfTypeAll<AudioClip>())
            {
                if (!AllAudioClips.Contains(clip)) AllAudioClips.Add(clip);
            }
        }

        private void CorruptScene()
        {
            if (CorruptRenderers.Value)
            {
                foreach (var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
                {
                    if (UnityEngine.Random.value < CorruptionIntensity)
                        CorruptRenderer(renderer);
                }
            }

            if (CorruptAudio.Value)
            {
                foreach (var audioSource in UnityEngine.Object.FindObjectsOfType<AudioSource>())
                {
                    if (UnityEngine.Random.value < CorruptionIntensity)
                        CorruptAudioSource(audioSource);
                }
            }

            if (CorruptParticles.Value)
            {
                foreach (var ps in UnityEngine.Object.FindObjectsOfType<ParticleSystem>())
                {
                    if (UnityEngine.Random.value < CorruptionIntensity)
                        CorruptParticleSystem(ps);
                }
            }

            if (CorruptTransforms.Value)
            {
                foreach (var tr in UnityEngine.Object.FindObjectsOfType<Transform>())
                {
                    if (UnityEngine.Random.value < CorruptionIntensity)
                        CorruptTransform(tr);
                }
            }
            if (CorruptRandomValues.Value)
            {
                MonoBehaviour[] allBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();

                List<MonoBehaviour> filtered = new List<MonoBehaviour>();
                foreach (var mb in allBehaviours)
                {
                    var typeName = mb.GetType().Name.ToLowerInvariant();
                    if (!typeName.Contains("photon"))
                    {
                        filtered.Add(mb);
                    }
                }
                //idk what a sbyte is but my friend said i should use this so idk bruh
                int corruptCount = Math.Min(sbyte.MaxValue, filtered.Count);

                for (int i = 0; i < corruptCount; i++)
                {
                    int index = rng.Next(filtered.Count);
                    var target = filtered[index];

                    CorruptFields(target);

                    filtered.RemoveAt(index);
                }
            }
        }
        private System.Random rng = new System.Random();

        private void CorruptFields(MonoBehaviour obj)
        {
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.IsInitOnly) continue;

                Type fieldType = field.FieldType;

                try
                {
                    if (fieldType == typeof(int))
                    {
                        field.SetValue(obj, rng.Next(-1000, 1000));
                    }
                    else if (fieldType == typeof(float))
                    {
                        float val = (float)(rng.NextDouble() * 2000.0 - 1000.0);
                        if (rng.NextDouble() < 0.1) val = float.NaN;
                        field.SetValue(obj, val);
                    }
                    else if (fieldType == typeof(bool))
                    {
                        field.SetValue(obj, rng.Next(2) == 0);
                    }
                    else if (fieldType == typeof(string))
                    {
                        field.SetValue(obj, RandomString(rng.Next(3, 10)));
                    }
                }
                catch
                {
                }
            }
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!?";
            char[] stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[rng.Next(chars.Length)];
            }
            return new string(stringChars);
        }
        private void CorruptRenderer(Renderer renderer)
        {
            var mat = renderer.material;
            if (mat.HasProperty("_Color")) mat.color = UnityEngine.Random.ColorHSV();
            if (mat.HasProperty("_MainTex") && mat.mainTexture != null)
                mat.mainTextureScale = new Vector2(UnityEngine.Random.Range(0.5f, 2f), UnityEngine.Random.Range(0.5f, 2f));

            var mf = renderer.GetComponent<MeshFilter>();
            if (mf && mf.mesh && mf.mesh.isReadable)
            {
                CorruptMesh(mf.mesh);
            }
        }

        private void CorruptAudioSource(AudioSource src)
        {
            src.pitch = UnityEngine.Random.Range(0f, 100f);
            src.volume = UnityEngine.Random.Range(0f, 1f);
            if (AllAudioClips.Count > 0)
                src.clip = AllAudioClips[UnityEngine.Random.Range(0, AllAudioClips.Count)];
        }

        private void CorruptParticleSystem(ParticleSystem ps)
        {
            var main = ps.main;
            main.startSize = UnityEngine.Random.Range(0.1f, 5f);
            main.startColor = UnityEngine.Random.ColorHSV();
        }

        private void CorruptTransform(Transform tr)
        {
            tr.position += UnityEngine.Random.insideUnitSphere * CorruptionIntensity * 2f;
            tr.rotation = Quaternion.Euler(UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f));
            tr.localScale = new Vector3(
                tr.localScale.x * UnityEngine.Random.Range(0.5f, 1.5f),
                tr.localScale.y * UnityEngine.Random.Range(0.5f, 1.5f),
                tr.localScale.z * UnityEngine.Random.Range(0.5f, 1.5f)
            );
        }

        private void CorruptMesh(Mesh mesh)
        {
            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                if (UnityEngine.Random.value < CorruptionIntensity)
                    vertices[i] += UnityEngine.Random.insideUnitSphere * CorruptionIntensity;
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    }
}
