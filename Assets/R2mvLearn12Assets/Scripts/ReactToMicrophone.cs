using UnityEngine;


// See Valem Tutorials https://www.youtube.com/watch?v=dzD0qP8viLw
// See also https://forum.unity.com/threads/check-current-microphone-input-volume.133501/

public class ReactToMicrophone : MonoBehaviour
{
    [Tooltip("Name if the microphone you would like to use, see your audio settings for full name.")]
    [SerializeField] private string _microphoneName;
    
    [Tooltip("Multiplier to emphasize/exaggerate the loudness.")]
    [SerializeField] private float _sensitivity = 100f;
    
    [Tooltip("Minimal loudness value. If loudness is below, input is ignored.")]
    [SerializeField] private float _threshold = 0.1f;
    [SerializeField] private float _reactionSpeed = 3f;

    [Space(20)]
    [Header("Scale values")]
    [SerializeField] private Vector3 _minScale = Vector3.one;
    [SerializeField] private Vector3 _maxScale = Vector3.one;

    private readonly int _sampleLength = 64;
    private float _loudness;
    private bool _listening;
    private float[] _sampleData;
    private AudioClip _audioClip;
    private Vector3 _targetScale;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _sampleData = new float[_sampleLength];
    }

    // Update is called once per frame
    void Update()
    {
        if (_listening)
        {
            _loudness = getLoudnessFromMicrophone() * _sensitivity;

            if (_loudness < _threshold) _loudness = 0;
            
            _targetScale = Vector3.Lerp(_minScale, _maxScale, _loudness);
        }
        else
        {
            _targetScale = _minScale;
        }
        
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, _reactionSpeed * Time.deltaTime);
        
    }

    public void StartListening()
    {
        if(_listening) return;
        
        _audioClip = Microphone.Start(_microphoneName, true, 24, AudioSettings.outputSampleRate);
        _listening = true;
    }

    public void StopListening()
    {
        if(!_listening) return;
        
        Microphone.End(_microphoneName);
        _listening = false;
    }
    

    private float getLoudnessFromMicrophone()
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
    
}
