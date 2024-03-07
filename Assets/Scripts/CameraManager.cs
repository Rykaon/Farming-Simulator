using Cinemachine;
using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] public PlayerManager PC_Manager;
    [SerializeField] public CinemachineFreeLook worldCamera;
    [SerializeField] private Transform followTransitionTarget;
    [SerializeField] private Transform lookTransitionTarget;

    [Header("General References")]
    [SerializeField] private float cameraTransitionDuration;
    [SerializeField] private float playerOrbitHeight;
    [SerializeField] private float playerOrbitRadius;
    [SerializeField] private float unitOrbitHeight;
    [SerializeField] private float unitOrbitRadius;
    [HideInInspector] public bool isCameraSet = true;
    [HideInInspector] public Transform previousTarget;
    [HideInInspector] public Coroutine cameraTransition = null;

    public IEnumerator SetCameraTarget(Transform follow, Transform look)
    {
        isCameraSet = false;
        previousTarget = worldCamera.m_Follow;

        float elapsedTime = 0f;
        Transform startFollow = worldCamera.m_Follow;
        Transform startLook = worldCamera.m_LookAt;
        followTransitionTarget.position = startFollow.position;
        followTransitionTarget.rotation = startFollow.rotation;
        lookTransitionTarget.position = startLook.position;
        lookTransitionTarget.rotation = startLook.rotation;
        worldCamera.m_Follow = followTransitionTarget;
        worldCamera.m_LookAt = lookTransitionTarget;
        float startHeight = worldCamera.m_Orbits[0].m_Height;
        float startRadius = worldCamera.m_Orbits[0].m_Radius;

        while (elapsedTime < cameraTransitionDuration)
        {
            float time = elapsedTime / cameraTransitionDuration;

            if (follow == PC_Manager.transform)
            {
                for (int i = 0; i < worldCamera.m_Orbits.Length; ++i)
                {
                    worldCamera.m_Orbits[i].m_Height = Mathf.Lerp(startHeight, playerOrbitHeight, time);
                    worldCamera.m_Orbits[i].m_Radius = Mathf.Lerp(startRadius, playerOrbitRadius, time);
                }
            }
            else
            {
                for (int i = 0; i < worldCamera.m_Orbits.Length; ++i)
                {
                    worldCamera.m_Orbits[i].m_Height = Mathf.Lerp(startHeight, unitOrbitHeight, time);
                    worldCamera.m_Orbits[i].m_Radius = Mathf.Lerp(startRadius, unitOrbitRadius, time);
                }
            }

            worldCamera.m_Follow.position = Vector3.Lerp(startFollow.position, follow.position, time);
            worldCamera.m_Follow.rotation = Quaternion.Slerp(startFollow.rotation, follow.rotation, time);

            worldCamera.m_LookAt.position = Vector3.Lerp(startLook.position, look.position, time);
            worldCamera.m_LookAt.rotation = Quaternion.Slerp(startLook.rotation, look.rotation, time);

            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        worldCamera.m_Follow = follow;
        worldCamera.m_LookAt = look;
        isCameraSet = true;
        cameraTransition = null;
    }
}
