using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CityIcon : MonoBehaviour, IComparable<CityIcon>
{
    [SerializeField] private Sprite dityIcon;
    [SerializeField] private Sprite cleanIcon;

    private Image image;

    public City City { get; private set; }
    public RectTransform RectTransform { get {return GetComponent<Image>().rectTransform;} }

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void LinkCity(City city)
    {
        City = city;
        City.CityIcon = this;
    }

    public void ToggleIcon(bool clean)
    {
        image.sprite = cleanIcon;
    }

    public int CompareTo(CityIcon other)
    {
        if (other.City.CityState == CityStates.CLEAN)
            return 0;
        return 1;

//        return City.IsDirty.CompareTo(other.City.IsDirty);
    }
}
