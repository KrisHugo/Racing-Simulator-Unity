using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance {get; private set;}
    public Camera playerCamera;
    public Transform followingObjects;

    public Vector3 moveOffset;
    public Vector3 rotOffset;
  
    [SerializeField] private float posSmoothness = 8.0f;
    [SerializeField] private float rotSmoothness = 100f;
    private void Awake(){
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = Camera.main;        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPostition = new Vector3();
        targetPostition = followingObjects.TransformPoint(moveOffset);
        
        playerCamera.transform.position = Vector3.Slerp(playerCamera.transform.position, targetPostition, posSmoothness * Time.fixedDeltaTime);

        Vector3 direction = followingObjects.position - playerCamera.transform.position;
        Quaternion rotation = new Quaternion();
        rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

        playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, rotation, rotSmoothness * Time.fixedDeltaTime);
    }


}
