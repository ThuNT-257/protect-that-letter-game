using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LetterController : MonoBehaviour {
    #region Serialized Fields
    [Header("Envelope Settings")]
    [SerializeField] private Button envelopButton;
    [SerializeField] private CanvasGroup envelopCanvasGroup;

    [Header("Pages Setup")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;
    [SerializeField] private CanvasGroup pagesCanvasGroup; 

    [Header("Page Navigation Buttons")]
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    #endregion

    #region Private Fields
    private bool isTransitioning = false;
    #endregion

    #region Lifecycle
    private void Start() {
        if (envelopButton != null) {
            envelopButton.onClick.AddListener(OpenEnvelope);
        }

        if (nextPageButton != null) nextPageButton.onClick.AddListener(SwitchToPage2);
        if (prevPageButton != null) prevPageButton.onClick.AddListener(SwitchToPage1);

        InitState();
    }
    #endregion

    #region Private Methods
    private void InitState() {
        if (envelopCanvasGroup != null) {
            envelopCanvasGroup.alpha = 1f;
            envelopCanvasGroup.blocksRaycasts = true;
            envelopCanvasGroup.gameObject.SetActive(true);
        }

        if (page1 != null) page1.SetActive(true);
        if (page2 != null) page2.SetActive(true);
        if (page1 != null) page1.transform.SetAsLastSibling();

        if (pagesCanvasGroup != null) {
            pagesCanvasGroup.alpha = 0f;
            pagesCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OpenEnvelope() {
        if (isTransitioning) return;
        StartCoroutine(OpenEnvelopeRoutine());
    }

    private void SwitchToPage1() {
        if (isTransitioning) return;
        StartCoroutine(SwitchPageRoutine(page1));
    }

    private void SwitchToPage2() {
        if (isTransitioning) return;
        StartCoroutine(SwitchPageRoutine(page2));
    }

    private IEnumerator OpenEnvelopeRoutine() {
        isTransitioning = true;
        float timer = 0f;

        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            if (envelopCanvasGroup != null) envelopCanvasGroup.alpha = 1f - progress;
            if (pagesCanvasGroup != null) pagesCanvasGroup.alpha = progress;

            yield return null;
        }

        if (envelopCanvasGroup != null) {
            envelopCanvasGroup.alpha = 0f;
            envelopCanvasGroup.blocksRaycasts = false;
            envelopCanvasGroup.gameObject.SetActive(false);
        }

        if (pagesCanvasGroup != null) {
            pagesCanvasGroup.alpha = 1f;
            pagesCanvasGroup.blocksRaycasts = true;
        }

        isTransitioning = false;
    }

    private IEnumerator SwitchPageRoutine(GameObject targetPage) {
        isTransitioning = true;
        float timer = 0f;

        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            if (pagesCanvasGroup != null) {
                pagesCanvasGroup.alpha = 1f - (timer / fadeDuration);
            }
            yield return null;
        }

        if (pagesCanvasGroup != null) pagesCanvasGroup.alpha = 0f;

        if (targetPage != null) {
            targetPage.transform.SetAsLastSibling();
        }

        timer = 0f;
        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            if (pagesCanvasGroup != null) {
                pagesCanvasGroup.alpha = timer / fadeDuration;
            }
            yield return null;
        }

        if (pagesCanvasGroup != null) {
            pagesCanvasGroup.alpha = 1f;
            pagesCanvasGroup.blocksRaycasts = true;
        }

        isTransitioning = false;
    }
    #endregion
}