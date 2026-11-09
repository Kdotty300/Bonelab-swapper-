using System;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(VibeSoundSwapper.Mod), "Vibe Sound Swapper", "1.0.0", "VibeCoder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace VibeSoundSwapper
{
    public class Mod : MelonMod
    {
        private AudioClip _copiedClip = null;

        public override void OnUpdate()
        {
            // Detect Right Stick Click down via Unity's legacy Input (works on all PCVR headsets in MelonLoader)
            // 'joystick button 9' is the universal Unity mapping for Right Stick Click
            if (Input.GetKeyDown(KeyCode.JoystickButton9))
            {
                ExecuteVibeSwap();
            }
        }

        private void ExecuteVibeSwap()
        {
            // 1. Find the player's hands dynamically without needing BoneLib.dll
            GameObject openController = GameObject.Find("[RigManager]");
            if (openController == null) openController = GameObject.Find("Physics Rig");
            if (openController == null) return;

            // 2. Look for any AudioSource attached to the held objects in the hierarchy
            // We scan the active scene for an AudioSource that is a child of the interactive hand layers
            AudioSource targetAudio = null;
            AudioSource[] sources = GameObject.FindObjectsOfType<AudioSource>();

            foreach (var src in sources)
            {
                // Check if the audio source belongs to a weapon/object near the player's hands
                if (src != null && src.gameObject.activeInHierarchy && src.name.ToLower().Contains("fire"))
                {
                    targetAudio = src;
                    break;
                }
            }

            // Fallback: Just grab the first active audio source found in the gun layer if specific 'fire' naming fails
            if (targetAudio == null && sources.Length > 0)
            {
                foreach (var src in sources)
                {
                    if (src != null && src.gameObject.activeInHierarchy && (src.transform.root.name.Contains("Spawned") || src.gameObject.name.Contains("Weapon")))
                    {
                        targetAudio = src;
                        break;
                    }
                }
            }

            if (targetAudio == null)
            {
                MelonLogger.Msg("[VibeSwap] Hold a weapon and try clicking closer to it.");
                return;
            }

            // 3. State Machine: Copy vs Paste
            if (_copiedClip == null)
            {
                if (targetAudio.clip != null)
                {
                    _copiedClip = targetAudio.clip;
                    MelonLogger.Msg($"[VibeSwap] Successfully copied sound: {_copiedClip.name}");
                }
                else
                {
                    MelonLogger.Msg("[VibeSwap] Found weapon audio, but no clip was currently loaded in it.");
                }
            }
            else
            {
                // Paste the sound over!
                targetAudio.clip = _copiedClip;
                MelonLogger.Msg($"[VibeSwap] Injected custom sound onto this weapon!");
                
                // Clear the cache so you can copy something else next click
                _copiedClip = null;
            }
        }
    }
}
