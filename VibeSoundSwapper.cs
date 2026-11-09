using System;
using MelonLoader;
using UnityEngine;
using BoneLib;

[assembly: MelonInfo(typeof(VibeSoundSwapper.Mod), "Vibe Sound Swapper", "1.0.0", "VibeCoder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace VibeSoundSwapper
{
    public class Mod : MelonMod
    {
        private AudioClip _copiedClip = null;

        public override void OnUpdate()
        {
            // Use BoneLib's safe input helper to detect the thumbstick click natively
            if (Player.rightHand != null)
            {
                if (InputManager.GetKeyDown(InputManager.ControllerHand.Right, InputManager.ControllerKey.ThumbstickClick))
                {
                    ExecuteVibeSwap();
                }
            }
        }

        private void ExecuteVibeSwap()
        {
            // Safely locate what object the player is holding via BoneLib
            var physicalHand = Player.rightHand.physicsHand;
            if (physicalHand == null || physicalHand.attachedObjects.Count == 0) return;

            GameObject heldRoot = physicalHand.attachedObjects[0].gameObject;
            if (heldRoot == null) return;

            // Search dynamically for an AudioSource component inside the held weapon
            AudioSource weaponAudio = heldRoot.GetComponentInChildren<AudioSource>();
            if (weaponAudio == null)
            {
                MelonLogger.Msg("[VibeSwap] No AudioSource found on this custom weapon object.");
                return;
            }

            // State Machine logic to manage clipboard cloning
            if (_copiedClip == null)
            {
                if (weaponAudio.clip != null)
                {
                    _copiedClip = weaponAudio.clip;
                    MelonLogger.Msg($"[VibeSwap] Successfully copied audio clip: {_copiedClip.name}");
                }
                else
                {
                    MelonLogger.Msg("[VibeSwap] Target AudioSource found, but its clip is empty.");
                }
            }
            else
            {
                // Inject the cached audio asset over the active weapon profile
                weaponAudio.clip = _copiedClip;
                MelonLogger.Msg("[VibeSwap] Successfully injected new gunshot sound!");
                
                // Clear the cache index so a new sound can be stored on the next action loop
                _copiedClip = null;
            }
        }
    }
}
