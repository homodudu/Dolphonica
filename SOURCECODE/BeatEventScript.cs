/******************************************************************************

- BeatEventScript [original class]

1) AudioAnalyzer class peforms an FFT on audio waveform.
2) Get the beat detetion data, calculate and scale averageSpectrumEnergy.
3) Sensitivity modifier so that subtle beat data such as Hi-Hats is detetcted.

AudioAnalyzer class  by Cobalt910 (2017)
Available at: https://drive.google.com/file/d/0B5z_0Xbe8HPZaVpvODFBTDVYN00/view

*******************************************************************************/

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAnalyzer.FrequencyBeatDetection;


public class BeatEventScript : MonoBehaviour
{
    public FreqBeatDetection m_BeatDetector;
    public float averageSpectrumEnergy;
    public bool isBeat;
    public int counter;

    [Range(0, 20f)]
    public float sensitivity;
    public float playbackDelaytime;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        DetectBeat();
    }

    void DetectBeat()
    {

        DetectAverageSpectrumEnergy();


        if (averageSpectrumEnergy >= sensitivity)
        {


            counter += 1;

            if (counter == 1) // Ensure beat is triggered only once per frame;
            {          // If energy is greater than threshold a beat has been detected.
            isBeat = true;
            }

        }
        else
        {
            isBeat = false;
            counter = 0;
        }

    }

    void DetectAverageSpectrumEnergy()
    {
        float sum = 0;
        for (var i = 0; i < m_BeatDetector.m_Frequency.Length; i++)
        {
            sum += m_BeatDetector.m_Frequency[i];
        }
        averageSpectrumEnergy = sum / m_BeatDetector.m_Frequency.Length * 1000000;
    }

}
