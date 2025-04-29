using System;
using System.Xml.Serialization;

namespace ConsoleApplication1
{
    [Serializable]
    public class Message
    {
        [XmlElement("TelefonoMittente")]
        public string TelefonoMittente { get; set; }

        [XmlElement("TelefonoDestinatario")]
        public string TelefonoDestinatario { get; set; }

        [XmlElement("DataOra")]
        public DateTime DataOra { get; set; }

        [XmlElement("CorpoMessaggio")]
        public string CorpoMessaggio { get; set; }

        [XmlElement("FileAllegato")]
        public string FileAllegato { get; set; }

        public Message() { }

        public Message(string mittente, string destinatario, string corpoMessaggio, string fileAllegato = null)
        {
            TelefonoMittente = mittente;
            TelefonoDestinatario = destinatario;
            CorpoMessaggio = corpoMessaggio;
            DataOra = DateTime.Now;
            FileAllegato = fileAllegato;
        }
    }
}
