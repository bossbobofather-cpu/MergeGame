using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Common.UI
{    
    public class UIBase : MonoBehaviour
    {
        /// <summary>
        /// OnOpened 메서드입니다.
        /// </summary>
        public virtual void OnOpened()
        {
        }
        /// <summary>
        /// OnClosed 메서드입니다.
        /// </summary>

        public virtual void OnClosed()
        {
        }
    }
}

