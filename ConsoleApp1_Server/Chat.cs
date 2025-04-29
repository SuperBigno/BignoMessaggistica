using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ConsoleApplication1
{
    [Serializable]
    public class Chat
    {
        [XmlElement("TelefonoUtente1")]
        public string TelefonoUtente1 { get; set; }

        [XmlElement("TelefonoUtente2")]
        public string TelefonoUtente2 { get; set; }

        [XmlArray("Messaggi")]
        [XmlArrayItem("Messaggio")]
        public List<Message> Messaggi { get; set; }

        public Chat()
        {
            Messaggi = new List<Message>();
        }

        public Chat(string tel1, string tel2)
        {
            TelefonoUtente1 = tel1;
            TelefonoUtente2 = tel2;
            Messaggi = new List<Message>();
        }

        public void AddMessage(Message message)
        {
            Messaggi.Add(message);
        }

        public void Save(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Chat));
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fs, this);
            }
        }

        public static Chat Load(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Chat));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return (Chat)serializer.Deserialize(fs);
            }
        }
    }
}
