using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wowsome {
  namespace UI {
    public class WowCheckbox : MonoBehaviour, IPointerClickHandler {
      public delegate void EvTap(bool state);

      public Image ImgCheck;

      bool _state;
      EvTap _onChange;

      public bool State {
        get { return _state; }
        set {
          _state = value;
          ImgCheck.gameObject.SetActive(_state);
        }
      }

      public void InitCheckbox(bool initialState, EvTap onChange) {
        State = initialState;
        _onChange = onChange;
      }

      public void OnPointerClick(PointerEventData eventData) {
        State = !State;
        _onChange(State);
      }
    }
  }
}

