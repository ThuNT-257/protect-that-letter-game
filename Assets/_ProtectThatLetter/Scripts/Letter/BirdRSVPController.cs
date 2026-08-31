using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BirdRSVPController : MonoBehaviour {
    #region Serialized Fields
    [Header("Bird Button & Countdown")]
    [SerializeField] private Button birdButton;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject confirmTooltip;

    [Header("Confirm Popup References")]
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private TMP_InputField noteField;
    [SerializeField] private Button saveButton;

    [Header("Countdown Settings")]
    [SerializeField] private float durationSeconds = 120f;

    [Header("Visual Colors for Yes/No")]
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    #endregion

    #region Private Fields
    private float timeRemaining;
    private bool isTimeExpired = false;
    private bool? isAttending = null; 
    #endregion

    #region Lifecycle
    private void Start() {
        if (birdButton != null) birdButton.onClick.AddListener(OpenConfirmPopup);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (yesButton != null) yesButton.onClick.AddListener(OnYesSelected);
        if (noButton != null) noButton.onClick.AddListener(OnNoSelected);

        if (confirmPopup != null) confirmPopup.SetActive(false);

        timeRemaining = durationSeconds;
        StartCoroutine(CountdownRoutine());
    }
    #endregion

    #region Public Methods
    public void OpenConfirmPopup() {
        if (isTimeExpired) return;
        if (confirmPopup != null) confirmPopup.SetActive(true);
    }

    public void CloseConfirmPopup() {
        if (confirmPopup != null) confirmPopup.SetActive(false);
    }
    #endregion

    #region Private Methods
    private IEnumerator CountdownRoutine() {
        while (timeRemaining > 0f) {
            timeRemaining -= Time.deltaTime;
            UpdateCountdownUI(timeRemaining);
            yield return null;
        }

        OnTimeExpired();
    }

    private void UpdateCountdownUI(float seconds) {
        if (countdownText == null) return;

        float clampedTime = Mathf.Max(0f, seconds);
        int mins = Mathf.FloorToInt(clampedTime / 60f);
        int secs = Mathf.FloorToInt(clampedTime % 60f);
        countdownText.text = string.Format("{0:00}:{1:00}", mins, secs);
    }

    private void OnYesSelected() {
        isAttending = true;
        UpdateYesNoVisuals();
    }

    private void OnNoSelected() {
        isAttending = false;
        UpdateYesNoVisuals();
    }

    private void UpdateYesNoVisuals() {
        if (yesButton != null) {
            ColorBlock cb = yesButton.colors;
            cb.normalColor = (isAttending == true) ? selectedColor : normalColor;
            yesButton.colors = cb;
        }

        if (noButton != null) {
            ColorBlock cb = noButton.colors;
            cb.normalColor = (isAttending == false) ? selectedColor : normalColor;
            noButton.colors = cb;
        }
    }

    private void OnSaveClicked() {
        if (isTimeExpired) return;

        string userNote = noteField != null ? noteField.text : "";
        SaveRSVPData(isAttending, userNote);

        CloseConfirmPopup();
    }

    private void SaveRSVPData(bool? attending, string note) {
        Debug.Log($"[RSVP Saved] Attending: {attending} | Message: {note}");
        // TODO: Send to API
    }

    private void OnTimeExpired() {
        isTimeExpired = true;

        CloseConfirmPopup();
        if (birdButton != null) birdButton.gameObject.SetActive(false);
        if (confirmTooltip != null) confirmTooltip.SetActive(false);

        Debug.Log("[RSVP] Time expired. Bird left!");
    }
    #endregion
}