using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace DrawDotGame
{
    public class GameplayUIManager : MonoBehaviour
    {
        public GameManager gameManager;
        public Text heartNumber;
        public Text levelText;
        public GameObject gameEndUI;
        public GameObject btnNext;
        public GameObject btnRetry;
        public GameObject btnRestart;
        public GameObject btnHint;
        public GameObject btnBack;
        public GameObject hintAlert;
        public GameObject heartShortageAlert;
        public AnimationClip showMenuPanel;
        public AnimationClip hideMenuPanel;

        [Header("Premium Features Buttons")]
        public GameObject btnWatchRewardedAd;
        public GameObject btnInAppPurchase;

        [Header("In-App Purchase Store")]
        public GameObject storeUI;

        [Header("Sharing-Specific")]
        public GameObject shareUI;
        public ShareUIController shareUIController;

        private bool hasWatchedAd;
        private bool isFirstWin;
        private const string HINT_ALERT_PPKEY = "SGLIB_HINT_ALERT";

        void OnEnable()
        {
            GameManager.GameEnded += OnGameEnded;
        }

        void OnDisable()
        {
            GameManager.GameEnded -= OnGameEnded;
        }

        // Use this for initialization
        void Start()
        {
            gameEndUI.SetActive(false);
            levelText.text = "level " + GameManager.LevelLoaded;
            btnHint.SetActive(true);
            btnRestart.SetActive(true);

            // Show or hide premium feature buttons
            btnInAppPurchase.SetActive(PremiumFeaturesManager.Instance.enablePremiumFeatures);

            // Hidden at start
            hintAlert.SetActive(false);
            heartShortageAlert.SetActive(false);
            storeUI.SetActive(false);
            shareUI.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            heartNumber.text = CoinManager.Instance.Coins.ToString();

            // Only show "watch rewarded ad" button if a rewarded ad is loaded and premium features are enabled, and ad was not watched earlier in this game
#if EASY_MOBILE && !UNITY_EDITOR
        if (PremiumFeaturesManager.Instance.enablePremiumFeatures && AdDisplayer.Instance.CanShowRewardedAd() && AdDisplayer.Instance.useRewardedAds && !hasWatchedAd)
        {
            btnWatchRewardedAd.SetActive(true);
        }
        else
        {
            btnWatchRewardedAd.SetActive(false);
        }
#elif UNITY_EDITOR
            btnWatchRewardedAd.SetActive(PremiumFeaturesManager.Instance.enablePremiumFeatures && !hasWatchedAd);
#endif
        }

        void OnGameEnded(bool isWin, bool firstWin)
        {
            if (isWin)
            {
                isFirstWin = firstWin;
                Invoke("ShowUIWhenWin", 1f);
            }
            else
            {
                Invoke("ShowUIWhenGameOver", 1f);
            }
        }

        void ShowUIWhenGameOver()
        {
            gameEndUI.SetActive(true);
            btnNext.SetActive(false);
            btnRetry.SetActive(true);

            // Whether to show share button (premium feature)

            //Get the img and showing up
            if (PremiumFeaturesManager.Instance.enablePremiumFeatures)
                ShowShareUI();
        }

        void ShowUIWhenWin()
        {
            // Award the user with hearts if needed
            if (isFirstWin && gameManager.heartsPerWin > 0)
            {
                CoinManager.Instance.AddCoins(gameManager.heartsPerWin);
                heartNumber.GetComponent<Animator>().Play("HeartIncrease");
                SoundManager.Instance.PlaySound(SoundManager.Instance.ping, true);
            }

            gameEndUI.SetActive(true);
            btnNext.SetActive(true);
            btnRetry.SetActive(false);
            ;

            // Whether to show share button (premium feature)

            //Get the img and showing up
            if (PremiumFeaturesManager.Instance.enablePremiumFeatures)
            {
                ShowShareUI();
            }
        }

        public void ShowStoreUI()
        {
            storeUI.SetActive(true);
            storeUI.GetComponentInChildren<Animator>().Play(showMenuPanel.name);
        }

        public void HideStoreUI()
        {
            StartCoroutine(CRHideStoreUI());
        }

        IEnumerator CRHideStoreUI()
        {
            storeUI.GetComponentInChildren<Animator>().Play(hideMenuPanel.name);
            yield return new WaitForSeconds(hideMenuPanel.length);
            storeUI.SetActive(false);
        }



        public void ShowShareUI()
        {
            if (!ScreenshotSharer.Instance.disableSharing)
            {
                StartCoroutine(CR_ActiveShareUI());
            }
        }

        IEnumerator CR_ActiveShareUI()
        {
            yield return new WaitForSeconds(0.6f);
#if EASY_MOBILE_PRO
            AnimatedClip clip = ScreenshotSharer.Instance.RecordedClip;
            shareUIController.AnimClip = clip;
#endif
            Texture2D texture = ScreenshotSharer.Instance.CapturedScreenshot;
            shareUIController.ImgTex = texture;
            yield return new WaitForSeconds(0.01f);
            shareUI.SetActive(true);
        }

        public void WatchRewardedAd()
        {
#if EASY_MOBILE && !UNITY_EDITOR
        // Hide the button
        btnWatchRewardedAd.SetActive(false);

        AdDisplayer.CompleteRewardedAdToEarnCoins += OnCompleteRewardedAdToEarnCoins;
        AdDisplayer.Instance.ShowRewardedAdToEarnCoins();
#elif UNITY_EDITOR
            // For testing in the editor
            // Already watch ad
            hasWatchedAd = true;
            // Give the reward!
            StartCoroutine(CRReward(3));
#endif
        }

        void OnCompleteRewardedAdToEarnCoins()
        {
#if EASY_MOBILE
            // Unsubscribe
            AdDisplayer.CompleteRewardedAdToEarnCoins -= OnCompleteRewardedAdToEarnCoins;

            // Already watch ad
            hasWatchedAd = true;

            // Give the reward!
            StartCoroutine(CRReward(AdDisplayer.Instance.rewardedHearts));
#endif
        }

        IEnumerator CRReward(int rewardValue)
        {
            Animator heartNumberAnim = heartNumber.GetComponent<Animator>();
            for (int i = 0; i < rewardValue; i++)
            {
                CoinManager.Instance.AddCoins(1);
                heartNumberAnim.Play("HeartIncrease");
                SoundManager.Instance.PlaySound(SoundManager.Instance.ping, true);
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void GoToHome()
        {
            StartCoroutine(LoadScene("FirstScene"));
        }

        public void Retry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ShowHint()
        {
            if (gameManager.ShowHint())
            {
                //if (PlayerPrefs.GetInt(HINT_ALERT_PPKEY, -99) == -99)
                if (JFLocalSave.Instance.GetInt(HINT_ALERT_PPKEY, -99) == -99)
                {
                    // Show hint alert.
                    hintAlert.SetActive(true);
                    hintAlert.GetComponentInChildren<Animator>().Play(showMenuPanel.name);

                    // Overwrite pp key.
                    //PlayerPrefs.SetInt(HINT_ALERT_PPKEY, 1);
                    JFLocalSave.Instance.SetInt(HINT_ALERT_PPKEY, 1);
                }
                btnHint.gameObject.SetActive(false);
            }
            else
            {
                // Not enough hearts to show hint, show message here.
                heartShortageAlert.SetActive(true);
                heartShortageAlert.GetComponentInChildren<Animator>().Play(showMenuPanel.name);
            }
        }

        public void HideHintAlert()
        {
            StartCoroutine(CRHideHintAlert());
        }

        public void HideHeartShortageAlert()
        {
            StartCoroutine(CRHideHeartShortageAlert());
        }

        IEnumerator CRHideHintAlert()
        {
            hintAlert.GetComponentInChildren<Animator>().Play(hideMenuPanel.name);
            yield return new WaitForSeconds(hideMenuPanel.length);
            hintAlert.SetActive(false);
        }

        IEnumerator CRHideHeartShortageAlert()
        {
            heartShortageAlert.GetComponentInChildren<Animator>().Play(hideMenuPanel.name);
            yield return new WaitForSeconds(hideMenuPanel.length);
            heartShortageAlert.SetActive(false);
        }

        public void NextLevel()
        {
            GameManager.LevelLoaded++;
            LevelScroller.levelSnapped = (int)((Mathf.Ceil(GameManager.LevelLoaded / (float)LevelScroller.LEVELS_PER_PACK) - 1) * LevelScroller.LEVELS_PER_PACK + 1);
            StartCoroutine(LoadScene(SceneManager.GetActiveScene().name));
        }

        IEnumerator LoadScene(string name)
        {
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene(name);
        }
    }
}
