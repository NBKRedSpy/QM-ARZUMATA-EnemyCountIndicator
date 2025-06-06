using UnityEngine;
using UnityEngine.Rendering;

namespace QM_ARZUMATA_EnemyCountIndicator
{
    public class SpriteHueAdjuster
    {
        private static float CalculateHueShift(Color sourceColor, Color targetColor)
        {
            // Convert colors to HSV
            Color.RGBToHSV(sourceColor, out float sourceH, out float sourceS, out float sourceV);
            Color.RGBToHSV(targetColor, out float targetH, out float targetS, out float targetV);

            // Calculate hue difference
            float hueShift = targetH - sourceH;

            // Handle wrap-around (hue is circular)
            if (hueShift > 0.5f) hueShift -= 1f;
            else if (hueShift < -0.5f) hueShift += 1f;

            return hueShift;
        }

        public static Sprite AdjustSpriteHue(Sprite originalSprite, Color sourceColor, Color targetColor)
        {
            if (originalSprite == null) return null;

            float hueShift = CalculateHueShift(sourceColor, targetColor);

            // Create readable texture copy
            Texture2D readableTexture = CreateReadableTexture(originalSprite.texture);

            // Get pixels and adjust hue
            Color[] pixels = readableTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0) // Only process non-transparent pixels
                {
                    Color.RGBToHSV(pixels[i], out float h, out float s, out float v);
                    h = Mathf.Repeat(h + hueShift, 1f);
                    pixels[i] = Color.HSVToRGB(h, s, v);
                    pixels[i].a = pixels[i].a; // Preserve alpha
                }
            }

            // Create new texture
            Texture2D newTexture = new Texture2D(readableTexture.width, readableTexture.height, TextureFormat.RGBA32, false);
            newTexture.SetPixels(pixels);
            newTexture.Apply();

            // Create sprite
            Sprite newSprite = Sprite.Create(
                newTexture,
                originalSprite.rect,
                originalSprite.pivot / originalSprite.rect.size,
                originalSprite.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect,
                originalSprite.border
            );

            // Clean up
            if (readableTexture != originalSprite.texture)
            {
                Object.DestroyImmediate(readableTexture);
            }

            return newSprite;
        }

        private static Texture2D CreateReadableTexture(Texture2D original)
        {
            // Create a temporary RenderTexture
            RenderTexture tmp = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32);

            // Blit the original texture to the RenderTexture
            Graphics.Blit(original, tmp);

            // Create a new readable Texture2D
            RenderTexture.active = tmp;
            Texture2D readable = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0);
            readable.Apply();

            // Clean up
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tmp);

            return readable;
        }
    }
}
