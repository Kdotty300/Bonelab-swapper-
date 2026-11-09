using System;
using MelonLoader;
using UnityEngine;
using BoneLib;

[assembly: MelonInfo(typeof(VibeSoundSwapper.Mod), "Vibe Sound Swapper GUI", "1.2.0", "VibeCoder")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace VibeSoundSwapper
{
    public class Mod : MelonMod
    {
        private static AudioClip _copiedClip = null;
        private static string _statusText = "Ready. Click Right Stick to Copy.";

        public override void OnUpdate()
        {
            // Safely verify Player and Hand data using real BoneLib hook properties
            if (Player.rightHand != null)
            {
                // Detect physical Right Stick Click down natively
                if (InputManager.GetKeyDown(InputManager.ControllerHand.Right, InputManager.ControllerKey.ThumbstickClick))
                {
                    ExecuteVibeSwap();
                }
            }
        }

        public override void OnGUI()
        {
            // Draw a native Unity GUI overlay panel on your desktop monitor screen
            GUILayout.BeginArea(new Rect(20, 20, 350, 120));
            GUILayout.Box("<b>[VIBE SOUND SWAPPER]</b>");
            GUILayout.Label("Status: " + _statusText);
            
            if (GUILayout.Button("Reset / Clear Clipboard"))
            {
                _copiedClip = null;
                _statusText = "Clipboard cleared! Ready to copy.";
            }
            GUILayout.EndArea();
        }

        private static void ExecuteVibeSwap()
        {
            try
            {
                var physicsHand = Player.rightHand.physicsHand;
                if (physicsHand == null || physicsHand.attachedObjects.Count == 0)
                {
                    _statusText = "Error: You are not holding an object.";
                    return;
                }

                // Get the physical GameObject of the held weapon
                GameObject heldWeapon = physicsHand.attachedObjects[0].gameObject;
                if (heldWeapon == null) return;

                // Look for the gun's fire audio source component natively
                AudioSource weaponAudio = heldWeapon.GetComponentInChildren<AudioSource>();
                if (weaponAudio == null)
                {
                    _statusText = "No AudioSource found on this weapon model.";
                    return;
                }

                // Copy vs Paste logic loop
                if (_copiedClip == null)
                {
                    if (weaponAudio.clip != null)
                    {
                        _copiedClip = weaponAudio.clip;
                        _statusText = "Copied sound: " + _copiedClip.name;
                    }
                    else
                    {
                        _statusText = "Found audio player, but gun has no clip loaded.";
                    }
                }
                else
                {
                    // Hot-swap the active audio asset reference inside the game object memory
                    weaponAudio.clip = _copiedClip;
                    _statusText = "Injected custom sound! Clipboard reset.";
                    _copiedClip = null;
                }
            }
            catch (Exception ex)
            {
                _statusText = "Error processing gun components.";
            }
        }
    }
}
