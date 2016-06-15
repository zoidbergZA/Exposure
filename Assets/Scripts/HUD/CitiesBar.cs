using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CitiesBar : MonoBehaviour
{
    [SerializeField] private Image cityIconPrefab;

    private RectTransform myRectTransform;
    private int cityCount;
    private Image[] cityIcons; //todo: replace with cityIcon class that has a reference to its city

    void Start ()
    {
	    Init();
	}

    private void Init()
    {
        //resize panel
        myRectTransform = GetComponent<RectTransform>();
        cityCount = GameManager.Instance.Cities.Length;
        myRectTransform.sizeDelta = new Vector2(cityIconPrefab.rectTransform.sizeDelta.x, cityIconPrefab.rectTransform.sizeDelta.y * cityCount);

        cityIcons = new Image[cityCount];

        for (int i = 0; i < cityCount; i++)
        {
            Image cityIcon = Instantiate(cityIconPrefab);
            cityIcon.rectTransform.SetParent(myRectTransform);
            cityIcon.rectTransform.localScale = Vector3.one;
            cityIcon.rectTransform.anchoredPosition = new Vector2(0, -cityIconPrefab.rectTransform.sizeDelta.y * i);
            cityIcons[i] = cityIcon;
        }

        //destroy prefab
        Destroy(cityIconPrefab.gameObject);
    }
}
