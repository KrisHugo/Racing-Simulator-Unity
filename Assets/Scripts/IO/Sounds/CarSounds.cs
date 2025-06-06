using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarSounds : MonoBehaviour
{
    [SerializeField] private CarMovement car;
    [Header("Basic Sound Settings")]
    [SerializeField] public AudioClip engineClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;

    [SerializeField] public float RPMoffSet = 1000;

    private float minRPM;
    private float maxRPM;
    private float currentRPM;


    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<CarMovement>();

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        minRPM = car.MinRPM + RPMoffSet;
        maxRPM = car.MaxRPM - RPMoffSet;
    }

    // Update is called once per frame
    void Update()
    {
        if(car.carStatus == CarMovement.Status.On){
            if(audioSource.isPlaying == false){
                audioSource.clip = engineClip;
                audioSource.Play();
            }
            PlayEngineSounds();
        }
        else{
            audioSource.Pause();
        }
    }

    private void PlayEngineSounds(){
        currentRPM = car.EngineRPM;

        if(currentRPM < minRPM){
            audioSource.pitch = minPitch;
        }
        else if(currentRPM > minRPM && currentRPM < maxRPM){
            float pitchFromRPM = currentRPM / maxRPM;
            audioSource.pitch = pitchFromRPM;
        }
        else{
            audioSource.pitch = maxPitch;
        }

        
    }
}
