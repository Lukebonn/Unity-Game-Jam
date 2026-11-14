using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Thin health bars over hurt enemies. Enemies at full health get none, to keep the screen clean.
// Goes on a full-screen object in the Canvas. The bars are made when they're first needed.
public class EnemyHealthBars : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(60f, 6f);
    [SerializeField] private Color background = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Color fill = new Color(0.9f, 0.25f, 0.2f);

    private readonly List<RectTransform> bars = new List<RectTransform>();
    private readonly List<RectTransform> fills = new List<RectTransform>();
    private RectTransform area;
    private Canvas canvas;
    private Camera cam;

    private void Awake()
    {
        area = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
    }

    private void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // An overlay canvas needs no camera to turn screen points into canvas points
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        int used = 0;
        foreach (EnemyBase enemy in EnemyBase.All)
        {
            float health = enemy.HealthFraction;
            if (enemy.IsDead || health >= 1f) continue;

            Vector3 overHead = enemy.transform.position + Vector3.up * enemy.healthBarHeight;
            Vector3 screen = cam.WorldToScreenPoint(overHead);
            if (screen.z < 0f) continue;   // behind the camera
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screen, uiCamera, out Vector2 local)) continue;

            if (used == bars.Count) MakeBar();
            RectTransform bar = bars[used];
            bar.gameObject.SetActive(true);
            bar.localPosition = local;
            fills[used].sizeDelta = new Vector2(size.x * health, size.y);
            used++;
        }

        // Hide the bars nobody needs this frame
        for (int i = used; i < bars.Count; i++)
            bars[i].gameObject.SetActive(false);
    }

    private void MakeBar()
    {
        var bar = new GameObject("Enemy Health Bar", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        bar.SetParent(area, false);
        bar.sizeDelta = size;
        Image back = bar.GetComponent<Image>();
        back.color = background;
        back.raycastTarget = false;

        // The fill grows from the bar's left edge
        var front = new GameObject("Fill", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        front.SetParent(bar, false);
        front.anchorMin = front.anchorMax = new Vector2(0f, 0.5f);
        front.pivot = new Vector2(0f, 0.5f);
        front.anchoredPosition = Vector2.zero;
        front.sizeDelta = size;
        Image frontImage = front.GetComponent<Image>();
        frontImage.color = fill;
        frontImage.raycastTarget = false;

        bars.Add(bar);
        fills.Add(front);
    }
}
