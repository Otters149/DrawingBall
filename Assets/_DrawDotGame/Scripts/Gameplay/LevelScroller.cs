using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DrawDotGame
{
    public class LevelScroller : MonoBehaviour
    {
        public const string JSON_PATH = "Json/LevelsData";

        private static int maxUnlockedLevel;
        public static int levelSnapped = 1;
        public const int LEVELS_PER_PACK = 6;
        public static bool isLevelDetailActive;

        public ScrollRect levelDetailScrollview;
        public ScrollRect levelPackScrollview;
        public GameObject levelDetailContent;
        public GameObject levelPackContent;
        public GameObject buttonGroupPrefab;
        public GameObject levelPackPrefab;

        [Header("Config")]
        public Color lockedColor;
        // How many percent of levels need to be solved to unlock next pack
        public int packCompletePercentage = 80;

        private string[] data;
        private const string MAX_UNLOCKED_LEVEL_PPK = "SGLIB_MAX_UNLOCKED_LEVEL";

        // Use this for initialization
        void Start()
        {
            //Get level data
            data = null;
            string path = JSON_PATH;
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            data = textAsset.ToString().Split(';');

            //Get level solved data
            //string[] levelSolvedData = PlayerPrefs.GetString(LevelManager.LEVELSOLVED_KEY).Split('_');
            string[] levelSolvedData = JFLocalSave.Instance.GetString(LevelManager.LEVELSOLVED_KEY).Split('_');
            int highestLevelSolved;

            //Find the highest level that solved
            if (levelSolvedData.Length == 1)
            {
                highestLevelSolved = 0;
            }
            else
            {
                highestLevelSolved = int.Parse(levelSolvedData[0].Trim());
                for (int i = 1; i < levelSolvedData.Length; i++)
                {
                    if (highestLevelSolved < int.Parse(levelSolvedData[i]))
                    {
                        highestLevelSolved = int.Parse(levelSolvedData[i]);
                    }
                }
            }

            //Find all levels that solved in the pack 
            float range = highestLevelSolved / LEVELS_PER_PACK;
            List<int> listLevelSolvedInRange = new List<int>();
            if (highestLevelSolved != 0)
            {
                foreach (string o in levelSolvedData)
                {
                    if (range > 0)
                    {
                        if (int.Parse(o) >= range * LEVELS_PER_PACK && int.Parse(o) <= (range + 1) * LEVELS_PER_PACK)
                        {
                            listLevelSolvedInRange.Add(int.Parse(o));
                        }
                    }
                    else
                    {
                        listLevelSolvedInRange.Add(int.Parse(o));
                    }
                }
            }

            //Handle level detail scrollview
            #region Handle Level Detail Scrollview   

            //Caculate level group
            float levelgroupNumber = Mathf.Ceil(data.Length / (float)buttonGroupPrefab.transform.childCount);
            ScrollToSelectedPack();
            int count = 0;

            //Generate level group, each level group has 6 levels 
            for (int i = 1; i <= levelgroupNumber; i++)
            {
                GameObject buttonGroup = Instantiate(buttonGroupPrefab, levelDetailContent.transform.position, Quaternion.identity) as GameObject;
                buttonGroup.transform.SetParent(levelDetailContent.transform);
                buttonGroup.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                int childCount = 0;
                for (int j = count; j < buttonGroup.transform.childCount * i; j++)
                {
                    if (j >= data.Length)
                    {
                        buttonGroup.transform.GetChild(childCount).gameObject.SetActive(false);
                    }
                    else
                    {
                        LevelData levelData = JsonUtility.FromJson<LevelData>(data[j]);
                        Transform theChild = buttonGroup.transform.GetChild(childCount);
                        theChild.GetComponentInChildren<Text>().text = levelData.levelNumber.ToString();

                        LevelButtonController levelButtonController = theChild.GetComponent<LevelButtonController>();
                        GameObject lockOb = theChild.Find("Lock").gameObject;
                        GameObject imgSolvedOb = theChild.Find("imgSolved").gameObject;
                        Image levelImg = theChild.Find("Mask").Find("Image").GetComponent<Image>();
                        RectTransform levelImgRT = levelImg.GetComponent<RectTransform>();
                        string file = Path.Combine(Application.persistentDataPath, levelData.levelNumber.ToString() + ".png");
                        //If this level is solved -> show the solved screenshot
                        if (LevelManager.IsLevelSolved(levelData.levelNumber) && File.Exists(file))
                        {
                            byte[] bytes = File.ReadAllBytes(file);
                            Texture2D tex2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                            tex2D.LoadImage(bytes);
                            float scaleFactor = levelImgRT.rect.height / tex2D.height;
                            levelImg.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.zero);
                            levelImg.SetNativeSize();
                            levelImg.transform.localScale = Vector3.one * scaleFactor;
                            imgSolvedOb.gameObject.SetActive(true);
                            lockOb.gameObject.SetActive(false);
                            maxUnlockedLevel = levelData.levelNumber;
                        }
                        else
                        {
                            Sprite sprite = Resources.Load<Sprite>("Screenshots/" + levelData.levelNumber.ToString());
                            float scaleFactor = levelImgRT.rect.height / sprite.rect.height;
                            levelImg.sprite = sprite;
                            levelImg.SetNativeSize();
                            levelImg.transform.localScale = Vector3.one * scaleFactor;
                            imgSolvedOb.gameObject.SetActive(false);

                            //Unlock all level from 0 to 6
                            if (levelData.levelNumber <= LEVELS_PER_PACK || levelData.levelNumber <= maxUnlockedLevel)
                            {
                                levelButtonController.isLocked = false;
                                lockOb.SetActive(false);
                                levelImg.color = Color.white;
                                maxUnlockedLevel = levelData.levelNumber;
                            }
                            //Unlock all levels in range
                            else if (levelData.levelNumber <= (range + 1) * LEVELS_PER_PACK)
                            {
                                levelButtonController.isLocked = false;
                                lockOb.SetActive(false);
                                levelImg.color = Color.white;
                                maxUnlockedLevel = levelData.levelNumber;
                            }
                            else //Check if reached complete percent -> unlock level
                            {
                                if (levelData.levelNumber <= (range + 2) * LEVELS_PER_PACK)
                                {
                                    float checkedPercent = Mathf.Ceil((packCompletePercentage * LEVELS_PER_PACK) / 100f);
                                    if (listLevelSolvedInRange.Count >= checkedPercent)
                                    {
                                        levelButtonController.isLocked = false;
                                        lockOb.SetActive(false);
                                        levelImg.color = Color.white;
                                        maxUnlockedLevel = levelData.levelNumber;
                                    }
                                    else
                                    {
                                        levelButtonController.isLocked = true;
                                        levelImg.color = lockedColor;
                                    }
                                }
                                else
                                {
                                    levelButtonController.isLocked = true;
                                    levelImg.color = lockedColor;
                                }
                            }
                        }
                        count++;
                    }
                    childCount++;
                }
            }

            levelDetailScrollview.GetComponent<BetterScrollview>().DisableInvisibleItems();
            #endregion

            #region Handle Level Pack Scrollview

            //Caculate pack number
            float packNumber = Mathf.Ceil(data.Length / (float)LEVELS_PER_PACK);
            for (int i = 1; i <= packNumber; i++)
            {
                GameObject levelPackTemp = Instantiate(levelPackPrefab, levelPackContent.transform.position, Quaternion.identity) as GameObject;
                levelPackTemp.transform.SetParent(levelPackContent.transform);
                levelPackTemp.transform.localScale = new Vector3(1, 1, 1);
                LevelPackController levelPackControl = levelPackTemp.GetComponent<LevelPackController>();
                Transform mainImgObj = levelPackTemp.transform.Find("MainImg");
                Image mainImg = mainImgObj.Find("Mask").Find("Image").GetComponent<Image>();
                RectTransform mainImgRT = mainImg.GetComponent<RectTransform>();

                int startLevelPack;
                int endLevelPack;

                if (i == 1)
                {
                    startLevelPack = i;
                    endLevelPack = LEVELS_PER_PACK;
                }
                else if (i == packNumber)
                {
                    startLevelPack = LEVELS_PER_PACK * (i - 1) + 1;
                    endLevelPack = data.Length;
                }
                else
                {
                    startLevelPack = LEVELS_PER_PACK * (i - 1) + 1;
                    endLevelPack = LEVELS_PER_PACK * i;
                }

                // Display level screenshot
                levelPackTemp.GetComponentInChildren<Text>().text = startLevelPack.ToString() + "-" + endLevelPack.ToString();
                string file = Path.Combine(Application.persistentDataPath, startLevelPack.ToString() + ".png");
                if (LevelManager.IsLevelSolved(startLevelPack) && File.Exists(file))
                {
                    byte[] bytes = File.ReadAllBytes(file);
                    Texture2D tex2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                    tex2D.LoadImage(bytes);
                    float scaleFactor = mainImgRT.rect.height / tex2D.height;
                    mainImg.sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.zero);
                    mainImg.SetNativeSize();
                    mainImg.transform.localScale = Vector3.one * scaleFactor;
                }
                else
                {
                    Sprite sprite = Resources.Load<Sprite>("Screenshots/" + startLevelPack.ToString());
                    float scaleFactor = mainImgRT.rect.height / sprite.rect.height;
                    mainImg.sprite = sprite;
                    mainImg.SetNativeSize();
                    mainImg.transform.localScale = Vector3.one * scaleFactor;
                }

                // Set lock or unlock
                if (endLevelPack <= maxUnlockedLevel)
                {
                    levelPackTemp.transform.Find("Lock").gameObject.SetActive(false);
                    mainImg.color = Color.white;
                    levelPackControl.isLocked = false;
                }
                else
                {
                    levelPackTemp.transform.Find("Lock").gameObject.SetActive(true);
                    mainImg.color = lockedColor;
                    levelPackControl.isLocked = true;
                }

                GridLayoutGroup grlLevelPack = levelPackContent.GetComponent<GridLayoutGroup>();
                float xSizeOfLevelPackContent = (grlLevelPack.cellSize.x + grlLevelPack.spacing.x) * (packNumber - 1);
                RectTransform levelPackContentRect = levelPackContent.GetComponent<RectTransform>();
                levelPackContentRect.sizeDelta = new Vector2(xSizeOfLevelPackContent, levelPackContentRect.sizeDelta.y);
            }

            //levelPackScrollview.GetComponent<BetterScrollview>().DisableInvisibleItems();

            #endregion

            // Check if a new pack has been unlocked
            //int oldMaxUnlockedLevel = PlayerPrefs.GetInt(MAX_UNLOCKED_LEVEL_PPK, LEVELS_PER_PACK);  // first pack's levels are unlocked from beginning
            int oldMaxUnlockedLevel = JFLocalSave.Instance.GetInt(MAX_UNLOCKED_LEVEL_PPK, LEVELS_PER_PACK);  // first pack's levels are unlocked from beginning

            if (maxUnlockedLevel > oldMaxUnlockedLevel)
            {
                //PlayerPrefs.SetInt(MAX_UNLOCKED_LEVEL_PPK, maxUnlockedLevel);
                JFLocalSave.Instance.SetInt(MAX_UNLOCKED_LEVEL_PPK, maxUnlockedLevel);
            }

            //BetterScrollview packScroller = levelPackScrollview.GetComponent<BetterScrollview>();
            //packScroller.SnapToElement((GameManager.LevelLoaded - 1) / LEVELS_PER_PACK);
            //packScroller.DisableInvisibleItems();
        }

        public void ScrollToSelectedPack()
        {
            float levelgroupNumber = Mathf.Ceil(data.Length / (float)buttonGroupPrefab.transform.childCount);

            GridLayoutGroup grlLevelDetailContent = levelDetailContent.GetComponent<GridLayoutGroup>();
            RectTransform levelDetailContentRect = levelDetailContent.GetComponent<RectTransform>();
            levelDetailContentRect = levelDetailContent.GetComponent<RectTransform>();

            // Set content width base on level group number
            float fixedWidth = grlLevelDetailContent.cellSize.x * (levelgroupNumber - 1) + grlLevelDetailContent.spacing.x * levelgroupNumber;
            levelDetailContentRect.sizeDelta = new Vector2(fixedWidth, levelDetailContentRect.sizeDelta.y);

            // Calculate the index of the current pack
            int curPackIndex = Mathf.FloorToInt(levelSnapped / LEVELS_PER_PACK);

            // Calculate the moving distance so that the pack is at screen center
            float difference = curPackIndex * (grlLevelDetailContent.cellSize.x + grlLevelDetailContent.spacing.x);
            levelDetailContentRect.GetComponent<RectTransform>().localPosition = new Vector3(-difference, 0, 0);

            levelDetailScrollview.GetComponent<BetterScrollview>().DisableInvisibleItems();
        }
    }
}
