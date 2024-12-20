namespace GorillaComputer.Models
{
    /// <summary>
    /// Override display information for screen, usually to display important messages, like failure messagges (lack of server connection, moderation actions, etc.)
    /// </summary>
    public class ScreenOverride
    {
        public string Title;
        public string Summary;
        public string Content;
    }
}
