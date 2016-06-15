using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CityIcon : MonoBehaviour
{
    [SerializeField] private Sprite dityIcon;
    [SerializeField] private Sprite cleanIcon;

    public City City { get; set; }
    public RectTransform RectTransform { get {return GetComponent<Image>().rectTransform;} }
}
