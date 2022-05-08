namespace BirthdayBot.Core.Types
{
    public class ClientSettings
    {
        public int StartLocationInputBlockDays { get; set; }

        public int ChangeLocationInputBlockDays { get; set; }

        public int StartLocationInputAttempts { get; set; }

        public int ChangeLocationInputAttempts { get; set; }
    }
}
