using System.Collections;
using UnityEngine;

namespace ProtectThatLetter.Managers
{
    /// <summary>
    /// Controls the Bootstrap execution flow and delegates scene transitions to SceneController.
    /// </summary>
    [DisallowMultipleComponent]
    public class InitManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Splash Timing Settings")]
        [SerializeField, Range(0.5f, 5f)] private float splashHoldDuration = 1.5f;
        [SerializeField, Range(0.1f, 3f)] private float transitionDuration = 1.0f;
        #endregion

        #region Private Fields
        private Coroutine initRoutine;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Starts the initialization routine when the scene loads
        /// </summary>
        private void Start()
        {
            initRoutine = StartCoroutine(InitAndSplashRoutine());
        }

        /// <summary>
        /// Stops the initialization coroutine if the object is disabled
        /// </summary>
        private void OnDisable()
        {
            if (initRoutine != null)
            {
                StopCoroutine(initRoutine);
                initRoutine = null;
            }
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// Holds the splash screen for a fixed duration, then delegates the scene transition
        /// </summary>
        private IEnumerator InitAndSplashRoutine()
        {
            yield return new WaitForSecondsRealtime(splashHoldDuration);

            if (SceneController.Instance != null)
            {
                SceneController.Instance.LoadNextScene(transitionDuration);
            }
            else
            {
                Debug.LogError($"[{nameof(InitManager)}] SceneController.Instance is NULL! Cannot transition.");
            }

            initRoutine = null;
        }
        #endregion
    }
}