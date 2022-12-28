using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace DrawDotGame
{
    public class LevelSelectionUIManager : MonoBehaviour
    {
        private static bool firstStart = true;

        public GameObject splashScreen;
        public GameObject levelDetailScrollview;
        public GameObject levelPackScrollview;
        public GameObject title;
        public Text heartNumber;
        public Button btnSoundOn;
        public Button btnSoundOff;
        public Button btnMusicOn;
        public Button btnMusicOff;
        public Button btnSwitch;
        public GameObject menuUI;
        public AnimationClip showMenuPanel;
        public AnimationClip hideMenuPanel;

        [Header("Splash Screen Display Config")]
        public bool showSplashScreen = true;
        public float splashScreenTime = 3f;

        [Header("Premium Features Buttons")]
        public GameObject btnRemoveAds;
        public GameObject btnRestorePurchase;

        private Animator menuAnimator;

        // Use this for initialization
        void Start()
        {
            if (firstStart && showSplashScreen)
            {
                firstStart = false;
                splashScreen.SetActive(true);
                StartCoroutine(CRHideSplashScreen(splashScreenTime));
            }
            else
            {
                splashScreen.SetActive(false);
            }

            menuAnimator = menuUI.GetComponentInChildren<Animator>();
            menuUI.SetActive(false);

            if (!LevelScroller.isLevelDetailActive)
            {
                title.SetActive(true);
                levelDetailScrollview.SetActive(false);
                btnSwitch.gameObject.SetActive(false);
                levelPackScrollview.SetActive(true);
            }
            else
            {
                title.SetActive(false);
                levelDetailScrollview.SetActive(true);
                btnSwitch.gameObject.SetActive(true);
                levelPackScrollview.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            heartNumber.text = CoinManager.Instance.Coins.ToString();
            UpdateSoundButtons();
            UpdateMusicButtons();
        }

        IEnumerator CRHideSplashScreen(float delay)
        {
            yield return new WaitForSeconds(delay);
            splashScreen.SetActive(false);

            if (!SoundManager.Instance.IsMusicOff() && SoundManager.Instance.MusicState != SoundManager.PlayingState.Playing)
            {
                SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
            }
        }

        public void PurchaseRemoveAds()
        {
#if EASY_MOBILE
            InAppPurchaser.Instance.Purchase(InAppPurchaser.Instance.removeAds);
#endif
        }

        public void RestorePurchase()
        {
#if EASY_MOBILE
            InAppPurchaser.Instance.RestorePurchase();
#endif
        }

        public void HandleExitButton()
        {
            StartCoroutine(HideMenuPanel());
        }

        IEnumerator HideMenuPanel()
        {
            menuAnimator.Play(hideMenuPanel.name);
            yield return new WaitForSeconds(hideMenuPanel.length);
            menuUI.SetActive(false);
        }

        public void HandleMenuButton()
        {
            // Whether to show premium-feature buttons
            bool enablePremium = PremiumFeaturesManager.Instance.enablePremiumFeatures;
            btnRemoveAds.SetActive(enablePremium);
            btnRestorePurchase.SetActive(enablePremium);

            menuUI.SetActive(true);
            menuAnimator.Play(showMenuPanel.name);
        }

        public void ToggleSound()
        {
            SoundManager.Instance.ToggleSound();
        }

        public void ToggleMusic()
        {
            SoundManager.Instance.ToggleMusic();
        }

        public void HandleSwitchButton()
        {
            //levelPackScrollview.GetComponent<BetterScrollview>().DisableInvisibleItems();
            title.SetActive(true);
            levelPackScrollview.SetActive(true);
            levelDetailScrollview.SetActive(false);
            btnSwitch.gameObject.SetActive(false);
            LevelScroller.isLevelDetailActive = false;
        }

        public void HandleLevelPackButton()
        {
            title.SetActive(false);
            levelPackScrollview.SetActive(false);
            levelDetailScrollview.SetActive(true);
            btnSwitch.gameObject.SetActive(true);
            LevelScroller.isLevelDetailActive = true;
        }

        void UpdateSoundButtons()
        {
            if (SoundManager.Instance.IsSoundOff())
            {
                btnSoundOn.gameObject.SetActive(false);
                btnSoundOff.gameObject.SetActive(true);
            }
            else
            {
                btnSoundOn.gameObject.SetActive(true);
                btnSoundOff.gameObject.SetActive(false);
            }
        }

        void UpdateMusicButtons()
        {
            if (SoundManager.Instance.IsMusicOff())
            {
                btnMusicOff.gameObject.SetActive(true);
                btnMusicOn.gameObject.SetActive(false);
            }
            else
            {
                btnMusicOff.gameObject.SetActive(false);
                btnMusicOn.gameObject.SetActive(true);
            }
        }
    }
}
