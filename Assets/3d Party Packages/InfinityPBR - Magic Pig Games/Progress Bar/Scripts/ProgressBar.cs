using System;
using System.Collections;
using UnityEngine;

namespace MagicPigGames
{
    [Serializable]
    public class ProgressBar : MonoBehaviour
    {
        [Header("Overlay Bar")]
        [Tooltip("This is the bar that moves to show progress, covering the fill bar.")]
        public RectTransform overlayBar;
        [Tooltip("Width as a percent of the Progress Bar Rect Transform.")]
        [Range(0f,1f)] public float sizeMin = 0f;
        [Tooltip("Width as a percent of the Progress Bar Rect Transform.")]
        [Range(0f,1f)] public float sizeMax = 1f;

        [Header("Options")] 
        [Tooltip("When true, the progress bar width will grow as the progress increases. When false, the progress bar width will shrink as the progress increases.")]
        public bool invertProgress = true;
        [Tooltip("When 0, the progress bar will update immediately. When greater than 0, the progress bar will take this many seconds to update.")]
        [Min(0f)]
        public float transitionTime = 0f;
        
        [Header("Plumbing")]
        public RectTransform rectTransform;
        
        private float _progress = 0f; // This is the runtime value for progress!
        protected float _elapsedTime = 0f;
        protected float _lastProgress = 0f;
        protected Vector2 _lastParentSize;
        protected Coroutine _transitionCoroutine;
        
        protected virtual float SizeAtCurrentProgress 
            => Mathf.Lerp(SizeMin, SizeMax, _progress);
        
        public virtual float Progress => _progress; // The 0-1 value of the progress bar.
        public virtual float ProgressPercent => _progress * 100f; // The 0-100 value of the progress bar.
        
        // Note: These are set for Horizontal, and will be overridden in VerticalProgressBar.
        protected virtual float SizeMin => rectTransform.sizeDelta.x * sizeMin; // Minimum size of the overlay bar.
        protected virtual  float SizeMax => rectTransform.sizeDelta.x * sizeMax; // Maximum size of the overlay bar.
        protected virtual  float CurrentOverlaySize => overlayBar.sizeDelta.x; // Current size of the overlay bar.

        /// <summary>
        /// Sets the progress bar to the given value between 0 and 1. Compute the percent and pass that in.
        /// </summary>
        /// <param name="progress"></param>
        public virtual void SetProgress(float progress)
        {
            if (progress is > 1 or < 0)
            {
                Debug.LogError($"Progress value must be between 0 and 1. Value was {progress}. Will clamp.");
                progress = Mathf.Clamp(progress, 0, 1);
            }
            
            _progress = ValueBasedOnInvert(progress);
            _lastProgress = _progress;
            
            // If we don't transition, set the bar directly
            if (transitionTime <= 0f)
            {
                SetBarValue(SizeAtCurrentProgress);
                return;
            }
            
            StartTheCoroutine();
        }

        protected float ValueBasedOnInvert(float value) => invertProgress ? 1 - value : value;

        protected virtual void LateUpdate() => HandleParentSizeChange();
        
        // Starts the coroutine, but stops the existing one if that is running, while also
        // inverting the _elapsedTime so that the transition continues from the current progress.
        private void StartTheCoroutine()
        {
            if (_transitionCoroutine != null)
            {
                _elapsedTime = transitionTime - _elapsedTime;
                StopCoroutine(_transitionCoroutine);
            }
            else
                _elapsedTime = 0f;
            
            _transitionCoroutine = StartCoroutine(TransitionProgress(CurrentOverlaySize));
        }

        // Check and adjust the RectTransform's stretching/anchor settings.
        protected virtual void CheckOverlayBarRectTransform()
        {
            if (overlayBar == null) return;
            if (overlayBar.anchorMin == new Vector2(1, 0) 
                && overlayBar.anchorMax == new Vector2(1, 1)) return;
            
            overlayBar.anchorMin = new Vector2(1, 0); // Anchor to top right corner
            overlayBar.anchorMax = new Vector2(1, 1); // Stretch vertically
        }

        // This handles situations where the parent size changes, to ensure the bar is always the correct size.
        protected virtual void HandleParentSizeChange()
        {
            if (_lastParentSize == rectTransform.sizeDelta)
                return;

            _lastParentSize = rectTransform.sizeDelta;

            // Inverts the progress back to normal before setting the progress.
            var progress = ValueBasedOnInvert(_progress);
            SetProgress(progress);
        }

        // Handles transitioning the overlay bar from one width to another.
        protected virtual IEnumerator TransitionProgress(float startWidth)
        {
            while (_elapsedTime < transitionTime)
            {
                var newWidth = Mathf.Lerp(startWidth, SizeAtCurrentProgress, _elapsedTime / transitionTime);
                SetBarValue(newWidth);
                
                _elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            _transitionCoroutine = null;
        }
        
        // Sets the size of the overlay bar in the direction of progress.
        protected virtual void SetBarValue(float value)
        {
            var sizeDelta = overlayBar.sizeDelta;
            sizeDelta = new Vector2(value, sizeDelta.y);
            overlayBar.sizeDelta = sizeDelta;
        }
        
        /*
         * This is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
         */
        protected virtual void OnValidate()
        {
            sizeMin = Mathf.Clamp(sizeMin, 0, sizeMax - 0.01f);
            sizeMax = Mathf.Clamp(sizeMax, sizeMin + 0.01f, 1);
            
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            if (overlayBar == null)
                Debug.Log("Please assign the Overlay Bar RectTransform.");

            CheckOverlayBarRectTransform();
            
            _lastParentSize = rectTransform.sizeDelta;
        }
    }

}
