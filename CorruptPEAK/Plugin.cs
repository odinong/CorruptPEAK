using BepInEx;
using CorruptPEAK;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI.Extensions;

namespace CorruptPEAK
{
    [BepInPlugin("com.zbytek.CorruptPEAK", "CorruptPEAK", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool readyToPlay = true;
        public static bool corruptionStarted = false;

        private bool showGui = false;
        private float fadeTimer = 0f;
        private bool showText = false;

        public float CorruptionIntensity = 0.0001f;
        public float CorruptionIncreaseRate = 0.0001f;

        public bool CorruptRenderers = true;
        public bool CorruptAudio = true;
        public bool CorruptParticles = true;
        public bool CorruptTransforms = false;
        public bool CorruptMeshes = true;
        public bool ShouldCorruptLightmaps = true;
        public bool memorycorruption = false;

        private List<AudioClip> AllAudioClips = new List<AudioClip>();
        public bool Init;

        public bool alreadysent = false;

        private void Update()
        {
            if (readyToPlay && corruptionStarted)
            {
                CorruptionIntensity = Mathf.Clamp(CorruptionIntensity + CorruptionIncreaseRate * Time.deltaTime, 0f, 1f);
            }

            if (showText)
            {
                fadeTimer += Time.deltaTime;
                if (fadeTimer > 2f)
                {
                    showText = false;
                    fadeTimer = 0f;
                }
            }
            // in case of memory leaks, this quits your game before it dies fully.
            if (CorruptionIntensity > 500f)
            {
                Application.Quit();
            }
        }

        public void LateUpdate()
        {
            if(PhotonNetwork.InRoom)
            {
                corruptionStarted = true;
            }
            if (readyToPlay && corruptionStarted && PhotonNetwork.InRoom)
            {
                if (!alreadysent)
                {
                    Notifications.SendOnce("[ CorruptPEAK ] | It has begun.");
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
                if (!AllAudioClips.Contains(clip))
                {
                    AllAudioClips.Add(clip);
                }
            }
        }

        private void CorruptScene()
        {
            if (CorruptRenderers)
            {
                foreach (var renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
                {
                    if (UnityEngine.Random.value < CorruptionIntensity)
                    {
                        CorruptRenderer(renderer);
                    }
                }
            }

            if (CorruptAudio)
            {
                foreach (var audioSource in UnityEngine.Object.FindObjectsOfType<AudioSource>())
                {
                    if (UnityEngine.Random.value < CorruptionIntensity)
                    {
                        CorruptAudioSource(audioSource);
                    }
                }
            }

            if (CorruptParticles)
            {
                foreach (var particleSystem in UnityEngine.Object.FindObjectsOfType<ParticleSystem>())
                {
                    if (UnityEngine.Random.value < CorruptionIntensity)
                    {
                        CorruptParticleSystem(particleSystem);
                    }
                }
            }

            if (CorruptTransforms)
            {
                foreach (var transform in UnityEngine.Object.FindObjectsOfType<Transform>())
                {
                    if (UnityEngine.Random.value < CorruptionIntensity)
                    {
                        CorruptTransform(transform);
                    }
                }
            }

            if (ShouldCorruptLightmaps)
            {
                CorruptLightmaps();
            }
        }

        private void CorruptRenderer(Renderer renderer)
        {
            if (renderer.materials.Length > 0)
            {
                var material = renderer.materials[UnityEngine.Random.Range(0, renderer.materials.Length)];
                if (material != null)
                {
                    if (material.HasProperty("_Color"))
                    {
                        material.color = UnityEngine.Random.ColorHSV();
                    }

                    if (material.HasProperty("_MainTex") && material.mainTexture != null)
                    {
                        material.mainTextureScale = new Vector2(
                            UnityEngine.Random.Range(0.5f, 2f),
                            UnityEngine.Random.Range(0.5f, 2f)
                        );
                    }
                }
            }

            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter?.mesh != null && meshFilter.mesh.isReadable)
            {
                CorruptMesh(meshFilter.mesh);
            }

            var skinnedMeshRenderer = renderer.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer?.bones != null && skinnedMeshRenderer.bones.Length > 0)
            {
                ResizeBones(skinnedMeshRenderer.bones);
            }
        }


        private void ResizeBones(Transform[] bones)
        {
            foreach (var bone in bones)
            {
                if (UnityEngine.Random.value < CorruptionIntensity)
                {
                    Vector3 scaleChange = new Vector3(UnityEngine.Random.Range(0.2f, 1.5f), UnityEngine.Random.Range(0.2f, 1.5f), UnityEngine.Random.Range(0.2f, 1.5f));
                    bone.localScale = Vector3.Scale(bone.localScale, scaleChange);
                }
            }
        }

        private void CorruptAudioSource(AudioSource audioSource)
        {
            audioSource.pitch = UnityEngine.Random.Range(0f, 100f);
            audioSource.volume = UnityEngine.Random.Range(0f, 1f);

            if (AllAudioClips.Count > 0)
            {
                audioSource.clip = AllAudioClips[UnityEngine.Random.Range(0, AllAudioClips.Count)];
            }

            if (UnityEngine.Random.value < CorruptionIntensity && audioSource.clip != null)
            {
                audioSource.time = UnityEngine.Random.Range(0f, audioSource.clip.length);
            }
        }

        private void CorruptParticleSystem(ParticleSystem ps)
        {
            var main = ps.main;
            main.startSize = UnityEngine.Random.Range(0.1f, 5f);
            main.startSpeed = UnityEngine.Random.Range(0.1f, 10f);
            main.startColor = UnityEngine.Random.ColorHSV();
            main.startRotation = UnityEngine.Random.Range(0f, 360f);
            main.gravityModifier = UnityEngine.Random.Range(0f, 5f);
            main.simulationSpeed = UnityEngine.Random.Range(0.1f, 5f);

            var emission = ps.emission;
            emission.rateOverTime = UnityEngine.Random.Range(1f, 100f);
            emission.rateOverDistance = UnityEngine.Random.Range(0f, 50f);

            var shape = ps.shape;
            shape.angle = UnityEngine.Random.Range(0f, 90f);
            shape.radius = UnityEngine.Random.Range(0.1f, 5f);
            shape.arc = UnityEngine.Random.Range(0f, 360f);
            shape.scale = new Vector3(UnityEngine.Random.Range(0.1f, 5f), UnityEngine.Random.Range(0.1f, 5f), UnityEngine.Random.Range(0.1f, 5f));

            var velocity = ps.velocityOverLifetime;
            velocity.x = new ParticleSystem.MinMaxCurve(UnityEngine.Random.Range(-5f, 5f));
            velocity.y = new ParticleSystem.MinMaxCurve(UnityEngine.Random.Range(-5f, 5f));
            velocity.z = new ParticleSystem.MinMaxCurve(UnityEngine.Random.Range(-5f, 5f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(UnityEngine.Random.ColorHSV(), UnityEngine.Random.ColorHSV());

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(UnityEngine.Random.Range(0.1f, 2f), UnityEngine.Random.Range(0.1f, 3f));

            var rotationOverLifetime = ps.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(UnityEngine.Random.Range(-180f, 180f));

            var textureSheetAnimation = ps.textureSheetAnimation;
            if (textureSheetAnimation.enabled)
            {
                textureSheetAnimation.numTilesX = UnityEngine.Random.Range(1, 10);
                textureSheetAnimation.numTilesY = UnityEngine.Random.Range(1, 10);
                textureSheetAnimation.animation = (ParticleSystemAnimationType)UnityEngine.Random.Range(0, 2);
            }
        }

        private void CorruptTransform(Transform transform)
        {
            if (UnityEngine.Random.value < CorruptionIntensity)
            {
                transform.position += UnityEngine.Random.insideUnitSphere * CorruptionIntensity * 2f;
            }

            if (UnityEngine.Random.value < CorruptionIntensity)
            {
                transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f));
            }

            if (UnityEngine.Random.value < CorruptionIntensity)
            {
                transform.localScale = new Vector3(
                    transform.localScale.x * UnityEngine.Random.Range(0.5f, 1.5f),
                    transform.localScale.y * UnityEngine.Random.Range(0.5f, 1.5f),
                    transform.localScale.z * UnityEngine.Random.Range(0.5f, 1.5f)
                );
            }
        }

        private void CorruptMesh(Mesh mesh)
        {
            var vertices = mesh.vertices;
            float maxSize = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z);

            for (int i = 0; i < vertices.Length; i++)
            {
                if (UnityEngine.Random.value < CorruptionIntensity)
                {
                    vertices[i] += UnityEngine.Random.insideUnitSphere * CorruptionIntensity * maxSize * 1.3f;
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void CorruptLightmaps()
        {
            if (LightmapSettings.lightmaps == null) return;

            foreach (var lightmap in LightmapSettings.lightmaps)
            {
                if (lightmap.lightmapColor != null)
                {
                    var pixels = lightmap.lightmapColor.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (UnityEngine.Random.value < CorruptionIntensity)
                        {
                            pixels[i] = UnityEngine.Random.ColorHSV();
                        }
                    }
                    lightmap.lightmapColor.SetPixels(pixels);
                    lightmap.lightmapColor.Apply();
                }

                if (lightmap.lightmapDir != null)
                {
                    var pixels = lightmap.lightmapDir.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (UnityEngine.Random.value < CorruptionIntensity)
                        {
                            pixels[i] = UnityEngine.Random.ColorHSV();
                        }
                    }
                    lightmap.lightmapDir.SetPixels(pixels);
                    lightmap.lightmapDir.Apply();
                }
            }
        }
    }
}
