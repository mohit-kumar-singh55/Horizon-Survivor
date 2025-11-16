using System;
using UnityEngine;

namespace MagicPigGames {
    [Serializable]
    public class VerticalProgressBar : ProgressBar {       
        protected override float SizeMin => rectTransform.sizeDelta.y * sizeMin; // Minimum size of the overlay bar.
        protected override float SizeMax => rectTransform.sizeDelta.y * sizeMax; // Maximum size of the overlay bar.
        protected override float CurrentOverlaySize => overlayBar.sizeDelta.y; // Current size of the overlay bar.

        protected override void SetBarValue(float value) 
        {
            var sizeDelta = overlayBar.sizeDelta;
            sizeDelta = new Vector2(sizeDelta.x, value);
            overlayBar.sizeDelta = sizeDelta;
        }
        
        // Check and adjust the RectTransform's stretching/anchor settings.
        protected override void CheckOverlayBarRectTransform()
        {
            if (overlayBar == null) return;
            if (overlayBar.anchorMin == new Vector2(0, 1)
                && overlayBar.anchorMax == new Vector2(1, 1)) return;

            overlayBar.anchorMin = new Vector2(0, 1); // Anchor to top left corner
            overlayBar.anchorMax = new Vector2(1, 1); // Stretch horizontally
        }
    }
}