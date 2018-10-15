using UnityEngine;
using System.Collections;

namespace Crosstales.RTVoice.Tool
{
    /// <summary>Process files with configured speeches.</summary>
    [ExecuteInEditMode]
    [HelpURL("https://crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_audio_file_generator.html")]
    public class AudioFileGenerator : MonoBehaviour
    {
        #region Variables

        /// <summary>Text files to generate.</summary>
        [Tooltip("Text files to generate.")]
        public TextAsset[] TextFiles;

        /// <summary>Are the specified file paths inside the Assets-folder (current project)? If this option is enabled, it prefixes the path with 'Application.dataPath' (default: true).</summary>
        [Tooltip("Are the specified file paths inside the Assets-folder (current project)? If this option is enabled, it prefixes the path with 'Application.dataPath' (default: true).")]
        public bool FileInsideAssets = true;

#if UNITY_STANDALONE_WIN
        /// <summary>Set the sample rate of the WAV files (default: 22050). Note: this works only under Windows standalone.</summary>
        [Tooltip("Set the sample rate of the WAV files (default: 22050). Note: this works only under Windows standalone.")]
        public int SampleRate = 22050;

        /// <summary>Set the bits per sample of the WAV files (default: 16). Note: this works only under Windows standalone.</summary>
        [Tooltip("Set the bits per sample of the WAV files (default: 16). Note: this works only under Windows standalone.")]
        public int BitsPerSample = 16;

        /// <summary>Set the channels of the WAV files (default: 1). Note: this works only under Windows standalone.</summary>
        [Tooltip("Set the channels of the WAV files (default: 1). Note: this works only under Windows standalone.")]
        [Range(1, 2)]
        public int Channels = 1;

        /// <summary>Creates a copy of the downsampled WAV file and leaves the original intact (default: false). Note: this works only under Windows standalone..</summary>
        [Tooltip("Creates a copy of the downsampled WAV file and leaves the original intact (default: false). Note: this works only under Windows standalone.")]
        public bool CreateCopy = false;

        ///// <summary>Normalize the volume of the WAV files (default: true). Note: this works only under Windows standalone.</summary>
        //[Tooltip("Normalize the volume of the WAV files (default: true). Note: this works only under Windows standalone.")]
        //public bool Normalize = true;
#endif

        private static char[] splitChar = new char[] { ';' };

        private string lastUid = string.Empty;

        private bool isGenerate = false;

        #endregion


        #region MonoBehaviour methods

        public void OnEnable()
        {
            Speaker.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete;
            Speaker.OnVoicesReady += onVoicesReady;
        }

        public void OnDisable()
        {
            Speaker.OnSpeakAudioGenerationComplete -= onSpeakAudioGenerationComplete;
            Speaker.OnVoicesReady -= onVoicesReady;
        }

        public void OnValidate()
        {
#if UNITY_STANDALONE_WIN
            if (SampleRate < 1000)
                SampleRate = 1000;

            if (SampleRate > 192000)
                SampleRate = 192000;

            if (BitsPerSample < 15)
            {
                BitsPerSample = 8;
            }
            else if (BitsPerSample < 31)
            {
                BitsPerSample = 16;
            }
            else
            {
                BitsPerSample = 32;
            }

            if (Channels <= 1)
            {
                Channels = 1;
            }
            else 
            {
                Channels = 2;
            }
#endif
        }

        #endregion


        #region Public methods

        /// <summary>Generate the audio files from the text files.</summary>
        public void Generate()
        {
            if (!isGenerate)
            {
                isGenerate = true;

                if (Util.Helper.isEditorMode)
                {
#if UNITY_EDITOR
                    generateInEditor();
#endif
                }
                else
                {
                    StartCoroutine(generate());
                }
            }
        }

        #endregion


        #region Private methods

        public IEnumerator generate()
        {
            foreach (TextAsset textFile in TextFiles)
            {
                if (textFile != null)
                {
                    System.Collections.Generic.List<string> speeches = Util.Helper.SplitStringToLines(textFile.text);

                    foreach (string speech in speeches)
                    {

                        if (!speech.StartsWith("#"))
                        {
                            string[] args = speech.Split(splitChar, System.StringSplitOptions.RemoveEmptyEntries);

                            if (args.Length >= 2)
                            {
                                Model.Wrapper wrapper = prepare(args, speech);
                                string uid = Speaker.Generate(wrapper);

                                if (Util.Helper.isWindowsPlatform)
                                {
                                    do
                                    {
                                        yield return null;
                                    } while (!uid.Equals(lastUid));

                                    convert(wrapper.OutputFile);

                                    //normalize(wrapper.OutputFile);
                                } else
                                {
                                    yield return null;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Invalid speech: " + speech);
                            }
                        }
                    }
                }
            }

            if (Util.Config.DEBUG)
                Debug.Log("Generate finished!");

            isGenerate = false;
        }

        private void convert(string outputFile)
        {
#if UNITY_STANDALONE_WIN
            string tmpFile = outputFile.Substring(0, outputFile.Length - 4) + "_" + SampleRate + Speaker.AudioFileExtension;
            bool converted = false;

            try
            {
                using (var reader = new NAudio.Wave.WaveFileReader(outputFile))
                {
                    if (reader.WaveFormat.SampleRate != SampleRate)
                    {
                        var newFormat = new NAudio.Wave.WaveFormat(SampleRate, BitsPerSample, Channels);
                        using (var conversionStream = new NAudio.Wave.WaveFormatConversionStream(newFormat, reader))
                        {
                            NAudio.Wave.WaveFileWriter.CreateWaveFile(tmpFile, conversionStream);
                        }

                        converted = true;
                    }
                    //else
                    //{
                    //    Debug.Log("File ignored: " + outputFile);
                    //}
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not convert audio file: " + ex);
            }

            if (converted)
            {
                try
                {
                    if (!CreateCopy)
                    {
                        System.IO.File.Delete(outputFile);

                        System.IO.File.Move(tmpFile, outputFile);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Could not delete and move audio files: " + ex);
                }
            }
#endif
        }

        /*
        private void normalize(string inputFile)
        {
            string tmpFile = inputFile.Substring(0, inputFile.Length - 4) + "_normalized" + Speaker.AudioFileExtension;

            try
            {
                float max = 0;

                using (var reader = new NAudio.Wave.AudioFileReader(inputFile))
                {
                    // find the max peak
                    float[] buffer = new float[reader.WaveFormat.SampleRate];
                    int read;

                    do
                    {
                        read = reader.Read(buffer, 0, buffer.Length);
                        for (int n = 0; n < read; n++)
                        {
                            var abs = Mathf.Abs(buffer[n]);
                            if (abs > max) max = abs;
                        }
                    } while (read > 0);

                    if (max == 0 || max > 1.0f)
                    {
                        Debug.LogWarning("File cannot be normalized!");
                    }
                    else
                    {
                        // rewind and amplify
                        reader.Position = 0;
                        reader.Volume = 1.0f / max;

                        // write out to a new WAV file
                        NAudio.Wave.WaveFileWriter.CreateWaveFile16(inputFile, reader);
                    }
                }

                //System.IO.File.Delete(tmpFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not normalize audio file: " + ex);
            }
        }
        */

        private Model.Wrapper prepare(string[] args, string speech)
        {
            Model.Wrapper wrapper = new Model.Wrapper();

            wrapper.Text = args[0];

            if (FileInsideAssets)
            {
                wrapper.OutputFile = Application.dataPath + @"/" + args[1];
            }
            else
            {
                wrapper.OutputFile = args[1];
            }

            if (args.Length >= 3)
            {
                wrapper.Voice = Speaker.VoiceForName(args[2]);
            }

            float rate = 1f;
            if (args.Length >= 4)
            {
                if (!float.TryParse(args[3], out rate))
                {
                    Debug.LogWarning("Rate was invalid: " + speech);
                }
                else
                {
                    wrapper.Rate = rate;
                }
            }

            float pitch = 1f;
            if (args.Length >= 5)
            {
                if (!float.TryParse(args[4], out pitch))
                {
                    Debug.LogWarning("Pitch was invalid: " + speech);
                }
                else
                {
                    wrapper.Pitch = pitch;
                }
            }

            float volume = 1f;
            if (args.Length >= 6)
            {
                if (!float.TryParse(args[5], out volume))
                {
                    Debug.LogWarning("Volume was invalid: " + speech);
                }
                else
                {
                    wrapper.Volume = volume;
                }
            }

            return wrapper;
        }

        #endregion


        #region Callbacks


        private void onVoicesReady()
        {
            Generate();
        }

        private void onSpeakAudioGenerationComplete(Model.Wrapper wrapper)
        {
            lastUid = wrapper.Uid;

            if (Util.Config.DEBUG)
                Debug.Log("Speech generated: " + wrapper);
        }

        #endregion


        #region Editor-only methods

#if UNITY_EDITOR
        public void generateInEditor()
        {
            foreach (TextAsset textFile in TextFiles)
            {
                if (textFile != null)
                {
                    System.Collections.Generic.List<string> speeches = Util.Helper.SplitStringToLines(textFile.text);

                    foreach (string speech in speeches)
                    {

                        if (!speech.StartsWith("#"))
                        {
                            string[] args = speech.Split(splitChar, System.StringSplitOptions.RemoveEmptyEntries);

                            if (args.Length >= 2)
                            {
                                Model.Wrapper wrapper = prepare(args, speech);
                                string uid = Speaker.GenerateInEditor(wrapper);

                                if (Util.Helper.isWindowsPlatform)
                                {
                                    do
                                    {
                                        System.Threading.Thread.Sleep(50);
                                    } while (!uid.Equals(lastUid));

                                    convert(wrapper.OutputFile);
                                    //normalize(wrapper.OutputFile);
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Invalid speech: " + speech);
                            }
                        }
                    }
                }
            }

            //if (Util.Config.DEBUG)
            Debug.Log("Generate finished!");

#if UNITY_EDITOR
            if (FileInsideAssets)
                UnityEditor.AssetDatabase.Refresh();
#endif

            isGenerate = false;
        }

#endif
        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)