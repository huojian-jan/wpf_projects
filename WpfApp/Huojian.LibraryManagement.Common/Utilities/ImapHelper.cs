using ShadowBot.Common.LocalizationResources;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using MimeKit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class IMAPHelper
    {
        private string _ip;
        private int _port;
        private bool _useSsl;
        private string _userName;
        private string _authCode;

        public IMAPHelper(string ip, int port, bool useSsl, string userName, string authCode)
        {
            _ip = ip;
            _port = port;
            _useSsl = useSsl;
            _userName = userName;
            _authCode = authCode;
        }

        public bool TryConnect(out string errorMessage)
        {
            errorMessage = null;
            var cancelTokenSource = new CancellationTokenSource(10000);      // 设置超时, 避免长时间卡死
            using (var client = new ImapClient())
            {
                try
                {
                    Init(client, cancelTokenSource);
                    return true;
                }
                catch (Exception em)
                {
                    Logging.Error("Unable to connect to mail server, error reason: ", em);

                    if (em is TaskCanceledException)
                        errorMessage = $"{Strings.ImapHelper_EmailConfigurationInformationIsIncorrect}"; // 这种情况一般是没连接上邮件服务器, 或者可能是断网了, 所以会走到超时取消的流程
                    else
                        errorMessage = em.Message;  // 连接上了邮箱服务器, 但是用户名或密码不正确之类的情况

                    return false;
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
        }

        public string[] GetAllFolders()
        {
            using (var client = new ImapClient())
            {
                Init(client);

                var folders = client.GetFolders(client.PersonalNamespaces[0]);
                client.Disconnect(true);

                return folders.Select(folder => folder.FullName).ToArray();
            }
        }

        public IList<MimeMessage> PullEmails(string folderName, Predicate<MimeMessage> predicate)
        {
            var messages = new List<MimeMessage>();
            using (var client = new ImapClient())
            {
                // 1、connect to server
                Init(client);

                var folder = client.GetFolder(folderName);
                folder.Open(FolderAccess.ReadOnly);

                // 2、pull emails from server
                //folder.Status(StatusItems.Count);
                //var messageIndex = folder.Count - 1;
                //while (messageIndex >= 0)
                //{
                //    var message = folder.GetMessage(messageIndex);
                //    if (predicate(message))
                //    {
                //        messages.Add(message);
                //        //break;
                //    }
                //    messageIndex--;
                //}
                messages.AddRange(folder.Where(message => predicate(message))); // 这个接口过滤的时候耗时很长

                // 3、
                client.Disconnect(true);
            }
            return messages;
        }
        private DateTimeOffset ParseEmailDate(MimeMessage message)
        {
            DateTimeOffset date = DateTimeOffset.MinValue;
            var receivedHeader = message.Headers.FirstOrDefault(s => s.Id == HeaderId.Received && s.Value.StartsWith("from"));
            if (receivedHeader != null)
            {
                //https://www.rfc-editor.org/rfc/inline-errata/rfc5322.html 3.6.7.  Trace Fields
                string[] dateValue = receivedHeader.Value.Split(';');
                if (dateValue.Length < 2 || !DateUtils.TryParse(dateValue[1], out date))
                {
                    Logging.Warn($"Get Date From Headers['Received'] Fail,From :{message.From}");
                    //如果从Received获取不到时间（目前看下来都会有时间），则在尝试从Date字段获取时间
                    var dateHeader = message.Headers.FirstOrDefault(s => s.Id == HeaderId.Date);
                    if (dateHeader != null)
                    {
                        if (!DateUtils.TryParse(dateHeader.RawValue, 0, dateHeader.RawValue.Length, out date))
                            Logging.Warn($"Get Date From Headers['Date'] Fail,From :{message.From}");
                    }
                }
            }
            if (date != DateTimeOffset.MinValue)
            {
                //一般匹配的时间不是UTC时间，都是服务器本地时间，这里需要将服务器的偏移换成本地的偏移
                DateTimeOffset localDateTimeOffset = DateTimeOffset.Now;
                if (date.Offset != localDateTimeOffset.Offset)
                    return new DateTimeOffset(date.UtcTicks + localDateTimeOffset.Offset.Ticks, localDateTimeOffset.Offset);
                return date;
            }
            Logging.Warn($"Get Date From Headers Fail,From :{message.From.ToString()}");
            return message.Date;
        }
        /// <summary>
        /// 生成md5字符串
        /// </summary>
        /// <param name="emailBody"></param>
        /// <returns></returns>
        private string GenerateMD5Str(string emailBody)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] buffer = Encoding.Default.GetBytes(emailBody);
                //开始加密
                byte[] newBuffer = mi.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private MimeMessage FetchMessage(IMailFolder folder, int messageIndex)
        {
            try
            {
                //遇到收邮件本身失败情况：MailKit.Net.Imap.ImapProtocolException: Syntax error in response. Unexpected token: [atom: A00000124]
                return folder.GetMessage(messageIndex);
            }
            catch (Exception ex)
            {
                Logging.Warn($"Failed to get the {messageIndex} message, ignore the message", ex);
            }
            return null;
        }

        private MimeMessage FetchMessage(IMailFolder folder, UniqueId messageId)
        {
            try
            {
                //遇到收邮件本身失败情况：MailKit.Net.Imap.ImapProtocolException: Syntax error in response. Unexpected token: [atom: A00000124]
                return folder.GetMessage(messageId);
            }

            catch (Exception ex)
            {
                Logging.Warn($"Failed to get the message of {messageId}, ignore the message", ex);
            }
            return null;
        }


        public IList<MimeMessage> PullArrivedEmailsAfterCertainTimeStamp(string folderName, DateTime lastTriggeredEmailTime, ref HashSet<string> lastReceivedEmails)
        {
            using (var client = new ImapClient())
            {
                // 1、connect to server
                Init(client);

                var folder = client.GetFolder(folderName);
                folder.Open(FolderAccess.ReadOnly);

                // 2、pull email from server
                var arrivedEmails = new List<MimeMessage>();
                var currentReceivedEmails = new List<string>();
                folder.Status(StatusItems.Count);
                var messageIndex = folder.Count - 1;

                DateTime newestTimestamp = DateTime.Now;
                bool firstValidMessage = false;
                while (messageIndex >= 0)
                {
                    var message = FetchMessage(folder, messageIndex);
                    if (message == null)
                    {
                        messageIndex--;
                        continue;
                    }

                    /*
                     解析从邮箱服务器获取的接收时间
                     */
                    message.Date = ParseEmailDate(message);
                    Logging.Debug($"Email Details:  Id => {message.MessageId}, From =>{message.From}, Subject=> {message.Subject}, Date=> {message.Date.ToString("yyyy-MM-dd HH:mm:ss")}");
                    foreach (var header in message.Headers)
                    {
                        Logging.Debug($"{header.Id}: {header.Value}");
                    }

                    if (!firstValidMessage)
                    {
                        newestTimestamp = message.Date.LocalDateTime;
                        //newtimestamp > lastTriggeredEmailTime 一定要触发，  newtimestamp <= lastTriggeredEmailTime 是否触发需要看是否有遗漏的邮件
                        Logging.Info($"newtimeStamp: {newestTimestamp.ToString("yyyy-MM-dd HH:mm:ss")} vs lastTriggeredEmailTime:{lastTriggeredEmailTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                        firstValidMessage = true;
                    }
                    /*
                        问题：自定义的Smtp发送邮件，在拉取邮件的时候，MessageId可能为null，比如阿里云的SMTP服务
                        导致的bug：(1)MessageId为null，添加arrivedEmails缓存中，导致下次再为null的邮件过来，就不会触发应用执行；
                                  (2)如果中间有正常的MessageId的邮件过来，后面在过来null的MessageId，会导致上一个正常的邮件反复被触发；
                        解决方案：在自定义的SMTP服务器发送时，在请求头自定义一个MessageId，如果邮件本身的MessageId为null，则从请求头取
                    */
                    string currentMsgId = message.MessageId;
                    if (string.IsNullOrWhiteSpace(currentMsgId))
                    {
                        var md5Str = GenerateMD5Str(message.Body.ToString());
                        currentMsgId = $"{md5Str}.{message.From}";
                    }

                    //获取这一轮接收的邮件， buffer 时间为5分钟， 
                    //TODO: 这里理论上应该是之前触发的邮件的时间, 这一轮暂时不改。
                    if (message.Date.LocalDateTime > newestTimestamp.AddSeconds(-300))
                    {
                        currentReceivedEmails.Add(currentMsgId);
                    }

                    //这里处理遗漏的邮件, 遗漏的邮件定义： 新接收的邮件时间落在了 上次处理的邮件时间之前的5分钟内
                    if (message.Date.LocalDateTime <= lastTriggeredEmailTime)
                    {
                        //当前邮件时间落在了 buffer时间之前， 则不继续处理接下来的邮件。
                        if (message.Date.LocalDateTime <= lastTriggeredEmailTime.AddSeconds(-300))
                        {
                            AddEmailRecordsToHistoryList(lastReceivedEmails, currentReceivedEmails);
                            break;
                        }
                        else
                        {
                            //1. !lastReceivedEmails.Contains(currentMsgId) 代表上次接收的邮件列表里面没有这个邮件
                            //同时这个邮件时间又落在了上一次处理邮件的时间前 5分钟内
                            //2. lastReceivedEmails.Count != 0 代表不是第一次进入
                            //TODO: 这里有个问题， 邮箱邮件列表为空， 在影刀启动后收到第一封邮件， 并且 message.Date < 启动时间时，不会触发。
                            if (!lastReceivedEmails.Contains(currentMsgId) && lastReceivedEmails.Count != 0)
                            {
                                //这一步添加进来的说明是可能被漏掉的邮件
                                arrivedEmails.Add(message);
                                Logging.Warn($"mail leak, MessageId = {currentMsgId}, Message.Date = {message.Date.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                            }
                        }
                    }
                    else
                    {
                        if (!lastReceivedEmails.Contains(currentMsgId))
                        {
                            arrivedEmails.Add(message);
                        }
                    }
                    messageIndex--;
                }

                //如果最后一封邮件都没超过指定时间范围，则表示正常退出，也要将延迟邮件记录下来
                if (messageIndex < 0)
                {
                    AddEmailRecordsToHistoryList(lastReceivedEmails, currentReceivedEmails);
                }

                // 3、
                client.Disconnect(true);

                //读取的顺序按照收件先后来，所以逆序处理
                arrivedEmails.Reverse();
                return arrivedEmails;
            }
        }

        private void AddEmailRecordsToHistoryList(HashSet<string> lastReceivedEmails, List<string> currentReceivedEmails)
        {
            currentReceivedEmails.ForEach(each => lastReceivedEmails.Add(each));
        }


        public IList<MimeMessage> PullEmails(bool onlyUnread, int top, string folderName, bool markAsRead,
            string keywordInFrom, string keywordInTo, string keywordInSubject, string keywordInTextBody)
        {
            var arrivedEmails = new List<MimeMessage>();

            using (var client = new ImapClient())
            {
                // 1、connect to server
                Init(client);

                // 2、pull email from server
                IMailFolder folder = null;
                try
                {
                    if (!string.IsNullOrEmpty(folderName))
                        folder = client.GetFolder(folderName);
                    else
                        folder = client.Inbox;

                    if (folder == null)
                        throw new ArgumentException($"{Strings.ImapHelper_MailboxFolderDoesNotExist}");

                    folder.Open(FolderAccess.ReadWrite);


                    var mailIds = folder.Search(onlyUnread ? SearchQuery.NotSeen : SearchQuery.All);
                    for (int i = mailIds.Count() - 1; i >= 0; i--)
                    {
                        if (arrivedEmails.Count >= top)
                            break;

                        var mailId = mailIds[i];
                        var mailMessage = FetchMessage(folder, mailId);
                        if (mailMessage == null)
                            continue;


                        if (!string.IsNullOrEmpty(keywordInFrom))
                        {
                            if (!mailMessage.From.Any(f => Decode((f as MailboxAddress).Address).Contains(keywordInFrom)))
                                continue;
                        }

                        if (!string.IsNullOrEmpty(keywordInTo))
                        {
                            var isContainsTo = mailMessage.To.Any(f => Decode((f as MailboxAddress).Address).Contains(keywordInTo)) ||  //收件人
                                               mailMessage.Cc.Any(f => Decode((f as MailboxAddress).Address).Contains(keywordInTo)) ||  //抄送人
                                               mailMessage.Bcc.Any(f => Decode((f as MailboxAddress).Address).Contains(keywordInTo));   //秘抄
                            if (!isContainsTo)
                                continue;
                        }
                        if (!string.IsNullOrEmpty(keywordInSubject))
                        {
                            //判断主题中是否包含时需要对邮件主题进行空判断避免报错
                            if (string.IsNullOrEmpty(mailMessage.Subject) || !mailMessage.Subject.Contains(keywordInSubject))
                                continue;
                        }

                        if (!string.IsNullOrEmpty(keywordInTextBody))
                        {
                            //对正文进行包含判断时默认判断TextBody是否包含，如果TextBody等于null的话在判断HtmlBody，因为纯网页邮件的TextBody就是null
                            if (!string.IsNullOrEmpty(mailMessage.TextBody))
                            {
                                if (!mailMessage.TextBody.Contains(keywordInTextBody))
                                    continue;
                            }
                            else if (!string.IsNullOrEmpty(mailMessage.HtmlBody))
                            {
                                if (!mailMessage.HtmlBody.Contains(keywordInTextBody))
                                    continue;
                            }
                            else
                                continue;
                        }
                        mailMessage.Date = ParseEmailDate(mailMessage);
                        arrivedEmails.Add(mailMessage);
                        if (markAsRead)
                            folder.SetFlags(mailId, MessageFlags.Seen, true);
                    }
                }
                finally
                {
                    if (folder.IsOpen)
                        folder.Close(true);

                    client.Disconnect(true);
                }
            }

            return arrivedEmails;
        }

        private void Init(ImapClient client, CancellationTokenSource cancellationTokenSource = null)
        {
            CancellationToken cancellationToken = default;
            if (cancellationTokenSource != null)
                cancellationToken = cancellationTokenSource.Token;

            client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; // 取消客户端证书认证
            //对自建邮箱服务器仍使用老版本TLS和SSL协议的兼容
            client.SslProtocols = SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
            client.Connect(_ip, _port, _useSsl, cancellationToken);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(new NetworkCredential(_userName, _authCode), cancellationToken);
            try
            {
                client.Identify(new ImapImplementation { Name = "ShadowBot", Version = "1.0.0" }, cancellationToken);
            }
            catch (Exception e)
            {
                //针对某些邮箱会抛出异常，但不影响使用
                Logging.Warn($"Imap client identify implementation fail but is ok", e);
            }

        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc2047
        /// https://dmorgan.info/posts/encoded-word-syntax/
        /// https://stackoverflow.com/questions/454833/system-net-mail-and-utf-8bxxxxx-headers
        /// https://stackoverflow.com/questions/10828807/how-to-decode-utf-8b-to-string-in-c-sharp
        /// http://www.mimekit.net/docs/html/T_MimeKit_Utils_Rfc2047.htm
        /// http://www.mimekit.net/docs/html/Overload_MimeKit_Utils_Rfc2047_DecodeText.htm
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Decode(string data)
        {
            // 格式    =?<charset>?<encoding>?<encoded-text>?=
            // 正则    =\?{1}(.+)\?{1}([B|Q])\?{1}(.+)\?{1}=

            var bytes = Encoding.UTF8.GetBytes(data);
            return Rfc2047.DecodeText(ParserOptions.Default, bytes);
        }

        /// <summary>
        /// http://help.163.com/09/1223/14/5R7P3QI100753VB8.html
        // http://help.163.com/10/1116/11/6LK0S8T100753VB9.html
        // https://service.mail.qq.com/cgi-bin/help?subtype=1&id=28&no=331
        /// </summary>
        public static List<EmailService> CommonServices = new List<EmailService>(){
                new EmailService(EmailProvider.Netease163, $"{Strings.ImapHelper_163Mailbox}", "imap.163.com", 143, 993),
                new EmailService(EmailProvider.Netease126, $"{Strings.ImapHelper_126Mailbox}","imap.126.com", 143, 993),
                new EmailService(EmailProvider.TencentQQ, $"{Strings.ImapHelper_QQMailbox}","imap.qq.com", 143, 993),
                new EmailService(EmailProvider.Google, Strings.SMTPHelper_Google,"imap.gmail.com", 143, 993),
                new EmailService(EmailProvider.Outlook, Strings.SMTPHelper_Outlook, "outlook.office365.com", 143, 993),
                new EmailService(EmailProvider.iCloud, Strings.SMTPHelper_iCloud,"imap.mail.me.com", 143, 993),
                new EmailService(EmailProvider.Custom, $"{Strings.ImapHelper_CustomMailbox}", "imap.xxx.com", 143, 993),
            };
    }
}
