using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DrawDotGame
{
    [RequireComponent(typeof(ScrollRect))]
    public class BetterScrollview : MonoBehaviour, IEndDragHandler // required interface when using the OnEndDrag method. 
    {
        [Header("Snapping Config")]
        public float snapTime = 0.3f;

        [Header("Flick Config")]
        public float flickSpeedThreshold = 5f;

        ScrollRect scrollRect;
        [HideInInspector]
        public RectTransform content;
        GridLayoutGroup contentGridLayoutGroup;
        IEnumerator snapCoroutine;


        Vector2 beginDragPos;
        Vector2 endDragPos;
        float beginDragTime;
        float endDragTime;
        float dragTime;
        int prevIndex = -1;

        public float ItemWidth
        {
            get
            {
                return contentGridLayoutGroup.cellSize.x + contentGridLayoutGroup.spacing.x;
            }
        }

        public float ElementCount
        {
            get
            {
                return content.childCount;
            }
        }

        void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            content = scrollRect.content;
            contentGridLayoutGroup = content.GetComponent<GridLayoutGroup>();

            // Disable inertia so that it won't affect our snapping 
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            scrollRect.inertia = false;
        }

        // Called when the user starts dragging the element this script is attached to..
        public void OnBeginDrag(PointerEventData data)
        {
            // Stop snapping if any
            if (snapCoroutine != null)
            {
                StopCoroutine(snapCoroutine);
                snapCoroutine = null;
            }

            beginDragTime = Time.time;
            beginDragPos = data.pressPosition;
        }

        // Called when the user stops dragging this UI Element.
        public void OnEndDrag(PointerEventData data)
        {
            float currentPosX = -content.localPosition.x;
            float itemWidth = contentGridLayoutGroup.cellSize.x + contentGridLayoutGroup.spacing.x;
            float rawIndex = currentPosX / itemWidth;

            // Taking into account drag speed
            float adjusment = 0;
            if (prevIndex > -1)
            {
                endDragTime = Time.time;
                dragTime = endDragTime - beginDragTime;
                endDragPos = data.pressPosition;
                float dragLength = endDragPos.x - beginDragPos.x;
                float dragSpeed = dragLength / dragTime;
                float speedAbs = Mathf.Abs(dragSpeed);

                if (speedAbs > flickSpeedThreshold)
                {
                    adjusment = (rawIndex > prevIndex) ? 0.5f : (rawIndex < prevIndex) ? -0.5f : 0;
                }
            }

            int index = Mathf.RoundToInt(rawIndex + adjusment);
            index = index < 0 ? 0 : index > content.childCount - 1 ? content.childCount - 1 : index;
            prevIndex = index;
            float newX = Mathf.Round(index) * itemWidth;
            Vector3 newPos = new Vector3(-newX, 0, 0);

            // Only displayed visible items
            DisableInvisibleItems(index);

            snapCoroutine = CRSnap(newPos, snapTime);
            StartCoroutine(snapCoroutine);
        }

        IEnumerator CRSnap(Vector3 newPos, float duration)
        {
            float timePast = 0;
            Vector3 startPos = content.localPosition;
            Vector3 endPos = newPos;

            while (timePast < duration)
            {
                timePast += Time.deltaTime;
                float factor = timePast / duration;
                content.localPosition = Vector3.Lerp(startPos, endPos, factor);
                yield return null;
            }

            SoundManager.Instance.PlaySound(SoundManager.Instance.tick);
        }

        public void DisableInvisibleItems(int index = -1)
        {
            if (content.childCount <= 3)
            {
                return;
            }

            // Find the centered child item.
            if (index < 0)
            {
                float currentPosX = -content.localPosition.x;
                float itemWidth = contentGridLayoutGroup.cellSize.x + contentGridLayoutGroup.spacing.x;
                index = Mathf.RoundToInt(currentPosX / itemWidth);
                index = index < 0 ? 0 : index > content.childCount - 1 ? content.childCount - 1 : index;
            }


            // Calculate the index of 3 currently centered children of the scrollview
            // Here we assume that the scrollview always has more than 3 child items.
            int first, middle, last;
            if (index == 0)
            {
                first = 0;
                middle = 1;
                last = 2;
            }
            else if (index == content.childCount - 1)
            {
                first = index - 2;
                middle = index - 1;
                last = index;
            }
            else
            {
                first = index - 1;
                middle = index;
                last = index + 1;
            }

            foreach (RectTransform child in content)
            {
                Image[] images = child.GetComponentsInChildren<Image>(true);
                Text[] texts = child.GetComponentsInChildren<Text>(true);

                int curIndex = child.GetSiblingIndex();
                bool isActive = curIndex == first || curIndex == middle || curIndex == last;

                foreach (Image img in images)
                {
                    img.enabled = isActive;
                }

                foreach (Text txt in texts)
                    txt.enabled = isActive;
                //child.gameObject.SetActive(isActive);
            }
        }

        public void SnapToElement(int elementIndex)
        {
            if (elementIndex < 0 || elementIndex >= ElementCount)
                return;
            Vector2 nextPos = elementIndex * ItemWidth * Vector2.left;
            content.anchoredPosition = nextPos;
        }
    }
}
