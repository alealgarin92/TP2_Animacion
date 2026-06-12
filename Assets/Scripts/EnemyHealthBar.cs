using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Damageable))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0f, 1.8f, 0f);
    public Vector2 size = new Vector2(1.2f, 0.15f);

    private Damageable _damageable;
    private GameObject _canvasGo;
    private Image _fillImage;
    private Vector3 _originalCanvasScale;

    private void Awake()
    {
        _damageable = GetComponent<Damageable>();
    }

    private void Start()
    {
        CreateHealthBar();
        UpdateHealthBar();
    }

    private void OnEnable()
    {
        if (_damageable != null)
        {
            _damageable.OnHit += HandleHit;
        }
    }

    private void OnDisable()
    {
        if (_damageable != null)
        {
            _damageable.OnHit -= HandleHit;
        }
    }

    private void HandleHit(int damage)
    {
        UpdateHealthBar();
    }

    private void CreateHealthBar()
    {
        // 1. Create Canvas GameObject
        _canvasGo = new GameObject("EnemyHealthBarCanvas");
        _canvasGo.transform.SetParent(transform);
        _canvasGo.transform.localPosition = offset;
        _canvasGo.transform.localRotation = Quaternion.identity;
        
        // Calculate original scale relative to parent scale
        float scaleX = Mathf.Approximately(transform.localScale.x, 0f) ? 1f : (1f / transform.localScale.x);
        float scaleY = Mathf.Approximately(transform.localScale.y, 0f) ? 1f : (1f / transform.localScale.y);
        _canvasGo.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        _originalCanvasScale = _canvasGo.transform.localScale;

        Canvas canvas = _canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100; // Render on top of character sprites

        // Add CanvasScaler for sharpness
        CanvasScaler scaler = _canvasGo.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        // 2. Create Background Image
        GameObject bgGo = new GameObject("Background");
        bgGo.transform.SetParent(_canvasGo.transform, false);
        Image bgImage = bgGo.AddComponent<Image>();
        bgImage.sprite = CreateBackgroundSprite();
        bgImage.color = Color.white;

        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.sizeDelta = size;

        // 3. Create Fill Image
        GameObject fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(_canvasGo.transform, false);
        _fillImage = fillGo.AddComponent<Image>();
        _fillImage.sprite = CreateFillSprite();
        _fillImage.color = Color.white;
        _fillImage.type = Image.Type.Filled;
        _fillImage.fillMethod = Image.FillMethod.Horizontal;
        _fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        RectTransform fillRect = fillGo.GetComponent<RectTransform>();
        // Shrink the fill slightly to fit perfectly inside the black border
        fillRect.sizeDelta = new Vector2(size.x - 0.06f, size.y - 0.04f);
    }

    private Sprite CreateBackgroundSprite()
    {
        int width = 128;
        int height = 16;
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 1-pixel border
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    texture.SetPixel(x, y, Color.black);
                }
                else
                {
                    // Dark grey interior
                    texture.SetPixel(x, y, new Color(0.08f, 0.08f, 0.08f, 0.95f));
                }
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateFillSprite()
    {
        int width = 128;
        int height = 16;
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            float t = (float)y / (height - 1);
            Color color;

            // Bevel gloss/cylinder shading
            if (t < 0.2f)
            {
                // Dark bottom edge
                color = Color.Lerp(new Color(0.35f, 0f, 0f), new Color(0.65f, 0f, 0f), t / 0.2f);
            }
            else if (t < 0.65f)
            {
                // Bright highlight
                color = Color.Lerp(new Color(0.65f, 0f, 0f), new Color(0.95f, 0.1f, 0.1f), (t - 0.2f) / 0.45f);
            }
            else
            {
                // Top shadow
                color = Color.Lerp(new Color(0.95f, 0.1f, 0.1f), new Color(0.45f, 0f, 0f), (t - 0.65f) / 0.35f);
            }

            for (int x = 0; x < width; x++)
            {
                // Subtle side borders
                if (x == 0 || x == width - 1)
                {
                    texture.SetPixel(x, y, new Color(0.2f, 0f, 0f));
                }
                else
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    private void UpdateHealthBar()
    {
        if (_fillImage != null && _damageable != null)
        {
            if (!_damageable.IsAlive || _damageable.Health <= 0)
            {
                if (_canvasGo != null && _canvasGo.activeSelf)
                {
                    _canvasGo.SetActive(false);
                }
                return;
            }

            if (_canvasGo != null && !_canvasGo.activeSelf)
            {
                _canvasGo.SetActive(true);
            }

            float fillAmount = Mathf.Clamp01((float)_damageable.Health / _damageable.MaxHealth);
            _fillImage.fillAmount = fillAmount;
        }
    }

    private void LateUpdate()
    {
        UpdateHealthBar();

        if (_canvasGo != null)
        {
            // Keep the health bar facing forward and not mirrored even when the parent flips scale
            float parentScaleX = transform.localScale.x;
            
            _canvasGo.transform.localScale = new Vector3(
                Mathf.Sign(parentScaleX) * Mathf.Abs(_originalCanvasScale.x),
                _originalCanvasScale.y,
                _originalCanvasScale.z
            );
        }
    }
}
