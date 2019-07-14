using UnityEngine;
using NAudio.Dsp;

public class AudioAnalyzer : MonoBehaviour
{
    [SerializeField] BasicAudioVisualizer m_visualizer;

    public const int FFT_SIZE = 512;
    private const int CONST_M = 9 ;

    public Texture2D FFTTexture
    {
        get { return m_fftTexture; }
    }

    public Texture2D WaveTex
    {
        get { return m_waveTexture; }
    }

    public float[] AudioBuffer
    {
        get { return m_rawAudioBuffer; }
    }

    public float[] RemappedFFT
    {
        get { return m_remappedFFT; }
    }

    public float HistoryOffset
    {
        get { return (float)m_historyOffset / (float)m_historyLength; }
    }

    public int HistoryLength
    {
        get { return m_historyLength; }
    }
    public int RMSHistoryLength
    {
        get { return m_rmsHistoryLength; }
    }

    public float[] RMS
    {
        get { return m_RMSHistory.ToArray(); }
    }

    public float[] SmnoothRMS
    {
        get { return m_smoothRMSHistory.ToArray(); }
    }

    public float[] RMSTreble
    {
        get { return m_trebleRMSHistory.ToArray(); }
    }

    public float[] RMSBass
    {
        get { return m_bassRMSHistory.ToArray(); }
    }

    public float[] d1RMS
    {
        get { return m_d1RMSHistory.ToArray(); }
    }

    public float[] impulseHistory
    {
        get { return m_impulseHistory.ToArray(); }
    }

    public float[] ImpulseBPMHistogram
    {
        get { return m_impulseDeltaHistogram; }
    }

    public CircularBuffer<float> impulse
    {
        get { return m_impulseHistory; }
    }

    public CircularBuffer<float> d1
    {
        get { return m_d1RMSHistory; }
    }

    private float[] m_remappedFFT;
    private float[] m_rawBuffer;
    private float[] m_rawAudioBuffer;
    private float[] m_rawBufferComplex;

    [SerializeField] int m_historyLength;
    [SerializeField] int m_rmsHistoryLength;
    [SerializeField] int m_impulseHistoryLength;


    [SerializeField] float m_fftAmpPow = 0.1f;
    [SerializeField] float m_RMSSmoothing = 0.1f;

    [SerializeField] float m_impulsedecay = 0.5f;
    [SerializeField] float m_impulseThreshold = 0.8f;
    [SerializeField] TextMesh m_estimatedBPM;

    private Texture2D m_fftTexture;
    private Texture2D m_waveTexture;
    private int m_historyOffset = 0;

    private CircularBuffer<float> m_RMSHistory;
    private CircularBuffer<float> m_bassRMSHistory;
    private CircularBuffer<float> m_trebleRMSHistory;

    private CircularBuffer<float> m_d1RMSHistory;
    private CircularBuffer<float> m_smoothRMSHistory;
    private CircularBuffer<float> m_impulseHistory;

    private CircularBuffer<float> m_impulseTimestamps;
    private float[] m_impulseDeltaHistogram;
    private float[] m_impulseDeltaHistogramSmooth;

    AutoGainControl m_d1GainControl;

    int ImpulseCooldownFrames = 10;
    float impulseCooldown = 0;

    void Start()
    {
        m_RMSHistory = new CircularBuffer<float>(m_rmsHistoryLength);
        m_d1RMSHistory = new CircularBuffer<float>(m_rmsHistoryLength);
        m_smoothRMSHistory = new CircularBuffer<float>(m_rmsHistoryLength);

        m_bassRMSHistory = new CircularBuffer<float>(m_rmsHistoryLength);
        m_trebleRMSHistory = new CircularBuffer<float>(m_rmsHistoryLength);
        m_impulseHistory = new CircularBuffer<float>(m_rmsHistoryLength);

        m_impulseTimestamps = new CircularBuffer<float>(m_impulseHistoryLength);
        m_impulseDeltaHistogram = new float[200];
        m_impulseDeltaHistogramSmooth = new float[200];
        m_impulseBPM = new float[m_rmsHistoryLength];

        m_rawBuffer = new float[FFT_SIZE];
        m_rawAudioBuffer = new float[FFT_SIZE];
        m_rawBufferComplex = new float[FFT_SIZE];
        m_remappedFFT = new float[FFT_SIZE/2];

        m_fftTexture = new Texture2D(m_historyLength, FFT_SIZE / 2, TextureFormat.RGBAFloat, true);
        m_fftTexture.wrapMode = TextureWrapMode.Clamp;
        m_waveTexture = new Texture2D(m_historyLength, FFT_SIZE / 2, TextureFormat.RGBAFloat, true);
        m_waveTexture.wrapMode = TextureWrapMode.Clamp;

        m_d1GainControl = new AutoGainControl(m_impulsedecay);
    }

    int frameNum = 0;

    float windowFreq = 44100f / FFT_SIZE;
    float maxFreq = 44100f / 2f;

        
    void Update()
    {

        m_d1GainControl.Decay = m_impulsedecay;

        Lasp.AudioInput.RetrieveWaveform(Lasp.FilterType.LowPass, m_rawBuffer);

        float val = CalcRMS(m_rawBuffer);
        m_bassRMSHistory.PushBack(val);

        Lasp.AudioInput.RetrieveWaveform(Lasp.FilterType.HighPass, m_rawBuffer);

        val = CalcRMS(m_rawBuffer);
        m_trebleRMSHistory.PushBack(val);

        Lasp.AudioInput.RetrieveWaveform(Lasp.FilterType.Bypass, m_rawBuffer);

        val = CalcRMS(m_rawBuffer);

        var smoothRMS = Mathf.Lerp(val, (m_smoothRMSHistory.IsEmpty ? 0 : m_smoothRMSHistory.Back()), m_RMSSmoothing);

        var newD1val = smoothRMS - (m_smoothRMSHistory.IsEmpty? 0 : m_smoothRMSHistory.Back());
        m_smoothRMSHistory.PushBack(smoothRMS);
        
        m_d1RMSHistory.PushBack(m_d1GainControl.NormalizeVal(newD1val));
        m_RMSHistory.PushBack(val);

        bool isImpulse = impulseCooldown++ > ImpulseCooldownFrames && m_d1RMSHistory.Back() > m_impulseThreshold;
        if (isImpulse)
            impulseCooldown = 0;

        m_impulseHistory.PushBack(isImpulse ? 1f : 0f);

        DrawBPMImpulses();

        for (int i = 0; i < FFT_SIZE; i++)
        {
            m_waveTexture.SetPixel(m_historyOffset, i, Color.white * (m_rawBuffer[i] * 0.5f + 0.5f));
        }

        m_rawBuffer.CopyTo(m_rawAudioBuffer,0);

        FastFourierTransform.FFT(true, CONST_M, m_rawBuffer, m_rawBufferComplex);

        m_historyOffset = frameNum++ % m_historyLength;

        for (int i = 1; i < FFT_SIZE / 2; i++)
        {
            float frequency = ((float)i * windowFreq) / maxFreq;

            float y = Mathf.Sqrt((m_rawBuffer[i] * m_rawBuffer[i] + m_rawBufferComplex[i] * m_rawBufferComplex[i]));
            float vv = Mathf.Pow(frequency * y, m_fftAmpPow);

            m_fftTexture.SetPixel(m_historyOffset, i, Color.white * vv);
            m_remappedFFT[i] = vv;
        }

        m_fftTexture.SetPixel(m_historyOffset, 0, Color.white * m_fftTexture.GetPixel(m_historyOffset, 1));

        UnityEngine.Profiling.Profiler.BeginSample("texApply");
        m_fftTexture.Apply();
        m_waveTexture.Apply();
        UnityEngine.Profiling.Profiler.EndSample();
    }

    private float[] m_impulseBPM;

    private void DrawBPMImpulses()
    {
        if (!m_impulseHistory.IsEmpty && m_impulseHistory.Back() > 0.5)
        {
            if (!m_impulseTimestamps.IsEmpty)
            {
                var thisTime = Time.time;

                for ( int ii = m_impulseTimestamps.Capacity; ii > m_impulseTimestamps.Capacity -5; ii --)
                {
                    try // will throw exception if we don't have enough impulses in buffer
                    {
                        var lastTime = m_impulseTimestamps[ii];
                        var bpm = 60f / (thisTime - lastTime);

                        int bin = GetBinFromBPM(bpm);
                        for (int i = bin; i < m_impulseDeltaHistogram.Length; i *= 2)
                        {
                            m_impulseDeltaHistogram[i] += 0.1f;
                            m_impulseDeltaHistogram[i] *= 1.1f;
                        }
                    }
                    catch { }
                }
            }

            m_impulseTimestamps.PushBack(Time.time);
        }

        m_impulseDeltaHistogram[0] = 0;
        m_impulseDeltaHistogram[m_impulseDeltaHistogram.Length - 1] = 0;

        // decay and smooth
        for (int i = 1; i < m_impulseDeltaHistogram.Length-1; i++)
        {
            m_impulseDeltaHistogram[i] = 
                (0.98f * m_impulseDeltaHistogram[i] +
                0.01f * m_impulseDeltaHistogram[i - 1] +
                0.01f * m_impulseDeltaHistogram[i + 1]) * 0.999f;
        }

        int maxBin = -1;
        float maxVal = 0;
        // find max of spectrum
        for (int i = 1; i < m_impulseDeltaHistogram.Length - 1; i++)
        {
            float smoothVal =
                0.5f * m_impulseDeltaHistogram[i] +
                0.25f * m_impulseDeltaHistogram[i - 1] +
                0.25f * m_impulseDeltaHistogram[i + 1];

            if(smoothVal > maxVal && i > 70 && i < 200 )
            {
                maxVal = smoothVal;
                maxBin = i;
            }
        }
        m_estimatedBPM.text = $"{maxBin}: {maxVal}";

    }

    private int GetBinFromBPM(float bpm)
    {
        return (int)Mathf.Round(bpm);
    }

    private float CalcRMS(float[] buffer)
    {
        float rms = 0;

        for (int i =0; i < buffer.Length; i++)
        {
            rms += buffer[i] * buffer[i];
        }
        rms /= buffer.Length;
        return Mathf.Sqrt(rms);
    }

    public struct AutoGainControl
    {
        public float Gain;
        public float Peak;
        public float Decay;

        public AutoGainControl(float decay)
        {
            this.Peak = 1.0f;
            this.Decay = decay;
            this.Gain = 1.0f;
        }

        public float NormalizeVal(float rawVal)
        {
            // NGS: prevent numerical instability
            if (rawVal < 0.000001f)
            {
                return 0;
            }

            float decayingPeak = this.Peak * Mathf.Exp(-this.Decay * Time.deltaTime);
            this.Peak = Mathf.Max(decayingPeak, rawVal);

            if (this.Peak > 0.001f)
            {
                this.Gain = 1.0f / this.Peak;
            }

            return this.Gain * rawVal;

        }
    }
}