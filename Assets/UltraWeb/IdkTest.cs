using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IdkTest : MonoBehaviour
{
    private UltraWeb ultraWeb;
    private RawImage pomoc;
    void Start()
    {
        ultraWeb = new UltraWeb(1080, 1920);
        pomoc = GetComponent<RawImage>();
        ultraWeb.getTexture();
    }

    
    void Update()
    {
       
    }
}
