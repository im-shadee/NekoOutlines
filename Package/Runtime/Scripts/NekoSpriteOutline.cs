using UnityEngine;
using UnityEngine.Sprites;

namespace NekoOutlines.Runtime
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class NekoSpriteOutline : NekoOutlineBase
    {
        private SpriteRenderer m_SpriteRenderer = null;

        private SpriteRenderer SpriteRenderer
        {
            get
            {
                if (m_SpriteRenderer == null) m_SpriteRenderer = GetComponent<SpriteRenderer>();
                return m_SpriteRenderer;
            }
        }

        protected override Texture GetMainTexture() => SpriteRenderer != null && SpriteRenderer.sprite != null ? SpriteRenderer.sprite.texture : null;

        protected override Vector4 GetAtlasBounds()
        {
            return SpriteRenderer != null && SpriteRenderer.sprite != null
                ? DataUtility.GetOuterUV(SpriteRenderer.sprite)
                : new Vector4(0f, 0f, 1f, 1f);
        }

        protected override Material GetSourceMaterial() => m_OutlineMaterial != null ? m_OutlineMaterial : SpriteRenderer?.sharedMaterial;

        protected override void AssignMaterialToRenderer(Material mat)
        {
            if (SpriteRenderer != null)
            {
                // Shade: Use sharedMaterial in Edit Mode to avoid leaking materials into the scene.
                // Use material in Play Mode to ensure we are modifying a unique instance for this object.
                if (Application.isPlaying)
                {
                    if (SpriteRenderer.material != mat)
                    {
                        SpriteRenderer.material = mat;
                    }
                }
                else
                {
                    if (SpriteRenderer.sharedMaterial != mat)
                    {
                        SpriteRenderer.sharedMaterial = mat;
                    }
                }
            }
        }

        protected override void SetVerticesOrDirty() { } // Shade: SpriteRenderers don't use vertex dirty flags like UI Graphics.
    }
}
