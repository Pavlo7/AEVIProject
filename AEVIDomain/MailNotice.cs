using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.IO;

namespace AEVIDomain
{
    public class SMTPNotice
    {
        public string ConnectionString;
        public string LogPath;
        public string TemplatePath;
        public string ImagesPath;

        public string Host;
        public int Port;
        public bool UseSsl;
        public string UserName;
        public string Password;
        public string From;

        public SMTPNotice(string host, int port, bool useSsl, string userName, string password, string from,
            string connectionstring, string logpath, string templatepath, string imagespath)
        {
            Host = host;
            Port = port;
            UseSsl = useSsl;
            UserName = userName;
            Password = password;
            From = from;
            ConnectionString = connectionstring;
            LogPath = logpath;
            TemplatePath = templatepath;
            ImagesPath = imagespath;
        }

        private bool _SendNotice(MailMessage message, out string msg)
        {
            
            msg = null;
            try
            {
                SmtpClient client = new SmtpClient(Host, Port);
                client.EnableSsl = UseSsl;
                NetworkCredential credent = new NetworkCredential(UserName, Password);
                client.Credentials = credent;
                client.Send(message);
                client.Dispose();
            }
            catch (Exception ex) { msg = ex.Message;   return false; }
            return true;
        }

        public bool SendNotice(out string msg)
        {
            bool ret = true;
            Log log = new Log(LogPath);
            msg = null;
            MailMessage message;
         
            try
            {
                CMail clMail = new CMail(null, ConnectionString, LogPath);
                List<STMail> list = new List<STMail>();
                clMail.GetData(out list, out msg);
                if (list.Count > 0)
                {
                    foreach (STMail mail in list)
                    {
                        if (CreateMailMessage(mail, out message, out msg))
                        {
                            if (_SendNotice(message, out msg))
                            {
                                string info = string.Format("The message to {0} was send at {1}. Subject: {2}", message.To,
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message.Subject);
                                log.Write(LogType.Info, info);
                                clMail.Delete(mail.recid, out msg);
                            }
                            else
                            {
                                log.Write(LogType.Error, msg);

                                if (clMail.UpdateMissSentDate(mail.recid, mail.recvalue, out msg) != 0)
                                    log.Write(LogType.Error, msg);

                                ret = false;
                            }
                        }
                        else
                        {
                            log.Write(LogType.Error, msg);
                            if(clMail.UpdateMissSentDate(mail.recid, mail.recvalue, out msg) != 0)
                                log.Write(LogType.Error, msg);
                            
                            ret =  false;
                        }
                    }
                }
            }
            catch (Exception ex) { msg = ex.Message; log.Write(LogType.Error, ex.Message); return false; }
            return ret;
        }


        private bool CreateMailMessage(STMail mail, out MailMessage message, out string msg)
        {
            message = new MailMessage();
            msg = null;

            string to;

            try
            {
                // Читаем шаблон
                string body = null;
                string path = Path.Combine(TemplatePath, mail.tamplate);
                body = File.ReadAllText(path);

                if (!File.Exists(path))
                {
                    msg = string.Format("Hasnt't found template {0}", path);
                    return false;
                }

                // От кого и кому
                if (!string.IsNullOrEmpty(mail.to)) to = mail.to;
                else to = ExtractTag(body, "to");

                string[] toArray = to.Split(';');

                message = new MailMessage(new MailAddress(From), new MailAddress(toArray[0]));
                for (int i = 1; i < toArray.Length; i++) message.To.Add(new MailAddress(toArray[i]));
                // Тема
                message.Subject = ExtractTag(body, "subject");
                message.IsBodyHtml = true;

                body = ExtractTag(body, "body");

                // Заменяем все метки в теле письма
                body = body.Replace("[MASKEDPAN]", mail.pan);
                body = body.Replace("[PWD]", mail.fleetpwd);
                body = body.Replace("[LINKKEY]", mail.linkkey);
                body = body.Replace("[LOGIN]", mail.login);

                // Вложения
                if (mail.attachment != null)
                {
                    string[] mass = mail.attachment.Split(';');
                    foreach (string s in mass)
                    {
                        Attachment attachData = new Attachment(s);
                        message.Attachments.Add(attachData);
                    }
                }

                // Поиск картинок
                int count = 0;
                string oldChar = ExtractImages(body, ref count);
                Random RGen = new Random();
                List<LinkedResource> listimages = new List<LinkedResource>();
                while (oldChar != "")
                {
                    var image = new LinkedResource(Path.Combine(ImagesPath, oldChar), "image/jpg");
                    image.ContentId = Guid.NewGuid().ToString();
                    image.TransferEncoding = TransferEncoding.Base64;
                    listimages.Add(image);

                    body = body.Replace("{" + oldChar + "}", "cid:" + image.ContentId);

                    oldChar = ExtractImages(body, ref count);
                }
                message.Body = body;

                if (listimages.Count > 0)
                {
                    AlternateView html_view = AlternateView.CreateAlternateViewFromString(message.Body, null, "text/html");
                    foreach (LinkedResource lrs in listimages)
                        html_view.LinkedResources.Add(lrs);
                    message.AlternateViews.Add(html_view);
                }
            }
            catch (Exception e) { msg = e.Message; return false; }

            return true;
        }

        private string ExtractImages(string body, ref int count)
        {
            try
            {
                int startIndex = body.ToLower().IndexOf("<img src=\"{", count);
                int endIndex;
                if (startIndex >= 0)
                {
                    endIndex = body.IndexOf("}\"", startIndex + 11);
                }
                else
                {
                    return "";
                }
                startIndex = startIndex + 11;
                string imgurl = body.Substring(startIndex, (endIndex - (startIndex)));
                count = startIndex;
                return imgurl;
            }
            catch (Exception e) {  }
            return null;
        }

        private string ExtractTag(string body, string tag)
        {
            try
            {
                int startIndex = body.ToLower().IndexOf(string.Format("<{0}>", tag), 0) + 2 + tag.Length;
                int id = body.ToLower().IndexOf(string.Format("</{0}>", tag), 0);

                int finIndex = id - startIndex; 
                
                return body.Substring(startIndex, finIndex);
            }
            catch (Exception e) { }
            return null;
        }
    
        /*    public class MailNotice
        {
            public string host;
            public int port;
            public bool useSsl;
            public string userName;
            public string password;
            public string from;
            public string to;
            public string subject;
            public string messageText;
            public int timeout;
        }

        //Queue msgQueue = Queue.Synchronized(new Queue());

        public static bool _SendNotice(
            string host, int port, bool useSsl,
            string userName, string password,
            string from, string to,
            string subject,
            string messageText)
        {
            string[] toArray = to.Split(';');
            SmtpClient client = new SmtpClient(host, port);
            client.EnableSsl = useSsl;
            NetworkCredential credent = new NetworkCredential(userName, password);
            client.Credentials = credent;
            MailMessage message = new MailMessage(new MailAddress(from), new MailAddress(toArray[0]));
            for (int i = 1; i < toArray.Length; i++) message.To.Add(new MailAddress(toArray[i]));
            //message.IsBodyHtml = false;
            message.Body = messageText;
            //message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Subject = subject;
            //message.SubjectEncoding = System.Text.Encoding.UTF8;
            client.Send(message);
            message.Dispose();
            return true;
        }

        public static bool SendNotice(
            string host, int port, bool useSsl,
            string userName, string password,
            string from, string to,
            string subject,
            string messageText, 
            out string msg)
        {
            msg = null;

            try
            {
                _SendNotice(host, port, useSsl, userName, password, from, to, subject, messageText);
                return true;
            }
            catch (Exception e) { msg = e.Message; }
            return false;
        }*/
/*
        static void Send(object o)
        {
            MailNotice n;
            try { n = (MailNotice)o; }
            catch { return; } // error in mail body

            ManualTimer timer = new ManualTimer(n.timeout);

            string msg;

            while (!timer.Timeout)
            {
                try
                {
                    if (SendNotice(n.host, n.port, n.useSsl, n.userName, n.password, n.from, n.to, n.subject, n.messageText, out msg) == true)
                    {
                        //CLog.Log("*** Message delivered: " + n.messageText);
                        return;
                    }
                }
                catch { }
                System.Threading.Thread.Sleep(1 * 60 * 1000);
            }

            //CLog.Log("*** Cannot deliver message: " + n.messageText);
        }

        public static void Test()
        {
            string msg;

            SendNotice(
                "utd-ch.com", 25, false,
                "alex@utd-ch.com", "parol",
                "alex@utd-ch.com",
                "alex@utd-ch.com;mail4alex@tut.by",
                "subject",
                "Message Text",
                out msg);
            return;
        }
*/
    }
}

