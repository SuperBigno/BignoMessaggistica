using System;
using System.Xml.Serialization;

namespace ConsoleApp1_Server
{
    [Serializable]
    public class Utente
    {
        public string Nickname { get; set; }
        public string Telefono { get; set; }

        public Utente() { }

        public Utente(string nickname, string telefono)
        {
            Nickname = nickname;
            Telefono = telefono;
        }

        public override bool Equals(object obj)
        {
            if (obj is Utente)
            {
                Utente u = obj as Utente;
                return (u.Telefono.Equals(this.Telefono) && u.Nickname.Equals(this.Nickname));
            }
            return false;
        }

        public override string ToString()
        {
            return $"{Nickname} ({Telefono})";
        }
    }
}
