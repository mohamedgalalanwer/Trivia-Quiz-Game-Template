﻿using UnityEngine;
using System.Collections;

namespace Crosstales.RTVoice.Tool
{
    /// <summary>Simple sequencer for dialogues.</summary>
    //[ExecuteInEditMode]
    [HelpURL("https://www.crosstales.com/media/data/assets/rtvoice/api/class_crosstales_1_1_r_t_voice_1_1_tool_1_1_sequencer.html")]
    public class Sequencer : MonoBehaviour
    {
        #region Variables

        /// <summary>All available sequences.</summary>
        [Tooltip("All available sequences.")]
        public Model.Sequence[] Sequences;

        /// <summary>Fallback culture for all sequences (e.g. 'en', optional).</summary>
        [Tooltip("Fallback culture for all sequences (e.g. 'en', optional).")]
        public string Culture;

        /// <summary>Delay in seconds before the Sequencer starts processing (default: 0).</summary>
        [Tooltip("Delay in seconds before the Sequencer starts processing (default: 0).")]
        public float Delay = 0f;

        /// <summary>Enable the Sequencer on start (default: false).</summary>
        [Tooltip("Enable the Sequencer on start (default: false).")]
        public bool PlayOnStart = false;

        private int currentIndex = 0;

        private string uidCurrentSpeaker;

        private bool playAllSequences = false;

        private bool played = false;

        #endregion


        #region Properties

        /// <summary>Fallback culture for the text (main use is for UI).</summary>
        public string CurrentCulture
        {
            get
            {
                return Culture;
            }

            set
            {
                Culture = value;
            }
        }

        /// <summary>Returns the current Sequence.</summary>
        /// <returns>The current Sequence.</returns>
        public Model.Sequence CurrentSequence
        {
            get { return Sequences[currentIndex]; }
        }

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            // Subscribe event listeners
            Speaker.OnVoicesReady += onVoicesReady;
            Speaker.OnSpeakComplete += speakCompleteMethod;

            play();
        }

        public void OnDestroy()
        {
            // Unsubscribe event listeners
            Speaker.OnSpeakComplete -= speakCompleteMethod;
            Speaker.OnVoicesReady -= onVoicesReady;
        }

        public void OnValidate()
        {
            foreach (Model.Sequence seq in Sequences)
            {
                if (!seq.initalized)
                {
                    seq.Rate = 1f;
                    seq.Pitch = 1f;
                    seq.Volume = 1f;

                    seq.initalized = true;
                }
            }
        }

        #endregion


        #region Public methods
        /// <summary>Plays a Sequence with a given index.</summary>
        /// <param name="index">Index of the Sequence (default: 0, optional).</param>
        public void PlaySequence(int index = 0)
        {
            if (Sequences != null)
            {
                if (index >= 0 && index < Sequences.Length)
                {
                    StartCoroutine(playMe(Sequences[index]));

                    currentIndex = index + 1;
                }
                else
                {
                    Debug.LogWarning("The given index is outside the range of Sequences: " + index);
                }
            }
            else
            {
                Debug.LogWarning("Sequences is null!");
            }
        }

        /// <summary>Plays the next Sequence in the array.</summary>
        public void PlayNextSequence()
        {
            PlaySequence(currentIndex);
        }

        /// <summary>Plays all Sequences.</summary>
        public void PlayAllSequences()
        {
            StopAllSequences();

            playAllSequences = true;

            PlaySequence();
        }

        /// <summary>Stops and silences all active Sequences.</summary>
        public void StopAllSequences()
        {
            StopAllCoroutines();
            Speaker.Silence();
            playAllSequences = false;
        }

        #endregion


        #region Callback methods

        private void speakCompleteMethod(Model.Wrapper wrapper)
        {
            if (playAllSequences)
            {
                if (wrapper.Uid.Equals(uidCurrentSpeaker) && currentIndex < Sequences.Length)
                {
                    PlayNextSequence();
                }
                else
                {
                    StopAllSequences();
                }
            }
        }

        private void onVoicesReady()
        {
            play();
        }

        #endregion


        #region Private methods

        private void play()
        {
            if (PlayOnStart && !played && Speaker.Voices.Count > 0)
            {
                played = true;

                PlayAllSequences();
            }
        }

        private IEnumerator playMe(Model.Sequence seq)
        {
            yield return new WaitForSeconds(Delay);

            Model.Voice voice = Speaker.VoiceForName(seq.RTVoiceName);

            if (voice == null)
            {
                voice = Speaker.VoiceForCulture(Culture);
            }

            if (seq.Mode == Model.Enum.SpeakMode.Speak)
            {
                uidCurrentSpeaker = Speaker.Speak(seq.Text, seq.Source, voice, true, seq.Rate, seq.Pitch, seq.Volume);
            }
            else
            {
                uidCurrentSpeaker = Speaker.SpeakNative(seq.Text, voice, seq.Rate, seq.Pitch, seq.Volume);
            }
        }

        #endregion
    }
}
// © 2016-2018 crosstales LLC (https://www.crosstales.com)