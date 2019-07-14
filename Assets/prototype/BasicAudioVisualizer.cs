using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAudioVisualizer : MonoBehaviour
{

    [SerializeField] AudioAnalyzer m_analyzer;

    [SerializeField] TextMesh m_fps;

    [SerializeField] Material m_blitMat;

    [SerializeField] LineRenderer wavLine;
    [SerializeField] LineRenderer fftLine;

    [SerializeField] bool _debug;
    [SerializeField] float m_fftLerpFactor;
    [SerializeField] float m_fftPow = 0.1f;

    [SerializeField] LineRenderer rmsLine;
    [SerializeField] LineRenderer bassRMSLine;
    [SerializeField] LineRenderer trebRMSLine;
    [SerializeField] LineRenderer corrLine;
    [SerializeField] LineRenderer corrLineAvg;
    [SerializeField] float m_corrLerpSpeed;

    [SerializeField] Transform[] m_tempoPeaks;
    [SerializeField] TextMesh m_BPMEstimated;

    [SerializeField] GameObject m_impulse;

    [SerializeField] LineRenderer d1RMS;
    [SerializeField] LineRenderer smoothRMS;
    [SerializeField] LineRenderer impulseLine;
    [SerializeField] LineRenderer impulseBPMLine;

    float estimatedBPM;
    public float smoothEstimatedBPM;

    float[] correlationHistogram;
    float windowFreq = 44100f / AudioAnalyzer.FFT_SIZE;
    float maxFreq = 44100f / 2f;

    private void SetLine(LineRenderer line, float[] data)
    {
        if (line.positionCount != data.Length)
        {
            line.positionCount = data.Length;
        }

        for (int i = 0; i < data.Length; i++)
        {
            line.SetPosition(i,
                new Vector3((float)i / data.Length, data[i], 0f));
        }
    }

    void Update()
    {
        if (m_fps != null)
        {
            m_fps.text = $"{Mathf.Round(1f / Time.smoothDeltaTime)}FPS";
        }

        if (fftLine.positionCount != m_analyzer.RemappedFFT.Length)
        {
            fftLine.positionCount = m_analyzer.RemappedFFT.Length;
        }

        for (int i = 0; i < m_analyzer.RemappedFFT.Length; i++)
        {
            float frequency = ((float)i * windowFreq) / maxFreq;
            float xPos = Mathf.Pow(frequency, m_fftPow);

            Vector3 oldPos = fftLine.GetPosition(i);
            Vector3 newPos = new Vector3(xPos, m_analyzer.RemappedFFT[i], 0f);
            fftLine.SetPosition(i, Vector3.Lerp(oldPos, newPos, m_fftLerpFactor));
        }

        m_blitMat.SetFloat("_HistoryOffset", m_analyzer.HistoryOffset);
        m_blitMat.SetTexture("_MainTex", m_analyzer.FFTTexture);

        SetLine(wavLine, m_analyzer.AudioBuffer);
        SetLine(rmsLine, m_analyzer.RMS);
        SetLine(trebRMSLine, m_analyzer.RMSTreble);
        SetLine(bassRMSLine, m_analyzer.RMSBass);
        SetLine(d1RMS, m_analyzer.d1RMS);
        SetLine(smoothRMS, m_analyzer.SmnoothRMS);
        SetLine(impulseLine, m_analyzer.impulseHistory);
        SetLine(impulseBPMLine, m_analyzer.ImpulseBPMHistogram);


        if (m_analyzer.RMS.Length == m_analyzer.RMSHistoryLength)
        {
            UnityEngine.Profiling.Profiler.BeginSample("AutoCorrelation");
            var corr = Autocorrelation(m_analyzer.RMS);
            UnityEngine.Profiling.Profiler.EndSample();

            int peak = GetMomentaryCorrolationPeak( corr);

            SetLine(corrLine, corr);

            if (correlationHistogram == null)
            {
                correlationHistogram = new float[corr.Length];

            }
            if ( peak > -1)
                correlationHistogram[peak] += 1;

            for(int i = 0; i < correlationHistogram.Length; i ++)
            {
                correlationHistogram[i] *= 0.999f;
            }
            SetLine(corrLineAvg, correlationHistogram);

            DrawDebugBPM(corrLineAvg, correlationHistogram);
        }

        DrawImpulses();
    }

    float d1Peak;

    CircularBuffer<bool> impulseHistory;

    public void DrawImpulses()
    {
        if (impulseHistory == null)
        {
            impulseHistory = new CircularBuffer<bool>(m_analyzer.d1.Capacity);
        }

        if (m_analyzer.impulse.Back() > 0.5)
            Camera.main.backgroundColor = Color.white;
        else
            Camera.main.backgroundColor = Color.black;
    }

    private void DrawDebugBPM(LineRenderer line, float[] spectrum )
    {
        int maxPeakIndex = 1;
        for(int i = 0; i < spectrum.Length; i ++)
        {
            if (spectrum[i] > spectrum[maxPeakIndex])
            {
                maxPeakIndex = i;
            }
        }

        var quadFit = GetLocalMax(
            spectrum[maxPeakIndex - 1], 
            spectrum[maxPeakIndex], 
            spectrum[maxPeakIndex + 1]);

        estimatedBPM = OffsetToBPM(quadFit + maxPeakIndex);

        if (!float.IsNaN(estimatedBPM))
            smoothEstimatedBPM = Mathf.Lerp(smoothEstimatedBPM, estimatedBPM, 0.0001f);

        m_tempoPeaks[0].localPosition = new Vector3(line.GetPosition(maxPeakIndex).x, 0, 0);
        var text = m_tempoPeaks[0].GetComponentInChildren<TextMesh>();

        if (text != null)
        {
            text.text = $"{smoothEstimatedBPM}";
        }

        m_BPMEstimated.text = (Mathf.Round(smoothEstimatedBPM * 100f) / 100f).ToString();
    }

    private float GetLocalMax(float l, float m, float r)
    {
        var b = 2f * m - r / 2f;
        var a2 = r - 2f * (m + l);
        return -b / a2;
    }

    public int GetMomentaryCorrolationPeak(float[] spectrum)
    {
        float maxPeak = 0;
        int maxPeakIndex = -1;

        for (int i = 2; i < spectrum.Length-2; i ++)
        {

            if (spectrum[i] > spectrum[i-2] && spectrum[i] > spectrum[i +2] && spectrum[i] > maxPeak)
            {
                if (isValidBPM(i))
                {
                    maxPeakIndex = i;
                    maxPeak = spectrum[i];
                }
            }
        }

        return maxPeakIndex;
    }

    private float OffsetToBPM(float offset)
    {
        return (1f / Time.smoothDeltaTime) * 30f / (float)offset;
    }

    private bool isValidBPM(int i)
    {
        float bpm = (1f / Time.deltaTime) * 30f / (float)i;
        return (bpm > 70f && bpm < 200f);
    }

    public float Mean(float[] x)
    {
        float sum = 0;
        for (int i = 0; i < x.Length; i++)
            sum += x[i];
        return sum / x.Length;
    }

    public float[] Autocorrelation(float[] x)
    {
        float mean = Mean(x);

        float[] autocorrelation = new float[x.Length / 2];
        for (int t = 0; t < autocorrelation.Length; t++)
        {
            float n = 0;
            float d = 0;

            for (int i = 0; i < x.Length; i++)
            {
                float xim = x[i] - mean;
                n += xim * (x[(i + t) % x.Length] - mean);
                d += xim * xim;
            }

            autocorrelation[t] = n / d;
        }

        return autocorrelation;
    }


    void OnGUI()
    {
        float scale = 3f;
        if (_debug && Event.current.type.Equals(EventType.Repaint))
        {
            if (m_analyzer.FFTTexture && m_blitMat)
            {
                var rect = new Rect(0, 0, 
                     4 * m_analyzer.FFTTexture.width / scale, 
                     m_analyzer.FFTTexture.height/ scale);
                Graphics.DrawTexture(rect, m_analyzer.FFTTexture, m_blitMat);
            }
        }
    }

}
