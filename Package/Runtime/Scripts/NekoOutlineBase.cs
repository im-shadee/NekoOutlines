using UnityEngine;

namespace NekoOutlines.Runtime
{
    [ExecuteAlways]
    public abstract class NekoOutlineBase : MonoBehaviour
    {
        protected static readonly int s_AtlasBoundsID = Shader.PropertyToID("_AtlasBounds");
        protected static readonly int s_OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
        protected static readonly int s_OutlineBiasID = Shader.PropertyToID("_OutlineBias");
        protected static readonly int s_OutlineColorID = Shader.PropertyToID("_OutlineColor");
        protected static readonly int s_IsInnerOutlineID = Shader.PropertyToID("_IsInnerOutline");
        protected static readonly int s_PixelSnapID = Shader.PropertyToID("_PixelSnap");
        protected static readonly int s_SquareCornersID = Shader.PropertyToID("_SquareCorners");
        protected static readonly int s_MainTexID = Shader.PropertyToID("_MainTex");

        protected Material m_CustomMaterial = null;

        [Header("Outline Settings")]
        [SerializeField, Min(0f), Tooltip("The thickness of the outline in pixels.")]
        private float m_OutlineThickness = 2f;
        public float Thickness
        {
            get => m_OutlineThickness;
            set
            {
                // Shade: If the values are the same, early return to avoid heavy material updates
                if (Mathf.Approximately(m_OutlineThickness, value)) return;

                // Shade: Ensure thickness always positive
                m_OutlineThickness = Mathf.Max(0f, value);

                UpdateMaterialProperties();
                SetVerticesOrDirty();
            }
        }

        [SerializeField, Tooltip("Stretch or squish the outline filter radius horizontally and vertically.")]
        private Vector2 m_OutlineBias = Vector2.one;

        [SerializeField, Tooltip("The color applied to the generated outline.")]
        private Color m_OutlineColor = Color.black;

        [SerializeField, Tooltip("Whether the outline should render strictly inside the graphic bounds.")]
        protected bool m_bIsInnerOutline = false;

        [SerializeField, Tooltip("Snaps outline calculations to whole pixels for pixel-art clarity.")]
        private bool m_bPixelSnap = false;

        [SerializeField, Tooltip("Forces sharp 90-degree square corners instead of smooth rounded corners.")]
        private bool m_bSquareCorners = false;

        [SerializeField, Tooltip("The custom outline material assigned to this component.")]
        protected Material m_OutlineMaterial = null;

        #region Unity Lifecycle
        protected virtual void OnEnable()
        {
            Refresh();
        }

        protected virtual void OnDisable()
        {
            CleanupMaterial();
        }

        protected virtual void OnValidate()
        {
            UpdateMaterialProperties();
            SetVerticesOrDirty();
        }
        #endregion

        #region Public API
        /// <summary>
        /// Forces a complete refresh of the material properties and geometry vertex layout.
        /// </summary>
        public virtual void Refresh()
        {
            UpdateMaterialProperties();
            SetVerticesOrDirty();
        }

        /// <summary>
        /// Sets the color of the outline programmatically.
        /// </summary>
        /// <param name="color">The new outline color.</param>
        public void SetColor(Color color)
        {
            m_OutlineColor = color;
            UpdateMaterialProperties();
        }

        /// <summary>
        /// Updates all shader properties dynamically based on current component settings.
        /// </summary>
        public void UpdateMaterialProperties()
        {
            Material targetMat = GetMaterialInstance();
            if (targetMat == null) return;

            Vector4 atlasBounds = GetAtlasBounds();
            Texture mainTex = GetMainTexture();

            if (mainTex != null && targetMat.HasProperty(s_MainTexID))
            {
                targetMat.SetTexture(s_MainTexID, mainTex);
            }

            targetMat.SetVector(s_AtlasBoundsID, atlasBounds);
            targetMat.SetFloat(s_OutlineThicknessID, m_OutlineThickness);
            targetMat.SetVector(s_OutlineBiasID, m_OutlineBias);
            targetMat.SetColor(s_OutlineColorID, m_OutlineColor);
            targetMat.SetFloat(s_IsInnerOutlineID, m_bIsInnerOutline ? 1f : 0f);
            targetMat.SetFloat(s_PixelSnapID, m_bPixelSnap ? 1f : 0f);
            targetMat.SetFloat(s_SquareCornersID, m_bSquareCorners ? 1f : 0f);
        }
        #endregion

        #region Abstract / Overridable Implementation Hooks
        protected abstract Texture GetMainTexture();
        protected abstract Vector4 GetAtlasBounds();
        protected abstract Material GetSourceMaterial();
        protected abstract void AssignMaterialToRenderer(Material mat);
        protected abstract void SetVerticesOrDirty();
        #endregion

        #region Protected Material Management
        protected Material GetMaterialInstance()
        {
            Material sourceMat = GetSourceMaterial();
            if (sourceMat == null) return null;

            // Shade: Handle domain reloads and destroyed instances safely
            if (m_CustomMaterial == null || m_CustomMaterial.shader != sourceMat.shader)
            {
                CleanupMaterial(); // Ensure no orphaned materials
                m_CustomMaterial = new Material(sourceMat);
                m_CustomMaterial.hideFlags = HideFlags.HideAndDontSave;
                AssignMaterialToRenderer(m_CustomMaterial);
            }
            else
            {
                AssignMaterialToRenderer(m_CustomMaterial);
            }

            return m_CustomMaterial;
        }

        protected void CleanupMaterial()
        {
            if (m_CustomMaterial != null)
            {
                if (Application.isPlaying) Destroy(m_CustomMaterial);
                else DestroyImmediate(m_CustomMaterial);
                m_CustomMaterial = null;
            }
        }
        #endregion
    }
}
