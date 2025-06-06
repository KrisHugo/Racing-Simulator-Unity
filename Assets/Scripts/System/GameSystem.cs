using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance{get; private set;}
    public CarMovement playerCar;
    [SerializeField] public Transform respawnPoint;
    void Awake(){
        
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        CheckRespawn();
    }

    private void CheckRespawn(){
        if(InputManager.Instance.RespawnInput){
            playerCar.Respawn(respawnPoint);
        }
    }

    public CarMovement GetPlayer(){
        return playerCar;
    }
}
