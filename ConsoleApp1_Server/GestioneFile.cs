using ConsoleApplication1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ConsoleApp1_Server
{
    static class GestioneFile
    {
        public static string PATH = @"Chats\";
        public static string USERS = @"Users\users.xml";

        public static List<Utente> ReadUsers(string path)
        {
            if (!File.Exists(path))
            {
                return new List<Utente>();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<Utente>));
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                return (List<Utente>)serializer.Deserialize(fs);
            }
        }

        public static string GetUser(string tel)
        {
            return ReadUsers(USERS).Find(x=>x.Telefono.Equals(tel)).Nickname.ToString();
        }

        public static string GetUserByNick(string nickname)
        {
            return ReadUsers(USERS).Find(x => x.Nickname.Equals(nickname)).Telefono.ToString();
        }

        public static void WriteUsers(string path, List<Utente> utenti)
        {
            string directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<Utente>));
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(fs, utenti);
            }
        }


        public static void AddUser(string path, Utente newUser)
        {
            List<Utente> utenti = ReadUsers(path);

            if (!utenti.Contains(newUser))
            {
                utenti.Add(newUser);
                WriteUsers(path, utenti);
            }
        }

        public static bool CreateChat(string tel1, string tel2)
        {
            string dirName = GetChatDirectoryName(tel1, tel2);

            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
                return true;
            }

            return false;
        }

        public static string GetChatDirectoryName(string tel1, string tel2)
        {
            var phones = new List<string> { tel1, tel2 };
            phones.Sort();
            return Path.Combine(PATH, "-" + string.Join("_", phones) + "-");
        }

        public static string GetAllChats(string tel)
        {
            List<string> allchats = Directory.GetDirectories(PATH).ToList().FindAll(x => x.Contains("-" + tel + "_") || x.Contains("_" + tel + "-"));
            List<string> allusers = new List<string>();

            for (int i=0; i<allchats.Count; i++)
            {
                allchats[i] = allchats[i].Substring(6);
            }

            foreach (var chat in allchats)
            {
                string[] tels = chat.Split('_');
                for (int j = 0; j < tels.Length; j++)
                {
                    tels[j] = tels[j].Replace("-", "");
                }

                if (tels[0]==tel)
                    allusers.Add(GetUser(tels[1]));
                else
                    allusers.Add(GetUser(tels[0]));
            }
            
            return string.Join("_", allusers);
        }

        public static List<Message> ReadMessages(string tel1, string tel2)
        {
            string dirName = GetChatDirectoryName(tel1, tel2);
            string messagesFile = Path.Combine(dirName, "messages.xml");

            if (File.Exists(messagesFile))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Message>));
                using (FileStream fs = new FileStream(messagesFile, FileMode.Open))
                {
                    return (List<Message>)serializer.Deserialize(fs);
                }
            }

            return new List<Message>();
        }

        public static void WriteMessages(string tel1, string tel2, List<Message> messaggi)
        {
            string dirName = GetChatDirectoryName(tel1, tel2);
            string messagesFile = Path.Combine(dirName, "messages.xml");

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            XmlSerializer serializer = new XmlSerializer(typeof(List<Message>));
            using (FileStream fs = new FileStream(messagesFile, FileMode.Create))
            {
                serializer.Serialize(fs, messaggi);
            }
        }

        public static void AddMessage(string tel1, string tel2, Message messaggio)
        {
            List<Message> messaggi = ReadMessages(tel1, tel2);
            messaggi.Add(messaggio);
            WriteMessages(tel1, tel2, messaggi);
        }

        public static bool DeleteChat(string tel1, string tel2)
        {
            string dirName = GetChatDirectoryName(tel1, tel2);

            if (Directory.Exists(dirName))
            {
                Directory.Delete(dirName, true);
                return true;
            }

            return false;
        }

        public static bool UploadFile(string tel1, string tel2, string nomefile, byte[] data)
        {
            string dirName = GetChatDirectoryName(tel1, tel2);

            if (!Directory.Exists(dirName))
            {
                return false;
            }

            string filePath = Path.Combine(dirName, nomefile);
            File.WriteAllBytes(filePath, data);
            return true;
        }

        public static string GetFilePath(string tel1, string tel2, string nomefile)
        {
            string dirName = GetChatDirectoryName(tel1, tel2);
            string filePath = Path.Combine(dirName, nomefile);

            if (File.Exists(filePath))
                return filePath;

            return null;
        }

        public static byte[] GetFileContent(string tel1, string tel2, string nomeFile)
        {
            string directoryChat = GetChatDirectoryName(tel1, tel2);
            string filePath = GetFilePath(tel1, tel2, nomeFile);

            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }

            return null;
        }

        public static string GetNomeFileCompleto(string tel1, string tel2, string nomeFile)
        {
            string dirName = GetChatDirectoryName(tel1, tel2);
            string filePath = Path.Combine(dirName, nomeFile);

            if (File.Exists(filePath))
            {
                return Path.GetFileName(filePath);
            }

            return null;
        }
    }
}
