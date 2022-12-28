using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace DrawDotGame
{
    public class LevelEditor : EditorWindow
    {
        private LevelManager levelManager;
        private int totalLevel;
        private bool createNewLevel = false;
        private bool loadExistingLevel = true;
        private bool groupDisable = false;
        private int levelNumber = 1;
        private int controlHeight = 26;
        private int smallButtonWidth = 40;

        private const string LevelEditorScenePath = "Assets/_DrawDotGame/Scenes/LevelEditor.unity";

        [MenuItem("Tools/Level Editor")]
        public static void ShowWindow()
        {
            // Ask for a change scene confirmation if not on level editor scene
            if (!EditorSceneManager.GetActiveScene().path.Equals(LevelEditorScenePath))
            {
                if (EditorUtility.DisplayDialog(
                        "Open Level Editor",
                        "Do you want to close the current scene and open LevelEditor scene? Unsaved changes in this scene will be discarded.", "Yes", "No"))
                {
                    EditorSceneManager.OpenScene(LevelEditorScenePath);
                    GetWindow(typeof(LevelEditor));
                }
            }
            else
            {
                GetWindow(typeof(LevelEditor));
            }
        }

        void Update()
        {
            // Check if is in LevelEditor scene.
            Scene activeScene = EditorSceneManager.GetActiveScene();

            // Auto exit if not in level editor scene.
            if (!activeScene.path.Equals(LevelEditorScenePath))
            {
                this.Close();
                return;
            }
        }

        void OnGUI()
        {
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
            }

            totalLevel = levelManager.GetTotalLevelNumber();

            // Disable the whole editor window if the game is in playing mode
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("TOTAL LEVEL: " + totalLevel.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Create new level section
            EditorGUI.BeginDisabledGroup(groupDisable);
            EditorGUI.BeginChangeCheck();

            createNewLevel = EditorGUILayout.BeginToggleGroup("Create New Level", createNewLevel);

            EditorGUILayout.LabelField("Next Level: " + (totalLevel + 1).ToString());

            if (GUILayout.Button("Clear Level", GUILayout.Height(controlHeight)))
            {
                levelManager.ClearScene();
            }

            if (GUILayout.Button("Save Level", GUILayout.Height(controlHeight)))
            {
                levelManager.SaveLevel(totalLevel + 1);
            }

            if (EditorGUI.EndChangeCheck())
            {
                loadExistingLevel = !createNewLevel;
            }
            EditorGUI.EndDisabledGroup();

            // Load existing level section
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            loadExistingLevel = EditorGUILayout.BeginToggleGroup("Load Existing Level", loadExistingLevel);

            if (levelNumber > 0 && levelNumber <= totalLevel)
            {
                ShowLevelScreenshot(levelNumber);
            }
            else
            {
                if (totalLevel <= 0)
                {
                    EditorGUILayout.HelpBox("You don't have any level.", MessageType.Warning, true);
                }
                else
                {
                    EditorGUILayout.HelpBox("Level not found! Please enter a valid level number.", MessageType.Error, true);
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(levelNumber <= 1);

            // Load previous level
            if (GUILayout.Button("←", GUILayout.Width(smallButtonWidth), GUILayout.Height(controlHeight)))
            {
                levelNumber--;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();

            // Show current level 
            levelNumber = EditorGUILayout.IntField(levelNumber);

            EditorGUI.BeginDisabledGroup(levelNumber >= totalLevel);

            // Load next level
            if (GUILayout.Button("→", GUILayout.Width(smallButtonWidth), GUILayout.Height(controlHeight)))
            {
                levelNumber++;
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                if (levelNumber > 0 && levelNumber <= totalLevel)
                {
                    levelManager.LoadLevel(levelNumber);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (GUILayout.Button("Overwrite Level", GUILayout.Height(controlHeight)))
            {
                // Ask for confirmation
                if (EditorUtility.DisplayDialog(
                        "Overwrite Level?",
                        "Are you sure you want to overwrite this level?",
                        "Yes, overwrite it",
                        "No"))
                {
                    if (levelNumber < 1 || levelNumber > totalLevel)
                        EditorUtility.DisplayDialog("Not Overwritten!", "Level number doesn't exist!!!", "OK");
                    else
                    {
                        levelManager.OverwriteLevel(levelNumber);
                        EditorUtility.DisplayDialog("Level Overwritten!", "Level " + levelNumber.ToString() + " was updated!", "OK");
                    }
                }
            }

            EditorGUILayout.EndToggleGroup();

            if (EditorGUI.EndChangeCheck())
            {
                createNewLevel = !loadExistingLevel;
            }

            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
        }

        void ShowLevelScreenshot(int level)
        {
            Sprite sprite = Resources.Load<Sprite>("Screenshots/" + level.ToString());
            Rect c = sprite.rect;
            float ratio = c.height / c.width;
            Texture tex = EditorGUIUtility.Load(LevelManager.ScreenshotPath(level)) as Texture;
            Rect rect = GUILayoutUtility.GetRect(LevelManager.ssWidth, LevelManager.ssWidth * ratio + 30);
            GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit);
        }
    }
}
