using System;
using System.Reflection;

[assembly: MelonLoader.MelonInfo(typeof(VibeSoundSwapper.Mod), "Vibe Sound Swapper", "1.0.0", "VibeCoder")]
[assembly: MelonLoader.MelonGame("Stress Level Zero", "BONELAB")]

namespace VibeSoundSwapper
{
    public class Mod : MelonLoader.MelonMod
    {
        private static object _copiedClip = null;
        private static bool _initialized = false;
        
        private static Type _inputManagerType;
        private static Type _playerType;
        private static MethodInfo _getKeyDownMethod;
        private static PropertyInfo _rightHandProp;

        public override void OnInitializeMelon()
        {
            try
            {
                // Resolve game types dynamically from memory at runtime
                _inputManagerType = Type.GetType("BoneLib.InputManager, BoneLib");
                _playerType = Type.GetType("BoneLib.Player, BoneLib");

                if (_inputManagerType != null && _playerType != null)
                {
                    _getKeyDownMethod = _inputManagerType.GetMethod("GetKeyDown", new Type[] { Type.GetType("BoneLib.InputManager+ControllerHand, BoneLib"), Type.GetType("BoneLib.InputManager+ControllerKey, BoneLib") });
                    _rightHandProp = _playerType.GetProperty("rightHand", BindingFlags.Public | BindingFlags.Static);
                    _initialized = true;
                }
            }
            catch { }
        }

        public override void OnUpdate()
        {
            if (!_initialized) return;

            try
            {
                var rightHand = _rightHandProp?.GetValue(null);
                if (rightHand == null) return;

                // Map enum values manually (Hand: Right = 1, Key: ThumbstickClick = 7)
                bool stickClicked = (bool)_getKeyDownMethod.Invoke(null, new object[] { 1, 7 });

                if (stickClicked)
                {
                    ExecuteVibeSwap(rightHand);
                }
            }
            catch { }
        }

        private static void ExecuteVibeSwap(object rightHand)
        {
            try
            {
                var physicsHand = rightHand.GetType().GetProperty("physicsHand")?.GetValue(rightHand);
                if (physicsHand == null) return;

                var attachedObjects = (System.Collections.IList)physicsHand.GetType().GetField("attachedObjects")?.GetValue(physicsHand);
                if (attachedObjects == null || attachedObjects.Count == 0) return;

                var heldObj = attachedObjects[0];
                var gameObject = (object)heldObj.GetType().GetProperty("gameObject")?.GetValue(heldObj);
                if (gameObject == null) return;

                var getComponentMethod = gameObject.GetType().GetMethod("GetComponentInChildren", new Type[] { typeof(Type) });
                Type audioSourceType = Type.GetType("UnityEngine.AudioSource, UnityEngine.AudioModule");
                
                if (audioSourceType == null) return;
                var weaponAudio = getComponentMethod.Invoke(gameObject, new object[] { audioSourceType });
                if (weaponAudio == null) return;

                var clipProperty = audioSourceType.GetProperty("clip");

                if (_copiedClip == null)
                {
                    var activeClip = clipProperty?.GetValue(weaponAudio);
                    if (activeClip != null)
                    {
                        _copiedClip = activeClip;
                    }
                }
                else
                {
                    clipProperty?.SetValue(weaponAudio, _copiedClip);
                    _copiedClip = null;
                }
            }
            catch { }
        }
    }
}
