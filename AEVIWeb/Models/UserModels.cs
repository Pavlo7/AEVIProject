using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Security.Cryptography;
using AEVIDomain;
using System.IO;


namespace AEVIWeb.Models
{
    public class UserModelsViewParam
    {
        [StringLength(64)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [StringLength(128)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Display(Name = "Permission")]
        public string Permission { get; set; }

        [Display(Name = "Condition")]
        public string Condition { get; set; }

        [StringLength(20)]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Display(Name = "select all users")]
        public bool IsAll { get; set; }
    }

    public class UserModels
    {
        [Required]
        [StringLength(36)]
        [Display(Name = "UserId")]
        public string UserId { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "UserName")]
        public string UserName { get; set; }
        
        [Required]
        [StringLength(128)]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Incorrect email")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Permission")]
        public string Permission { get; set; }

        [Required]
        [Display(Name = "Condition")]
        public string Condition { get; set; }

        [StringLength(256)]
        [Display(Name = "Comments")]
        public string Comments { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [StringLength(36)]
        [Display(Name = "OwnerUserId")]
        public string OwnerUserId { get; set; }

        [StringLength(64)]
        [Display(Name = "OwnerUserName")]
        public string OwnerUserName { get; set; }

        [StringLength(64)]
        [Display(Name = "AccountState")]
        public string AccountState { get; set; }
    }

    public class UserModelsRepository
    {
        public static UserModelsRepository Instance = new UserModelsRepository();
 
        private static string GenerateKey()
        {
            Guid emailKey = Guid.NewGuid();

            return emailKey.ToString();
        }

        private static string CreateSalt()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[32];
            rng.GetBytes(buff);

            return Convert.ToBase64String(buff);
        }

        private static string CreatePasswordHash(string pwd, string salt)
        {
            string saltAndPwd = String.Concat(pwd, salt);
            string hashedPwd =
                    FormsAuthentication.HashPasswordForStoringInConfigFile(
                    saltAndPwd, "sha1");
            return hashedPwd;
        }

        public List<UserModels> GetListUser()
        {
            List<UserModels> ret = new List<UserModels>();
            UserModels item;
            List<STUser> data = new List<STUser>();
            string msg;
            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();
            CUser clUser;
            try
            {

                clUser =
                new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                STUserVP param = new STUserVP();
                
                int retvalue = clUser.GetData(param, out data, out msg);

                foreach (STUser row in data)
                {
                    item = new UserModels();
                    item.UserId = row.userid;
                    item.UserName = row.username;
                    item.Login = row.login;
                    item.Comments = row.comments;
                    item.Condition = clCondition.GetName(row.condition);
                    item.Permission = clPermission.GetName(row.permission);
                    //item.Password = row.password;
                    item.Email = row.email;
                    if (!row.isactivated) item.AccountState = "Not activated";
                    else item.AccountState = null;
                    ret.Add(item);
                }

            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public List<UserModels> GetListUser(STUserVP param)
        {
            List<UserModels> ret = new List<UserModels>();
            UserModels item;
            List<STUser> data = new List<STUser>();
            string msg;
            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();
            CUser clUser;
            try
            {

                clUser =
                new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                int retvalue = clUser.GetData(param, out data, out msg);

                foreach (STUser row in data)
                {
                    item = new UserModels();
                    item.UserId = row.userid;
                    item.UserName = row.username;
                    item.Login = row.login;
                    item.Comments = row.comments;
                    item.Condition = clCondition.GetName(row.condition);
                    item.Permission = clPermission.GetName(row.permission);
                  //  item.Password = row.password;
                    item.Email = row.email;
                    item.OwnerUserId = row.owneruserid;
                    item.OwnerUserName = row.ownerusername;
                    if (!row.isactivated) item.AccountState = "Not activated";
                    else item.AccountState = null;
                    ret.Add(item);
                }

            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public List<UserModels> GetListUser(int pageSize, int pageNum, STUserVP param)
        {
            List<UserModels> ret = new List<UserModels>();
            List<UserModels> data = new List<UserModels>();
            string msg = null;

            try
            {
                data = GetListUser(param);
                if (data.Count <= pageSize) return data;
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (i >= pageNum * pageSize && i < pageNum * pageSize + pageSize)
                            ret.Add(data[i]);
                    }
                }
            }
            catch (Exception ex) { msg = ex.Message; }

            return ret;
        }

        public int AddUser(UserModels model, out string msg)
        {
            int ret = 0;
            STUser data = new STUser();
         
            msg = null;
            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();
            try
            {
                CUser clUser =
                new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());

                ret = clUser.GetRecordByUserLogin(model.Login, out data, out msg);
                if (ret != 0) return ret;
                else
                {
                    if (data.login != null)
                    {
                        msg = "The login already exists in the database for the application.";
                        return 1;
                    }
                }
                
                data.comments = model.Comments;
                data.activateddate = null;
                data.condition = 0;
                data.creationdate = DateTime.Now;
                data.email = model.Email;
                data.isactivated = false;
                data.login = model.Login;
                data.modifieddate = data.creationdate;
                data.owneruserid = LocalData.UserId();
          //      data.passwordsalt = CreateSalt();
          //      data.password = CreatePasswordHash(model.Password, data.passwordsalt);
                data.permission = clPermission.GetId(model.Permission);
                data.username = model.UserName;
                data.passvaliddate = DateTime.Now.AddDays(-1);
                data.newemailkey = GenerateKey();

                string[] arr = new[] {"'","\"","--"};
                if (CheckerField.CheckField(arr, data.comments, data.email, data.login, data.username))
                {
                    msg = "One or more fields contain invalid characters.";
                    return 2;
                }

                ret = clUser.Insert(data, out msg);

                if (ret == 0)
                {
                    CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clAction.AddAction(ActionType.AddUser, string.Format("Add user {0}, {1}", data.username,
                        data.login), out msg);

                    CMail clMail = new CMail(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());

                    STMail maildata = new STMail();
                    maildata.to = data.email;
                    maildata.tamplate = "MailToUserActivateAccount.txt";
                    maildata.linkkey = data.newemailkey;
                    maildata.fleetpwd = null;
                    maildata.pan = null;
                    maildata.dtcreate = DateTime.Now.ToString("yyyyMMddHHmmss");
                    maildata.dtmistsent = null;
                    maildata.login = data.login;
                    clMail.Insert(maildata, out msg);
                   
                    SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                                          LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                                          LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                    smtp.SendNotice(out msg);
                }
            }
            catch (Exception ex) { msg = ex.Message; ret = -1; }
            return ret;
        }

        public STUser GetLocalUser()
        {
            STUser ret = new STUser();
            string msg;
            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();
            try
            {
                CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                int retvalue = clUser.GetRecordByUserId(LocalData.UserId(), out ret, out msg);
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }

        public UserModels GetUser(string userid)
        {
            UserModels ret = new UserModels();
            STUser data = new STUser();
            string msg;
            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();
            try
            {
                CUser clUser =
                new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                int retvalue = clUser.GetRecordByUserId(userid, out data, out msg);

                ret = new UserModels();
                ret.UserId = data.userid;
                ret.UserName = data.username;
                ret.Login = data.login;
                ret.Comments = data.comments;
                ret.Condition = clCondition.GetName(data.condition);
                ret.Permission = clPermission.GetName(data.permission);
              //  ret.Password = data.password;
              //  ret.ConfirmPassword = ret.Password;
                ret.Email = data.email;
                ret.OwnerUserId = data.owneruserid;
                ret.OwnerUserName = data.ownerusername;
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public UserModels GetUserByLogin(string login)
        {
            UserModels ret = new UserModels();
            STUser data = new STUser();
            string msg;
            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();
            try
            {
                CUser clUser =
                new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                int retvalue = clUser.GetRecordByUserLogin(login, out data, out msg);

                ret = new UserModels();
                ret.UserId = data.userid;
                ret.UserName = data.username;
                ret.Login = data.login;
                ret.Comments = data.comments;
                ret.Condition = clCondition.GetName(data.condition);
                ret.Permission = clPermission.GetName(data.permission);
           //     ret.Password = data.password;
           //     ret.ConfirmPassword = ret.Password;
                ret.Email = data.email;
                ret.OwnerUserId = data.owneruserid;
                ret.OwnerUserName = data.ownerusername;
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }

        public int UpdateUser(UserModels model, out string msg)
        {
            int ret = 0;

            msg = null;
            STUser data = new STUser();
            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();
            try
            {
                CUser clUser =  new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
               
                data.comments = model.Comments;
                data.condition = clCondition.GetId(model.Condition);
                data.email = model.Email;
                data.login = model.Login;
                data.modifieddate = DateTime.Now;
                data.passwordsalt = CreateSalt();
          //      data.password = CreatePasswordHash(model.Password, data.passwordsalt);
                data.permission = clPermission.GetId(model.Permission);
                data.username = model.UserName;
                
                string[] arr = new[] { "'", "\"", "--" };
                if (CheckerField.CheckField(arr, data.comments, data.email, data.login, data.username))
                {
                    msg = "One or more fields contain invalid characters.";
                    return 2;
                }

                ret = clUser.Update(model.UserId, data, out msg);

                if (ret == 0)
                {
                    CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clAction.AddAction(ActionType.EditUser, string.Format("Edit user {0}, {1}", data.username,
                        data.login), out msg);
                }
                
            }
            catch (Exception ex) { msg = ex.Message; ret = -1; }

            return ret;
        }

        public void DeleteUser(string id)
        {
            string msg;
            try
            {
               CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());

                  int retvalue = clUser.Delete(id, out msg);

                  if (retvalue == 0)
                  {
                      CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                      clAction.AddAction(ActionType.DeleteUser, string.Format("Delete user {0}", id), out msg);
                  }
            }
            catch (Exception ex) { msg = ex.Message; }
        }

        // проверяем простой ли пароль
        public bool IsSimplePassword(string password)
        {
            int iXX = 0;

            
            if (ContainsDigit(password)) iXX++;             // нашли цифру
            if (ContainsLowerLetter(password)) iXX++;       // нашли строчную букву
            if (ContainsUpperLetter(password)) iXX++;       // нашли заглавную букву
            if (ContainsSpecial(password)) iXX++;           // нашли спец. символ

            // если всего хватает - пароль сложный
            if (iXX == 4) return false;

            return true;
        }
        private static bool ContainsDigit(string pass)
        {
            foreach (char c in pass)
            {
                if (Char.IsDigit(c))
                    return true;
            }
            return false;
        }
        private static bool ContainsLowerLetter(string pass)
        {
            foreach (char c in pass)
            {
                if ((Char.IsLetter(c)) && (Char.IsLower(c)))
                    return true;
            }
            return false;
        }
        private static bool ContainsUpperLetter(string pass)
        {
            foreach (char c in pass)
            {
                if ((Char.IsLetter(c)) && (Char.IsUpper(c)))
                    return true;
            }
            return false;
        }
        private static bool ContainsSpecial(string pass)
        {
            foreach (char c in pass)
            {
                if ((!Char.IsLetter(c)) && !(Char.IsDigit(c)))
                    return true;
            }
            return false;
        }

        /*public bool ValidateUser(string login, string password)
        {
            string msg;
            CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            STUser dbuser = new STUser();

            int ret = clUser.GetRecordByUserLogin(login, out dbuser, out msg);

            if (ret != 0) return false;
            else
            {
                if (dbuser.password == CreatePasswordHash(password, dbuser.passwordsalt) && dbuser.isactivated == true)
                {
                    clUser.Lock(dbuser.userid, 0, null, null, out msg);
                    // скинем количество неверных попыток и доту временной блокировки и дату ввода не верной попытки
                    return true;
                }
                else
                {
                    
                    // необходимо проверить дату последней ошибочной попытки
                    // если она+30 минут меньше текущей количество попыток устанавливаем 1 и записываем дату
                    // если нет считаем попытку
                    // если это не 3 пишем дату последней попытки и увеличиваем попытку
                    // иначе скидываем дату и попытку и вносим дату блокировки
                    if (dbuser.lastmisstime != null && dbuser.lastmisstime <= DateTime.Now.AddMinutes(-30))
                    {
                        clUser.Lock(dbuser.userid, 1, DateTime.Now, null, out msg);
                    }
                    else
                    {
                        if (dbuser.cntmisstry < 2)
                            clUser.Lock(dbuser.userid, dbuser.cntmisstry + 1, DateTime.Now, null, out msg);
                        else clUser.Lock(dbuser.userid, 0, null, DateTime.Now.AddMinutes(30), out msg);
                    }
                    return false;
                }
            }
        }*/

        public bool ValidateLogOnPassword(STUser dbuser, string password, out string msg)
        {
            msg = null;
            bool ret = true;
            try
            {
            CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());

            if (dbuser.password == CreatePasswordHash(password, dbuser.passwordsalt) && dbuser.isactivated == true)
            {
                clUser.Lock(dbuser.userid, 0, null, null, out msg);
                // скинем количество неверных попыток и доту временной блокировки и дату ввода не верной попытки
                return true;
            }
            else
            {
                // необходимо проверить дату последней ошибочной попытки
                // если она+30 минут меньше текущей количество попыток устанавливаем 1 и записываем дату
                // если нет считаем попытку
                // если это не 3 пишем дату последней попытки и увеличиваем попытку
                // иначе скидываем дату и попытку и вносим дату блокировки
                if (dbuser.lastmisstime != null && dbuser.lastmisstime <= DateTime.Now.AddMinutes(-30))
                {
                    clUser.Lock(dbuser.userid, 1, DateTime.Now, null, out msg);
                }
                else
                {
                    if (dbuser.cntmisstry < 2)
                        clUser.Lock(dbuser.userid, dbuser.cntmisstry + 1, DateTime.Now, null, out msg);
                    else clUser.Lock(dbuser.userid, 0, null, DateTime.Now.AddMinutes(30), out msg);
                }
                return false;
            }
            }
            catch (Exception ex) { msg = ex.Message; ret = false;}
            return ret;
        }
        // проверка текущего пароля
        public bool ValidateChangePassword(string login, string password, out string msg)
        {
            msg = null;
            bool ret = true;
            try
            {
                STUser dbuser;
                CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                // находим юзера в БД по логину
                int retvalue = clUser.GetRecordByUserLogin(login, out dbuser, out msg);
                // сравниваем пароль
                if (dbuser.password == CreatePasswordHash(password, dbuser.passwordsalt) && dbuser.isactivated == true)
                    return true;
                else
                {
                    msg = "The current password is incorrect.";
                    return false;
                }
            }
            catch (Exception ex) { msg = ex.Message; ret = false; }
            return ret;
        }

        // проверка кэша паролей
        public bool CheckPassCache(string login, string password)
        {
            string msg = null;

            STUser dbuser;
            CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            // находим юзера в БД по логину
            int retvalue = clUser.GetRecordByUserLogin(login, out dbuser, out msg);

            // находим кэш паролей в БД по логину юзера
            List<STPassCache> lstpc = new List<STPassCache>();
            retvalue = clUser.GetPassCache(login, out lstpc, out msg);
            
            if (lstpc.Count <= 0) return false;

            // сравниваем введенный пароль со спсиком паролей 
            foreach (STPassCache pc in lstpc)
            {
                string PASS = CreatePasswordHash(password, pc.passwordsalt);
                if (pc.password == PASS) return true;
            }

            return false;
        }
        
        public STUserVP GetParam(UserModelsViewParam prm)
        {
            STUserVP ret = new STUserVP();
            string msg;

            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();

            try
            {
                ret.strdata = null;
              
                if (prm.UserName != null)
                {
                    ret.maskusername = prm.UserName;
                    ret.strdata += string.Format("<UserName={0}>", ret.maskusername);
                }
                if (prm.Permission != null)
                {
                    ret.permission = clPermission.GetId(prm.Permission);
                    ret.strdata += string.Format("<Permission={0}>", prm.Permission);
                }
                if (prm.Condition != null)
                {
                    ret.condition = clCondition.GetId(prm.Condition);
                    ret.strdata += string.Format("<Condition={0}>", prm.Condition);
                }
                
                if (prm.Email != null)
                {
                    ret.maskemail = prm.Email;
                    ret.strdata += string.Format("<Email={0}>", ret.maskemail);
                }

                if (prm.Login != null)
                {
                    ret.masklogin = prm.Login;
                    ret.strdata += string.Format("<Login={0}>", ret.masklogin);
                }
                if (prm.IsAll) ret.isall = true;
                else ret.isall = false;
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public UserModelsViewParam SetParam(STUserVP param)
        {
            UserModelsViewParam ret = new UserModelsViewParam();
            string msg;

            CCondition clCondition = new CCondition();
            CPermission clPermission = new CPermission();

            try
            {
                if (param.condition != null)
                    ret.Condition = clCondition.GetName((int)param.condition);
                else ret.Condition = null;
                ret.Email = param.maskemail;
                ret.Login = param.masklogin;
                if (param.permission != null)
                    ret.Permission = clPermission.GetName((int)param.permission);
                else ret.Permission = null;
                ret.UserName = param.maskusername;
                if (!param.isall) ret.IsAll = false;
                else ret.IsAll = true;
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }

        public bool ActivateUser(ActivateModel model)
        {
            if (string.IsNullOrEmpty(model.Key)) return false;

            string msg;
            CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            STUser dbuser = new STUser();

            int ret = clUser.GetRecordByUserKey(model.Key, out dbuser, out msg);
            if (ret != 0) return false;
            else
            {
                string salt = CreateSalt();
                string password = CreatePasswordHash(model.NewPassword, salt);
                if (clUser.Activate(dbuser.userid, password, salt, out msg) != 0) return false; 
            }

            return true;
        }

        public bool SentLink(STUser stUser)
        {
            string msg = null;

            try
            {

                CUser clUser = new CUser(stUser.userid, LocalData.CSDbUsers(), LocalData.LogPath());
                string key = GenerateKey();
                int ret = clUser.SetKeyFPS(stUser.userid, key, out msg);
                if (ret != 0) return false;

                CMail clMail = new CMail(stUser.userid, LocalData.CSDbUsers(), LocalData.LogPath());

                STMail maildata = new STMail();
                maildata.to = stUser.email;
                maildata.linkkey = key;
                maildata.tamplate = "MailToUserChangePassword.txt";
                maildata.fleetpwd = null;
                maildata.pan = null;
                maildata.dtcreate = DateTime.Now.ToString("yyyyMMddHHmmss");
                maildata.dtmistsent = null;
                clMail.Insert(maildata, out msg);
               
                SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                                                         LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                                                         LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                smtp.SendNotice(out msg);
            }
            catch (Exception ex) { msg = ex.Message; return false; }

            return true;
        }

        public bool FPS(ActivateModel model)
        {
            if (string.IsNullOrEmpty(model.Key)) return false;

            string msg;
            CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
            STUser dbuser = new STUser();

            int ret = clUser.GetRecordByUserKey(model.Key, out dbuser, out msg);
            if (ret != 0) return false;
            else
            {
                string salt = CreateSalt();
                string password = CreatePasswordHash(model.NewPassword, salt);
                if (clUser.FPS(dbuser.userid, password, salt, out msg) != 0) return false;
            }

            return true;
        }
    }
}