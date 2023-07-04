using DBGA.ThroughScenes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DBGA.UI
{
    [DisallowMultipleComponent]
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        private Toggle animateGenerationToggle;

        public void LoadHandMadeMazeScene()
        {
            SceneManager.LoadScene("PrecomputedMazeScene");
        }

        public void LoadConnectedGeneratedMazeScene()
        {
            ThroughScenesParameters.animateGeneration = animateGenerationToggle.isOn;
            ThroughScenesParameters.useCompleteTilesListForGeneration = false;
            SceneManager.LoadScene("GeneratedMazeScene");
        }

        public void LoadDisconnectedGeneratedMazeScene()
        {
            ThroughScenesParameters.animateGeneration = animateGenerationToggle.isOn;
            ThroughScenesParameters.useCompleteTilesListForGeneration = true;
            SceneManager.LoadScene("GeneratedMazeScene");
        }
    }
}
