using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct ChildData
{
    public RectTransform transform;
    public float value;
}

public class BattleUnitStatsUIBarHandler : MonoBehaviour
{
    [SerializeField] public RectTransform parentT;
    public List<ChildData> data = new List<ChildData>();
    public List<BattleUnitStatsUIBar> statsUIBars = new List<BattleUnitStatsUIBar>();
    public float animationDuration = 0.2f; // Tweak for speed
    public Ease animationEase = Ease.OutQuad; // Smooth easing type

    // Assuming a VerticalLayoutGroup for vertical stacking; adjust if horizontal/grid
    private VerticalLayoutGroup layoutGroup;
    private float lastTime;
    private float delay = .5f;

    void Start()
    {
        layoutGroup = parentT.GetComponent<VerticalLayoutGroup>();
        BattleController.onStatsChange += UpdateData;
    }

    void OnDestroy()
    {
        BattleController.onStatsChange -= UpdateData;
        DOTween.Kill(parentT);
    }

    void FillData()
    {
        data.Clear();
        for (int i = 0; i < parentT.childCount; i++)
        {
            RectTransform kid = parentT.GetChild(i) as RectTransform;
            float val = GetValueForChild(kid);
            data.Add(new ChildData { transform = kid, value = val });
        }
    }

    void ReorderChildrenSmoothly()
    {
        // Sort data by value
        //data.Sort((a, b) => a.value.CompareTo(b.value));
        data.Sort((a, b) => b.value.CompareTo(a.value));

        // Temporarily disable layout to allow manual tweening
        if (layoutGroup != null) layoutGroup.enabled = false;

        // Calculate target positions based on new order (assuming vertical layout)
        Vector2[] targetPositions = new Vector2[data.Count];
        float currentY = -66.8f; // Start from top
        for (int i = 0; i < data.Count; i++)
        {
            RectTransform rt = data[i].transform;
            targetPositions[i] = new Vector2(rt.anchoredPosition.x, currentY);
            currentY -= (rt.rect.height + (layoutGroup != null ? layoutGroup.spacing : 0f)); // Accumulate downward
        }

        // Tween each to its target
        DG.Tweening.Sequence seq = DOTween.Sequence();
        for (int i = 0; i < data.Count; i++)
        {
            int index = i; // Capture for closure
            if (data[i].transform.gameObject != null)
            {
                seq.Join(data[i].transform.DOAnchorPos(targetPositions[index], animationDuration).SetEase(animationEase));
            }
        }

        // After all tweens finish, set sibling indices and re-enable layout
        seq.OnComplete(() =>
        {
            for (int i = 0; i < data.Count; i++)
            {
                data[i].transform.GetComponent<BattleUnitStatsUIBar>().RankChange(i);
                data[i].transform.SetSiblingIndex(i);
            }
            if (layoutGroup != null) layoutGroup.enabled = true;
            seq.Kill();
        });
    }

    float GetValueForChild(RectTransform kid)
    {
        return kid.GetComponent<BattleUnitStatsUIBar>().StatValue();
    }

    void UpdateData()
    {
        // Example: Call when values change (optimize to not call every frame)
        if (Timer()) { return; }
        FillData();
        ReorderChildrenSmoothly();
    }

    public void AddUIBar(BattleUnitStatsUIBar bar)
    {
        statsUIBars.Add(bar);
    }

    public void RemoveUIBAr(BattleUnitStatsUIBar bar)
    {
        statsUIBars.Remove(bar);
    }

    public bool Timer()
    {
        if (Time.time > (lastTime + delay))
        {
            lastTime = Time.time;
            return false;
        }
        else
        {
            return true;

        }
    }
}
