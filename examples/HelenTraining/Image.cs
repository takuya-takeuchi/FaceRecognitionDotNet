using System;
using System.Xml.Serialization;

namespace HelenTraining
{

    [Serializable]
    public class Image
    {

        [XmlAttribute("file")]
        public string File;

        [XmlElement("box")]
        public Box Box;

    }

}