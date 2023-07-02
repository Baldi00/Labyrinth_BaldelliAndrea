using System.Collections;
using UnityEngine;

namespace DBGA.UI
{
    [DisallowMultipleComponent]
    public class InvalidMoveUIManager : MonoBehaviour
    {
        [SerializeField]
        private float messageDuration = 1f;

        private Coroutine waitThanHideMessageCoroutine;

        /// <summary>
        /// Shows the invalid move message than waits and hide it
        /// </summary>
        public void Show()
        {
            if (waitThanHideMessageCoroutine != null)
                StopCoroutine(waitThanHideMessageCoroutine);

            gameObject.SetActive(true);
            waitThanHideMessageCoroutine = StartCoroutine(WaitThenHideMessage());
        }

        private IEnumerator WaitThenHideMessage()
        {
            yield return new WaitForSecondsRealtime(messageDuration);
            gameObject.SetActive(false);
        }
    }
}
