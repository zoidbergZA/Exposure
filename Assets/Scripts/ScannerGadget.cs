﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ScannerGadget : MonoBehaviour
{
    [SerializeField] private GameObject model;
    private Scanner scanner;
    private Collider myCollider;

    public bool IsGrabbed { get; private set; }

    void Awake()
    {
        scanner = FindObjectOfType<Scanner>();
        myCollider = GetComponent<Collider>();
    }

    void Start()
    {
        transform.SetParent(GameManager.Instance.PlanetTransform);
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.Player.PlayerState != Player.PlayerStates.Normal)
            return;

        if (!IsGrabbed)
            CheckGrab();
        else
        {
            CheckRelease();
        }
    }

    private void CheckGrab()
    {
        if (GameManager.Instance.TouchInput && Input.touchCount == 0)
            return;
        if (!GameManager.Instance.TouchInput && !Input.GetMouseButton(0))
            return;

        Vector2 rayPos;

        if (GameManager.Instance.TouchInput)
        {
            rayPos = Input.touches[0].position;
        }
        else
            rayPos = Input.mousePosition;

        
        Ray ray = Camera.main.ScreenPointToRay(rayPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject == gameObject)
            {
                Grab();
            }
        }
    }

    private void CheckRelease()
    {
        if (GameManager.Instance.TouchInput && Input.touchCount == 0)
            Release();
        if (!GameManager.Instance.TouchInput && !Input.GetMouseButton(0))
            Release();
    }

    private void Grab()
    {
        IsGrabbed = true;
        myCollider.enabled = false;
        model.SetActive(false);
    }

    private void Release()
    {
        IsGrabbed = false;
        myCollider.enabled = true;
        transform.position = scanner.transform.position;
        model.SetActive(true);
    }
}