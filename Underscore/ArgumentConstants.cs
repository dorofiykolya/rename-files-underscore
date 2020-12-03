namespace Underscore
{
    public class ArgumentConstants
    {
        [Key(false, Description = "help")] public const string Help = "help";

        [Key(false, Description = "verbose messages")]
        public const string Verbose = "verbose";

        [Key(false, Description = "show status, do not rename files")]
        public const string Status = "status";

        [Key(false, Description = "press ENTER to exit")]
        public const string WaitPressEnter = "pressEnterToExit";

        [Key(false, Description = "force replace invalid chars")]
        public const string Force = "force";
    }
}