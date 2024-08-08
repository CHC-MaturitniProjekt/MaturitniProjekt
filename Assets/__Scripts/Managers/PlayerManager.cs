using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    private Movement _movement;
    private CameraController _camera;

    void Start()
    {
        _movement = FindObjectOfType<Movement>();
        _camera = FindObjectOfType<CameraController>();
    }

    void Update()
    {
        _camera.FovChange(_movement.isSprinting);
        
    }
}
