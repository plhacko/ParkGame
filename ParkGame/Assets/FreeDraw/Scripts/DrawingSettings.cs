using UnityEngine;

namespace FreeDraw
{
    // Helper methods used to set drawing settings
    public class DrawingSettings : MonoBehaviour
    {
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite activeSpritePressed;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Sprite inactiveSpritePressed;
    
        [SerializeField] private GameObject boundsObject;
        [SerializeField] private GameObject wallsObject;
        [SerializeField] private GameObject pathsObject;
        [SerializeField] private GameObject eraserObject;
        private GameObject activeSpriteGameObject;

        private void Start()
        {
            AudioManager.Instance.notificationsSource = Camera.main.GetComponent<AudioSource>();

            SetActiveSprite(boundsObject);
        }
    

        public static bool isCursorOverUI = false;
        public float Transparency = 1f;

        // Changing pen settings is easy as changing the static properties Drawable.Pen_Colour and Drawable.Pen_Width
        public void SetMarkerColour(Color new_color)
        {
            Drawable.Pen_Colour = new_color;
        }
        // new_width is radius in pixels
        public void SetMarkerWidth(int new_width)
        {
            Drawable.Pen_Width = new_width;
        }
        public void SetMarkerWidth(float new_width)
        {
            SetMarkerWidth((int)new_width);
        }

        public void SetTransparency(float amount)
        {
            Transparency = amount;
            Color c = Drawable.Pen_Colour;
            c.a = amount;
            Drawable.Pen_Colour = c;
        }


        // Call these these to change the pen settings
        public void SetMarkerRed()
        {
            Color c = Color.red;
            c.a = Transparency;
            SetMarkerColour(c);
            Drawable.drawable.SetPenBrush();
            SetActiveSprite(boundsObject);
        }
        public void SetMarkerGreen()
        {
            Color c = Color.green;
            c.a = Transparency;
            SetMarkerColour(c);
            Drawable.drawable.SetPenBrush();
        }
        
        public void SetMarkerYellow()
        {
            Color c = Color.yellow;
            c.a = Transparency;
            SetMarkerColour(c);
            Drawable.drawable.SetPenBrush();
            SetActiveSprite(pathsObject);
        }
        
        public void SetMarkerBlue()
        {
            Color c = Color.blue;
            c.a = Transparency;
            SetMarkerColour(c);
            Drawable.drawable.SetPenBrush();
            SetActiveSprite(wallsObject);
        }

        public void SetEraser()
        {
            SetMarkerColour(new Color(255f, 255f, 255f, 0f));
            SetActiveSprite(eraserObject);
        }

        public void PartialSetEraser()
        {
            SetMarkerColour(new Color(255f, 255f, 255f, 0.5f));
        }

        private void SetActiveSprite(GameObject go)
        {
            if (activeSpriteGameObject != null) {
                var currentImage = activeSpriteGameObject.GetComponent<UnityEngine.UI.Image>();
                var currentButton = activeSpriteGameObject.GetComponent<UnityEngine.UI.Button>();
                var currentSpriteState = currentButton.spriteState;

                currentImage.sprite = inactiveSprite;
                currentSpriteState.pressedSprite = inactiveSpritePressed;
                currentButton.spriteState = currentSpriteState;
            }

            var newImage = go.GetComponent<UnityEngine.UI.Image>();
            var newButton = go.GetComponent<UnityEngine.UI.Button>();
            var newSpriteState = newButton.spriteState;

            newImage.sprite = activeSprite;
            newSpriteState.pressedSprite = activeSpritePressed;
            newButton.spriteState = newSpriteState;

            activeSpriteGameObject = go;
        }
    }
}