using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ErrorBox : MonoBehaviour {

    public static ErrorBox Instance;

    [SerializeField] private CanvasGroup _messageBox;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _copyError;

    private void Awake() {
#if !UNITY_SERVER || UNITY_EDITOR
        if (Instance) {
            Debug.LogWarning($"Overriding current instance: {Instance.name} with {name}...", this);
        }

        Instance = this;
        Debug.Log("Error Box Initialised");
        _messageBox.alpha = 0;
        _copyError.onClick.AddListener(CopyError);
#endif
    }

    public void ShowError(string message) {
        _messageText.text += message;
        _messageBox.alpha = 1;
        _messageBox.interactable = true;
        _messageBox.blocksRaycasts = true;
    }

    private void CopyError() {
        TextEditor editor = new TextEditor() {
            text = _messageText.text
        };
        editor.SelectAll();
        editor.Copy();
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    private static void OnDomainReload() {
        // Instance = null;
    }
#endif
}