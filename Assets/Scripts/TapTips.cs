using UnityEngine;
using System.Collections;

public class TapTips : MonoBehaviour
{
    [SerializeField] private RectTransform tipsPanel;
    [SerializeField] private Animator[] tips;
    [SerializeField] private float tipDuration = 6f;

    private float timeleft;
    private Transform targetTransform;

    void Awake()
    {
        HideTips();
    }

    void Update()
    {
        if (timeleft > 0)
        {
            timeleft -= Time.deltaTime;

            if (targetTransform)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position);

                tipsPanel.position = screenPos;
            }

            if (timeleft <= 0)
            {
                HideTips();
            }
        }
    }

    public void ShowRandomTip(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
        timeleft = tipDuration;
        tipsPanel.gameObject.SetActive(true);
        int rand = Random.Range(0, tips.Length);

        for (int i = 0; i < tips.Length; i++)
        {
            if (i == rand)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position);

                tipsPanel.position = screenPos;
                tips[i].gameObject.SetActive(true);
            }
            else
            {
                tips[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideTips()
    {
        tipsPanel.gameObject.SetActive(false);
    }
}
