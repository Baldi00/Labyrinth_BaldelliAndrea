using System.Collections;
using UnityEngine;

namespace DBGA.Tiles
{
    [DisallowMultipleComponent]
    public class Fog : MonoBehaviour
    {
        [SerializeField]
        private Renderer fogRenderer;
        [SerializeField]
        private float animationDuration = 0.5f;

        private Coroutine animationCoroutine;
        private Material material;

        private bool isVisible;
        private bool playerExplored;

        public bool PlayerExplored
        {
            set
            {
                if (value)
                    Hide();

                playerExplored = value;
            }
        }

        void Awake()
        {
            material = fogRenderer.material;
            isVisible = true;
        }

        public void Hide()
        {
            if (playerExplored || !isVisible)
                return;

            isVisible = false;

            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(AnimateFog(1, 0));
        }

        public void Show()
        {
            if (playerExplored || isVisible)
                return;

            isVisible = true;

            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(AnimateFog(0, 1));
        }

        private IEnumerator AnimateFog(float startOpacity, float endOpacity)
        {
            float timer = 0;
            while (timer <= animationDuration)
            {
                material.SetFloat("_Opacity", Mathf.Lerp(startOpacity, endOpacity, timer / animationDuration));
                timer += Time.deltaTime;
                yield return null;
            }
            material.SetFloat("_Opacity", endOpacity);
        }
    }
}
