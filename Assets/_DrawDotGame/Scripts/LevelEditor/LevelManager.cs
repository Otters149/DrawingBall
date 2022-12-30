using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;



namespace DrawDotGame
{
    public class LevelManager : MonoBehaviour
    {
        public const string LEVELSOLVED_KEY = "LevelSolved";

        public static int ssWidth = 230;
        public static int ssHeight = 130;
        public static int bitType = 16;

        public List<Vector2> listLineRendererPos;

        public static string JsonPath()
        {
            string path = "Assets/_DrawDotGame/Resources/Json/LevelsData.json";
            return path;
        }

        public static string ScreenshotPath(int levelNumber)
        {
            string path = "Assets/_DrawDotGame/Resources/Screenshots/" + levelNumber.ToString() + ".png";
            return path;
        }

        public void ClearScene()
        {
            GameObject[] allObstacle = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (GameObject o in allObstacle)
            {
                DestroyImmediate(o);
            }

            BallController[] ballControllers = FindObjectsOfType<BallController>();
            foreach (BallController o in ballControllers)
            {
                DestroyImmediate(o.gameObject);
            }

            DestroyImmediate(GameObject.FindGameObjectWithTag("Hint"));
        }

        public void LoadLevel(int levelNumber)
        {
            ClearScene();
            StreamReader reader = new StreamReader(JsonPath());
            string[] data = reader.ReadToEnd().Split(';');
            GameManager gameManager = FindObjectOfType<GameManager>();
            LoadObject(levelNumber, data, gameManager.pinkBallPrefab, gameManager.blueBallPrefab, gameManager.hintPrefab, gameManager.obstacleManager);
            reader.Close();
        }

        public void OverwriteLevel(int levelNumber)
        {
            string[] data = null;
            StreamReader reader = new StreamReader(JsonPath());
            data = reader.ReadToEnd().Split(';');
            for (int i = 0; i < data.Length; i++)
            {
                LevelData levelData = JsonUtility.FromJson<LevelData>(data[i]);
                if (levelData.levelNumber == levelNumber)
                {
                    string dataToOverride = GetLevelData(levelNumber);
                    data[i] = "\n" + dataToOverride;
                    data[i].Trim();
                    break;
                }
            }
            reader.Close();
            OverrideJsonFile(data);
            TakeScreenshot(levelNumber);
        }

        void OverrideJsonFile(string[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append((i == data.Length - 1) ? (data[i]) : (data[i] + ";"));
            }

            using (StreamWriter writer = new StreamWriter(JsonPath(), false))
            {
                writer.Write(sb.ToString());
                writer.Close();
            }
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public void TakeScreenshot(int levelNumber)
        {
            StartCoroutine(SnapShot(ssWidth, ssHeight, levelNumber));
        }

        IEnumerator SnapShot(int width, int height, int levelNumber)
        {
            // Hide the hint
            GameObject hint = GameObject.FindGameObjectWithTag("Hint");
            hint.SetActive(false);

            // Capture screenshot using render texture
            RenderTexture rt = new RenderTexture(width, height, bitType, RenderTextureFormat.ARGB32);
            yield return new WaitForEndOfFrame();
            Camera.main.targetTexture = rt;
            Camera.main.Render();
            Camera.main.targetTexture = null;

            RenderTexture.active = rt;
            Texture2D tx = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tx.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tx.Apply();
            RenderTexture.active = null;
            DestroyImmediate(rt);

            byte[] bytes = tx.EncodeToPNG();
            File.WriteAllBytes(ScreenshotPath(levelNumber), bytes);
            hint.SetActive(true);
        }

        public int GetTotalLevelNumber()
        {
            if (File.Exists(JsonPath()))
            {
                StreamReader reader = new StreamReader(JsonPath());
                string[] data = reader.ReadToEnd().Split(';');
                reader.Close();
                return data.Length;
            }
            else
            {
                return 0;
            }
        }

        public void SaveLevel(int currentLevel)
        {
            BallController[] ballsController = FindObjectsOfType<BallController>();

            if (ballsController == null || ballsController.Length < 2)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("Level Not Saved", "There must be two balls in a level!", "OK");
#endif
                return;
            }
            else
            {
                if (currentLevel < 0)
                {
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.DisplayDialog("Error", "The level number must be a positive number!", "OK");
#endif
                    return;
                }
                else
                {
                    string data = GetLevelData(currentLevel);
                    AddDataForJson(data);
                    TakeScreenshot(currentLevel);

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.DisplayDialog("Level Saved", "Level " + currentLevel.ToString() + " was saved!", "OK");
#endif
                }
            }
        }

        string GetLevelData(int currentLevel)
        {
            BallController[] ballsController = FindObjectsOfType<BallController>();
            GameObject blueBall = null;
            GameObject pinkBall = null;
            foreach (BallController o in ballsController)
            {
                if (o.gameObject.name.Split('(')[0].Trim().Equals("PinkBall"))
                    pinkBall = o.gameObject;
                else
                    blueBall = o.gameObject;
            }

            GameObject[] allObstacle = GameObject.FindGameObjectsWithTag("Obstacle");
            GameObject hint = GameObject.FindGameObjectWithTag("Hint");

            List<ObstacleData> listObstacleData = new List<ObstacleData>();
            for (int i = 0; i < allObstacle.Length; i++)
            {
                ObstacleData obstacleData = new ObstacleData();
                string id = allObstacle[i].name.Split('(')[0].Trim();
                obstacleData.id = id;
                obstacleData.position = allObstacle[i].gameObject.transform.position;
                obstacleData.rotation = allObstacle[i].gameObject.transform.rotation;
                obstacleData.scale = allObstacle[i].gameObject.transform.localScale;

                ConveyorController carouselController = allObstacle[i].GetComponent<ConveyorController>();
                if (carouselController != null)
                {
                    obstacleData.rotateDirection = carouselController.rotateDirection;
                    obstacleData.rotatingSpeed = carouselController.rotateSpeed;
                }

                listObstacleData.Add(obstacleData);
            }
            HintData hintData = new HintData();
            hintData.position = hint.transform.position;
            hintData.rotation = hint.transform.rotation;
            hintData.scale = hint.transform.localScale;

            LevelData levelData = new LevelData();
            levelData.levelNumber = currentLevel;
            levelData.blueBallPosition = blueBall.transform.position;
            levelData.pinkBallPosition = pinkBall.transform.position;
            levelData.listObstacleData = listObstacleData;
            levelData.hintData = hintData;

            string data = JsonUtility.ToJson(levelData);
            return data;
        }

        void AddDataForJson(string data)
        {
            data.Trim();
            if (File.Exists(JsonPath())) //File already exists 
            {
                using (StreamWriter writer = new StreamWriter(JsonPath(), true))
                {
                    string currentData = ";\n" + data;
                    writer.Write(currentData);
                    writer.Close();
                }
            }
            else //File not exists -> create new json file
            {
                using (StreamWriter writer = new StreamWriter(JsonPath(), true))
                {
                    writer.Write(data);
                    writer.Close();
                }
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }


        public static void LoadObject(int levelNumber, string[] data, GameObject pinkBallPrefab, GameObject blueBallPrefab, GameObject hintPrefab, ObstacleManager obstacleManager)
        {
            for (int i = 0; i < data.Length; i++)
            {
                LevelData levelData = JsonUtility.FromJson<LevelData>(data[i]);
                if (levelData.levelNumber == levelNumber)
                {
                    //Create balls
                    Instantiate(pinkBallPrefab, levelData.pinkBallPosition, Quaternion.identity);
                    Instantiate(blueBallPrefab, levelData.blueBallPosition, Quaternion.identity);

                    //On level editor mode
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals("LevelEditor"))
                    {
                        //Create hint
                        GameObject hint = (GameObject)Instantiate(hintPrefab, levelData.hintData.position, levelData.hintData.rotation);
                        hint.transform.localScale = levelData.hintData.scale;
                        hint.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Hints/hint" + levelData.levelNumber.ToString());
                    }

                    foreach (ObstacleData o in levelData.listObstacleData)
                    {
                        foreach (GameObject a in obstacleManager.obstacles)
                        {
                            if (o.id.Trim().Equals(a.name.Trim()))
                            {
                                //Create obstacles 
                                GameObject obstacle = Instantiate(a, o.position, o.rotation) as GameObject;
                                obstacle.transform.localScale = o.scale;
                                ConveyorController carouselController = obstacle.GetComponent<ConveyorController>();
                                if (carouselController != null)
                                {
                                    carouselController.rotateDirection = o.rotateDirection;
                                    carouselController.rotateSpeed = o.rotatingSpeed;
                                }
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }

        //Check the given level is solved or not
        public static bool IsLevelSolved(int levelNumber)
        {
            string data = JFLocalSave.Instance.GetString(LEVELSOLVED_KEY, string.Empty);
            if (!string.IsNullOrEmpty(data))
            {
                string[] dataSplited = JFLocalSave.Instance.GetString(LEVELSOLVED_KEY).Split('_');
                for (int i = 0; i < dataSplited.Length; i++)
                {
                    if (dataSplited[i].Equals(levelNumber.ToString()))
                        return true;
                }
            }
            return false;
        }

        // Store the solved level number to PlayerPrefs to remember that it's solved
        public static void MarkLevelAsSolved(int level)
        {
            if (!IsLevelSolved(level))
            {
                string data = JFLocalSave.Instance.GetString(LEVELSOLVED_KEY);
                string newData;
                if (string.IsNullOrEmpty(data))
                {
                    newData = level.ToString();
                }
                else
                {
                    newData = JFLocalSave.Instance.GetString(LEVELSOLVED_KEY) + "_" + level.ToString();
                }
                JFLocalSave.Instance.SetString(LEVELSOLVED_KEY, newData);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Test/Mark all level as solved")]
        public static void MarkAllLevelAsSolved()
        {
            int levelCount = GetTotalLevelCount();
            for (int i = 0; i < levelCount; ++i)
            {
                MarkLevelAsSolved(i);
            }
            Debug.Log(string.Format("Marked {0} level(s) as solved!", levelCount));
        }
#endif

        public static int GetTotalLevelCount()
        {
            TextAsset textAsset = Resources.Load<TextAsset>(LevelScroller.JSON_PATH);
            string[] data = textAsset.ToString().Split(';');
            return data.Length;
        }

        public static bool IsMaxLevel(int levelNumber)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(LevelScroller.JSON_PATH);
            string[] data = textAsset.ToString().Split(';');
            return levelNumber == data.Length;
        }
    }
}
