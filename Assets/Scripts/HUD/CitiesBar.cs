using System;
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

//        //test
//        GameManager.Instance.Cities[3].CleanUp();
	}

    public void SortIcons()
    {
        Array.Sort(cityIcons);

        for (int i = 0; i < cityCount; i++)
        {
            cityIcons[i].RectTransform.anchoredPosition = new Vector2(0, -cityIcons[i].RectTransform.sizeDelta.y * i);
        }
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

            //link icon and city
            cityIcons[i].LinkCity(GameManager.Instance.Cities[i]);
        }

        //destroy prefab
        Destroy(cityIconPrefab.gameObject);

        SortIcons();
    }
}
