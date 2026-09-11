using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GuideTextController : MonoBehaviour {
    [Header("Display Settings")]
    [SerializeField] private float fadeDuration = 0.8f; 

    private TextMeshProUGUI textMesh;
    private Coroutine fadeCoroutine;

    private void Awake() {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void ShowGuide() {
        gameObject.SetActive(true);
        Color c = textMesh.color;
        c.a = 1f;
        textMesh.color = c;
    }

    public void HideGuideWithFade() {
        if (!gameObject.activeSelf) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(RoutineFadeOut());
    }

    private IEnumerator RoutineFadeOut() {
        float elapsed = 0f;
        Color originalColor = textMesh.color;

        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            originalColor.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            textMesh.color = originalColor;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}