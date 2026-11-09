using System;

// Dummy namespaces to satisfy the online compiler without needing external DLL dependencies
namespace MelonLoader 
{ 
    public class MelonMod { public virtual void OnUpdate() {} }
    [AttributeUsage(AttributeTargets.Assembly)] public class MelonInfoAttribute : Attribute { public MelonInfoAttribute(Type t, string n, string v, string a) {} }
    [AttributeUsage(AttributeTargets.Assembly)] public class MelonGameAttribute : Attribute { public MelonGameAttribute(string d, string g) {} }
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

[assembly: MelonLoader.MelonInfo(typeof(VibeSoundSwapper.Mod), "Vibe Sound Swapper", "1.0.0", "VibeCoder")]
[assembly: MelonLoader.MelonGame("Stress Level Zero", "BONELAB")]

namespace VibeSoundSwapper
{
    public class Mod : MelonLoader.MelonMod
    {
        private object _copiedClipRef = null;

        public override void OnUpdate()
        {
            // Unity legacy mapping for Right Stick Click
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.JoystickButton9))
            {
                ExecuteVibeSwap();
            }
        }

        private void ExecuteVibeSwap()
        {
            // 1. Locate rig objects
            var rigManager = UnityEngine.GameObject.Find("[RigManager]") ?? UnityEngine.GameObject.Find("Physics Rig");
            
            // 2. Scan scene for the weapon audio sources
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

            // 3. Clone and Paste Logic
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
