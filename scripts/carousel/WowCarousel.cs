using System;
using System.Collections.Generic;
using UnityEngine;
using Wowsome.Chrono;

namespace Wowsome {
  namespace UI {
    [RequireComponent(typeof(RectTransform))]
    public class WowCarousel : MonoBehaviour {
      [Serializable]
      public class Item {
        public string ResourcePath;
        public bool IsMulti;
        public List<string> Urls = new List<string>();
      }

      public List<Item> Models = new List<Item>();
      public WowCarouselItem TemplateItem;
      public WowCarouselIndicator TemplateIndicator;
      public RectTransform ItemContainer;
      public bool IsAutoPlay = false;

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
        if (Mathf.Abs(deltaSlide.x) > 5f) {
          int direction = -Math.Sign(deltaSlide.x);
          _counter = (_counter + direction).Clamp(0, _items.Count - 1);
        }

        _timerSlide = new Timer(.3f);
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
            _timerSlide = null;
            Refresh();
          }
        }

        if (null != _timerAutoplay && !_timerAutoplay.UpdateTimer(dt)) {
          _timerAutoplay.Reset();

          float slideTime = .3f;
          _counter++;
          if (_counter >= _items.Count) {
            _counter = 0;
            ItemContainer.SetPos(Vector2.zero);
            Refresh();
          } else {
            _timerSlide = new Timer(slideTime);
            _last = ItemContainer.Pos();
            _targetSlide = -Size.x * _counter;
          }
        }
      }

      void Refresh() {
        // only render the necessary items for performance sake
        _items.Loop((item, i) => {
          item.Active = i <= _counter + 1 && i >= _counter - 1;
        });

        _indicators.Loop((ind, i) => {
          ind.Active = i == _counter;
        });

        if (IsAutoPlay) {
          _timerAutoplay = new Timer(2f);
        }
      }
    }
  }
}
