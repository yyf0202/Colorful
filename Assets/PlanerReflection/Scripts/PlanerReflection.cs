using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlanerReflection : MonoBehaviour
{
    public bool UseSpecifyNormal;
    public bool UseLocalSpecifyNormal;
    public Vector3 SpecifyNormal;
    public float DisableDistance = 300;
    public float FarClipModifier = 100;
    public bool EnableObliqueProjection;
    public Vector2 RTResolution = new Vector2(1024, 1024);
    public LayerMask ReflectLayers;
    public bool IgnoreSkybox;
    public Color BackgroundColor = Color.green;

    private int _ReflectionTexID;
    private Transform m_trans;
    private Renderer m_render;
    private bool m_bHasReflectionTexProperty = false;
    private Camera m_reflectionCamera;
    private RenderTexture m_reflectionRT;


    private void Awake()
    {
        _ReflectionTexID = Uniforms._PlannerReflectionTex;
        m_render = this.GetComponent<Renderer>();
        m_trans = this.transform;

        if (m_render != null)
            foreach (Material mat in m_render.sharedMaterials)
            {
                if (mat != null && mat.HasProperty(_ReflectionTexID))
                {
                    m_bHasReflectionTexProperty = true;
                    break;
                }
            }
    }

    private void OnWillRenderObject()
    {
        if (!m_bHasReflectionTexProperty)
            return;

        Camera srcCam = Camera.main;

        if (srcCam == null)
            return;

        Vector3 meshNormal = UseSpecifyNormal ? (UseLocalSpecifyNormal ? this.transform.TransformDirection(SpecifyNormal) : SpecifyNormal) : this.transform.up;
        Vector3 pos = this.transform.position;
        float distance = Vector3.Distance(srcCam.transform.position, m_trans.position);

        if (distance > DisableDistance)
            return;

        float oldZFar = srcCam.farClipPlane;
        float oldZNear = srcCam.nearClipPlane;
        float reflectionDot = Vector3.Dot(meshNormal, srcCam.transform.forward);

        //Calculate ZNEAR ZFAR
        float mDistance;
        new Plane(-srcCam.transform.forward, srcCam.transform.position).Raycast(new Ray(m_trans.position, meshNormal), out mDistance);
        srcCam.nearClipPlane = mDistance * Mathf.Abs(reflectionDot);
        srcCam.farClipPlane = srcCam.nearClipPlane + FarClipModifier;

        //Get Reflection Camera
        if (m_reflectionCamera == null)
        {
            GameObject go = new GameObject("reflectCam");
            m_reflectionCamera = go.AddComponent<Camera>();
            m_reflectionCamera.enabled = false;
            //go.hideFlags = HideFlags.HideAndDontSave;
            m_reflectionCamera.orthographic = false;
            m_reflectionCamera.GetAddComponent<FlareLayer>();
        }

        if (m_reflectionRT == null)
        {
            m_reflectionRT = new RenderTexture((int)RTResolution.x, (int)RTResolution.y, 8);
            m_reflectionRT.isPowerOfTwo = true;
            m_reflectionRT.hideFlags = HideFlags.DontSave;
        }

        //Sync Camera
        SyncReflectionCamera(srcCam);

        //Reflection Matrix
        Matrix4x4 reflectionMatrix = Matrix4x4.identity;
        CalculateReflectionMatrix(ref reflectionMatrix, new Vector4(meshNormal.x, meshNormal.y, meshNormal.z, -Vector3.Dot(meshNormal, this.transform.position)));

        m_reflectionCamera.transform.position = reflectionMatrix.MultiplyPoint(srcCam.transform.position);
        m_reflectionCamera.worldToCameraMatrix = srcCam.worldToCameraMatrix * reflectionMatrix;
        //Debug.Log(m_reflectionCamera.worldToCameraMatrix.ToString());

        //Set Oblique Projection Matrix
        if (EnableObliqueProjection)
        {
            m_reflectionCamera.projectionMatrix = srcCam.CalculateObliqueMatrix(GetCameraSpacePlane(m_reflectionCamera, m_trans.position, meshNormal));
            //Debug.Log(m_reflectionCamera.projectionMatrix.ToString());
            m_reflectionCamera.farClipPlane = m_reflectionCamera.nearClipPlane + FarClipModifier;
        }
        else
        {
            m_reflectionCamera.projectionMatrix = srcCam.projectionMatrix;
        }

        GL.invertCulling = true;

        m_reflectionCamera.useOcclusionCulling = false;
        m_reflectionCamera.depthTextureMode = DepthTextureMode.None;
        m_reflectionCamera.cullingMask = ReflectLayers;
        m_reflectionCamera.targetTexture = this.m_reflectionRT;
        m_reflectionCamera.Render();

        //Render & Set _ReflecionTex
        foreach (Material mat in m_render.sharedMaterials)
        {
            if (mat != null && mat.HasProperty(_ReflectionTexID))
                mat.SetTexture(_ReflectionTexID, m_reflectionRT);
        }

        m_reflectionCamera.enabled = false;
        srcCam.nearClipPlane = oldZNear;
        srcCam.farClipPlane = oldZFar;
        GL.invertCulling = false;
    }

    private void SyncReflectionCamera(Camera srcCamera)
    {
        if (srcCamera == null)
            return;

        m_reflectionCamera.nearClipPlane = srcCamera.nearClipPlane;
        m_reflectionCamera.farClipPlane = srcCamera.farClipPlane;
        m_reflectionCamera.fieldOfView = srcCamera.fieldOfView;
        m_reflectionCamera.aspect = srcCamera.aspect;

        m_reflectionCamera.clearFlags = IgnoreSkybox ? CameraClearFlags.SolidColor : srcCamera.clearFlags;
        m_reflectionCamera.orthographic = srcCamera.orthographic;
        m_reflectionCamera.orthographicSize = srcCamera.orthographicSize;
        m_reflectionCamera.backgroundColor = IgnoreSkybox ? BackgroundColor : srcCamera.backgroundColor;

        if (srcCamera.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox skybox = srcCamera.GetComponent<Skybox>();
            Skybox dstSky = m_reflectionCamera.GetAddComponent<Skybox>();

            if (skybox != null && skybox.material != null)
            {
                dstSky.material = skybox.material;
                dstSky.enabled = true;
            }
            else
                dstSky.enabled = false;
        }
    }

    //Reflection matrix calculations
    //http://www.euclideanspace.com/maths/geometry/affine/reflection/matrix/
    private void CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix, Vector4 reflectionPlane)
    {
        reflectionMatrix.m00 = (1F - 2F * reflectionPlane[0] * reflectionPlane[0]);
        reflectionMatrix.m01 = (-2F * reflectionPlane[0] * reflectionPlane[1]);
        reflectionMatrix.m02 = (-2F * reflectionPlane[0] * reflectionPlane[2]);
        reflectionMatrix.m03 = (-2F * reflectionPlane[3] * reflectionPlane[0]);

        reflectionMatrix.m10 = (-2F * reflectionPlane[1] * reflectionPlane[0]);
        reflectionMatrix.m11 = (1F - 2F * reflectionPlane[1] * reflectionPlane[1]);
        reflectionMatrix.m12 = (-2F * reflectionPlane[1] * reflectionPlane[2]);
        reflectionMatrix.m13 = (-2F * reflectionPlane[3] * reflectionPlane[1]);

        reflectionMatrix.m20 = (-2F * reflectionPlane[2] * reflectionPlane[0]);
        reflectionMatrix.m21 = (-2F * reflectionPlane[2] * reflectionPlane[1]);
        reflectionMatrix.m22 = (1F - 2F * reflectionPlane[2] * reflectionPlane[2]);
        reflectionMatrix.m23 = (-2F * reflectionPlane[3] * reflectionPlane[2]);

        reflectionMatrix.m30 = 0F;
        reflectionMatrix.m31 = 0F;
        reflectionMatrix.m32 = 0F;
        reflectionMatrix.m33 = 1F;
    }


    private Vector4 GetCameraSpacePlane(Camera reflectCamera, Vector3 pos, Vector3 normal)
    {
        Matrix4x4 matrix = reflectCamera.worldToCameraMatrix;
        Vector3 posCameraSpace = matrix.MultiplyPoint(pos + normal * 0.1f);
        Vector3 normalCameraSpace = matrix.MultiplyVector(normal).normalized;

        return new Vector4(normalCameraSpace.x, normalCameraSpace.y, normalCameraSpace.z, -Vector3.Dot(posCameraSpace, normalCameraSpace));
    }
}