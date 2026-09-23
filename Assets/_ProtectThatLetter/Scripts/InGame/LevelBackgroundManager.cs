using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct LevelColorData {
    public int levelIndex;
    public Color colorTop;
    public Color colorBottom;
}

public class LevelBackgroundManager : MonoBehaviour {
    #region Serialized Fields
    [Header("Components")]
    [SerializeField] private Renderer bgRendererOld;
    [SerializeField] private Renderer bgRendererNew;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Transition Settings")]
    [SerializeField] private GameObject bigCloudPrefab;
    [SerializeField] private float cloudSpeed = 4f;

    [Header("Text Anim Settings")]
    [SerializeField] private float textFadeInDuration = 0.5f;
    [SerializeField] private float textHoldDuration = 1.5f;
    [SerializeField] private float textFadeOutDuration = 0.5f;

    [Header("Level Palettes")]
    [SerializeField] private List<LevelColorData> levelColors;
    #endregion

    #region Private Fields
    private MaterialPropertyBlock propBlock;
    private LevelColorData currentPalette;
    private Coroutine textAnimCoroutine;
    #endregion

    #region Lifecycle
    private void Awake() {
        propBlock = new MaterialPropertyBlock();

        if (levelText != null) {
            levelText.text = "";
            SetTextAlpha(0f);
        }
    }
    #endregion

    #region Public Methods
    public void ChangeLevel(int nextLevelIndex) {
        LevelColorData targetPalette = GetPaletteForLevel(nextLevelIndex);

        if (nextLevelIndex > 0 && levelText != null) {
            if (textAnimCoroutine != null) StopCoroutine(textAnimCoroutine);
            textAnimCoroutine = StartCoroutine(AnimateLevelTextRoutine(nextLevelIndex.ToString()));
        }

        if (nextLevelIndex == 0) {
            currentPalette = targetPalette;
            SetMaterialColor(bgRendererOld, currentPalette);

            if (bgRendererNew != null) {
                bgRendererNew.gameObject.SetActive(false);
            }
            return;
        }

        if (bigCloudPrefab != null) {
            if (bgRendererNew != null) {
                bgRendererNew.gameObject.SetActive(true);
                SetMaterialColor(bgRendererNew, targetPalette);
            }
            TriggerCloudTransition(targetPalette);
        } else {
            OnTransitionComplete(targetPalette);
        }
    }

    public void OnTransitionComplete(LevelColorData newPalette) {
        currentPalette = newPalette;
        SetMaterialColor(bgRendererOld, currentPalette);

        if (bgRendererNew != null) {
            bgRendererNew.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Private Methods
    private IEnumerator AnimateLevelTextRoutine(string newText) {
        SetTextAlpha(0f);
        levelText.text = newText;

        float timer = 0f;
        while (timer < textFadeInDuration) {
            timer += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(0f, 1f, timer / textFadeInDuration));
            yield return null;
        }
        SetTextAlpha(1f);

        yield return new WaitForSeconds(textHoldDuration);

        timer = 0f;
        while (timer < textFadeOutDuration) {
            timer += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(1f, 0f, timer / textFadeOutDuration));
            yield return null;
        }
        SetTextAlpha(0f);
    }

    private void SetTextAlpha(float alpha) {
        if (levelText == null) return;
        Color c = levelText.color;
        c.a = alpha;
        levelText.color = c;
    }

    private void TriggerCloudTransition(LevelColorData targetPalette) {
        if (Camera.main == null) return;

        // Instantiate tạm thời, BigTransitionCloud sẽ tự căn chỉnh tọa độ spawn ẩn trên camera
        GameObject cloudObj = Instantiate(bigCloudPrefab, Vector3.zero, Quaternion.identity);
        BigTransitionCloud cloudScript = cloudObj.GetComponent<BigTransitionCloud>();

        if (cloudScript != null) {
            cloudScript.InitTransition(targetPalette, this, cloudSpeed);
        }
    }

    private void SetMaterialColor(Renderer rend, LevelColorData palette) {
        if (rend == null) return;

        Color top = palette.colorTop;
        Color bottom = palette.colorBottom;
        top.a = 1f;
        bottom.a = 1f;

        rend.GetPropertyBlock(propBlock);
        propBlock.SetColor("_ColorTop", top);
        propBlock.SetColor("_ColorBottom", bottom);
        rend.SetPropertyBlock(propBlock);
    }

    private LevelColorData GetPaletteForLevel(int level) {
        foreach (var data in levelColors) {
            if (data.levelIndex == level) {
                return data;
            }
        }

        return new LevelColorData {
            levelIndex = level,
            colorTop = new Color(0.2f, 0.5f, 0.9f),
            colorBottom = new Color(0.9f, 0.6f, 0.4f)
        };
    }
    #endregion
}