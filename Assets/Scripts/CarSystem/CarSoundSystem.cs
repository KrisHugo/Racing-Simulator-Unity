using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarSoundSystem : MonoBehaviour
{
    [SerializeField] private VehicleController car;
    [Header("Basic Sound Settings")]
    [SerializeField] public AudioClip engineClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;

    [SerializeField] public float RPMoffSet = 1000;
    private float minRPM;
    private float maxRPM;
    private float currentRPM;

    public void Initialize(float _minRPM, float _maxRPM)
    {
        // car = GetComponent<VehicleController>();
        minRPM = _minRPM;
        maxRPM = _maxRPM;
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
    }

    public void UpdateEngineSound(bool isEngineOn, float rpm){

        if(isEngineOn == true){
            if(audioSource.isPlaying == false){
                audioSource.clip = engineClip;
                audioSource.Play();
            }
            PlayEngineSounds(rpm);
        }
        else{
            audioSource.Pause();
        }
    }

    private void PlayEngineSounds(float rpm){
        

        if(rpm < minRPM){
            audioSource.pitch = minPitch;
        }
        else if(rpm > minRPM && rpm < maxRPM){
            float pitchFromRPM = rpm / maxRPM;
            audioSource.pitch = pitchFromRPM;
        }
        else{
            audioSource.pitch = maxPitch;
        }

        
    }
}
