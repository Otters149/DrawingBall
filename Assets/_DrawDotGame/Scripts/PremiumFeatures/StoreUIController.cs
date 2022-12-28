using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace DrawDotGame
{
    public class StoreUIController : MonoBehaviour
    {
        public GameObject packPrefab;

        private Transform productList;

        // Use this for initialization
        void Start()
        {
            productList = transform.Find("Panel");

            for (int i = 0; i < InAppPurchaser.Instance.heartPacks.Length; i++)
            {
                InAppPurchaser.CoinPack pack = InAppPurchaser.Instance.heartPacks[i];
                GameObject newPack = Instantiate(packPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                Transform newPackTf = newPack.transform;
                newPackTf.Find("Value").GetComponent<Text>().text = pack.value.ToString();
                newPackTf.Find("PriceString").GetComponent<Text>().text = pack.priceString;
                newPackTf.SetParent(productList, true);
                newPackTf.localScale = Vector3.one;

                // Add button listener
                newPackTf.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SoundManager.Instance.PlaySound(SoundManager.Instance.button);

                        #if EASY_MOBILE
                        InAppPurchaser.Instance.Purchase(pack.productName);
                        #endif
                    });
            }
        }
    }
}
