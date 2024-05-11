namespace shoppingcartwebsite.Service
{
    public interface IEmailPasswordSender
    {
        void SendEmail(Message message);
        Task SendEmailAsync(Message message);
    }
}
