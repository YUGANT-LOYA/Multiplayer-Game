using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance = null;

    public static CameraManager singleton
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CameraManager>();
            }

            return _instance;
        }
    }


    [SerializeField] [Range(-5f, 5f)] private float _defaultSensitivity = 1f;
    [SerializeField] [Range(-5f, 5f)] private float _aimingSensitivity = 0.75f;
    [SerializeField] private Camera _camera = null;
    [SerializeField] private CinemachineVirtualCamera _playerCamera = null;
    [SerializeField] private CinemachineVirtualCamera _aimingCamera = null;
    [SerializeField] private CinemachineBrain _cameraBrain = null;
    [SerializeField] private LayerMask _aimLayer;

    public static float defaultSensitivity => singleton._defaultSensitivity;
    public static float aimingSensitivity => singleton._aimingSensitivity;
    public static Camera mainCamera => singleton._camera;
    public static CinemachineVirtualCamera playerCamera => singleton._playerCamera;
    public static CinemachineVirtualCamera aimingCamera => singleton._aimingCamera;

    public float sensitivity => _aiming ? _aimingSensitivity : _defaultSensitivity;

    private bool _aiming = false;

    public bool aiming
    {
        get => _aiming;
        set => _aiming = value;
    }

    Vector3 _aimTargetPoint = Vector3.zero;
    public Vector3 aimTargetPoint => _aimTargetPoint;

    private void Awake()
    {
        _cameraBrain.m_DefaultBlend.m_Time = 0.1f;
    }

    private void Update()
    {
        _aimingCamera.gameObject.SetActive(_aiming);
        SetAimTarget();
    }


    void SetAimTarget()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _aimLayer))
        {
            _aimTargetPoint = hit.point;
        }
        else
        {
            _aimTargetPoint = ray.GetPoint(1000f);
        }

    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_aimTargetPoint, 0.1f);
    }
#endif
}