using System.Runtime.Serialization;

namespace BwmpsTools.Utils
{
    [DataContract]
    public class Settings
    {
        [DataMember]
        public bool debugMode;
    }
}
