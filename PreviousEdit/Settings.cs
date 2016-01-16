using System;
using System.ComponentModel;

namespace PreviousEdit
{
    [Serializable]
    public class Settings
    {
        public const int MINIMUM_BACKWARD = 10;
        int maxBackward = MINIMUM_BACKWARD;

        [Category("General")]
        [DisplayName("Maximum Navigate Backward")]
        [DefaultValue(MINIMUM_BACKWARD)]
        public int MaxBackward
        {
            get { return maxBackward; }
            set { maxBackward = Math.Max(MINIMUM_BACKWARD, value); }
        }
    }
}