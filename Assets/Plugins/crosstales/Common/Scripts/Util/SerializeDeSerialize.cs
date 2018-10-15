#if !UNITY_WSA
using UnityEngine;

namespace Crosstales.Common.Util
{
    /// <summary>Serialize and deserialize objects to/from binary files.</summary>
    //public partial class SerializeDeSerialize<T>
    public class SerializeDeSerialize<T>
    {

        private System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter
        {
            get
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

                return bf;
            }
        }


        #region Serialization

        public void ToFile(T o, string path)
        {
            try
            {
                using (System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    binaryFormatter.Serialize(fileStream, o);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not save serialized file: " + ex);
            }
        }

        public System.IO.MemoryStream ToMemory(T o)
        {
            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();

            binaryFormatter.Serialize(memoryStream, o);

            return memoryStream;
        }

        public byte[] ToByteArray(T o)
        {
            byte[] arr;

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                arr = memoryStream.ToArray();
            }

            return arr;
        }

        #endregion


        #region Deserialization

        public T FromFile(string path)
        {
            T o = default(T);

            try
            {
                using (System.IO.FileStream fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open))
                {
                    o = (T)binaryFormatter.Deserialize(fileStream);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not load serialized file: " + ex);
            }

            return o;
        }

        public T FromMemory(byte[] data)
        {
            T o;

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(data))
            {
                o = (T)binaryFormatter.Deserialize(memoryStream);
            }

            return o;
        }

        #endregion
    }
}
#endif
// © 2017-2018 crosstales LLC (https://www.crosstales.com)