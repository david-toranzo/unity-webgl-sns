using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace Zappar.Additional.SNS
{
    public class ZSNSListener : ZWebListener
    {
        public delegate void PromptAction();
        public event PromptAction OnSavedPrompt;
        public event PromptAction OnSharedPrompt;
        public event PromptAction OnPromptClosed;

        public void OnSavedPromptCallback()
        {
            OnSavedPrompt?.Invoke();
        }

        public void OnSharedPromptCallback()
        {
            OnSharedPrompt?.Invoke();
        }
        public void OnPromptClosedCallback()
        {
            OnPromptClosed?.Invoke();
        }

        public override void MessageCallback(string message)
        {
            Debug.Log("[ZWebGL]: " + message);
        }
    }

    public class ZSaveNShare
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        public const string PluginName = "__Internal";
#else
        public const string PluginName = "";
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport(PluginName)]
        private static extern bool zappar_sns_initialize(string canvas, string unityObjectName, string onSavedFunc, string onSharedFunc, string onClosedFunc);
        [DllImport(PluginName)]
        private static extern bool zappar_sns_is_initialized();
        [DllImport(PluginName)]
        private static extern void zappar_sns_jpg_snapshot(byte[] img, int size, float quality);
        [DllImport(PluginName)]
        private static extern void zappar_sns_png_snapshot(byte[] img, int size, float quality);
        [DllImport(PluginName)]
        private static extern void zappar_sns_open_snap_prompt();
        [DllImport(PluginName)]
        private static extern void zappar_sns_open_video_prompt();

        //Added
        [DllImport(PluginName)] 
        private static extern void zappar_sns_share_last_screenshot(string text);
        [DllImport(PluginName)]
        public static extern void upload_external_screenshot(byte[] img, int size);
#else
        private static bool zappar_sns_initialize(string canvas, string unityObjectName, string onSavedFunc, string onSharedFunc, string onClosedFunc) { return false; }
        private static bool zappar_sns_is_initialized() { return false; }
        private static void zappar_sns_jpg_snapshot(byte[] img, int size, float quality) { }
        private static void zappar_sns_png_snapshot(byte[] img, int size, float quality) { }
        private static void zappar_sns_open_snap_prompt() {}
        private static void zappar_sns_open_video_prompt() { }
#endif
        private static ZSNSListener m_snsListener = null;

        private static void SetupListener()
        {
            if (m_snsListener == null)
            {
                GameObject go = (GameObject.FindObjectOfType<ZWebListener>()?.gameObject) ?? new GameObject(ZWebListener.UnityObjectName);
                m_snsListener = go.GetComponent<ZSNSListener>() ?? go.AddComponent<ZSNSListener>();
            }
        }

        public static void Initialize()
        {
            SetupListener();

#if UNITY_2020_1_OR_NEWER
            if (!zappar_sns_initialize("#unity-canvas", ZWebListener.UnityObjectName, nameof(ZSNSListener.OnSavedPromptCallback), nameof(ZSNSListener.OnSavedPromptCallback), nameof(ZSNSListener.OnPromptClosedCallback)))
#else
            if (!zappar_sns_initialize("#canvas", ZWebListener.UnityObjectName, nameof(ZSNSListener.OnSavedPromptCallback), nameof(ZSNSListener.OnSavedPromptCallback), nameof(ZSNSListener.OnPromptClosedCallback)))
#endif

            {
#if UNITY_EDITOR
                Debug.Log("ZSaveShare is not supported in editor mode.");
#else
                Debug.Log("Failed to initialize save and share plugin!");
#endif
                return;
            }
        }

        public static bool IsInitialized()
        {
            return zappar_sns_is_initialized();
        }

        public delegate void SnapshotCaptured();
        
        /// <param name="onCapturedCallback">callback method when snapshot capture is done</param>
        /// <param name="quality">Image encode quality. Range [0f,1f]</param>
        public static IEnumerator TakeSnapshot(SnapshotCaptured onCapturedCallback = null, float quality=0.75f)
        {
            yield return new WaitForEndOfFrame();
            quality = Mathf.Clamp(quality, 0f, 1f);
            Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
            tex.Apply();
            byte[] img = tex.EncodeToJPG((int)(quality * 100));
            zappar_sns_jpg_snapshot(img, img.Length, quality);
            onCapturedCallback?.Invoke();
        }

        public static void UploadScreenshot(byte[] image, int size)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            upload_external_screenshot(image, size);
#endif
        }

        public static void OpenSNSSnapPrompt()
        {
            zappar_sns_open_snap_prompt();
        }

        public static void OpenSNSVideoRecordingPrompt()
        {
            zappar_sns_open_video_prompt();
        }

        public static void ShareLastScreenshotWithSocialMedia(string text)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            zappar_sns_share_last_screenshot(text);
#else
            Debug.Log("Sharing: " + text);
#endif
        }

        public static void RegisterSNSCallbacks(ZSNSListener.PromptAction onSaved = null, ZSNSListener.PromptAction onShared = null, ZSNSListener.PromptAction onClosed = null)
        {
            SetupListener();
            if (onSaved != null) m_snsListener.OnSavedPrompt += onSaved;
            if (onShared != null) m_snsListener.OnSharedPrompt += onShared;
            if (onClosed != null) m_snsListener.OnPromptClosed += onClosed;
        }

        public static void DeregisterSNSCallbacks(ZSNSListener.PromptAction onSaved = null, ZSNSListener.PromptAction onShared = null, ZSNSListener.PromptAction onClosed = null)
        {
            SetupListener();
            if (onSaved != null) m_snsListener.OnSavedPrompt -= onSaved;
            if (onShared != null) m_snsListener.OnSharedPrompt -= onShared;
            if (onClosed != null) m_snsListener.OnPromptClosed -= onClosed;
        }
    }
}