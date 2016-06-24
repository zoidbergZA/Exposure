using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private Text textFieldBack;
    [SerializeField] private Text textField;
    [SerializeField] private float destroyTime = 2.1f;

    public RectTransform RectTransform { get { return GetComponent<RectTransform>(); } }
    public bool IsInitialized { get; private set; }
    public Transform TargetTransform { get; private set; }

    private float yOffset;
    private float initialOffset;

    void Update()
    {
        if (IsInitialized && TargetTransform)
        {
            RectTransform.position = Camera.main.WorldToScreenPoint(TargetTransform.position) + new Vector3(0, yOffset + initialOffset, 0);
        }
    }

    public void Init(string text, Transform target, bool positive)
    {
        if (IsInitialized)
            return;

        IsInitialized = true;
        TargetTransform = target;
        textFieldBack.text = text;
        textField.text = text;
        
        if (!positive)
            textField.color = Color.red;

        initialOffset = Random.Range(0f, 80f);

        RectTransform.position = Camera.main.WorldToScreenPoint(target.position);

        LeanTween.value(gameObject, updateTweenCallback, 0, 40f, destroyTime).setOnComplete(OnRiseComplete).setEase(LeanTweenType.easeOutSine);
        //        LeanTween.move(RectTransform, (Vector3)position + new Vector3(0f, 50f, 0f), 5f).setOnComplete(OnRiseComplete).setEase(LeanTweenType.easeOutSine);
    }

    void updateTweenCallback(float val, float ratio)
    {
        yOffset = val;
    }

    private void OnRiseComplete()
    {
//        LeanTween.move(RectTransform, -RectTransform.anchoredPosition, 2f).setOnComplete(Kill);
        Kill();
    }

    private void Kill()
    {
        Destroy(gameObject);
    }
}
