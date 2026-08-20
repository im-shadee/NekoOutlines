using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

namespace NekoOutlines.Runtime
{
    [ExecuteAlways]
    [Icon("d_Outline")]
    [RequireComponent(typeof(Graphic))]
    public class NekoOutline : NekoOutlineBase, IMeshModifier
    {
        // Shade: Tiny epsilon used to decide whether a vertex sits exactly on the outer boundary.
        private const float m_kBoundaryEpsilon = 0.0001f;
        private Graphic m_Graphic = null;

        private Graphic GraphicComponent
        {
            get
            {
                if (m_Graphic == null) m_Graphic = GetComponent<Graphic>();
                return m_Graphic;
            }
        }

        protected override Texture GetMainTexture() => GraphicComponent != null ? GraphicComponent.mainTexture : null;

        protected override Vector4 GetAtlasBounds()
        {
            if (GraphicComponent is Image img && img.overrideSprite != null)
            {
                return DataUtility.GetOuterUV(img.overrideSprite);
            }
            return new Vector4(0f, 0f, 1f, 1f);
        }

        protected override Material GetSourceMaterial() => m_OutlineMaterial != null ? m_OutlineMaterial : GraphicComponent?.defaultMaterial;

        protected override void AssignMaterialToRenderer(Material mat)
        {
            if (GraphicComponent != null && GraphicComponent.material != mat)
            {
                GraphicComponent.material = mat;
            }
        }

        protected override void SetVerticesOrDirty()
        {
            if (GraphicComponent != null)
            {
                GraphicComponent.SetVerticesDirty();
                GraphicComponent.SetMaterialDirty();
            }
        }

        #region IMeshModifier Implementation
        /// <inheritdoc/>
        public void ModifyMesh(Mesh mesh) { } // Shade: Unused; handled completely by the VertexHelper implementation.

        /// <inheritdoc/>
        public void ModifyMesh(VertexHelper vh)
        {
            // Shade: Exit early if the component is disabled, thickness is zero, or no vertices exist.
            // Also exit if in inner outline mode as the mesh expansion is handled by the shader's UV logic,
            // not by modifying the actual mesh geometry.
            if (!isActiveAndEnabled || Thickness <= 0f || vh.currentVertCount == 0 || m_bIsInnerOutline) return;

            int vertexCount = vh.currentVertCount;
            // Shade: Determine the bounding box of the graphic in local space and UV space.
            Vector2 posMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 posMax = new Vector2(float.MinValue, float.MinValue);
            Vector2 uvMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 uvMax = new Vector2(float.MinValue, float.MinValue);

            UIVertex vert = default;
            for (int i = 0; i < vertexCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                posMin = Vector2.Min(posMin, vert.position);
                posMax = Vector2.Max(posMax, vert.position);
                uvMin = Vector2.Min(uvMin, vert.uv0);
                uvMax = Vector2.Max(uvMax, vert.uv0);
            }

            Vector2 posSize = posMax - posMin;
            Vector2 uvSize = uvMax - uvMin;
            if (posSize.x <= 0f || posSize.y <= 0f || uvSize.x <= 0f || uvSize.y <= 0f) return;

            // Shade: Calculate the required padding based on the texel size of the texture.
            // This ensures the mesh expands by the exact number of pixels defined by Thickness.
            Texture tex = GetMainTexture();
            if (tex == null || tex.width == 0 || tex.height == 0) return;
            Vector2 texelSize = new Vector2(1f / tex.width, 1f / tex.height);

            Vector2 paddingUV = Thickness * texelSize;
            Vector2 localPerUV = new Vector2(posSize.x / uvSize.x, posSize.y / uvSize.y);
            Vector2 paddingLocal = new Vector2(paddingUV.x * localPerUV.x, paddingUV.y * localPerUV.y);

            float posEpsX = posSize.x * m_kBoundaryEpsilon;
            float posEpsY = posSize.y * m_kBoundaryEpsilon;
            float uvEpsX = uvSize.x * m_kBoundaryEpsilon;
            float uvEpsY = uvSize.y * m_kBoundaryEpsilon;

            // Shade: Iterate through vertices and shift them outward if they lie on the boundary edges.
            for (int i = 0; i < vertexCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);

                // Shade: Offset position based on whether the vertex sits on a boundary edge.
                float dx = 0f;
                if (Mathf.Abs(vert.position.x - posMin.x) < posEpsX) dx = -paddingLocal.x;
                else if (Mathf.Abs(vert.position.x - posMax.x) < posEpsX) dx = paddingLocal.x;

                float dy = 0f;
                if (Mathf.Abs(vert.position.y - posMin.y) < posEpsY) dy = -paddingLocal.y;
                else if (Mathf.Abs(vert.position.y - posMax.y) < posEpsY) dy = paddingLocal.y;

                vert.position += new Vector3(dx, dy, 0f);

                // Shade: Offset UV coordinates to ensure the shader samples the correct expanded area.
                float du = 0f;
                if (Mathf.Abs(vert.uv0.x - uvMin.x) < uvEpsX) du = -paddingUV.x;
                else if (Mathf.Abs(vert.uv0.x - uvMax.x) < uvEpsX) du = paddingUV.x;

                float dv = 0f;
                if (Mathf.Abs(vert.uv0.y - uvMin.y) < uvEpsY) dv = -paddingUV.y;
                else if (Mathf.Abs(vert.uv0.y - uvMax.y) < uvEpsY) dv = paddingUV.y;

                vert.uv0 += new Vector4(du, dv);
                vh.SetUIVertex(vert, i);
            }
        }

        private void OnRectTransformDimensionsChanged()
        {
            SetVerticesOrDirty();
        }
        #endregion
    }
}
