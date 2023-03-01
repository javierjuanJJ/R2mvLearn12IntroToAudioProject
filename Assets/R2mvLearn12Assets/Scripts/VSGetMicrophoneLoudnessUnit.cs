using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("Get Microphone Loudness")]
[UnitCategory("Road to the Metaverse/Microphone")]

public class VSGetMicrophoneLoudnessUnit : Unit
{
    
    [DoNotSerialize] // No need to serialize ports
    [PortLabelHidden] // Hiding Label
    public ControlInput inputTrigger; // Adding an input port

    [DoNotSerialize] // No need to serialize ports.
    [PortLabelHidden] // Hiding Label
    public ControlOutput outputTrigger; // Adding an output port
    
    [DoNotSerialize] // No need to serialize ports
    public ValueInput microphoneName;
    
    [DoNotSerialize] // No need to serialize ports
    public ValueInput theshhold;
    
    [DoNotSerialize] // No need to serialize ports
    public ValueOutput loudness; // Amount of electrical power generated

    
    
    private static readonly int _sampleLength = 64;
    private static float _loudness;
    private static bool _listening;
    private static float[] _sampleData;
    private static AudioClip _audioClip;
    private static Vector3 _targetScale;
    private static string _microphoneName;
    
    protected override void Definition()
    {

        _sampleData = new float[_sampleLength];
        
        inputTrigger = ControlInput("inputTrigger", (flow) =>
        {

            if (!_listening)
            {
                _microphoneName = flow.GetValue<string>(microphoneName);
                StartListening();
            }
            
            _loudness = GetLoudnessFromMicrophone();
            
            if (_loudness > flow.GetValue<float>(theshhold))
                return outputTrigger;
            else
            {
                _loudness = 0;
                return null;
            }

        });

        microphoneName = ValueInput<string>(nameof(microphoneName), "Name");
        theshhold = ValueInput<float>(nameof(theshhold), 0.1f);

        outputTrigger = ControlOutput("outputTrigger");
        loudness = ValueOutput<float>(nameof(loudness), (flow) => _loudness);
        
        Succession(inputTrigger, outputTrigger);
    }
    
    private float GetLoudnessFromMicrophone()
    {
        float levelMax = 0;
        
        int micPosition = Microphone.GetPosition(_microphoneName) - (_sampleLength + 1); // null means the first microphone
        
        if (micPosition < 0) return 0;
        _audioClip.GetData(_sampleData, micPosition);
        
        // Getting a peak on the last 128 samples
        for (int i = 0; i < _sampleLength; i++)
        {
            float wavePeak = _sampleData[i] * _sampleData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        
        return levelMax;
        
    }
    
    private void StartListening()
    {
        if(_listening) return;
        
        _audioClip = Microphone.Start(_microphoneName, true, 24, AudioSettings.outputSampleRate);
        _listening = true;
    }

    private void StopListening()
    {
        if(!_listening) return;
        
        Microphone.End(_microphoneName);
        _listening = false;
    }
}
