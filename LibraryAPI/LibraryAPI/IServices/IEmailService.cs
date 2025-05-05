namespace LibraryAPI.IServices
{
    public interface IEmailService
    {
        public Task SendMailAsync(string toEmail, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default);
    }
}
