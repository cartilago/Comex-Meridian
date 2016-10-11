/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

#if !UNITY_4_3 && !UNITY_4_5

using UnityEngine;
using UnityEngine.UI;

namespace InfinityCode.OnlineMapsExamples
{
    [AddComponentMenu("")]
    public class uGUICustomMarkerExample:MonoBehaviour
    {
        public double lng;
        public double lat;
        public string text;

        public Text textField;
        public float height;

        public void Dispose()
        {
            textField = null;
        }

        public void Start()
        {
            textField.text = text;
        }
    }
}

#endif