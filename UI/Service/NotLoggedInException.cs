namespace UI.Service
{
    /// <summary>
    /// The API refused the call: there is no token, or the token is no longer valid.
    /// Pages catch this and send the visitor back to the login page.
    /// </summary>
    public sealed class NotLoggedInException : Exception
    {
        public NotLoggedInException()
            : base("The API refused the call because the visitor is not logged in.")
        {
        }
    }
}
