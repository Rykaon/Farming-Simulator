using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private GridSystem gridSystem;
    private PathfindingMe pathfinding;

    private Vector3 orangeStartPosition;
    private Vector3 orangeStartRotation;
    private Vector3 blueStartPosition;
    private Vector3 blueStartRotation;

    public Vector3 position;
    public Vector3 rotation;
    public bool isReadyToRotate;

    [SerializeField] float moveSpeed;
    [SerializeField] float rotateSpeed;

    public Team team;

    public enum Team
    {
        Blue,
        Orange
    }

    void Start()
    {
        gridSystem = transform.GetComponent<GridSystem>();
        pathfinding = PathfindingMe.instance;

        orangeStartPosition = new Vector3(1f, 5f, 2.5f);
        orangeStartRotation = new Vector3(30f, 45f, 0f);
        blueStartPosition = new Vector3(15f, 5f, 13f);
        blueStartRotation = new Vector3(30f, 225f, 0f);

        team = Team.Orange;
        SetCamera();
    }

    public void SetCamera()
    {
        switch (team)
        {
            case Team.Blue:
                position = blueStartPosition;
                rotation = blueStartRotation;
                break;

            case Team.Orange:
                position = orangeStartPosition;
                rotation = orangeStartRotation;
                break;
        }

    }

    public void SetCamera(Vector3 unitPosition)
    {
        switch (Mathf.Round(transform.rotation.eulerAngles.y))
        {
            case 45:
                unitPosition.x = Mathf.Clamp(unitPosition.x, 1, 30);
                unitPosition.z = Mathf.Clamp(unitPosition.z, -1, 30);
                break;

            case 135:
                unitPosition.x = Mathf.Clamp(unitPosition.x, 1, 30);
                unitPosition.z = Mathf.Clamp(unitPosition.z, -15, 15);
                break;

            case 225:
                unitPosition.x = Mathf.Clamp(unitPosition.x, -15, 15);
                unitPosition.z = Mathf.Clamp(unitPosition.z, -15, 15);
                break;

            case 315:
                unitPosition.x = Mathf.Clamp(unitPosition.x, -15, 15);
                unitPosition.z = Mathf.Clamp(unitPosition.z, -1, 30);
                break;
        }
        position = unitPosition;
    }

    public void Zoom(float value)
    {
        float ratio = 0.01f;
        float max = 5f;
        float min = 2f;
        float actual = transform.GetComponent<Camera>().orthographicSize;

        if (value < 0)
        {
            if ((actual += ratio) <= max)
            {
                transform.GetComponent<Camera>().orthographicSize = actual += ratio;
            }
            else
            {
                transform.GetComponent<Camera>().orthographicSize = max;
            }
        }
        else if (value > 0)
        {
            if ((actual -= ratio) >= min)
            {
                transform.GetComponent<Camera>().orthographicSize = actual -= ratio;
            }
            else
            {
                transform.GetComponent<Camera>().orthographicSize = min;
            }
        }
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
        }

        if (transform.rotation.eulerAngles != rotation)
        {
            isReadyToRotate = false;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(rotation), rotateSpeed * Time.deltaTime);
        }
        else
        {
            isReadyToRotate = true;
        }
    }
}
