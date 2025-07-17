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

    private VehicleController playerCar;
    // private GearSystem gearSystem;

    private void Start()
    {
        playerCar = GameSystem.Instance.GetPlayer();
        // gearSystem = playerCar.GetComponent<GearSystem>();
    }

    private void Update()
    {
        UpdateGearDisplay();
        UpdateVelocityDisplay();
    }

    private void UpdateVelocityDisplay()
    {
        string displayText = ((int)playerCar.engineSystem.VehicleSpeed).ToString() + "\n" + ((int)playerCar.engineSystem.CurrentRPM).ToString();
        
        velocityText.text = displayText;
        velocityText.color = (playerCar.engineSystem.CurrentGearState == GearState.Reverse) ? 
            reverseColor : 
            driveColor;
    }

    private void UpdateGearDisplay()
    {
        string displayText;
        if(playerCar.engineSystem.IsShifting){
            displayText = "N";
        }
        else{
            
            displayText = 
            playerCar.engineSystem.CurrentGearState switch
            {
                GearState.Reverse => "R",
                GearState.Neutral => "N",
                _ => $"{playerCar.engineSystem.CurrentGear}"
            };
        }

        gearText.text = displayText;
        gearText.color = (playerCar.engineSystem.CurrentGearState == GearState.Reverse) ? 
            reverseColor : 
            driveColor;
    }
}
