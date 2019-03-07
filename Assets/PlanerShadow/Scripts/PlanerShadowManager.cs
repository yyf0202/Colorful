using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanerShadowManager : MonoBehaviour
{
    public Transform LightTrans;
    public Transform PlaneTrans;
    public Color ShadowColor;
    public float ShadowAttenuation;

    // Use this for initialization
    void ResetShader()
    {
        Vector3 m_currLightDir = LightTrans.forward;
        Vector4 lightDir = new Vector4(m_currLightDir.x, m_currLightDir.y, m_currLightDir.z, PlaneTrans.position.y);
        Shader.SetGlobalVector(Uniforms._LightDir, lightDir);
        Shader.SetGlobalColor(Uniforms._ShadowColor, ShadowColor);
        Shader.SetGlobalFloat(Uniforms._ShadowAttenuation, ShadowAttenuation);
    }
    private void Update()
    {
        ResetShader();
    }
}
