using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PickUp : MonoBehaviour
{
    [SerializeField] private InputReader input;

    [SerializeField] private Transform itemHolster;
    [SerializeField] private Transform dropPoint;
    
    [SerializeField] private float itemScale = 0.5f;
    [SerializeField] private float throwForce = 8f;
    private Vector3 initItemScale;

    public bool isHoldingItem = false;
    private Animator animator;
    public GameObject currentItem;
    private int itemIndex;
    private GameObject[] items;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetLayerWeight(animator.GetLayerIndex("Holding"), isHoldingItem ? 1 : 0);
    }

    void Start()
    {
        input.DropEvent += OnDrop;
    }

    private void OnDrop()
    {
        if (!isHoldingItem) return;

        DropItem(currentItem);
    }

    public void CarryItem(GameObject item)
    {
        if (isHoldingItem) return;
        
        animator.SetLayerWeight(animator.GetLayerIndex("Holding"),1);
        
        PrepareItem(item);
        isHoldingItem = true;
    }

    private void PrepareItem(GameObject item)
    {
        currentItem = item;
        initItemScale = item.transform.localScale;
        item.transform.SetParent(itemHolster, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.rotation = Quaternion.Euler(0, 0, -90);
        item.transform.localRotation = Quaternion.Euler(0, 0, -90);
        
        Vector3 rigScale = itemHolster.lossyScale;
        Vector3 correctedScale = (initItemScale * itemScale);
        correctedScale.x /= rigScale.x;
        correctedScale.y /= rigScale.y;
        correctedScale.z /= rigScale.z;

        item.transform.localScale = correctedScale;
        
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.freezeRotation = true;
        }

        Collider collider = item.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private void DropItem(GameObject item)
    {
        if (!isHoldingItem) return;

        item.transform.SetParent(null);
        item.transform.position = dropPoint.position;
        item.transform.rotation = dropPoint.rotation;
        item.transform.localScale = initItemScale;

        animator.SetLayerWeight(animator.GetLayerIndex("Holding"), 0);

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.freezeRotation = false;

            Vector3 launchDirection = dropPoint.forward;
            rb.AddForce(launchDirection * throwForce, ForceMode.Impulse);
        }

        Collider collider = item.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        isHoldingItem = false;
    }
}
