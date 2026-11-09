using System;

namespace MelonLoader 
{ 
    public class MelonMod { public virtual void OnUpdate() {} }
    public static class MelonLogger { public static void Msg(string txt) => Console.WriteLine(txt); }
}

namespace UnityEngine 
{ 
    public class MonoBehaviour {}
    public class GameObject : MonoBehaviour {
        public bool activeInHierarchy { get; set; } = true;
        public string name { get; set; } = "";
        public Transform transform { get; set; } = new Transform();
        public static GameObject Find(string name) => null;
        public static AudioSource[] FindObjectsOfType<T>() => new AudioSource[0];
    }
    public class Transform { public GameObject root { get; set; } = new GameObject(); }
    public class AudioClip { public string name { get; set; } = "SwappedSound"; }
    public class AudioSource : MonoBehaviour { 
        public AudioClip clip { get; set; } = new AudioClip(); 
        public GameObject gameObject { get; set; } = new GameObject();
        public Transform transform { get; set; } = new Transform();
    }
    public enum KeyCode { JoystickButton9 }
    public static class Input { public static bool GetKeyDown(KeyCode k) => false; }
}

namespace VibeSoundSwapper
{
    public class Mod : MelonLoader.MelonMod
    {
        private object _copiedClipRef = null;

        public override void OnUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.JoystickButton9))
            {
                ExecuteVibeSwap();
            }
        }

        private void ExecuteVibeSwap()
        {
            var rigManager = UnityEngine.GameObject.Find("[RigManager]") ?? UnityEngine.GameObject.Find("Physics Rig");
            
            UnityEngine.AudioSource targetAudio = null;
            var sources = UnityEngine.GameObject.FindObjectsOfType<UnityEngine.AudioSource>();

            foreach (var src in sources)
            {
                if (src != null && src.gameObject.activeInHierarchy && src.name.ToLower().Contains("fire"))
                {
                    targetAudio = src;
                    break;
                }
            }

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

            if (targetAudio == null) return;

            if (_copiedClipRef == null)
            {
                if (targetAudio.clip != null)
                {
                    _copiedClipRef = targetAudio.clip;
                    MelonLoader.MelonLogger.Msg($"[VibeSwap] Copied sound from held weapon!");
                }
            }
            else
            {
                targetAudio.clip = (UnityEngine.AudioClip)_copiedClipRef;
                MelonLoader.MelonLogger.Msg($"[VibeSwap] Injected custom sound onto this weapon!");
                _copiedClipRef = null;
            }
        }
    }
}
