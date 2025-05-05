namespace LibraryAPI.Helpers
{
    public class EmailSettingsOption
    {
        public string MailServer { get; set; } = default!;
        public int MailPort { get; set; } = default!;
        public string SenderName { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
