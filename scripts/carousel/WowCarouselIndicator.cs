using UnityEngine;
using UnityEngine.UI;

namespace Wowsome {
  namespace UI {
    [RequireComponent(typeof(Image))]
    public class WowCarouselIndicator : MonoBehaviour {
      public Image Img;
      public Color ColActive;
      public Color ColInactive;

      public bool Active {
        set {
          Img.color = value ? ColActive : ColInactive;
        }
      }
    }
  }
}

