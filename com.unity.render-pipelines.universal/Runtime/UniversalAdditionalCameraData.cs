using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.LWRP
{
    [Obsolete("LWRP -> Universal (UnityUpgradable) -> UnityEngine.Rendering.Universal.UniversalAdditionalCameraData", true)]
    public class LWRPAdditionalCameraData
    {
    }
}

namespace UnityEngine.Rendering.Universal
{
    [MovedFrom("UnityEngine.Rendering.LWRP")] public enum CameraOverrideOption
    {
        Off,
        On,
        UsePipelineSettings,
    }

    [MovedFrom("UnityEngine.Rendering.LWRP")] public enum RendererOverrideOption
    {
        Custom,
        UsePipelineSettings,
    }

    public enum AntialiasingMode
    {
        None,
        FastApproximateAntialiasing,
        SubpixelMorphologicalAntiAliasing,
        //TemporalAntialiasing
	}
	
    public enum LWRPCameraType
    {
        Offscreen,
        Base,
        Overlay,
        ScreenSpaceUI,
    }

    // Only used for SMAA right now
    public enum AntialiasingQuality
    {
        Low,
        Medium,
        High
	}
	
    static class LWRPCameraTypeUtility
    {
        static string[] s_LWRPCameraTypeNames = Enum.GetNames(typeof(LWRPCameraType)).ToArray();

        public static string GetName(this LWRPCameraType type)
        {
            return s_LWRPCameraTypeNames[(int)type];
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [ImageEffectAllowedInSceneView]
    [MovedFrom("UnityEngine.Rendering.LWRP")] public class UniversalAdditionalCameraData : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Tooltip("If enabled shadows will render for this camera.")]
        [FormerlySerializedAs("renderShadows"), SerializeField]
        bool m_RenderShadows = true;

        [Tooltip("If enabled depth texture will render for this camera bound as _CameraDepthTexture.")]
        [SerializeField]
        CameraOverrideOption m_RequiresDepthTextureOption = CameraOverrideOption.UsePipelineSettings;

        [Tooltip("If enabled opaque color texture will render for this camera and bound as _CameraOpaqueTexture.")]
        [SerializeField]
        CameraOverrideOption m_RequiresOpaqueTextureOption = CameraOverrideOption.UsePipelineSettings;

        [SerializeField] RendererOverrideOption m_RendererOverrideOption = RendererOverrideOption.UsePipelineSettings;
        [SerializeField] ScriptableRendererData m_RendererData = null;
		[SerializeField] LWRPCameraType m_CameraType = LWRPCameraType.Base;
		[SerializeField] List<Camera> m_Cameras = new List<Camera>();
		
        ScriptableRenderer m_Renderer = null;

        [SerializeField] LayerMask m_VolumeLayerMask = 1; // "Default"
        [SerializeField] Transform m_VolumeTrigger = null;

        [SerializeField] bool m_RenderPostProcessing = false;
        [SerializeField] AntialiasingMode m_Antialiasing = AntialiasingMode.None;
        [SerializeField] AntialiasingQuality m_AntialiasingQuality = AntialiasingQuality.High;
        [SerializeField] bool m_StopNaN = false;
        [SerializeField] bool m_Dithering = false;

        // Deprecated:
        [FormerlySerializedAs("requiresDepthTexture"), SerializeField]
        bool m_RequiresDepthTexture = false;

        [FormerlySerializedAs("requiresColorTexture"), SerializeField]
        bool m_RequiresColorTexture = false;

        [HideInInspector] [SerializeField] float m_Version = 2;

        public float version => m_Version;

        public bool renderShadows
        {
            get => m_RenderShadows;
            set => m_RenderShadows = value;
        }

        public CameraOverrideOption requiresDepthOption
        {
            get => m_RequiresDepthTextureOption;
            set => m_RequiresDepthTextureOption = value;
        }

        public CameraOverrideOption requiresColorOption
        {
            get => m_RequiresOpaqueTextureOption;
            set => m_RequiresOpaqueTextureOption = value;
        }

        public LWRPCameraType cameraType
        {
            get => m_CameraType;
            set => m_CameraType = value;
        }

        public List<Camera> cameras
        {
            get => m_Cameras;
        }

        public void AddCamera(Camera camera)
        {
            m_Cameras.Add(camera);
        }

        public bool requiresDepthTexture
        {
            get
            {
                if (m_RequiresDepthTextureOption == CameraOverrideOption.UsePipelineSettings)
                {
                    UniversalRenderPipelineAsset asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                    return asset.supportsCameraDepthTexture;
                }
                else
                {
                    return m_RequiresDepthTextureOption == CameraOverrideOption.On;
                }
            }
            set { m_RequiresDepthTextureOption = (value) ? CameraOverrideOption.On : CameraOverrideOption.Off; }
        }

        public bool requiresColorTexture
        {
            get
            {
                if (m_RequiresOpaqueTextureOption == CameraOverrideOption.UsePipelineSettings)
                {
                    UniversalRenderPipelineAsset asset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                    return asset.supportsCameraOpaqueTexture;
                }
                else
                {
                    return m_RequiresOpaqueTextureOption == CameraOverrideOption.On;
                }
            }
            set { m_RequiresOpaqueTextureOption = (value) ? CameraOverrideOption.On : CameraOverrideOption.Off; }
        }

        public ScriptableRenderer scriptableRenderer
        {
            get
            {
                if (m_RendererOverrideOption == RendererOverrideOption.UsePipelineSettings || m_RendererData == null)
                    return UniversalRenderPipeline.asset.scriptableRenderer;

                if (m_RendererData.isInvalidated || m_Renderer == null)
                    m_Renderer = m_RendererData.InternalCreateRenderer();

                return m_Renderer;
            }
        }

        public LayerMask volumeLayerMask
        {
            get => m_VolumeLayerMask;
            set => m_VolumeLayerMask = value;
        }

        public Transform volumeTrigger
        {
            get => m_VolumeTrigger;
            set => m_VolumeTrigger = value;
        }

        public bool renderPostProcessing
        {
            get => m_RenderPostProcessing;
            set => m_RenderPostProcessing = value;
        }

        public AntialiasingMode antialiasing
        {
            get => m_Antialiasing;
            set => m_Antialiasing = value;
        }

        public AntialiasingQuality antialiasingQuality
        {
            get => m_AntialiasingQuality;
            set => m_AntialiasingQuality = value;
        }

        public bool stopNaN
        {
            get => m_StopNaN;
            set => m_StopNaN = value;
        }

        public bool dithering
        {
            get => m_Dithering;
            set => m_Dithering = value;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (version <= 1)
            {
                m_RequiresDepthTextureOption = (m_RequiresDepthTexture) ? CameraOverrideOption.On : CameraOverrideOption.Off;
                m_RequiresOpaqueTextureOption = (m_RequiresColorTexture) ? CameraOverrideOption.On : CameraOverrideOption.Off;
            }
        }

        public void OnDrawGizmos()
        {
            string gizmoName = "Packages/com.unity.render-pipelines.lightweight/Editor/Gizmos/";
            Color tint = Color.white;
            if (m_CameraType == LWRPCameraType.Base)
            {
                gizmoName += "Camera_Base.png";
            }
            else if (m_CameraType == LWRPCameraType.Overlay)
            {
                gizmoName += "Camera_Overlay.png";
            }
            else if (m_CameraType == LWRPCameraType.Offscreen)
            {
                gizmoName += "Camera_Offscreen.png";
            }
            else
            {
                gizmoName += "Camera_UI.png";
            }

#if UNITY_2019_2_OR_NEWER
            if (Selection.activeObject == gameObject)
            {
                // Get the preferences selection color
                tint = SceneView.selectedOutlineColor;
            }
            Gizmos.DrawIcon(transform.position, gizmoName, true, tint);
#else
            Gizmos.DrawIcon(transform.position, gizmoName);
#endif
        }
    }
}
