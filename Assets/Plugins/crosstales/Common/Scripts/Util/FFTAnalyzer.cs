using UnityEngine;

namespace Crosstales.DJ.Demo.Util
{
    /// <summary>FFT analyzer for an audio channel.</summary>
    //[HelpURL("https://www.crosstales.com/media/data/assets/radio/api/class_crosstales_1_1_radio_1_1_demo_1_1_util_1_1_f_f_t_analyzer.html")]
    public class FFTAnalyzer : MonoBehaviour
    {
        #region Variables

        public float[] Samples = new float[256];
        public int Channel = 0;
        public FFTWindow FFTMode = FFTWindow.BlackmanHarris;

        #endregion


        #region MonoBehaviour methods

        public void Update()
        {
            AudioListener.GetSpectrumData(Samples, Channel, FFTMode);
        }

        #endregion
    }
}
// © 2015-2018 crosstales LLC (https://www.crosstales.com)