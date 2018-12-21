using UnityEngine;
using UnityEngine.UI;

public class CarbonColorChooser : MonoBehaviour
{

    //private RectTransform _imageRect;

    private int _lastS = 254;
    private int _lastV = 254;

    //private RectTransform _SVimageRect;

    [SerializeField]
    private Image HueCursor;

    [SerializeField]
    private Image HueWheel;

    [SerializeField]
    private RectTransform SVCursor;

    [SerializeField]
    private Image SVImage;

    [SerializeField]
    private UpdateValues updateValues;

    public Color color { get { return updateValues.color; } }
    public string hexColor { get { return updateValues.hexColor; } }

    // Use this for initialization
    private void Start()
    {
        //_imageRect = HueWheel.GetComponent<RectTransform>();
        //_SVimageRect = SVImage.GetComponent<RectTransform>();

     
        updateValues.onColorChanged = UpdateColorFromValues;
        UpdateCursor(new Vector3(HueWheel.sprite.texture.width, HueWheel.sprite.texture.height/2));
    }

    public void SetColor(Color color)
    {
        UpdateSVTexture(color);
        updateValues.SetValues(color);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3[] cornerPoints = new Vector3[4];
            HueWheel.rectTransform.GetWorldCorners(cornerPoints);

            Vector3[] cornerPointsSV = new Vector3[4];
            SVImage.rectTransform.GetWorldCorners(cornerPointsSV);

            Rect cornerRect = new Rect(cornerPoints[0], cornerPoints[2] - cornerPoints[0]);
            Rect cornerRectSV = new Rect(cornerPointsSV[0], cornerPointsSV[2] - cornerPointsSV[0]);

            var imageRectsizeDelta = new Vector2(cornerPoints[2].x - cornerPoints[1].x, cornerPoints[1].y - cornerPoints[0].y);
            //Pre test if mouse in hue range
            if (cornerRect.Contains(Input.mousePosition) && !cornerRectSV.Contains(Input.mousePosition))
            {
                var imagePosition = Input.mousePosition - new Vector3(cornerRect.position.x, cornerRect.position.y);

                //transform selectedImagePosition to real image data
                imagePosition = new Vector3(
                    (int)(imagePosition.x / (imageRectsizeDelta.x) * HueWheel.sprite.texture.width),
                    (int)(imagePosition.y / (imageRectsizeDelta.y) * HueWheel.sprite.texture.height));

                // Test if in ring
                if (IsInCircle(new Vector2(imagePosition.x, imagePosition.y), new Vector2(HueWheel.sprite.texture.width, HueWheel.sprite.texture.height), 400)
                    && !IsInCircle(new Vector2(imagePosition.x, imagePosition.y), new Vector2(HueWheel.sprite.texture.width, HueWheel.sprite.texture.height), 300))
                {
                    var col = HueWheel.sprite.texture.GetPixel((int)imagePosition.x, (int)imagePosition.y);
                    col.a = 1;

                    UpdateSVTexture(col);

                    var color = SVImage.sprite.texture.GetPixel(_lastS, _lastV);
                    updateValues.SetValues(color);
                    SVCursor.gameObject.SetActive(true);

                    UpdateCursor(imagePosition);
                }
            }
            var SVimageRectsizeDelta = new Vector2(cornerPointsSV[2].x - cornerPointsSV[1].x, cornerPointsSV[1].y - cornerPointsSV[0].y);
            //Test if mouse in SV image space
            if (cornerRectSV.Contains(Input.mousePosition))
            {
                var imagePosition = Input.mousePosition - new Vector3(cornerRectSV.position.x, cornerRectSV.position.y);

                imagePosition = new Vector3(
                    (int)(imagePosition.x / (SVimageRectsizeDelta.x) * SVImage.sprite.texture.width),
                    (int)(imagePosition.y / (SVimageRectsizeDelta.y) * SVImage.sprite.texture.height));

                _lastS = (int)Mathf.Clamp((imagePosition.x / SVImage.sprite.texture.width * 255), 0, 255);
                _lastV = (int)Mathf.Clamp((imagePosition.y / SVImage.sprite.texture.height * 255), 0, 255);

                SVCursor.position = Input.mousePosition;
                SVCursor.gameObject.SetActive(true);

                var col = SVImage.sprite.texture.GetPixel((int)imagePosition.x, (int)imagePosition.y);
                
                updateValues.SetValues(col);
                SVCursor.gameObject.SetActive(true);
            }
        }
    }

    private bool IsInCircle(Vector2 point, Vector2 textureDimension, float radius)
    {
        return Mathf.Pow(point.x - textureDimension.x / 2, 2) + Mathf.Pow(point.y - textureDimension.y / 2, 2) < Mathf.Pow(radius, 2);
    }

    private void UpdateColorFromValues(float angle, Color color)
    {
        UpdateSVTexture(color);
        HueCursor.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        SVCursor.gameObject.SetActive(false);
    }

    /// <summary>
    /// Update current SV texture from given color hue.
    /// </summary>
    /// <param name="color"></param>
    private void UpdateSVTexture(Color color)
    {
        var resultTexture = CalculateSVTexture(color);

        SVImage.sprite = Sprite.Create(resultTexture, new Rect(0, 0, resultTexture.width, resultTexture.height), new Vector2(0.5f, 0.5f));
        SVImage.sprite.texture.Apply();
        SVImage.sprite.name = "HSV Map";
    }

    /// <summary>
    /// Update cursor "HueCursor" rotation from selected image position in ring.
    /// </summary>
    /// <param name="selectedImagePosition"></param>
    private void UpdateCursor(Vector3 selectedImagePosition)
    {
        Vector3 direction = selectedImagePosition - new Vector3(HueWheel.sprite.texture.width / 2, HueWheel.sprite.texture.height / 2);
        float angle = Vector3.Angle(direction, Vector3.left);

        if (HueCursor != null)
        {
            HueCursor.transform.rotation = Quaternion.Euler(direction.y >= 0 ? new Vector3(0, 0, -angle) : new Vector3(0, 0, angle));
        }
    }

    private static Texture2D CalculateSVTexture(Color inColor)
    {
        float h, s, v;

        Color.RGBToHSV(inColor, out h, out s, out v);
        Texture2D texture = new Texture2D(255, 255);

        for (int x = 1; x < texture.width; x++)
        {
            for (int y = 1; y < texture.height; y++)
            {
                texture.SetPixel(x, y, Color.HSVToRGB(h, ((float)x - 1) / 255f, ((float)y - 1) / 255f));
            }
        }

        return texture;
    }
 
}