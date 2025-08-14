using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance{get; private set;}
    // public VehicleController playerCar;
    public AxlePhysics playerCar;
    [SerializeField] public Transform respawnPoint;
    public float airDragCoefficient = 0.3f;
    public GroundFrictions groundFrictions;
    public LowSpeedConfig lowSpeedControl;
    void Awake(){
        
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    // void Update()
    // {
    //     CheckRespawn();
    // }

    // private void CheckRespawn(){
    //     // if(InputManager.Instance.RespawnInput){
    //     //     playerCar.Respawn(respawnPoint);
    //     // }
    // }

    public AxlePhysics GetPlayer()
    {
        return playerCar;
    }
}
