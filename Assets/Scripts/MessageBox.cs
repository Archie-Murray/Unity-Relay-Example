using TMPro;

using UnityEngine;

public class MessageBox : MonoBehaviour {

    public static MessageBox Instance;

    private void Awake() {
#if !UNITY_CLIENT || UNITY_EDITOR
        if (Instance) {
            Debug.LogWarning($"Overriding current instance: {Instance.name} with {name}...", this);
        }

        Instance = this;
        Debug.Log("Message Box Initialised");
        _messageBox.alpha = 0;
#endif
    }

    [SerializeField] private CanvasGroup _messageBox;
    [SerializeField] private TMP_Text _messageText;

    public void ShowMessage(string message) {
        _messageText.text = message;
        _messageBox.alpha = 1;
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    private static void OnDomainReload() {
        // Instance = null;
    }
#endif
}