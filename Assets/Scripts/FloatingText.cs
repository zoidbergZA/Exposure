using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private Text textField; 

    public RectTransform RectTransform { get { return GetComponent<RectTransform>(); } }
    public bool IsInitialized { get; private set; }
    public Transform TargetTransform { get; private set; }

    public void Init(string text, Transform target)
    {
        if (IsInitialized)
            return;


        TargetTransform = target;
        textField.text = text;
        RectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(target.position);

//        LeanTween.move(RectTransform, (Vector3)position + new Vector3(0f, 50f, 0f), 5f).setOnComplete(OnComplete).setEase(LeanTweenType.easeOutSine);
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
