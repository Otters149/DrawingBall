using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    string appID = "";
    string iosAppID = "ios_app_id";
    string androidAppID = "63ae87e07330a23adfba08fa";
    string windowsAppID = "windows_app_id";
    string placementID;
    public bool Initialized { get; private set; }
    public bool BannerAvailable { get; private set; }

    string bannerPlacement = "AND_BANNER-5703433";
    string intersPlacement = "AND_INTERSTITIAL-3926374";
    void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
#if UNITY_IPHONE
        appID = iosAppID;
        placementID = "ios_placement_id";
#elif UNITY_ANDROID
        appID = androidAppID;
        placementID = "android_placement_id";
#elif UNITY_WSA_10_0 || UNITY_WINRT_8_1 || UNITY_METRO
        appID = windowsAppID;
        placementID = "windows_placement_id";
#endif
        
        Vungle.init(appID);
        Vungle.onInitializeEvent += () =>
        {
            JFLog.Debug(JFLog.LogTag.ADS, "Vungle Initialized");
            Initialized = true;
            initializeEventHandlers();
            //Vungle.loadAd("AND_BANNER-5703433");
            //Vungle.loadAd("AND_REWARED-7696442");
        };
    }

    public void ShowInterstitial()
    {
        
        if (Vungle.isAdvertAvailable(intersPlacement))
        {
            //Vungle.loadAd(inters);
            Vungle.playAd(intersPlacement);
        }
        else
        {
            JFLog.Error(JFLog.LogTag.ADS, "Interstitial not available");
            Vungle.loadAd(intersPlacement);
            Vungle.playAd(intersPlacement);
        }
    }

    public void ShowBanner(Vungle.VungleBannerPosition pos)
    {
        if (!Vungle.isAdvertAvailable(bannerPlacement, Vungle.VungleBannerSize.VungleAdSizeBanner))
        {
            JFLog.Debug(JFLog.LogTag.ADS, "Banner not available. Start load again!");
            Vungle.loadBanner(bannerPlacement, Vungle.VungleBannerSize.VungleAdSizeBanner, pos);
        }
        JFLog.Debug(JFLog.LogTag.ADS, "Show banner");
        Vungle.showBanner(bannerPlacement);
    }

    public void CloseBanner()
    {
        Vungle.closeBanner(bannerPlacement);
    }

    void initializeEventHandlers()
    {
        Vungle.onAdStartedEvent += (placementID) => {
            Debug.Log("Ad " + placementID + " is starting!  Pause your game  animation or sound here.");
        };

        Vungle.adPlayableEvent += (placementID, adPlayable) => {
            Debug.Log("Ad's playable state has been changed! placementID " + placementID + ". Now: " + adPlayable);
            if (placementID == bannerPlacement && adPlayable)
            {
                BannerAvailable = true;
            }
        };

        //For iOS and Android only
        Vungle.onAdClickEvent += (placementID) => {
            Debug.Log("onClick - Log: " + placementID);
        };

        //For iOS and Android only
        Vungle.onAdRewardedEvent += (placementID) => {
            Debug.Log("onAdRewardedEvent - Log: " + placementID);
        };

        //For iOS and Android only
        Vungle.onAdEndEvent += (placementID) => {
            Debug.Log("onAdEnd - Log: " + placementID);
        };
    }
}
