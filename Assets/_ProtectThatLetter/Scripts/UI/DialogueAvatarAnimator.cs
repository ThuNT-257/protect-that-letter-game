using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ProtectThatLetter.UI
{
    public class DialogueAvatarAnimator : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private RectTransform leftAvatarRect;
        [SerializeField] private Image leftAvatarImage;

        [SerializeField] private RectTransform rightAvatarRect;
        [SerializeField] private Image rightAvatarImage;

        [SerializeField] private float avatarAnimDuration = 0.3f;
        [SerializeField] private float inactiveScale = 0.75f;
        [SerializeField] private float inactiveShiftX = 50f;
        [SerializeField] private Color activeAvatarColor = Color.white;
        [SerializeField] private Color inactiveAvatarColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        #endregion

        #region Private Fields
        private Vector2 leftAvatarDefaultPos;
        private Vector2 rightAvatarDefaultPos;
        private Coroutine leftAvatarCoroutine;
        private Coroutine rightAvatarCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            CacheReferencesAndPositions();
        }

        private void OnDisable()
        {
            SafeStopCoroutine(ref leftAvatarCoroutine);
            SafeStopCoroutine(ref rightAvatarCoroutine);
        }
        #endregion

        #region Public Methods
        public void AnimateAvatars(bool isLeftSpeaking, DialogueLine line, bool isFirstLine)
        {
            AnimateSingleAvatar(isLeft: true, isSpeaking: isLeftSpeaking, line: line, isFirstLine: isFirstLine);
            AnimateSingleAvatar(isLeft: false, isSpeaking: !isLeftSpeaking, line: line, isFirstLine: isFirstLine);
        }
        #endregion

        #region Private Methods
        private void CacheReferencesAndPositions()
        {
            if (leftAvatarRect != null)
            {
                leftAvatarDefaultPos = leftAvatarRect.anchoredPosition;
            }

            if (rightAvatarRect != null)
            {
                rightAvatarDefaultPos = rightAvatarRect.anchoredPosition;
            }
        }

        private void SafeStopCoroutine(ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        private void AnimateSingleAvatar(bool isLeft, bool isSpeaking, DialogueLine line, bool isFirstLine)
        {
            RectTransform avatarRect = isLeft ? leftAvatarRect : rightAvatarRect;
            Image avatarImg = isLeft ? leftAvatarImage : rightAvatarImage;
            Vector2 defaultPos = isLeft ? leftAvatarDefaultPos : rightAvatarDefaultPos;

            if (avatarRect == null || avatarImg == null) return;

            if (isSpeaking && line != null && !string.IsNullOrEmpty(line.avatar))
            {
                string avatarPath = line.avatar;

                if (avatarPath.Contains("."))
                {
                    avatarPath = System.IO.Path.GetFileNameWithoutExtension(avatarPath);
                }

                Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{avatarPath}");
                if (avatarSprite != null)
                {
                    avatarImg.sprite = avatarSprite;
                }
            }

            if (avatarImg.sprite == null)
            {
                avatarRect.gameObject.SetActive(false);
                return;
            }

            if (isFirstLine && !isSpeaking)
            {
                avatarRect.gameObject.SetActive(false);
                return;
            }

            avatarRect.gameObject.SetActive(true);

            Vector3 targetScale = isSpeaking ? Vector3.one : new Vector3(inactiveScale, inactiveScale, 1f);
            Vector2 targetPos = defaultPos;
            Color targetColor = isSpeaking ? activeAvatarColor : inactiveAvatarColor;

            if (!isSpeaking)
            {
                float shiftX = isLeft ? -inactiveShiftX : inactiveShiftX;
                targetPos.x += shiftX;

                float height = avatarRect.rect.height;
                float scaleDifference = 1f - inactiveScale;
                float bottomOffset = height * scaleDifference * avatarRect.pivot.y;
                targetPos.y -= bottomOffset;
            }

            if (isLeft)
            {
                SafeStopCoroutine(ref leftAvatarCoroutine);
                leftAvatarCoroutine = StartCoroutine(AnimateAvatarRoutine(avatarRect, avatarImg, targetScale, targetPos, targetColor, isSpeaking));
            }
            else
            {
                SafeStopCoroutine(ref rightAvatarCoroutine);
                rightAvatarCoroutine = StartCoroutine(AnimateAvatarRoutine(avatarRect, avatarImg, targetScale, targetPos, targetColor, isSpeaking));
            }
        }

        private IEnumerator AnimateAvatarRoutine(RectTransform avatarRect, Image img, Vector3 targetScale, Vector2 targetPos, Color targetColor, bool isSpeaking)
        {
            float elapsed = 0f;
            Vector3 startScale = avatarRect.localScale;
            Vector2 startPos = avatarRect.anchoredPosition;
            Color startColor = img != null ? img.color : Color.white;

            while (elapsed < avatarAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / avatarAnimDuration);

                float scaleT = isSpeaking ? EaseOutBack(t) : EaseOutCubic(t);

                avatarRect.localScale = Vector3.LerpUnclamped(startScale, targetScale, scaleT);
                avatarRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, EaseOutCubic(t));

                if (img != null)
                {
                    img.color = Color.Lerp(startColor, targetColor, t);
                }

                yield return null;
            }

            avatarRect.localScale = targetScale;
            avatarRect.anchoredPosition = targetPos;
            if (img != null)
            {
                img.color = targetColor;
            }
        }

        private float EaseOutBack(float x)
        {
            float c1 = 1.4f;
            float c3 = c1 + 4f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private float EaseOutCubic(float x)
        {
            return 1f - Mathf.Pow(1f - x, 3f);
        }
        #endregion
    }
}