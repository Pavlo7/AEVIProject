using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using System.Security.Cryptography;
using AEVIDomain;
using System.Threading;

namespace AEVIWeb.Models
{

    #region Models
    
    public class ActivateModel
    {
        [Required]
        [Display(Name = "Key")]
        public string Key { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }


    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    #endregion

    #region Services
    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

    //    bool ValidateUser(string userName, string password, out string msg);
        int LogON(string login, string password, out string msg);
     //   bool ValidatePass(string userName, string password, out string msg);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword, out string msg);
    }

    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public int LogON(string login, string password, out string msg)
        {
            
            int ret = 0;
            msg = null;

            CUser clUser = new CUser(null, LocalData.CSDbUsers(), LocalData.LogPath());
            STUser stUser;
            int retvalue = clUser.GetRecordByUserLogin(login, out stUser, out msg);
            if (retvalue != 0) return -1;
            else
            {
                if (stUser.userid == null)
                {
                    string smsg = string.Format("Invalid user ({0})", login);
                    CUdpSender clUDp = new CUdpSender(LocalData.Host(), LocalData.Port(), LocalData.LogPath());
                    clUDp.Send(LocalData.Facility(), LocalData.TagId(), "UWA101", smsg);

                    return 2;
                }

                if (stUser.islock)
                {
                    msg = string.Format("The user \"{0}\" has temporarily blocked for 30 minutes", login);
                    return 3;
                }

                if (!UserModelsRepository.Instance.ValidateLogOnPassword(stUser, password, out msg))
                {
                    string smsg = string.Format("Invalid password for user ({0})", login);
                    CUdpSender clUDp = new CUdpSender(LocalData.Host(), LocalData.Port(), LocalData.LogPath());
                    clUDp.Send(LocalData.Facility(), LocalData.TagId(), "UWA102", smsg);
                   // msg = smsg;
                    return 4;
                }

                if (stUser.passvaliddate <= DateTime.Now)
                    return 1;
            }

            return ret;
        }


        /*public bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            return _provider.ValidateUser(userName, password);

        }*/
              
       /* public bool ValidateUser(string userName, string password, out string msg)
        {
            msg = null;

            if (String.IsNullOrEmpty(userName))
            {
                msg = "Value \"userName\" cannot be null or empty.";
                return false;
            }

            STUser data = new STUser();
            CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());

            int retval = clUser.GetRecordByUserLogin(userName, out data, out msg);
            if (retval != 0) return false;

            if (data.username == null)
            {
                msg = string.Format("The user \"{0}\" doesn't exist", userName);
                return false;
            }

            return true;
        }*/

        /*public bool ValidatePass(string userName, string password, out string msg)
        {
            msg = "The user name or password provided is incorrect.";

            if (String.IsNullOrEmpty(userName))
            {
                msg = "Value \"userName\" cannot be null or empty.";
                return false;
            }
            if (String.IsNullOrEmpty(password))
            {
                msg = "Value \"password\" cannot be null or empty.";
                return false;
            }

            CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            STUser dbuser = new STUser();

            int ret = clUser.GetRecordByUserLogin(userName, out dbuser, out msg);

            if (ret != 0) return false;
            else
            {
                if (dbuser.userid == null)
                {
                    string smsg = string.Format("Invalid user ({0})", userName);
                    CUdpSender clUDp = new CUdpSender(LocalData.Host(), LocalData.Port(), LocalData.LogPath());
                    clUDp.Send(LocalData.Facility(), LocalData.TagId(), "UWA101", smsg);

                    return false;
                }

                if (dbuser.islock)
                {
                    msg = string.Format("The user \"{0}\" has temporarily blocked for 30 minutes", userName);
                    return false;
                }
            }

            return _provider.ValidateUser(userName, password);
        }*/

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword, out string msg)
        {
            msg = null;

            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");
            
            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                // проверяем, правильно ли введен старый пароль
                if (!UserModelsRepository.Instance.ValidateChangePassword(userName, oldPassword, out msg))
                    return false;

                // проверяем новый пароль на наличее букв верхнего и нижнего регистра, цифры и спец символа 
                if (UserModelsRepository.Instance.IsSimplePassword(newPassword))
                {
                    msg =  "The new password is too simple. " +
                        "Passwords must be at least 8 characters in length. " +
                        "Passwords must contain: " +
                        "a minimum of 1 lower case letter [a-z] and a minimum of 1 upper case letter [A-Z] and " +
                        "a minimum of 1 numeric character [0-9] and a minimum of 1 special character: " +
                        "~`!@#$%^&*()-_+={}[]|\\;:\"<>,./?";
                    return false;
                }

                // проверяем новый пароль на совпадение с предыдущими 4 паролями 
                if (UserModelsRepository.Instance.CheckPassCache(userName, newPassword))
                {
                    msg = "Your password cannot repeat any of your 4 previous passwords!";
                    return false;
                }


                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }
       
    }

    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
    #endregion

    #region Validation
    public static class AccountValidation
    {
        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
    {
        private const string _defaultErrorMessage = "'{0}' must be at least {1} characters long.";
        private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

        public ValidatePasswordLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[]{
                new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), _minCharacters, int.MaxValue)
            };
        }
    }
    #endregion

    public class AccountRepository
    {
        private static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPwd = String.Concat(pwd, salt);
            string hashedPwd =
                    FormsAuthentication.HashPasswordForStoringInConfigFile(
                    saltAndPwd, "sha1");
            return hashedPwd;
        }

        private static string CreateSalt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[32];
            rng.GetBytes(buff);

            return Convert.ToBase64String(buff);
        }

       /* public string GetUserNameByEmail(string email)
        {
            string ret = null;

            STUser stUser = new STUser();
            string msg = null;
            CUser clUser =
               new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            int retvalue = clUser.GetRecordByUserEmail(email, out stUser, out msg);

            return stUser.userid;
        }*/

        public MembershipUser GetUser(string username)
        {
            STUser data = new STUser();
            string msg;

            CUser clUser =  new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            int retvalue = clUser.GetRecordByUserLogin(username, out data, out msg);

            string _username = data.login;
            int _providerUserKey = 0;
            string _email = data.email;
            string _passwordQuestion = "";
            string _comment = data.comments;
            bool _isApproved = data.isactivated;
            bool _isLockedOut = false;
            DateTime _creationDate = data.creationdate;
            DateTime _lastLoginDate = data.modifieddate;
            DateTime _lastActivityDate = DateTime.Now;
            DateTime _lastPasswordChangedDate = DateTime.Now;
            DateTime _lastLockedOutDate = DateTime.Now;
            MembershipUser user = new MembershipUser("LocalMembershipProvider",
                                                            _username,
                                                            _providerUserKey,
                                                            _email,
                                                            _passwordQuestion,
                                                            _comment,
                                                            _isApproved,
                                                            _isLockedOut,
                                                            _creationDate,
                                                            _lastLoginDate,
                                                            _lastActivityDate,
                                                            _lastPasswordChangedDate,
                                                            _lastLockedOutDate);
            return user;
        }
        

        // смена пароля в БД
        public bool ChangePassword(string login, string oldPassword, string newPassword)
        {
            STUser data = new STUser();
            string msg;
            CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            // находим юзера в БД по логину
            int ret = clUser.GetRecordByUserLogin(login, out data, out msg);

            if (ret != 0) return false;

            // соль
            string passwordsalt = CreateSalt();
            // кодируем пароль
            string password = CreatePasswordHash(newPassword, passwordsalt);
            // дата действия пароля 6 месяцев
            DateTime passvaliddate = DateTime.Now.AddMonths(6); ;

            // делаем изменения в БД
            int retvalue = clUser.UpdatePassword(data.userid, password, passwordsalt, passvaliddate, out msg);
            if (retvalue != 0) return false;

            // добавляем пароль в кэш паорелей в БД
            STPassCache pc = new STPassCache();
            pc.password = password;
            pc.passwordsalt = passwordsalt;
            clUser.AddPassToPassCache(login, pc, out msg);

            return true;
        }
        
    }
}
