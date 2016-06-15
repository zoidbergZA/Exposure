using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CitiesBar : MonoBehaviour
{
    [SerializeField] private CityIcon cityIconPrefab;

    private RectTransform myRectTransform;
    private int cityCount;
    private CityIcon[] cityIcons;

    void Start ()
    {
	    Init();
	}

    private void Init()
    {
        //resize panel
        myRectTransform = GetComponent<RectTransform>();
        cityCount = GameManager.Instance.Cities.Length;
        myRectTransform.sizeDelta = new Vector2(cityIconPrefab.RectTransform.sizeDelta.x, cityIconPrefab.RectTransform.sizeDelta.y * cityCount);

        cityIcons = new CityIcon[cityCount];

        for (int i = 0; i < cityCount; i++)
        {
            CityIcon cityIcon = Instantiate(cityIconPrefab);
            cityIcon.RectTransform.SetParent(myRectTransform);
            cityIcon.RectTransform.localScale = Vector3.one;
            cityIcon.RectTransform.anchoredPosition = new Vector2(0, -cityIconPrefab.RectTransform.sizeDelta.y * i);
            cityIcons[i] = cityIcon;
        }

        //destroy prefab
        Destroy(cityIconPrefab.gameObject);
    }
}
