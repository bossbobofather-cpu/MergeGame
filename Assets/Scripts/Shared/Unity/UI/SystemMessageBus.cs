using System;
using UnityEngine;

namespace MyProject.Common.UI
{
    /// <summary>
    /// ?쒖뒪??硫붿떆吏 ?꾨떖???꾪븳 ?뺤쟻 ?대깽??踰꾩뒪?낅땲??
    /// </summary>
    public static class SystemMessageBus
    {
        /// <summary>
        /// 硫붿떆吏媛 諛쒗뻾?섏뿀????諛쒖깮?섎뒗 ?대깽?몄엯?덈떎.
        /// </summary>
        public static event Action<SystemMessage> MessagePublished;

        /// <summary>
        /// 硫붿떆吏瑜?諛쒗뻾?⑸땲??(湲곕낯 ?됱긽).
        /// </summary>
        public static void Publish(string message)
        {
            Publish(message, new Color(0f, 0f, 0f, 0.6f));
        }

        /// <summary>
        /// 硫붿떆吏瑜?諛쒗뻾?⑸땲??(?됱긽 吏??.
        /// </summary>
        public static void Publish(string message, Color backgroundColor)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            MessagePublished?.Invoke(new SystemMessage
            {
                Text = message,
                BackgroundColor = backgroundColor
            });
        }
    }

    /// <summary>
    /// ?쒖뒪??硫붿떆吏 ?곗씠?곗엯?덈떎.
    /// </summary>
    public struct SystemMessage
    {
        /// <summary>
        /// 硫붿떆吏 ?띿뒪?몄엯?덈떎.
        /// </summary>
        public string Text;

        /// <summary>
        /// 諛곌꼍 ?됱긽?낅땲??
        /// </summary>
        public Color BackgroundColor;
    }
}


