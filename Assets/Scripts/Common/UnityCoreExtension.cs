using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class UnityCoreExtension
{
    #region Int Float Bool

    public static bool EqualZero(this float f)
    {
        return f < float.Epsilon && f > -float.Epsilon;
    }

    public static bool FloatInThreshold(this float f1, float f2, float threshold = 0.000001f)
    {
        return FloatEqual(f1, f2, threshold);
    }

    public static bool FloatEqual(this float f1, float f2, float threshold = 0.000001f)
    {
        return Math.Abs(f1 - f2) < threshold;
    }

    #endregion

    #region GameObject

    /// <summary>
    /// Get component. If target type component is not exist, then add it.
    /// </summary>
    /// <returns>Target component.</returns>
    /// <param name="comp">Target component.</param>
    /// <typeparam name="T">Component type.</typeparam>
    public static T GetAddComponent<T>(this Component comp) where T : Component
    {
        return comp.gameObject.GetAddComponent<T>();
    }

    /// <summary>
    /// Get component. If target type component is not exist, then add it.
    /// </summary>
    /// <returns>Target component.</returns>
    /// <param name="gameObject">Target game object.</param>
    /// <typeparam name="T">Component type.</typeparam>
    public static T GetAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T comp = gameObject.GetComponent<T>();
        if (comp == null)
        {
            comp = gameObject.AddComponent<T>();
        }
        return comp;
    }

    public static GameObject GetChildGameObjectByName(this GameObject root, string name, bool bRecursive = true,bool ignoreUnactive =false)
    {
        if (root == null)
            return null;

        for (int i = 0; i < root.transform.childCount; i++)
        {
            GameObject sub = root.transform.GetChild(i).gameObject;
            if(ignoreUnactive)
            {
                if (sub.name == name && sub.activeInHierarchy)
                {
                    return sub;
                }
            }
            else
            {
                if (sub.name == name)
                {
                    return sub;
                }
            }

            if (bRecursive)
            {
                GameObject recursiveInSub = GetChildGameObjectByName(sub, name, true,ignoreUnactive);
                if (recursiveInSub != null)
                    return recursiveInSub;
            }
        }

        return null;
    }

    public static GameObject ResetTransform(this GameObject go)
    {
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    public static RectTransform Rect(this GameObject gameObject)
    {
        return gameObject.GetAddComponent<RectTransform>();
    }

    #endregion

    #region Component Transform MonoBehaviour

    public static RectTransform Rect(this MonoBehaviour monoBehaviour)
    {
        return monoBehaviour.GetAddComponent<RectTransform>();
    }

    public static CanvasGroup CanvasGroup(this RectTransform rect)
    {
        return rect.GetAddComponent<CanvasGroup>();
    }

    public static float GetAlpha(this RectTransform rect)
    {
        return rect.GetAddComponent<CanvasGroup>().alpha;
    }

    public static void SetAlpha(this RectTransform rect, float val)
    {
        rect.GetAddComponent<CanvasGroup>().alpha = val;
    }

    public static bool GetInteractable(this RectTransform rect)
    {
        return rect.GetAddComponent<CanvasGroup>().interactable;
    }

    public static void SetInteractable(this RectTransform rect, bool val)
    {
        rect.GetAddComponent<CanvasGroup>().interactable = val;
    }

    public static void SetWorldTransAsHolder(this Transform trans, TransHolder holder)
    {
        holder.SetAsWorld(trans);
    }

    public static void SetLocalTransAsHolder(this Transform trans, TransHolder holder)
    {
        holder.SetAsLocal(trans);
    }

    public static void BindToParent(this Transform trans, Transform parent)
    {
        trans.SetParent(parent);
        trans.gameObject.ResetTransform();
    }

    #endregion

    #region Rotation

    public static Quaternion AddEularAngle(this Quaternion quaternion, Vector3 eularAngle)
    {
        return Quaternion.Euler(quaternion.eulerAngles + eularAngle);
    }

    public static Quaternion MinusEularAngle(this Quaternion quaternion, Vector3 eularAngle)
    {
        return Quaternion.Euler(quaternion.eulerAngles - eularAngle);
    }

    #endregion

    #region Array

    public static string ArrayToString<T>(this T[] array, string joinStr = ", ")
    {
        int count = array.Length;
        string rst = "";
        for (int i = 0; i < count; i++)
        {
            rst += array[i].ToString();
            if (i != count - 1)
            {
                rst += joinStr;
            }
        }
        return rst;
    }

    #endregion


    #region Dictionary

    /// <summary>
    /// Set value. If key is not exist then add, else just set.
    /// </summary>
    public static void SetAddValue<T, V>(this Dictionary<T, V> dic, T key, V value)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = value;
        }
        else
        {
            dic.Add(key, value);
        }
    }

    public static void SetAddValue<T, V>(this SortedDictionary<T, V> dic, T key, V value)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = value;
        }
        else
        {
            dic.Add(key, value);
        }
    }

    public static bool IsDictionaryEqual<T, V>(this Dictionary<T, V> thisDic, Dictionary<T, V> otherDic)
    {
        if (thisDic == null || otherDic == null)
            return thisDic == null && otherDic == null;

        if (thisDic.Count != otherDic.Count)
            return false;

        foreach (var p in thisDic)
            if (!otherDic.ContainsKey(p.Key) || !otherDic[p.Key].Equals(p.Value))
                return false;

        return true;
    }

    #endregion

    #region String

    public static Uri ToUrl(this string str)
    {
        Uri url = null;
        try
        {
            url = new Uri(str);
        }
        catch
        {
            Debug.LogError("can not create url by string : " + str);
        }
        return url;
    }

    #endregion

    #region Texture2D Sprite

    public static Sprite ToSprite(this Texture2D texture2d)
    {
        return Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), Vector2.zero);
    }

    #endregion

    #region MaskableGraphic

    public static void SetUIActiveRecursively(this GameObject gameObject, bool active)
    {
        MaskableGraphic[] maskableGraphics = gameObject.GetComponentsInChildren<MaskableGraphic>(true);
        int count = maskableGraphics.Length;
        for (int i = 0; i < count; i++)
        {
            maskableGraphics[i].SetUIActive(active);
        }
    }

    public static void SetUIActive(this MaskableGraphic maskableGraphic, bool active)
    {
        maskableGraphic.canvasRenderer.cull = !active;
    }

    #endregion
}
