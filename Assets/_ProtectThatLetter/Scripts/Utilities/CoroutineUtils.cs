using System;
using System.Collections;
using UnityEngine;

public class CoroutineUtils : MonoBehaviour {
    public static IEnumerator WaitUntilReady(Func<bool> readyCondition, float timeoutSeconds = 10f) {
        float timer = 0f;
        while (readyCondition != null && !readyCondition() && timer < timeoutSeconds) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
