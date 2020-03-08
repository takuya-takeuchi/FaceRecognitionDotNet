using System;
using System.Xml.Serialization;

namespace HelenTraining
{

    [Serializable]
    public sealed class Box
    {

        [XmlAttribute("top")]
        public int Top;

        [XmlAttribute("left")]
        public int Left;

        [XmlAttribute("width")]
        public int Width;

        [XmlAttribute("height")]
        public int Height;

        [XmlElement("part")]
        public Part[] Part;

    }

}