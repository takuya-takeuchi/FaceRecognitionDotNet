using System;
using System.Xml.Serialization;

namespace HelenTraining
{

    [Serializable]
    [XmlRoot(ElementName = "dataset")]
    public class Dataset
    {

        [XmlElement("name")]
        public string Name;

        [XmlElement("comment")]
        public string Comment;

        [XmlArray("images")]
        [XmlArrayItem("image")]
        public Image[] Images;

    }

}