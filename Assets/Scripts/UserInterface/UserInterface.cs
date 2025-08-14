using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserInterface : MonoBehaviour
{
    [SerializeField] private TMP_Text gearText;
    [SerializeField] private TMP_Text velocityText;
    [SerializeField] private Color reverseColor = Color.red;
    [SerializeField] private Color driveColor = Color.green;

    // private VehicleController playerCar;
    // private GearSystem gearSystem;
    private AxlePhysics carStatus;

    private void Start()
    {
        // playerCar = GameSystem.Instance.GetPlayer();
        // gearSystem = playerCar.GetComponent<GearSystem>();
        carStatus = GameSystem.Instance.GetPlayer();
    }

    private void Update()
    {
        UpdateGearDisplay();
        UpdateVelocityDisplay();
    }

    private void UpdateVelocityDisplay()
    {
        string displayText = $"{carStatus.GetVehicleKMH():F8}";
        
        velocityText.text = displayText;
        // velocityText.color = (carStatus.transmission.CurrentGearState == GearState.Reverse) ? 
        //     reverseColor : 
        //     driveColor;
    }

    private void UpdateGearDisplay()
    {
        // string displayText;
        // if(playerCar.transmission.IsShifting){
        //     displayText = "N";
        // }
        // else{
            
        //     displayText = 
        //     playerCar.transmission.CurrentGearState switch
        //     {
        //         GearState.Reverse => "R",
        //         GearState.Neutral => "N",
        //         _ => $"{playerCar.transmission.CurrentGear}"
        //     };
        // }

        // gearText.text = displayText;
        // gearText.color = (playerCar.transmission.CurrentGearState == GearState.Reverse) ? 
        //     reverseColor : 
        //     driveColor;
    }
}
