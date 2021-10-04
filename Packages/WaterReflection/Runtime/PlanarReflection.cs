using System;
using System.Collections.Generic;
using UnityEngine;


#if !NET_LEGACY
using System.Runtime.CompilerServices;
#endif

namespace IFL.Rendering.Water
{
    public class PlanarReflection : MonoBehaviour
    {
        public Material waterMaterial;
        public LayerMask reflectionMask;
        public bool reflectSkybox = false;
        public Color clearColor = Color.grey;

        public float clipPlaneOffset = 0.07F;

        const string REFLECTION_SAMPLER = "_ReflectionTex";
        const string REFLECTION_DEPTH_SAMPLER = "_ReflectionDepthTexture";
        const float TEX_SCALE = 1f;

        Vector3 oldpos;
        Camera reflectionCamera;

        public Camera mainCamera;

        public bool renderFromScript;

        RenderTexture reflectionColor;
        RenderTexture reflectionDepth;

        public void Start()
        {
            if (!mainCamera) mainCamera = Camera.main;
            reflectionCamera = CreateReflectionCameraFor(mainCamera);
        }

        public void LateUpdate()
        {
            RenderReflectionFor(mainCamera, reflectionCamera);

            if (reflectionCamera && waterMaterial)
            {
                waterMaterial.SetTexture(REFLECTION_SAMPLER, reflectionColor);
                waterMaterial.SetTexture(REFLECTION_DEPTH_SAMPLER, reflectionDepth);
            }
        }

        public void OnEnable()
        {
            Shader.EnableKeyword("WATER_REFLECTIVE");
            Shader.DisableKeyword("WATER_SIMPLE");
        }

        public void OnDisable()
        {
            Shader.EnableKeyword("WATER_SIMPLE");
            Shader.DisableKeyword("WATER_REFLECTIVE");
        }

        private void OnDestroy()
        {
            if (reflectionColor) Destroy(reflectionColor);
            if (reflectionDepth) Destroy(reflectionDepth);
        }

        Camera CreateReflectionCameraFor(Camera cam)
        {
            string reflName = gameObject.name + "Reflection" + cam.name;

            GameObject go = new GameObject(reflName, typeof(Camera));
            Camera reflectCamera = go.GetComponent<Camera>();

            reflectCamera.backgroundColor = clearColor;
            reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;

            reflectCamera.allowHDR = cam.allowHDR;

            // Makes sure UI doesn't use this camera for rendering:
            reflectCamera.depth = -1000;

            if (!reflectCamera.targetTexture)
            {
                Debug.Log("Camera texture");
                CreateTextureFor(cam, reflectCamera);
            }

            SetReflectionCameraSettings(reflectCamera);

            reflectCamera.enabled = !renderFromScript;

            reflectCamera.gameObject.AddComponent<ReflectionCamera>();

            return reflectCamera;
        }

        void CreateTextureFor(Camera cam, Camera reflectionCam)
        {
            int width = Mathf.FloorToInt(cam.pixelWidth * TEX_SCALE);
            int height = Mathf.FloorToInt(cam.pixelHeight * TEX_SCALE);

            Debug.Log("Created texture: w: " + width + ", h: " + height);

            var colorFormat = cam.allowHDR ? RenderTextureFormat.ARGBFloat : RenderTextureFormat.Default;

            reflectionColor = new RenderTexture(width, height, 0, colorFormat);
            reflectionDepth = new RenderTexture(width, height, 24, RenderTextureFormat.Depth);
            reflectionColor.hideFlags = HideFlags.DontSave;
            reflectionDepth.hideFlags = HideFlags.DontSave;
            //reflectionColor.filterMode = FilterMode.Point;
            reflectionColor.useMipMap = false;

            reflectionCam.targetTexture = reflectionColor;
            reflectionCam.SetTargetBuffers(reflectionColor.colorBuffer, reflectionDepth.depthBuffer);
        }

        void SetReflectionCameraSettings(Camera reflectCamera)
        {
            reflectCamera.cullingMask = reflectionMask & ~(1 << LayerMask.NameToLayer("Water"));

            reflectCamera.depthTextureMode = DepthTextureMode.Depth;
            reflectCamera.backgroundColor = Color.black;
            //reflectCamera.clearFlags = CameraClearFlags.SolidColor;
            reflectCamera.renderingPath = RenderingPath.Forward;

            reflectCamera.backgroundColor = clearColor;
            reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;

            reflectCamera.useOcclusionCulling = false;
            reflectCamera.allowHDR = false;
            reflectCamera.allowMSAA = false;

            reflectCamera.farClipPlane = 10000;
        }

        void RenderReflectionFor(Camera cam, Camera reflectCamera)
        {
            //if (!reflectCamera) return;
            //if (m_SharedMaterial && !m_SharedMaterial.HasProperty(reflectionSampler)) return;

            reflectCamera.fieldOfView = cam.fieldOfView;

            GL.invertCulling = true;

            Transform reflectiveSurface = transform; //waterHeight;

            Vector3 eulerA = cam.transform.eulerAngles;

            reflectCamera.transform.eulerAngles = new Vector3(-eulerA.x, eulerA.y, eulerA.z);
            reflectCamera.transform.position = cam.transform.position;

            Vector3 pos = reflectiveSurface.transform.position;
            pos.y = reflectiveSurface.position.y;
            Vector3 normal = reflectiveSurface.transform.up;
            float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            reflection = CalculateReflectionMatrix(reflection, reflectionPlane);
            oldpos = cam.transform.position;
            Vector3 newpos = reflection.MultiplyPoint(oldpos);

            reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

            Vector4 clipPlane = CameraSpacePlane(reflectCamera, pos, normal, 1.0f);

            Matrix4x4 projection = cam.projectionMatrix;
            //projection = CalculateObliqueMatrix(projection, clipPlane);
            projection = cam.CalculateObliqueMatrix(clipPlane);
            reflectCamera.projectionMatrix = projection;

            reflectCamera.transform.position = newpos;
            Vector3 euler = cam.transform.eulerAngles;
            reflectCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

            if (renderFromScript)
                reflectCamera.Render();

            GL.invertCulling = false;
        }

#if !NET_LEGACY
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
        {
            Vector4 q = projection.inverse * new Vector4(
                Sgn(clipPlane.x),
                Sgn(clipPlane.y),
                1.0F,
                1.0F
                );
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot(clipPlane, q)));
            // third row = clip plane - fourth row
            projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];

            return projection;
        }

#if !NET_LEGACY
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1.0F - 2.0F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2.0F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2.0F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2.0F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2.0F * plane[1] * plane[0]);
            reflectionMat.m11 = (1.0F - 2.0F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2.0F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2.0F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2.0F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2.0F * plane[2] * plane[1]);
            reflectionMat.m22 = (1.0F - 2.0F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2.0F * plane[3] * plane[2]);

            reflectionMat.m30 = 0.0F;
            reflectionMat.m31 = 0.0F;
            reflectionMat.m32 = 0.0F;
            reflectionMat.m33 = 1.0F;

            return reflectionMat;
        }

#if !NET_LEGACY
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static float Sgn(float a) // Whyyyyyy not Mathf.Sign?
        {
            if (a > 0.0F)
            {
                return 1.0F;
            }
            if (a < 0.0F)
            {
                return -1.0F;
            }
            return 0.0F;
        }

        Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (reflectionCamera)
                {
                    reflectionCamera.enabled = !renderFromScript;
                }
            }
        }
#endif
    }
}