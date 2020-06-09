using System;

namespace SE.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HeadlessModeAttribute : Attribute
    {
        internal HeadlessSupportMode SupportMode;

        public HeadlessModeAttribute(HeadlessSupportMode supportMode = HeadlessSupportMode.Default)
        {
            SupportMode = supportMode;
        }
    }

    public enum HeadlessSupportMode
    {
        Default,
        NoHeadless,
        OnlyHeadless
    }
}
