//using MailKit.Net.Smtp;
//using MimeKit;

//namespace shoppingcartwebsite.Service
//{
//    public class EmailService : IEmailService
//    {
//        private readonly EmailConfiguration _emailConfig;

//        public EmailService(EmailConfiguration emailConfig)
//        {
//            _emailConfig = emailConfig;
//        }

//        public async Task SendEmailAsync(string email, string subject, string message)
//        {
//            var emailMessage = new MimeMessage();

//            emailMessage.From.Add(new MailboxAddress(_emailConfig.From, _emailConfig.From));
//            emailMessage.To.Add(new MailboxAddress(email, email));

//            emailMessage.Subject = subject;

//            var bodyBuilder = new BodyBuilder
//            {
//                HtmlBody = message
//            };

//            emailMessage.Body = bodyBuilder.ToMessageBody();

//            using (var client = new SmtpClient())
//            {
//                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, _emailConfig.UseSsl);
//                await client.AuthenticateAsync(_emailConfig.Username, _emailConfig.Password);
//                await client.SendAsync(emailMessage);
//                await client.DisconnectAsync(true);
//            }
//        }
//    }

//}
