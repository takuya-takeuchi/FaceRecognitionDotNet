using System;
using System.Xml.Serialization;

namespace HelenTraining
{

    [Serializable]
    public sealed class Part
    {

        [XmlAttribute("x")]
        public float X;

        [XmlAttribute("y")]
        public float Y;

        [XmlAttribute("name")]
        public string Name;

    }

}