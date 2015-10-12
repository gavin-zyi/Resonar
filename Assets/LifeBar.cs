using System;
using UnityEngine;
using System.Collections;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
public class LifeBar : MonoBehaviour
{
    public Font FontStyle;
    private GUIText label;
    private GUITexture bar;

    private float health;
    private const float FullHealth = 100f;

    private readonly Color[] barColors = new[] {Color.red, Color.yellow, Color.green};

    // Use this for initialization
    protected void Start()
    {
        health = FullHealth;
        label = CreateLabel();
        bar = CreateBar();
        StartCoroutine(AnimateBar());
    }

    public void Damage(float amount)
    {
        SetHealth(health - amount);
    }

    public void Heal(float amount)
    {
        SetHealth(health + amount);
    }

    private void SetHealth(float value)
    {
        health = Mathf.Max(0, value);
    }

    private IEnumerator AnimateBar()
    {
        var local = health;
        while (gameObject != null)
        {
            if (Mathf.Abs(local - health) > Mathf.Epsilon)
            {
                var inset = bar.pixelInset;
                var normal = Mathf.Lerp(local, health, Time.time*0.25f)/FullHealth;
                inset.width = normal*Screen.height*0.5f;
                bar.color = barColors[Mathf.Min(barColors.Length - 1, (int) (normal*barColors.Length))];
                bar.pixelInset = inset;
            }

            yield return 0;
        }
    }

    private GUITexture CreateBar()
    {
        GameObject value = new GameObject("Bar-Value", typeof (GUITexture)),
                   overlay = new GameObject("Bar-Overlay", typeof (GUITexture));

        value.transform.parent = overlay.transform.parent = transform;

        GUITexture valueImg = value.GetComponent<GUITexture>(), overlayImg = overlay.GetComponent<GUITexture>();

        valueImg.texture = new Texture2D(100, 100);
        overlayImg.texture = new Texture2D(100, 100);

        var width = Screen.height*0.5f;
        var height = Screen.height*0.025f;
        var left = Screen.height*0.2f;
        var top = Screen.height*0.05f;

        valueImg.pixelInset = new Rect(left, -(height + top), width, height);
        valueImg.color = barColors[barColors.Length - 1];

        var offset = Screen.height*0.005f;
        overlayImg.pixelInset = new Rect(left - offset, -(height + top + offset), width + offset*2, height + offset*2);
        overlayImg.color = new Color(0.28f, 0.28f, 0.28f);

        SetPosition(value, 1);
        SetPosition(overlay, 0);
        return valueImg;
    }

    private GUIText CreateLabel()
    {
        var obj = new GameObject("Label", typeof (GUIText));
        obj.transform.parent = transform;

        var text = obj.GetComponent<GUIText>();
        text.font = FontStyle;
        text.text = "Player";
        text.fontSize = (int) (Screen.height*0.05f);
        text.anchor = TextAnchor.UpperLeft;
        text.alignment = TextAlignment.Left;

        var left = Screen.height*0.05f;
        var top = Screen.height*0.025f;
        text.pixelOffset = new Vector2(left, -top);

        SetPosition(obj, 0);
        return text;
    }

    private static void SetPosition(GameObject obj, float depth)
    {
        obj.transform.localScale = new Vector3(0, 0, 1);
        obj.transform.position = new Vector3(0, 1, depth);
    }

    // Update is called once per frame
    protected void Update()
    {

    }
}
