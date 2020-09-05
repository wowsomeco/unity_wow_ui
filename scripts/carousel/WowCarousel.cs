using System;
using System.Collections.Generic;
using UnityEngine;
using Wowsome.Chrono;

namespace Wowsome {
  namespace UI {
    /// <summary>
    /// A Component for cycling through elements, like slideshow.
    /// for now, ideally the item needs to be more than 3, 
    /// otherwise it might look broken in terms of the swiping as well as the autoplay logic.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class WowCarousel : MonoBehaviour {
      /// <summary>
      /// The Carousel item model.
      /// </summary>
      [Serializable]
      public class Item {
        /// <summary>
        /// The Path to the Resources folder.
        /// </summary>
        public string ResourcePath;
        /// <summary>
        /// When set to true, it wil automagically find all the sprites in the ResourcePath.
        /// </summary>
        public bool IsMulti;
        /// <summary>
        ///  the Url to open when the item is tapped.
        /// </summary>        
        public List<string> Urls = new List<string>();
      }

      public List<Item> Models = new List<Item>();
      public WowCarouselItem TemplateItem;
      public WowCarouselIndicator TemplateIndicator;
      public RectTransform ItemContainer;
      /// <summary>
      /// The delta position of the end drag - the first time the item gets dragged.
      /// when exceeded, it will slide to the next or prev item accordingly.
      /// </summary>
      public float SlideThreshold = 5f;
      /// <summary>
      /// The duration of slide to the next / prev item in second
      /// e.g. 0.3f = 0.3 sec, 1f = 1 second, etc.
      /// </summary>
      public float SlideTime = .3f;
      /// <summary>
      /// When set to true, the carousel will automagically slide to the next item.
      /// </summary>
      public bool IsAutoPlay = false;
      /// <summary>
      /// The duration of autoplay slide to the next item in second
      /// </summary>
      [ConditionalHide("IsAutoPlay", true)]
      public float AutoPlayTime = 3f;

      RectTransform _rt;
      List<WowCarouselItem> _items = new List<WowCarouselItem>();
      List<WowCarouselIndicator> _indicators = new List<WowCarouselIndicator>();
      List<string> _urls = new List<string>();
      Timer _timerSlide = null;
      Timer _timerAutoplay = null;
      float _targetSlide = 0f;
      int _counter = 0;
      Vector2 _start = Vector2.zero;
      Vector2 _last = Vector2.zero;

      public Vector2 Size {
        get { return _rt.Size(); }
      }

      public void OnTapItem(WowCarouselItem item) {
        // FIXME: this one is still wrong...
        // e.g. item.Index might not be the same as _urls when there are multiple IsMulti Model.
        string url = _urls[0];
        if (_urls.Count > item.Index) {
          url = _urls[item.Index];
        }
        Application.OpenURL(url);
      }

      public void OnBeginDragItem(WowCarouselItem item) {
        _timerAutoplay = null;
        _timerSlide = null;
        _start = ItemContainer.Pos();
      }

      public void OnDraggingItem(WowCarouselItem item, Vector2 delta) {
        ItemContainer.AddPosX(delta.x);
      }

      public void OnEndDragItem(WowCarouselItem item, SwipeEventData ev) {
        _last = ItemContainer.Pos();

        Vector2 deltaSlide = _last - _start;
        if (Mathf.Abs(deltaSlide.x) > SlideThreshold) {
          int direction = -Math.Sign(deltaSlide.x);
          ChangeCounter(direction);
        }

        _timerSlide = new Timer(SlideTime);
        _targetSlide = -Size.x * _counter;
      }

      public void InitCarousel() {
        _rt = GetComponent<RectTransform>();

        ItemContainer.SetWidth(Models.Count * Size.x);

        List<Sprite> sprites = new List<Sprite>();
        Models.ForEach(m => {
          if (m.IsMulti) {
            Sprite[] spriteResources = Resources.LoadAll<Sprite>(m.ResourcePath);
            sprites.AddRange(spriteResources);
          } else {
            Sprite sprite = Resources.Load<Sprite>(m.ResourcePath);
            Debug.Assert(sprite != null, $"cant find sprite from resource path {m.ResourcePath}");
            sprites.Add(sprite);
          }

          _urls.AddRange(m.Urls);
        });

        for (int i = 0; i < sprites.Count; i++) {
          WowCarouselItem item = i == 0 ? TemplateItem : TemplateItem.gameObject.Clone<WowCarouselItem>(ItemContainer);
          item.InitCarouselItem(this, sprites[i], i);
          _items.Add(item);

          WowCarouselIndicator indicator = i == 0 ? TemplateIndicator : TemplateIndicator.gameObject.Clone<WowCarouselIndicator>(TemplateIndicator.transform.parent);
          _indicators.Add(indicator);
        }

        Refresh();
      }

      public void UpdateCarousel(float dt) {
        if (null != _timerSlide) {
          if (_timerSlide.UpdateTimer(dt)) {
            float delta = Mathf.Lerp(_last.x, _targetSlide, _timerSlide.GetPercentage());
            ItemContainer.SetPos(new Vector2(delta, 0f));
          } else {
            ItemContainer.SetPos(new Vector2(_targetSlide, 0f));
            _timerSlide = null;
            Refresh();
          }
        }

        if (null != _timerAutoplay && !_timerAutoplay.UpdateTimer(dt)) {
          _timerAutoplay.Reset();

          ChangeCounter(1);

          _timerSlide = new Timer(SlideTime);
          _last = ItemContainer.Pos();
          _targetSlide = -Size.x * _counter;
        }
      }

      void ChangeCounter(int delta) {
        _counter += delta;
        ValidateCounter();
      }

      void ValidateCounter() {
        // slide back to cur counter if the current counter exceeds the limit and
        // we only have 2 items or less.
        if (_items.Count < 3 && (_counter < 0 || _counter >= _items.Count)) {
          _counter = _counter.Clamp(0, _items.Count - 1);
        }
      }

      void Refresh() {
        // reset pos if the counter exceeds the list count or -1
        if (_counter >= _items.Count) {
          _counter = 0;
          ItemContainer.SetPos(Vector2.zero);
        } else if (_counter < 0) {
          _counter = _items.Count - 1;
          ItemContainer.SetPos(new Vector2(-_counter * Size.x, 0f));
        }
        // only render the necessary items for performance sake
        _items.Loop((item, i) => {
          // reset pos
          item.RectTransform.SetPos(new Vector2(i * Size.x, 0f));
          item.Active = i <= _counter + 1 && i >= _counter - 1;
        });
        // this logic below re-positions:
        // - the last item to the left of the first index if the cur counter is 0
        // - the first item to the right of the last index if the cur equals last index - 1
        // this only applies if the item has more than 2 items.        
        if (_items.Count >= 3) {
          if (_counter == 0) {
            _items[_items.Count - 1].RectTransform.SetPos(new Vector2(-Size.x, 0f));
            _items[_items.Count - 1].Active = true;
          } else if (_counter == _items.Count - 1) {
            _items[0].RectTransform.SetPos(new Vector2(_items.Count * Size.x, 0f));
            _items[0].Active = true;
          }
        }
        // activate indicators accordingly
        _indicators.Loop((ind, i) => ind.Active = i == _counter);
        // reset autoplay on refresh
        if (IsAutoPlay) {
          _timerAutoplay = new Timer(AutoPlayTime);
        }
      }
    }
  }
}
