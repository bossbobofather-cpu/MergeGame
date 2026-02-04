    /// <summary>
    /// ?˜ì •??ê°?ê³„ì‚° ëª¨ë“œ?…ë‹ˆ??
    /// </summary>
    public enum AttributeModifierValueMode
    {
        None,
        /// <summary>
        /// ?•ì  ê°?ëª¨ë“œ. AttributeId, Operation, Magnitude ?„ë“œë¥??¬ìš©?©ë‹ˆ??
        /// </summary>
        Static,

        /// <summary>
        /// ê³„ì‚°ê¸?ëª¨ë“œ. CalculatorType, Coefficient ?„ë“œë¥??¬ìš©?©ë‹ˆ??
        /// ê³„ì‚°ê¸°ê? source/target ASC ?•ë³´ë¥?ê¸°ë°˜?¼ë¡œ ?ì„±???˜ì •?©ë‹ˆ??
        /// </summary>
        Calculated
    }
