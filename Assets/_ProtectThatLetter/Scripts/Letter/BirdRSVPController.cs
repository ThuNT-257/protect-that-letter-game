using ProtectThatLetter.Controllers;
using ProtectThatLetter.Managers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BirdRSVPController : MonoBehaviour {
    #region Serialized Fields
    [Header("Bird Interactive Button")]
    [SerializeField] private Button birdButton;

    [Header("Confirm Popup References")]
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Button returnButton;
    [SerializeField] private TMP_InputField noteField;
    [SerializeField] private Button saveButton;

    [Header("Selection Circle Visuals")]
    [SerializeField] private GameObject yesSelectionCircle;
    [SerializeField] private GameObject noSelectionCircle;

    [Header("Optional UI References")]
    [SerializeField] private LetterController letterController;

    [Header("Countdown Settings")]
    [SerializeField] private float durationSeconds = 120f;
    #endregion

    #region Private Fields
    private float timeRemaining;
    private bool isTimeExpired = false;
    private bool isPaused = false;

    private bool isLetterRead = false;

    private bool? isAttending = null;
    private bool? initialIsAttending = null;
    private string initialNote = "";

    private string accessCode = "";
    #endregion

    #region Lifecycle
    private void Start() {
        LoadAccessCodeFromPrefs();

        if (GuestDataManager.Instance != null) {
            if (!string.IsNullOrEmpty(GuestDataManager.Instance.AccessCode)) {
                accessCode = GuestDataManager.Instance.AccessCode;
            }

            SetLetterRead(GuestDataManager.Instance.IsLetterRead);
            SetInitialData(GuestDataManager.Instance.IsAttending, GuestDataManager.Instance.GuestNote);
        }

        if (birdButton != null) birdButton.onClick.AddListener(OpenConfirmPopup);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (yesButton != null) yesButton.onClick.AddListener(OnYesSelected);
        if (noButton != null) noButton.onClick.AddListener(OnNoSelected);
        if (returnButton != null) returnButton.onClick.AddListener(CloseConfirmPopup);

        if (noteField != null) noteField.onValueChanged.AddListener(OnNoteChanged);

        if (confirmPopup != null) confirmPopup.SetActive(false);

        ResetSelectionVisuals();
        ValidateSaveButtonState();

        Debug.Log($"[BirdRSVPController] Start - Initial isAttending: {(isAttending.HasValue ? isAttending.Value.ToString() : "null")}");

        timeRemaining = durationSeconds;
        StartCoroutine(CountdownRoutine());
    }

    private void OnDestroy() {
        if (birdButton != null) birdButton.onClick.RemoveListener(OpenConfirmPopup);
        if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveClicked);
        if (yesButton != null) yesButton.onClick.RemoveListener(OnYesSelected);
        if (noButton != null) noButton.onClick.RemoveListener(OnNoSelected);
        if (returnButton != null) returnButton.onClick.RemoveListener(CloseConfirmPopup);

        if (noteField != null) noteField.onValueChanged.RemoveListener(OnNoteChanged);
    }
    #endregion

    #region Public Methods
    public void SetLetterRead(bool isRead) {
        this.isLetterRead = isRead;
        Debug.Log($"[BirdRSVPController] SetLetterRead updated -> isLetterRead: {this.isLetterRead}");
    }

    public void SetAccessCode(string code) {
        this.accessCode = code;
    }

    public void SetInitialData(bool? initialAttending, string initialUserNote) {
        this.initialIsAttending = initialAttending;
        this.initialNote = initialUserNote ?? "";
        this.isAttending = initialAttending;

        if (noteField != null) {
            noteField.text = this.initialNote;
        }

        UpdateYesNoVisuals();
        ValidateSaveButtonState();
    }

    public void OpenConfirmPopup() {
        if (GuestDataManager.Instance != null) {
            isLetterRead = GuestDataManager.Instance.IsLetterRead;
        }

        Debug.Log($"[BirdRSVPController] OpenConfirmPopup requested. Conditions -> isTimeExpired: {isTimeExpired}, isLetterRead: {isLetterRead}");

        if (isTimeExpired) {
            Debug.LogWarning("[BirdRSVPController] Cannot open popup: Countdown expired.");
            return;
        }

        if (!isLetterRead) {
            Debug.LogWarning("[BirdRSVPController] Cannot open popup: Letter has not been read yet (isLetterRead == false).");
            return;
        }

        if (string.IsNullOrEmpty(accessCode)) {
            LoadAccessCodeFromPrefs();
        }

        isPaused = true;

        if (confirmPopup != null) confirmPopup.SetActive(true);

        UpdateYesNoVisuals();
        ValidateSaveButtonState();
    }

    public void CloseConfirmPopup() {
        Debug.Log("[BirdRSVPController] CloseConfirmPopup called.");
        if (confirmPopup != null) confirmPopup.SetActive(false);
        isPaused = false;
    }
    #endregion

    #region Private Methods
    private void LoadAccessCodeFromPrefs() {
        if (PlayerPrefs.HasKey("SavedAccessCode")) {
            accessCode = PlayerPrefs.GetString("SavedAccessCode");
        }
    }

    private IEnumerator CountdownRoutine() {
        while (timeRemaining > 0f) {
            if (!isPaused) {
                timeRemaining -= Time.unscaledDeltaTime;
            }
            yield return null;
        }

        OnTimeExpired();
    }

    private void OnYesSelected() {
        isAttending = true;
        UpdateYesNoVisuals();
        ValidateSaveButtonState();
    }

    private void OnNoSelected() {
        isAttending = false;
        UpdateYesNoVisuals();
        ValidateSaveButtonState();
    }

    private void OnNoteChanged(string text) {
        ValidateSaveButtonState();
    }

    private void ValidateSaveButtonState() {
        if (saveButton == null) return;

        if (!isAttending.HasValue) {
            saveButton.interactable = false;
            return;
        }

        string currentNote = noteField != null ? noteField.text.Trim() : "";

        bool hasAttendingChanged = isAttending != initialIsAttending;
        bool hasNoteChanged = currentNote != initialNote;

        saveButton.interactable = hasAttendingChanged || hasNoteChanged;
    }

    private void ResetSelectionVisuals() {
        if (yesSelectionCircle != null) yesSelectionCircle.SetActive(false);
        if (noSelectionCircle != null) noSelectionCircle.SetActive(false);
    }

    private void UpdateYesNoVisuals() {
        if (yesSelectionCircle != null) {
            yesSelectionCircle.SetActive(isAttending == true);
        }

        if (noSelectionCircle != null) {
            noSelectionCircle.SetActive(isAttending == false);
        }
    }

    private void OnSaveClicked() {
        if (GuestDataManager.Instance != null) {
            isLetterRead = GuestDataManager.Instance.IsLetterRead;
        }

        if (isTimeExpired || !isAttending.HasValue || !isLetterRead) {
            Debug.LogWarning($"[BirdRSVPController] OnSaveClicked blocked -> isTimeExpired: {isTimeExpired}, isAttending: {isAttending.HasValue}, isLetterRead: {isLetterRead}");
            return;
        }

        if (string.IsNullOrEmpty(accessCode)) {
            LoadAccessCodeFromPrefs();
        }

        string userNote = noteField != null ? noteField.text.Trim() : "";
        SaveRSVPData(isAttending.Value, userNote);
    }

    private void SaveRSVPData(bool attending, string note) {
        if (saveButton != null) saveButton.interactable = false;
        Debug.Log($"[BirdRSVPController] Sending RSVP payload -> Code: {accessCode}, Attending: {attending}, Note: {note}");
    }

    private void OnTimeExpired() {
        Debug.LogWarning("[BirdRSVPController] Countdown expired!");
        isTimeExpired = true;

        CloseConfirmPopup();
    }
    #endregion
}