using UnityEngine;

namespace QM_ARZUMATA_EnemyCountIndicator
{
    public class SpriteHueAdjuster
    {
        public static Sprite AdjustSpriteHue(Sprite originalSprite, Color sourceColor, Color targetColor)
        {
            float hueShift = CalculateHueShift(sourceColor, targetColor);

            // Create readable texture copy with pixel-perfect settings
            Texture2D readableTexture = CreateReadableTexture(originalSprite.texture);

            // Get pixels and adjust hue
            Color[] pixels = readableTexture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                // Only process non-transparent pixels
                if (pixels[i].a > 0)
                {
                    Color.RGBToHSV(pixels[i], out float h, out float s, out float v);

                    // Ensures that the hue value h stays within the range of 0 to 1 by repeating it as necessary.
                    h = Mathf.Repeat(h + hueShift, 1f);
                    pixels[i] = Color.HSVToRGB(h, s, v);
                }
            }

            // Create new texture with pixel-perfect settings
            Texture2D newTexture = new Texture2D(readableTexture.width, readableTexture.height, TextureFormat.RGBA32, false);

            // Set pixel-perfect filtering
            newTexture.filterMode = FilterMode.Point; // No smoothing/bilinear filtering
            newTexture.wrapMode = TextureWrapMode.Clamp;
            // newTexture.anisoLevel = 0; // Disable anisotropic filtering

            newTexture.SetPixels(pixels);
            newTexture.Apply(false); // We don't generate mipmaps

            // Create sprite with exact same properties to maintain pixel-perfect rendering
            Sprite newSprite = Sprite.Create(
                newTexture,
                originalSprite.rect,
                originalSprite.pivot / originalSprite.rect.size, // Normalized pivot
                originalSprite.pixelsPerUnit,
                0, // No extrude
                SpriteMeshType.FullRect,
                originalSprite.border,
                false // Don't generate fallback physics shape
            );

            // Clean up temporary texture
            if (readableTexture != originalSprite.texture)
            {
                Object.DestroyImmediate(readableTexture);
            }

            return newSprite;
        }

        private static float CalculateHueShift(Color sourceColor, Color targetColor)
        {
            // Convert colors to HSV
            Color.RGBToHSV(sourceColor, out float sourceH, out float sourceS, out float sourceV);
            Color.RGBToHSV(targetColor, out float targetH, out float targetS, out float targetV);

            // Calculate hue difference
            float hueShift = targetH - sourceH;

            // Handle wrap-around (hue is circular)
            if (hueShift > 0.5f)
            {
                hueShift -= 1f;
            }
            else if (hueShift < -0.5f)
            {
                hueShift += 1f;
            }

            return hueShift;
        }

        private static Texture2D CreateReadableTexture(Texture2D original)
        {
            // Create a temporary RenderTexture with no anti-aliasing and exact dimensions
            RenderTexture tmp = RenderTexture.GetTemporary(
                original.width,
                original.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default,
                1 // No anti-aliasing
            );

            // Set pixel-perfect settings for RenderTexture
            tmp.filterMode = FilterMode.Point;
            tmp.wrapMode = TextureWrapMode.Clamp;

            // Blit with pixel-perfect material (no smoothing)
            RenderTexture.active = tmp;
            GL.Clear(true, true, Color.clear);

            // Use point filtering for the blit operation
            Graphics.Blit(original, tmp);

            // Create a new readable Texture2D with pixel-perfect settings
            Texture2D readable = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
            readable.filterMode = FilterMode.Point;
            readable.wrapMode = TextureWrapMode.Clamp;
            readable.anisoLevel = 0;

            // Read pixels at exact coordinates (pixel-perfect)
            readable.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0, false);
            readable.Apply(false); // Don't generate mipmaps

            // Clean up
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tmp);

            return readable;
        }
    }
}
