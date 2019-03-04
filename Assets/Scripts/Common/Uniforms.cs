using UnityEngine;

static class Uniforms
{
    #region planner shadow
    internal static readonly int _ShadowColor = Shader.PropertyToID("_ShadowColor");
    internal static readonly int _ShadowAttenuation = Shader.PropertyToID("_ShadowAttenuation");
    internal static readonly int _LightDir = Shader.PropertyToID("_LightDir");
    #endregion

    #region outterline
    internal static readonly int _OutterLineSize = Shader.PropertyToID("outterLineSize");
    internal static readonly int _OutterlineBlurSilhouette = Shader.PropertyToID("BlurSilhouette");
    internal static readonly int _OutterLineColor = Shader.PropertyToID("_Outline_Color");
    #endregion


    #region planner reflection
    internal static readonly int _PlannerReflectionTex = Shader.PropertyToID("_ReflectionTex");
    #endregion
}