using UnityEngine;

namespace Crosstales.RTVoice.Model
{
    /// <summary>Model for a voice.</summary>
    [System.Serializable]
    public class Voice
    {
        #region Variables

        /// <summary>Name of the RT-Voice.</summary>
        [Tooltip("Name of the RT-Voice.")]
        public string Name;

        /// <summary>Description of the RT-Voice.</summary>
        [Tooltip("Description of the RT-Voice.")]
        public string Description = string.Empty;

        /// <summary>Gender of the RT-Voice (Windows only).</summary>
        [Tooltip("Gender of the RT-Voice (Windows only).")]
        public string Gender = string.Empty;

        /// <summary>Age of the RT-Voice (Windows only).</summary>
        [Tooltip("Age of the RT-Voice (Windows only).")]
        public string Age = string.Empty;

        /// <summary>Culture of the RT-Voice.</summary>
        [Tooltip("Culture of the RT-Voice.")]
        public string Culture = string.Empty;

        #endregion


        #region Constructors

        /// <summary>Instantiate the class.</summary>
        /// <param name="name">Name of the RT-Voice.</param>
        /// <param name="description">Description of the RT-Voice.</param>
        /// <param name="gender">Gender of the RT-Voice (Windows only).</param>
        /// <param name="age">Age of the RT-Voice (Windows only).</param>
        /// <param name="culture">Culture of the RT-Voice.</param>
        public Voice(string name, string description, string gender, string age, string culture)
        {
            Name = name;
            Description = description;
            Gender = gender;
            Age = age;
            Culture = culture;
        }

        /// <summary>Instantiate the class.</summary>
        /// <param name="name">Name of the RT-Voice.</param>
        /// <param name="description">Description of the RT-Voice.</param>
        /// <param name="culture">Culture of the RT-Voice.</param>
        public Voice(string name, string description, string culture)
        {
            Name = name;
            Description = description;
            Culture = culture;
        }

        #endregion


        #region Overridden methods

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            result.Append(GetType().Name);
            result.Append(Util.Constants.TEXT_TOSTRING_START);

            result.Append("Name='");
            result.Append(Name);
            result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);

            result.Append("Description='");
            result.Append(Description);
            result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);

            result.Append("Gender='");
            result.Append(Gender);
            result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);

            result.Append("Age='");
            result.Append(Age);
            result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);

            result.Append("Culture='");
            result.Append(Culture);
            //result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER);
            result.Append(Util.Constants.TEXT_TOSTRING_DELIMITER_END);

            result.Append(Util.Constants.TEXT_TOSTRING_END);

            return result.ToString();
        }

        #endregion
    }
}
// © 2015-2018 crosstales LLC (https://www.crosstales.com)