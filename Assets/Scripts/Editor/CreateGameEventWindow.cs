using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DBGA.Editor
{
    /// <summary>
    /// Editor utility window to automatically create a Game Event
    /// </summary>
    public class CreateGameEventWindow : EditorWindow
    {
        private const string templatePath = "Assets/Scripts/Editor/NewGameEventTemplate.txt";
        private const string helpBoxText = "Put each parameter on a single line with its type. For example:\n\n" +
                "Vector3 Position\n" +
                "Player Player\n" +
                "int AmmoCount\n" +
                "float Stamina";

        private string eventName = "NewGameEvent";
        private string parametersGUI = "";
        private string newEventPath;

        [MenuItem("Assets/Create/Game Event")]
        [MenuItem("DBGA/Create Game Event")]
        private static void OpenWindow()
        {
            GetWindow<CreateGameEventWindow>("Create Game Event").Show();
        }

        void OnGUI()
        {
            newEventPath = $"Assets/Scripts/EventSystem/CustomEvents/{eventName}.cs";

            bool wasGUIEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.TextField("Event path", newEventPath);
            GUI.enabled = wasGUIEnabled;

            eventName = EditorGUILayout.TextField("Event name", eventName);
            EditorGUILayout.LabelField("Event parameters");
            parametersGUI = EditorGUILayout.TextArea(parametersGUI);
            EditorGUILayout.HelpBox(helpBoxText, MessageType.Info);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Generate Game Event"))
                GenerateGameEvent();
        }

        private void GenerateGameEvent()
        {
            TextAsset template = AssetDatabase.LoadAssetAtPath<TextAsset>(templatePath);

            string[] parameters = parametersGUI.Split("\n");
            parameters = parameters
                .Select<string, string>(s => "public " + s.Replace(";", "") + " { set; get; }")
                .ToArray<string>();

            StringBuilder parametersFinal = new();
            if (parametersGUI != "")
            {
                foreach (string p in parameters)
                    parametersFinal.Append(p + "\n        ");

                parametersFinal.Remove(parametersFinal.ToString().LastIndexOf("\n"), 5);
            }

            File.WriteAllText(newEventPath, template.text
                .Replace("#EVENTNAME#", eventName)
                .Replace("#EVENTPARAMS#", parametersFinal.ToString()));

            AssetDatabase.ImportAsset(newEventPath);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<TextAsset>(newEventPath));
        }
    }
}