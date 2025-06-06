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

    private CarMovement playerCar;
    private GearSystem gearSystem;

    private void Start()
    {
        playerCar = GameSystem.Instance.GetPlayer();
        gearSystem = playerCar.GetComponent<GearSystem>();
    }

    private void Update()
    {
        UpdateGearDisplay();
        UpdateVelocityDisplay();
    }

    private void UpdateVelocityDisplay()
    {
        string displayText = ((int)playerCar.CurrentSpeed).ToString() + "\n" + ((int)playerCar.EngineRPM).ToString();
        
        velocityText.text = displayText;
        velocityText.color = (gearSystem.CurrentGearState == GearState.Reverse) ? 
            reverseColor : 
            driveColor;
    }

    private void UpdateGearDisplay()
    {
        string displayText;
        if(gearSystem.IsShifting){
            displayText = "N";
        }
        else{
            
            displayText = 
            gearSystem.CurrentGearState switch
            {
                GearState.Reverse => "R",
                GearState.Neutral => "N",
                _ => $"{gearSystem.currentGear}"
            };
        }

        gearText.text = displayText;
        gearText.color = (gearSystem.CurrentGearState == GearState.Reverse) ? 
            reverseColor : 
            driveColor;
    }
}
