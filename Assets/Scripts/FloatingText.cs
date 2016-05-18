using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private Text textField;
    [SerializeField] private float destroyTime = 2.1f;

    public RectTransform RectTransform { get { return GetComponent<RectTransform>(); } }
    public bool IsInitialized { get; private set; }
    public Transform TargetTransform { get; private set; }

    private float yOffset;

    void Update()
    {
        if (IsInitialized && TargetTransform)
        {
            RectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(TargetTransform.position) + new Vector3(0, yOffset, 0);
        }
    }

    public void Init(string text, Transform target)
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
        TargetTransform = target;
        textField.text = text;
        RectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(target.position);

        LeanTween.value(gameObject, updateTweenCallback, 0, 40f, destroyTime).setOnComplete(OnComplete).setEase(LeanTweenType.easeOutSine);
        //        LeanTween.move(RectTransform, (Vector3)position + new Vector3(0f, 50f, 0f), 5f).setOnComplete(OnComplete).setEase(LeanTweenType.easeOutSine);
    }

    void updateTweenCallback(float val, float ratio)
    {
        yOffset = val;
    }

    private void OnComplete()
    {
        Kill();
    }

    private void Kill()
    {
        Destroy(gameObject);
    }
}
