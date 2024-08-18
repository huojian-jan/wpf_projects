using System.Net.Mail;
using MimeKit;

namespace ShadowBot.Common.Utilities
{
    /// <summary>
    /// .net自带的邮件类SmtpClient已经过时了，官方推荐使用MailKit
    /// https://github.com/jstedfast/MailKit
    /// </summary>
    public class SMTPHelper
    {
        private string _host;
        private int _port;
        private bool _useSsl;
        private string _userName;
        private string _authCode;

        public SMTPHelper(string host, int port, bool useSsl, string userName, string authCode)
        {
            _host = host;
            _port = port;
            _useSsl = useSsl;
            _userName = userName;
            _authCode = authCode;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="senderName"></param>
        /// <param name="senderAddress"></param>
        /// <param name="recipients">(name, address)</param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="attachFilePaths"></param>
        public void SendEmail(string senderName, string senderAddress, Dictionary<string, string> recipients, string subject, string content, List<string> attachFilePaths = null)
        {
            //TODO(注释放开)
            //// 0、预处理
            //attachFilePaths = attachFilePaths ?? new List<string>();

            //// 1、构造消息
            //var message = new MimeMessage();
            //message.From.Add(new MailboxAddress(senderName, senderAddress));

            //foreach (var recipient in recipients)
            //    message.To.Add(new MailboxAddress(recipient.Key, recipient.Value));

            //message.Subject = subject;

            //var builder = new BodyBuilder();
            //builder.TextBody = content;
            //attachFilePaths.ForEach(attachFilePath => builder.Attachments.Add(attachFilePath));
            //message.Body = builder.ToMessageBody();

            //// 2、发送消息
            //using (var client = new SmtpClient())
            //{
            //    client.Connect(_host, _port, _useSsl);
            //    client.Authenticate(_userName, _authCode);  // Note: only needed if the SMTP server requires authentication
            //    client.Send(message);
            //    client.Disconnect(true);
            //}
        }

        public static List<EmailService> CommonServices = new List<EmailService>()
        {
            //// http://help.163.com/09/1223/14/5R7P3QI100753VB8.html
            //// http://help.163.com/10/1116/11/6LK0S8T100753VB9.html
            //// https://service.mail.qq.com/cgi-bin/help?subtype=1&id=28&no=331
            //    new EmailService(EmailProvider.Netease163, , "smtp.163.com", 25, 465),
            //    new EmailService(EmailProvider.Netease126, Strings.SMTPHelper_Email126, "smtp.126.com", 25, 465),
            //    new EmailService(EmailProvider.TencentQQ, Strings.SMTPHelper_EmailQQ, "smtp.qq.com", 25, 465),
            //    new EmailService(EmailProvider.Google, Strings.SMTPHelper_Google,"smtp.gmail.com", 25, 587, false),
            //    new EmailService(EmailProvider.Outlook, Strings.SMTPHelper_Outlook, "smtp.office365.com", 25, 587, false),
            //    new EmailService(EmailProvider.iCloud, Strings.SMTPHelper_iCloud,"smtp.mail.me.com", 25, 587, false),
            //    new EmailService(EmailProvider.Custom, Strings.SMTPHelper_CustomEmail, "smtp.xxx.com", 25, 465),
            //};
        };
    }
}
