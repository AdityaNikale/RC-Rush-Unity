using UnityEngine;
using UnityEngine.EventSystems;

namespace RCRush.UI
{
    /// <summary>
    /// Detects continuous press and release touch events for mobile UI buttons.
    /// </summary>
    public class TouchButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsPressed { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
        }

        private void OnDisable()
        {
            IsPressed = false;
        }
    }
}