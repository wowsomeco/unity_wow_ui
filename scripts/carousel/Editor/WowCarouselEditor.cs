using UnityEditor;
using UnityEngine;

namespace Wowsome {
  namespace UI {
    using EU = EditorUtils;

    [CustomEditor(typeof(WowCarousel))]
    public class WowCarouselEditor : Editor {
      public override void OnInspectorGUI() {

        DrawDefaultInspector();
        WowCarousel tgt = (WowCarousel)target;

        EU.Btn("Refresh", () => {
          Vector2 pivot = new Vector2(0f, .5f);

          Vector2 carouselSize = tgt.GetComponent<RectTransform>().Size();
          tgt.ItemContainer.SetWidth(carouselSize.x);
          tgt.ItemContainer.pivot = pivot;

          RectTransform itemRt = tgt.TemplateItem.GetComponent<RectTransform>();
          itemRt.pivot = pivot;
          itemRt.SetSize(carouselSize);

          EU.SetSceneDirty();
        });
      }
    }
  }
}

