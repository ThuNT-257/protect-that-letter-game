using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProtectThatLetter.Controllers;

public class QuizUI : MonoBehaviour {
    #region Serialized Fields
    [Header("Panels")]
    [SerializeField] private GameObject quizOverlay;

    [Header("Question & Answers")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private List<Button> answerButtons = new List<Button>();
    [SerializeField] private List<TextMeshProUGUI> answerTexts = new List<TextMeshProUGUI>();
    [SerializeField] private Button returnButton;

    [Header("Countdown UI")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Button Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    #endregion

    #region Private Variables
    private Action<int> onAnswerSelectedCallback;
    #endregion

    #region Lifecycle
    private void Awake() {
        HidePanel();
        if (countdownText != null) {
            countdownText.gameObject.SetActive(false);
        }

        if (returnButton != null) {
            returnButton.onClick.AddListener(OnReturnButtonClicked);
        }
    }

    private void Start() {
        for (int i = 0; i < answerButtons.Count; i++) {
            int index = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerButtonClicked(index));
        }
    }

    private void OnDestroy() {
        if (returnButton != null) {
            returnButton.onClick.RemoveListener(OnReturnButtonClicked);
        }
    }
    #endregion

    #region Public Methods
    public void DisplayQuestion(string question, string[] options, System.Action<int> onAnswerSelected) {
        // Dừng lực đẩy và tạm khóa điều khiển shield khi hiển thị Quiz
        SetShieldState(canControl: false, freezeVelocity: true);

        ResetButtonColors();
        SetAllButtonsInteractable(true);

        if (questionText != null) {
            questionText.text = question;
        }

        for (int i = 0; i < answerButtons.Count; i++) {
            bool hasOption = options != null && i < options.Length;

            if (hasOption) {
                answerButtons[i].gameObject.SetActive(true);

                if (i < answerTexts.Count && answerTexts[i] != null) {
                    answerTexts[i].gameObject.SetActive(true);
                    answerTexts[i].text = options[i];
                }
            } else {
                if (answerButtons[i] != null) {
                    answerButtons[i].gameObject.SetActive(false);
                }

                if (i < answerTexts.Count && answerTexts[i] != null) {
                    answerTexts[i].gameObject.SetActive(false);
                }
            }
        }

        onAnswerSelectedCallback = onAnswerSelected;

        if (quizOverlay != null) {
            quizOverlay.SetActive(true);
        }
    }

    public void ShowAnswerResult(int selectedIndex, int correctIndex) {
        SetAllButtonsInteractable(false);

        if (selectedIndex == correctIndex) {
            SetButtonColor(answerButtons[selectedIndex], correctColor);
        } else {
            SetButtonColor(answerButtons[selectedIndex], wrongColor);
            SetButtonColor(answerButtons[correctIndex], correctColor);
        }
    }

    public IEnumerator StartCountdownRoutine(float countdownTime = 3f) {
        // Đảm bảo dừng lực đẩy và khóa điều khiển shield trong thời gian đếm ngược
        SetShieldState(canControl: false, freezeVelocity: true);

        if (countdownText == null) yield break;

        countdownText.gameObject.SetActive(true);
        float current = countdownTime;

        while (current > 0) {
            countdownText.text = Mathf.CeilToInt(current).ToString();
            yield return new WaitForSecondsRealtime(1f);
            current -= 1f;
        }

        countdownText.gameObject.SetActive(false);
        SetAllButtonsInteractable(true);

        // Mở lại quyền điều khiển shield sau khi đếm ngược kết thúc
        SetShieldState(canControl: true, freezeVelocity: false);
    }

    public void HidePanel() {
        if (quizOverlay != null) {
            quizOverlay.SetActive(false);
        }

        // Mở lại quyền điều khiển shield khi ẩn bảng Quiz UI
        SetShieldState(canControl: true, freezeVelocity: false);
    }
    #endregion

    #region Private Methods
    private void OnAnswerButtonClicked(int index) {
        onAnswerSelectedCallback?.Invoke(index);
    }

    private void OnReturnButtonClicked() {
        HidePanel();

        if (CollisionUI.Instance != null) {
            CollisionUI.Instance.ShowPanel();
        } else {
            CollisionUI collisionUI = FindFirstObjectByType<CollisionUI>(FindObjectsInactive.Include);
            if (collisionUI != null) {
                collisionUI.ShowPanel();
            }
        }
    }

    private void SetButtonColor(Button btn, Color color) {
        Image btnImage = btn.GetComponent<Image>();
        if (btnImage != null) {
            btnImage.color = color;
        }
    }

    private void ResetButtonColors() {
        foreach (Button btn in answerButtons) {
            SetButtonColor(btn, normalColor);
        }
    }

    private void SetAllButtonsInteractable(bool isInteractable) {
        foreach (Button btn in answerButtons) {
            if (btn != null && btn.gameObject.activeSelf) {
                btn.interactable = isInteractable;
            }
        }

        if (returnButton != null) {
            returnButton.interactable = isInteractable;
        }
    }

    private void SetShieldState(bool canControl, bool freezeVelocity) {
        ShieldController shield = FindFirstObjectByType<ShieldController>();
        if (shield != null) {
            if (freezeVelocity) {
                shield.FreezeShield();
            }
        }
    }
    #endregion
}