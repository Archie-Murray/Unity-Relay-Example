using UnityEngine;

using UnityEngine.InputSystem;

public class ScreenManager : MonoBehaviour {

    [SerializeField] private FullScreenMode _fullscreenMode = FullScreenMode.Windowed;

    public void Start() {
#if !(UNITY_SERVER || UNITY_EDITOR)
        if (_fullscreenMode != FullScreenMode.Windowed && _fullscreenMode != FullScreenMode.MaximizedWindow) {
            _fullscreenMode = FullScreenMode.Windowed;
        }

        Screen.fullScreen = false;
        Screen.fullScreenMode = _fullscreenMode;
#endif

#if UNITY_EDITOR
        Debug.Log($"[Screen Manager]: Not setting window mode to {_fullscreenMode} as in Editor...");
#endif
    }

#if !UNITY_SERVER
    private void Update() {
        if (Keyboard.current[Key.Escape].wasPressedThisFrame) {
            Screen.fullScreen = false;
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }
#endif
}