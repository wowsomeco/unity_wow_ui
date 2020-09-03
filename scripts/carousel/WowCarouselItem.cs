using UnityEngine;
using UnityEngine.UI;

namespace Wowsome {
  namespace UI {
    [RequireComponent((typeof(RectTransform)), typeof(Image))]
    public class WowCarouselItem : MonoBehaviour {
      public Image Img;

      WowCarousel _controller;

      public RectTransform RectTransform { get; private set; }
      public int Index { get; private set; }
      public bool Active {
        set {
          gameObject.SetActive(value);
          Img.enabled = value;
        }
      }

      public void InitCarouselItem(WowCarousel controller, Sprite spr, int idx) {
        _controller = controller;

        Index = idx;
        RectTransform = GetComponent<RectTransform>();
        Vector2 rootSize = _controller.Size;
        RectTransform.SetPos(new Vector2(Index * rootSize.x, 0f));
        RectTransform.SetSize(rootSize);

        Img.sprite = spr;

        CGestureHandler gestureHandler = new CGestureHandler(gameObject);
        gestureHandler.SetTappable();
        gestureHandler.SetDraggable();

        gestureHandler.OnTapListeners += pos => _controller.OnTapItem(this);
        gestureHandler.OnStartSwipeListeners += ev => _controller.OnBeginDragItem(this);
        gestureHandler.OnSwipingListeners += (SwipeEventData ev) => _controller.OnDraggingItem(this, RectTransform.GetScaledPos(ev.Delta));
        gestureHandler.OnEndSwipeListeners += (SwipeEventData ev) => _controller.OnEndDragItem(this, ev);
      }
    }
  }
}
