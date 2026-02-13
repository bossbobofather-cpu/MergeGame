using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace MyProject.Common.UI
{    
    public class UIBase : MonoBehaviour
    {
        /// <summary>
        /// OnOpened 함수를 처리합니다.
        /// </summary>
        public virtual void OnOpened()
        {
            // 핵심 로직을 처리합니다.
            
        }
        /// <summary>
        /// OnClosed 함수를 처리합니다.
        /// </summary>

        public virtual void OnClosed()
        {
            // 핵심 로직을 처리합니다.
            
        }
    }
}

