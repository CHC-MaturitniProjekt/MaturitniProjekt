using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PickUp : MonoBehaviour
{
    [SerializeField] private InputReader input;

    [SerializeField] private Transform itemHolster;
    [SerializeField] private float itemScale = 0.5f;
    [SerializeField] private float throwForce = 8f;

    private bool isHoldingItem = false;
    private GameObject currentItem;

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

        PrepareItem(item);
        isHoldingItem = true;
    }

    private void PrepareItem(GameObject item)
    {
        currentItem = item;
        item.transform.SetParent(itemHolster);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one * itemScale;

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

    public void DropItem(GameObject item)
    {
        if (!isHoldingItem) return;

        item.transform.SetParent(null);
        item.transform.localScale = Vector3.one;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.freezeRotation = false;

            Vector3 launchDirection = itemHolster.right;
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
