using UnityEngine;

public class ScreenManager : MonoBehaviour {

    [SerializeField] private FullScreenMode _fullscreenMode = FullScreenMode.Windowed;

    public void Start() {
#if UNITY_CLIENT || !UNITY_EDITOR
        Screen.fullScreenMode = _fullscreenMode;
#endif

#if UNITY_EDITOR
        Debug.Log("[Screen Manager]: Not setting window mode as in Editor...");
#endif
    }
}