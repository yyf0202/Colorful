using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OutlineTest : MonoBehaviour
{
    public List<GameObject> OutterlineGOs;

    private void Start()
    {
        foreach (var go in OutterlineGOs)
        {
            this.GetComponent<Outline>().AddOutterlineTarget(go);
        }
    }

}
