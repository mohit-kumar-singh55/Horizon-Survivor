using UnityEngine;
using UnityEngine.UI;

// Add this to a UI RawImage to make its texture scroll
// Note: Your RawImage's texture should have 'Wrap Mode' set to 'Repeat' in its import settings

namespace MagicPigGames
{
    [RequireComponent(typeof(RawImage))]
    public class ScrollingUVs : MonoBehaviour
    {
        [Header("Scroll")]
        public Vector2 scrollSpeed = new Vector2(0f, -0.1f);

        [Header("Wiggle")] 
        public bool wiggle = true;
        public Vector2 wiggleSpeed = new Vector2(0.05f, 0f);
        public Vector2 wiggleMagnitude = new Vector2(0.05f, 0.05f);
        public RawImage rawImage;

        private Vector2 _wiggle;
        private float _time;

        private void Start()
        {
            if (!wiggle) _wiggle = Vector2.zero;
        }
        
        private void Update()
        {
            CalculateWiggle();
            
            var effectiveSpeed = scrollSpeed + _wiggle;
            var offset = Time.time * effectiveSpeed;

            rawImage.uvRect = new Rect(offset, Vector2.one);
        }

        private void CalculateWiggle()
        {
            if (!wiggle) return;

            _time += Time.deltaTime;
            
            _wiggle = new Vector2(
                wiggleSpeed.x * (Mathf.Sin(_time) * wiggleMagnitude.x),
                wiggleSpeed.y * (Mathf.Sin(_time) * wiggleMagnitude.y)
            );
        }

        private void OnValidate()
        {
            if (rawImage == null)
                rawImage = GetComponent<RawImage>();
        }
    }
}