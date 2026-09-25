using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISkill : MonoBehaviour
{
    private Material mat;
    private static readonly int YinValue = Shader.PropertyToID("_YinAmount");
    private static readonly int YangValue = Shader.PropertyToID("_YangAmount");
    private static readonly int OverflowValue = Shader.PropertyToID("_OverflowAmount");
    private static readonly int BoolOverflowYin = Shader.PropertyToID("_BoolOverflowYin");

    public float duration = 1.0f;
    private float preOverflowValue = 0;
    private float overflowValue = 0;

    void Start()
    {
        mat = GetComponent<Image>().material;
        Debug.Log("Material assigned: " + mat.name + " | Shader: " + mat.shader.name);

        mat.SetFloat(YinValue, 0);
        mat.SetFloat(YangValue, 0);
        mat.SetFloat(OverflowValue, 0);
    }

    public void UpdateSkill(float yinValue, float yangValue, float preYinValue, float preYangValue)
    {
        if (mat == null) return;

        yinValue = yinValue/4;
        yangValue = yangValue/4;
        preYinValue = preYinValue/4;
        preYangValue = preYangValue/4;

        bool overflowExists = false;
        if (yinValue > 1 || yangValue > 1) overflowExists = true;
        
        if (!overflowExists)
        {
            preOverflowValue = overflowValue;
            overflowValue = 0;
        }
        else if (yinValue > 1)
        {
            mat.SetInt(BoolOverflowYin, 1);
            preOverflowValue = overflowValue;
            overflowValue = yinValue - 1;
        }
        else if (yangValue > 1)
        {
            mat.SetInt(BoolOverflowYin, 0);
            preOverflowValue = overflowValue;
            overflowValue = yangValue - 1;
        }

        StartCoroutine(LerpOverTime(yinValue, yangValue, overflowValue, preYinValue, preYangValue, preOverflowValue));

    }

    private IEnumerator LerpOverTime(float yinValue, float yangValue, float overflowValue, float preYinValue, float preYangValue, float preOverflowValue)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime * 2;
            
            float t = elapsedTime / duration; 

            mat.SetFloat(YinValue, Mathf.Lerp(preYinValue, yinValue, t));
            mat.SetFloat(YangValue, Mathf.Lerp(preYangValue, yangValue, t));
            mat.SetFloat(OverflowValue, Mathf.Lerp(preOverflowValue, overflowValue, t));

            yield return null;

        }
        mat.SetFloat(YinValue, yinValue);
        mat.SetFloat(YangValue, yangValue);
        mat.SetFloat(OverflowValue, overflowValue);
    }

}