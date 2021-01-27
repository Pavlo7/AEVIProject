using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using AEVIDomain;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;

namespace AEVIWeb.Models
{
    public class CardModelsViewParam
    {
        [Display(Name = "Pan")]
        public string MaskPan { get; set; }

        [Display(Name = "Expiration Date (YYMM)")]
        public string Expdate { get; set; }

        [Display(Name = "Loyalty flag")]
        public string Loyaltyflag { get; set; }

        [Display(Name = "Fleet ID flag")]
        public string Fleetidflag { get; set; }

        [Display(Name = "Odometer flag")]
        public string Odometerflag { get; set; }

        [Display(Name = "VRN ")]
        public string Vrn { get; set; }

        [Display(Name = "Driver name")]
        public string Drivername { get; set; }

        [Display(Name = "Company name")]
        public string Companyname { get; set; }

        [Display(Name = "Interchange Designator ")]
        public string Intdesignator { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Display(Name = "Owner UserId")]
        public string Owneruserid { get; set; }

        [Display(Name = "Blocked flag")]
        public string Blockflag { get; set; }

        [Display(Name = "Registered flag")]
        public string CntBindFlag { get; set; }

        [Display(Name = "Account")]
        public string Account { get; set; }

        [Display(Name = "Subaccount")]
        public string Subaccount { get; set; }

      //  [Display(Name = "select all cards")]
      //  public bool IsAll { get; set; }

       

    }

    public class CardModels
    {
        [Required]
        [StringLength(64)]
        [Display(Name = "Pan")]
        public string Pan { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "DispledPan")]
        public string DispledPan { get; set; }

        [Required]
        [StringLength(4)]
        [Display(Name = "Expiration Date (YYMM)")]
        public string Expdate { get; set; }

        [Required]
        [Display(Name = "Loyalty flag")]
        public string Loyaltyflag { get; set; }

        [Required]
        [Display(Name = "Fleet ID flag")]
        public string Fleetidflag  { get; set; }

        [Required]
        [Display(Name = "Odometer flag")]
        public string Odometerflag { get; set; }

        [Display(Name = "VRN ")]
        public string Vrn { get; set; }

        [Display(Name = "Driver name")]
        public string Drivername { get; set; }

        [Required]
        [StringLength(32)]
        [Display(Name = "Company name")]
        public string Companyname { get; set; }

        [Required]
        [Display(Name = "Interchange Designator ")]
        public string Intdesignator { get; set; }

        [Required]
        [StringLength(64)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Display(Name = "Owner UserId")]
        public string Owneruserid { get; set; }

        [Required]
        [Display(Name = "Blocked flag")]
        public string Blockflag { get; set; }

        [Required]
        [Display(Name = "Count registers")]
        public int CntBind { get; set; }

        [Required]
        [Display(Name = "Fleet password")]
        public string Fleetpwd { get; set; }

        [Display(Name = "Fleet ID data")]
        public string FleetIdData { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Account")]
        public string Account { get; set; }

        [Display(Name = "Subaccount")]
        public string Subaccount { get; set; }
    }

    public class CardModelsRepository
    {
        public static CardModelsRepository Instance = new CardModelsRepository();
      
        public static string CreatePassword(int length)
        {
            const string valid = "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ23456789";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public List<CardModels> GetListCard(STCardVP param)
        {
            List<CardModels> ret = new List<CardModels>();
            CardModels item;

            List<STCard> data = new List<STCard>();
            string msg;

            CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());

            try
            {
                int retvalue = clCard.GetData(param, out data, out msg);

                foreach (STCard row in data)
                {
                    item = new CardModels();
                    if (row.blockflag == 1)
                    {
                        item.Blockflag = "Yes";
                        item.Status = "blocked";
                    }
                    else
                    {
                        item.Blockflag = "No";
                        item.Status = "active";
                    }
                    item.CntBind = 0;
                    item.CntBind = row.cntbinds;
                    item.Companyname = row.companyname;
                    item.DispledPan = row.maskedpan;
                    item.Drivername = row.drivername;
                    item.Email = row.email;
                    item.Expdate = row.expdate;
                    if (row.fleetidflag == 1)
                        item.Fleetidflag = "Yes";
                    else item.Fleetidflag = "No";
                    item.Fleetpwd = row.fleetpwd;
                    if (row.intdesignator == 1)
                        item.Intdesignator = "1 - for international use";
                    if (row.intdesignator == 5)
                        item.Intdesignator = "5 - local use";
                    if (row.loyaltyflag == 1)
                        item.Loyaltyflag = "Yes";
                    else item.Loyaltyflag = "No";
                    if (row.odometerflag == 1)
                        item.Odometerflag = "Yes";
                    else item.Odometerflag = "No";
                    item.Owneruserid = row.owneruserid;
                    item.Pan = row.pan;
                    item.Vrn = row.vrn;
                    item.Account = row.account;
                    item.Subaccount = row.subaccount;

                    ret.Add(item);
                }

            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public List<CardModels> GetListCard(int pageSize, int pageNum, STCardVP param)
        {
            List<CardModels> ret = new List<CardModels>();
            List<CardModels> data = new List<CardModels>();
            string msg = null;

            try
            {
                data = GetListCard(param);
                if (data.Count <= pageSize) return data;
                else
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (i>= pageNum*pageSize && i<pageNum*pageSize+pageSize) 
                            ret.Add(data[i]);
                    }
                }
            }
            catch (Exception ex) { msg = ex.Message; }
            
            return ret;
        }

        public CardModels GetCard(string pan)
        {
            CardModels ret = new CardModels();

            STCard data = new STCard();
            string msg;

            CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());

            try
            {
                int retvalue = clCard.GetRecord(pan, out data, out msg);
                
                ret = new CardModels();
                if (data.blockflag == 1)
                    ret.Blockflag = "Yes";
                else ret.Blockflag = "No";
                ret.CntBind = data.cntbinds;
                if (!string.IsNullOrEmpty(data.companyname))
                    ret.Companyname = data.companyname;
                else ret.Companyname = null;
                ret.DispledPan = data.maskedpan;
                if (!string.IsNullOrEmpty(data.drivername))
                    ret.Drivername = data.drivername;
                else ret.Drivername = " ";
                if (!string.IsNullOrEmpty(data.email))
                    ret.Email = data.email;
                else ret.Email = " ";
                ret.Expdate = data.expdate;
                if (data.fleetidflag == 1)
                    ret.Fleetidflag = "Yes";
                else ret.Fleetidflag = "No";
                ret.Fleetpwd = data.fleetpwd;
                if (data.intdesignator == 1)
                    ret.Intdesignator = "1 - for international use";
                if (data.intdesignator == 5)
                    ret.Intdesignator = "5 - local use";
                if (data.loyaltyflag == 1)
                    ret.Loyaltyflag = "Yes";
                else ret.Loyaltyflag = "No";
                if (data.odometerflag == 1)
                    ret.Odometerflag = "Yes";
                else ret.Odometerflag = "No";
                
                ret.Owneruserid = data.owneruserid;
                
                ret.Pan = data.pan;

                if (!string.IsNullOrEmpty(data.vrn))
                    ret.Vrn = data.vrn;
                else ret.Vrn = " ";

                if (!string.IsNullOrEmpty(data.account))
                    ret.Account = data.account;
                else ret.Account = " ";

                if (!string.IsNullOrEmpty(data.subaccount))
                    ret.Subaccount = data.subaccount;
                else ret.Subaccount = " ";

                List<STBind> dataBind;
                clCard.GetDataBind(pan, out dataBind, out msg);
                foreach (STBind stbind in dataBind)
                {
                    ret.FleetIdData += stbind.flit + ", ";
                }
                if (string.IsNullOrEmpty(ret.FleetIdData)) ret.FleetIdData = " ";
                else ret.FleetIdData.Remove(ret.FleetIdData.Length - 2, 2);
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }

        private string clear_cm(string data)
        {
         //   string ret = data;

            //char[] arr_ch = new char[] { " ", ".", ',', '*', '/', '\\', '?', '&', '%', '`', '@', '#', '$', 
            //    '^', '(', ')', '-', '_', '\'', ';', ':' }; 

            Regex rgx = new Regex("[^a-zA-Z]");
            return rgx.Replace(data, "").ToUpper();

           // try
           // {
           //     foreach (char ch in arr_ch)
           //     {
           //         ret = ret.To
           //     }
           // }
           // catch (Exception ex) {  }
        //    return ret;
        }

        public bool CreateCard(CardModels model, out string msg)
        {
            bool ret = true;
            msg = null;
            try
            {
                STCard data = new STCard();

                CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());

                data.blockflag = 0;
                data.companyname = model.Companyname;
                data.companyname_ir = clear_cm(model.Companyname);
                data.drivername = model.Drivername;
                data.email = model.Email;
                data.expdate = model.Expdate;
                if (model.Fleetidflag == "Yes")
                    data.fleetidflag = 1;
                else data.fleetidflag = 0;
                data.fleetpwd = CreatePassword(8);
                if (model.Intdesignator == "1 - for international use")
                    data.intdesignator = 1;
                if (model.Intdesignator == "5 - local use")
                    data.intdesignator = 5;
                if (model.Loyaltyflag == "Yes")
                    data.loyaltyflag = 1;
                else data.loyaltyflag = 0;
                if (model.Odometerflag == "Yes")
                    data.odometerflag = 1;
                else data.odometerflag = 0;
                data.owneruserid = LocalData.UserId();
                data.pan = model.Pan;
                data.vrn = model.Vrn;
                data.maskedpan = clCard.get_mask_pan(data.pan);
                data.account = model.Account;
                data.subaccount = model.Subaccount;

                string[] arr = new[] { "'", "\"", "--" };
                if (CheckerField.CheckField(arr, data.companyname, data.email, data.drivername, data.expdate, data.pan, data.vrn, data.account, data.subaccount))
                {
                    msg = "One or more fields contain invalid characters.";
                    return false;
                }

                int retvalue = 
                    clCard.Insert(data, out msg, LocalData.bLocal(), LocalData.ChannelsArray(), LocalData.ChannelsName());

                if (retvalue == 0)
                {
                    CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clAction.AddAction(ActionType.AddCard, string.Format("Registry card {0}, {1}", data.maskedpan,
                        data.expdate), out msg);

                    CMail clMail = new CMail(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clMail.Insert(create_mail_to_client("Add", data.pan.Substring(4,2), data.maskedpan, data.fleetpwd, 
                        data.email), out msg);
                    clMail.Insert(create_mail_to_aevi("Add", data.pan.Substring(4, 2), data.maskedpan, data.email), out msg);
                    
                    SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                        LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                        LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                    smtp.SendNotice(out msg);
                }
                else ret = false;
            }
            catch (Exception ex) { msg = ex.Message; ret = false;}
            return ret;
        }
    
        public bool UpdateCard(CardModels model, out string msg)
        {
            bool ret = true;
            msg = null;
            try
            {
                STCard data = new STCard();

                CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());

                if (model.Blockflag == "Yes")
                    data.blockflag = 1;
                else data.blockflag = 0;
                data.companyname = model.Companyname;
                data.companyname_ir = clear_cm(model.Companyname);
                data.drivername = model.Drivername;
                data.email = model.Email; ;
                if (model.Fleetidflag == "Yes")
                    data.fleetidflag = 1;
                else data.fleetidflag = 0;
                data.fleetpwd = model.Fleetpwd;
                if (model.Intdesignator == "1 - for international use")
                    data.intdesignator = 1;
                if (model.Intdesignator == "5 - local use")
                    data.intdesignator = 5;
                if (model.Loyaltyflag == "Yes")
                    data.loyaltyflag = 1;
                else data.loyaltyflag = 2;
                if (model.Odometerflag == "Yes")
                    data.odometerflag = 1;
                else data.odometerflag = 0;
                data.owneruserid = model.Owneruserid;
                data.vrn = model.Vrn;
                data.expdate = model.Expdate;
                data.pan = (CCrypto.DecryptPAN(model.Pan));
                data.maskedpan = clCard.get_mask_pan(data.pan);
                data.account = model.Account;
                data.subaccount = model.Subaccount;

                string[] arr = new[] { "'", "\"", "--" };
                if (CheckerField.CheckField(arr, data.companyname, data.email, data.drivername, data.expdate, data.pan, data.vrn, data.account, data.subaccount))
                {
                    msg = "One or more fields contain invalid characters.";
                    return false;
                }

                int retvalue = clCard.Update(model.Pan, data, out msg, LocalData.bLocal(), LocalData.ChannelsArray(), LocalData.ChannelsName());
              
                if (retvalue == 0)
                {
                    CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clAction.AddAction(ActionType.EditCard, string.Format("Edit card {0}, {1}", data.maskedpan,
                        data.expdate), out msg);

                    CMail clMail = new CMail(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clMail.Insert(create_mail_to_client("Edit", data.pan.Substring(4, 2), data.maskedpan, data.fleetpwd,
                        data.email), out msg);
                    clMail.Insert(create_mail_to_aevi("Edit", data.pan.Substring(4, 2), data.maskedpan, data.email), out msg);

                    SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                        LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                        LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                    smtp.SendNotice(out msg);
                }
                else ret = false;
            }
            catch (Exception ex) { msg = ex.Message; ret = false; }
            return ret;
        }

        public bool DeleteCard(string pan, out string msg)
        {
            bool ret = true;
            msg = null;
            try
            {
                CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());
                STCard data = new STCard();
                clCard.GetRecord(pan, out data, out msg);

                int retvalue = clCard.Delete(pan, out msg, LocalData.bLocal(), LocalData.ChannelsArray(), LocalData.ChannelsName());

                if (retvalue == 0)
                {
                    CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clAction.AddAction(ActionType.DeleteCard, string.Format("Delete card {0}", data.maskedpan), out msg);
                    
                    CMail clMail = new CMail(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clMail.Insert(create_mail_to_client("Del", data.pan.Substring(4, 2), data.maskedpan, data.fleetpwd,
                        data.email), out msg);
                    clMail.Insert(create_mail_to_aevi("Del", data.pan.Substring(4, 2), data.maskedpan, data.email), out msg);

                    SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                        LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                        LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                    smtp.SendNotice(out msg);
                }
                else ret = false;
            }
            catch (Exception ex) { msg = ex.Message; ret = false; }
            return ret;
        }

        public bool UnRegistredCard(string pan, out string msg)
        {
            bool ret = true;
            msg = null;
            try
            {
                CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());
                STCard data = new STCard();
                clCard.GetRecord(pan, out data, out msg);

                int retvalue = clCard.Unregistred(pan, out msg, LocalData.bLocal(), LocalData.ChannelsArray(), LocalData.ChannelsName());

                if (retvalue == 0)
                {
                    CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clAction.AddAction(ActionType.DeleteCard, string.Format("Unregistred card {0}", data.maskedpan), out msg);
                    
                    CMail clMail = new CMail(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    clMail.Insert(create_mail_to_client("Edit", data.pan.Substring(4, 2), data.maskedpan, data.fleetpwd,
                        data.email), out msg);
                    clMail.Insert(create_mail_to_aevi("Edit", data.pan.Substring(4, 2), data.maskedpan, data.email), out msg);

                    SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                        LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                        LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                    smtp.SendNotice(out msg);
                }
                else ret = false;
            }
            catch (Exception ex) { msg = ex.Message; ret = false; }
            return ret;
        }
               
        public STCardVP GetParam(CardModelsViewParam prm)
        {
            STCardVP ret = new STCardVP();
            string msg;

            try
            {
                ret.strdata = null;
                ret.bParam = false;

          

                if (prm.MaskPan != null)
                {
                    ret.bParam = true;
                    ret.maskedpan = prm.MaskPan;
                    ret.strdata += string.Format("<MaskPAN={0}>", ret.maskedpan);
                }
                if (prm.Expdate != null)
                {
                    ret.bParam = true;
                    ret.expdate = prm.Expdate;
                    ret.strdata += string.Format("<ExpDate={0}>", ret.expdate);
                }
                if (prm.Blockflag != null)
                {
                    ret.bParam = true;
                    if (prm.Blockflag == "Yes")
                    {
                        ret.blockflag = 1;
                        ret.strdata += string.Format("<Bloked=Yes>");
                    }
                    else
                    {
                        ret.blockflag = 0;
                        ret.strdata += string.Format("<Bloked=No>");
                    }
                }
                else ret.blockflag = null;
                if (prm.CntBindFlag != null)
                {
                    ret.bParam = true;
                    if (prm.CntBindFlag == "Yes")
                    {
                        ret.bindflag = 1;
                        ret.strdata += string.Format("<Binded=Yes>");
                    }
                    else
                    {
                        ret.bindflag = 0;
                        ret.strdata += string.Format("<Binded=No>");
                    }
                }
                else ret.bindflag = null;
                if (prm.Companyname != null)
                {
                    ret.bParam = true;
                    ret.maskedcompanyname = prm.Companyname;
                    ret.strdata += string.Format("<MaskCompanyName={0}>", ret.maskedcompanyname);
                }

                if (prm.Drivername != null)
                {
                    ret.bParam = true;
                    ret.maskeddrivername = prm.Drivername;
                    ret.strdata += string.Format("<MasklDriverName={0}>", ret.maskeddrivername);
                }

                if (prm.Email != null)
                {
                    ret.bParam = true;
                    ret.maskedemail = prm.Email;
                    ret.strdata += string.Format("<MaskEmail={0}>", ret.maskedemail);
                }

                if (prm.Fleetidflag != null)
                {
                    ret.bParam = true;
                    if (prm.Fleetidflag == "Yes")
                    {
                        ret.fleetidflag = 1;
                        ret.strdata += string.Format("<Fleet=Yes>");
                    }
                    else
                    {
                        ret.fleetidflag = 0;
                        ret.strdata += string.Format("<Fleet=No>");
                    }
                }

                if (prm.Intdesignator != null)
                {
                    ret.bParam = true;
                    if (prm.Intdesignator == "1 - for international use")
                    {
                        ret.intdesignator = 1;
                        ret.strdata += string.Format("<Intdesignator=for international use>");
                    }
                    if (prm.Intdesignator == "5 - local use")
                    {
                        ret.intdesignator = 5;
                        ret.strdata += string.Format("<Intdesignator=local use>");
                    }
                }

                if (prm.Loyaltyflag != null)
                {
                    ret.bParam = true;
                    if (prm.Loyaltyflag == "Yes")
                    {
                        ret.loyaltyflag = 1;
                        ret.strdata += string.Format("<Loyalty=Yes>");
                    }
                    else
                    {
                        ret.loyaltyflag = 0;
                        ret.strdata += string.Format("<Loyalty=No>");
                    }
                }

                if (prm.Vrn != null)
                {
                    ret.bParam = true;
                    ret.maskedvrn = prm.Vrn;
                    ret.strdata += string.Format("<MaskVRN={0}>", ret.maskedvrn);
                }

                if (prm.Odometerflag != null)
                {
                    ret.bParam = true;
                    if (prm.Odometerflag == "Yes")
                    {
                        ret.odometerflag = 1;
                        ret.strdata += string.Format("<CM=Yes>");
                    }
                    else
                    {
                        ret.odometerflag = 0;
                        ret.strdata += string.Format("<CM=No>");
                    }
                }

                if (prm.Account != null)
                {
                    ret.bParam = true;
                    ret.maskaccount = prm.Account;
                    ret.strdata += string.Format("<MasklAccount={0}>", ret.maskaccount);
                }

                if (prm.Subaccount != null)
                {
                    ret.bParam = true;
                    ret.masksubaccount = prm.Subaccount;
                    ret.strdata += string.Format("<MasklSubaccount={0}>", ret.masksubaccount);
                }
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public CardModelsViewParam SetParam(STCardVP param)
        {
            CardModelsViewParam ret = new CardModelsViewParam();
            string msg;

            try
            {
                ret.MaskPan = param.maskedpan;
                ret.Expdate = param.expdate;
                if (param.blockflag != null)
                {
                    if (param.blockflag == 1)
                        ret.Blockflag = "Yes";
                    else ret.Blockflag = "No";
                }
                else ret.Blockflag = null;

                if (param.bindflag != null)
                {
                    if (param.bindflag == 1)
                        ret.CntBindFlag = "Yes";
                    else ret.CntBindFlag = "No";
                }
                else ret.CntBindFlag = null;

                ret.Companyname = param.maskedcompanyname;
                ret.Drivername = param.maskeddrivername;
                ret.Email = param.maskedemail;

                if (param.fleetidflag != null)
                {
                    if (param.fleetidflag == 1)
                        ret.Fleetidflag = "Yes";
                    else ret.Fleetidflag = "No";
                }
                else ret.Fleetidflag = null;

                if (param.intdesignator != null)
                {

                    if (param.intdesignator == 1)
                        ret.Intdesignator = "1 - for international use";
                    if (param.intdesignator == 5)
                        ret.Intdesignator ="5 - local use";
                }
                else ret.Intdesignator = null;

                if (param.loyaltyflag != null)
                {
                    if (param.loyaltyflag == 1)
                        ret.Loyaltyflag = "Yes";
                    else ret.Loyaltyflag = "No";
                }
                else ret.Loyaltyflag = null;

                ret.Vrn = param.maskedvrn;

                if (param.odometerflag != null)
                {
                    if (param.odometerflag == 1)
                        ret.Odometerflag = "Yes";
                    else ret.Odometerflag = "No";
                }
                else ret.Odometerflag = null;

                ret.Account = param.maskaccount;
                ret.Subaccount = param.masksubaccount;
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }

        public bool BlockListCard(STCardVP param, out string msg)
        {
            msg = null;
            List<STCard> data = new List<STCard>();

            CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());

            int retvalue = clCard.GetData(param, out data, out msg);

            if (data.Count <= 0)
            {
                msg = "The number of cards has to be more than 0";
                return false;
            }

            if (data.Count > LocalData.MaxCntBlockCard())
            {
                msg = "The number of cards exceeds legal number for blocking";
                return false;
            }
            
            foreach (STCard row in data)
            {
                if (row.blockflag == 0)
                {
                    if (clCard.Block(row.pan, row, out msg, LocalData.bLocal(), LocalData.ChannelsArray(),
                        LocalData.ChannelsName()) == 0)
                    {
                        CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                        clAction.AddAction(ActionType.EditCard, string.Format("Block card {0}", row.maskedpan), out msg);

                        CMail clMail = new CMail(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                        clMail.Insert(create_mail_to_client("Edit", row.pan.Substring(4, 2), row.maskedpan, row.fleetpwd,
                            row.email), out msg);
                        clMail.Insert(create_mail_to_aevi("Edit", row.pan.Substring(4, 2), row.maskedpan, row.email), out msg);

                        SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                            LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                            LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                        smtp.SendNotice(out msg);
                    }
                }
            }

            return true;
        }

        
        private STMail create_mail_to_client(string action, string code, string maskedpan, string pwd, string to)
        {
            STMail mail = new STMail();
            try
            {
                mail.linkkey = null;
                
                mail.to = to;
                mail.pan = maskedpan;
                mail.fleetpwd = pwd;
                mail.dtcreate = DateTime.Now.ToString("yyyyMMddHHmmss");
                mail.dtmistsent = null;

                if (action == "Add" && code=="33") 
                    mail.tamplate = "MailToClientSKAddCard.txt";
                if (action == "Edit" && code=="33")
                    mail.tamplate = "MailToClientSKEditCard.txt";
                if (action == "Del" && code=="33")
                    mail.tamplate = "MailToClientSKDelCard.txt";
                if (action == "Add" && code!="33") 
                    mail.tamplate = "MailToClientCZAddCard.txt";
                if (action == "Edit" && code!="33")
                    mail.tamplate = "MailToClientCZEditCard.txt";
                if (action == "Del" && code!="33")
                    mail.tamplate = "MailToClientCZDelCard.txt";
              
            }
            catch (Exception ex) { }
            return mail;
        }

        private STMail create_mail_to_aevi(string action, string code, string maskedpan, string pan)
        {
            STMail mail = new STMail();
            try
            {
                mail.linkkey = null;
                mail.to = null;
                mail.fleetpwd = null;
                mail.dtcreate = DateTime.Now.ToString("yyyyMMddHHmmss");
                mail.dtmistsent = null;

                mail.pan = maskedpan;
                
                if (action == "Add" && code == "33")
                    mail.tamplate = "MailToAEVISKAddCard.txt";
                if (action == "Edit" && code == "33")
                    mail.tamplate = "MailToAEVISKEDitCard.txt";
                if (action == "Del" && code == "33")
                    mail.tamplate = "MailToAEVISKDelCard.txt";
                if (action == "Add" && code != "33")
                    mail.tamplate = "MailToAEVICZAddCard.txt";
                if (action == "Edit" && code != "33")
                    mail.tamplate = "MailToAEVICZEditCard.txt";
                if (action == "Del" && code != "33")
                    mail.tamplate = "MailToAEVICZDelCard.txt";

            }
            catch (Exception ex) { }
            return mail;
        }


        public bool ValidMassUpload(string file, Stream stream, out List<STCard> data, out string msg)
        {
            bool ret = true;
            data = new List<STCard>();
            string line;
            int cntline=1;
            STCard row;
            msg = null;

            List<string> badline = new List<string>();
       
            try
            {

                CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());
                CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                STUser stUser;

                clUser.GetRecordByUserId(LocalData.UserId(), out stUser, out msg);

                string fleetpwd = CreatePassword(8);

                System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                line = sr.ReadLine();

                while ((line = sr.ReadLine()) != null)
                {
                    row = new STCard();
                    cntline++;
                    string[] parts = line.Split(';');

                    if (!string.IsNullOrEmpty(parts[0]))
                    {
                        row.companyname = parts[0];
                        row.companyname_ir = clear_cm(row.companyname);
                    }
                    else badline.Add(string.Format("Line {0} Column {1}. Empty field.", cntline, 1));


                    if (!string.IsNullOrEmpty(parts[1]))
                    {
                        row.pan = parts[1];
                        row.maskedpan = clCard.get_mask_pan(row.pan);

                        if (row.pan.Substring(0, 1) != "7" || row.pan.Length != 19)
                            badline.Add(string.Format("Line {0} Column {1}. Invalid lenght pan or invalid first digit.", cntline, 2));
                        else
                        {
                            foreach (char ch in row.pan)
                                if (!char.IsDigit(ch))
                                    badline.Add(string.Format("Line {0} Column {1}. Pan contains not digits.", cntline, 2));
                        }
                    }
                    else badline.Add(string.Format("Line {0} Column {1}. Empty field.", cntline, 2));

                    if (!string.IsNullOrEmpty(parts[2])) row.vrn = parts[2];
                    if (!string.IsNullOrEmpty(parts[3])) row.drivername = parts[3];

                    if (!string.IsNullOrEmpty(parts[4]))
                    {
                        //DateTime dt;
                        //if (DateTime.TryParse(parts[4], out dt)) row.expdate = dt.Year.ToString();
                        //else badline.Add(string.Format("Line {0} Column {1}. Invalid expiration date", cntline, 5));
                        string s = parts[4];
                        if (s.Length == 4) row.expdate = s;
                        else if (s.Length ==5 && s[2] == '/')
                        {
                            row.expdate = s.Substring(3, 2) + s.Substring(0, 2);
                        }
                        else badline.Add(string.Format("Line {0} Column {1}. Invalid lenght Exp date.", cntline, 4));
                    }
                    else badline.Add(string.Format("Line {0} Column {1}. Empty field.", cntline, 5));

                    if (!string.IsNullOrEmpty(parts[5]))
                    {
                        string s = parts[5];
                        if (s.ToUpper() == "YES") row.loyaltyflag = 1;
                        else if (s.ToUpper() == "NO") row.loyaltyflag = 0;
                        else badline.Add(string.Format("Line {0} Column {1}. Unknown value.", cntline, 6));
                    }
                    else badline.Add(string.Format("Line {0} Column {1}. Empty field.", cntline, 6));

                    if (!string.IsNullOrEmpty(parts[6]))
                    {
                        string s = parts[6];
                        if (s.ToUpper() == "YES") row.fleetidflag = 1;
                        else if (s.ToUpper() == "NO") row.fleetidflag = 0;
                        else badline.Add(string.Format("Line {0} Column {1}. Unknown value.", cntline, 7));
                    }
                    else badline.Add(string.Format("Line {0} Column {1}. Empty field.", cntline, 7));

                    if (!string.IsNullOrEmpty(parts[7]))
                    {
                        string s = parts[7];
                        if (s.ToUpper() == "YES") row.odometerflag = 1;
                        else if (s.ToUpper() == "NO") row.odometerflag = 0;
                        else badline.Add(string.Format("Line {0} Column {1}. Unknown value.", cntline, 8));
                    }
                    else badline.Add(string.Format("Line {0} Column {1}. Empty field.", cntline, 8));

                    if (!string.IsNullOrEmpty(parts[8]))
                    {
                        string s = parts[8];
                        if (s.ToUpper() == "1") row.intdesignator = 1;
                        else if (s.ToUpper() == "5") row.intdesignator = 5;
                        else badline.Add(string.Format("Line {0} Column {1}. Unknown value.", cntline, 9));
                    }
                    else badline.Add(string.Format("Line {0} Column {1}. Empty field.", cntline, 9));


                    if (!string.IsNullOrEmpty(parts[9]))
                    {
                        row.email = parts[9];
                        if (!isValidEmail(row.email)) badline.Add(string.Format("Line {0} Column {1}. Bad e_mail.", cntline, 10));
                    }
                    else badline.Add(string.Format("Line {0} Column {1}. Empty field.", cntline, 10));

                    row.fleetpwd = fleetpwd;
                    row.owneruserid = stUser.userid;


                    // проверяем карту
                    STCard stResd;
                    clCard.GetRecordWithoutDecrypto(row.pan, out stResd, out msg);
                    if (stResd.pan != null)
                        badline.Add(string.Format("Line {0} Card already exists.", cntline));

                    data.Add(row);
                }

                sr.Close();

                if (badline.Count > 0)
                {
                    string fullpath = Path.Combine(LocalData.ReportMassUploadPath(), string.Format("{0}.Report.{1}.csv", file, DateTime.Now.ToString("yyyyMMddHHmmss")));
                    StreamWriter sw = new StreamWriter(fullpath, false, Encoding.Default);
                    foreach (string s in badline)
                    {
                        sw.WriteLine(s);

                        
                    }
                    sw.Close();

                    CMail clMail = new CMail(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                    STMail mail = new STMail();
                    mail.linkkey = null;
                    mail.to = stUser.email; ;
                    mail.pan = null;
                    mail.fleetpwd = null;
                    mail.dtcreate = DateTime.Now.ToString("yyyyMMddHHmmss");
                    mail.dtmistsent = null;
                    mail.tamplate = "MailToUserValidateErrorMassUpload.txt";
                    mail.attachment = fullpath;

                    clMail.Insert(mail, out msg);

                    SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                        LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                           LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                    smtp.SendNotice(out msg);

                    return false;
                }
            }
            catch (Exception ex) { msg = ex.Message; ret = false; }
        
            return ret;
        }

        private bool isValidEmail(string email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateCardMassUpload(string File, List<STCard> listdata, out string msg)
        {
            bool ret = true;
            msg = null;
            int retvalue = 0;

            try
            {
                if (listdata.Count <= 0) return false;
                STCard data = new STCard();


                CCard clCard = new CCard(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(), LocalData.LogPath());
                CUser clUser = new CUser(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                STUser stUser;

                clUser.GetRecordByUserId(LocalData.UserId(), out stUser, out msg);

                string fullpath = Path.Combine(LocalData.ReportMassUploadPath(), string.Format("{0}.Report.{1}.csv", File, DateTime.Now.ToString("yyyyMMddHHmmss")));
                StreamWriter sw = new StreamWriter(fullpath, false, Encoding.Default);
                sw.WriteLine("Masked PAN;Process result;Password");

                foreach (STCard item in listdata)
                {
                    retvalue = clCard.Insert(item, out msg, LocalData.bLocal(), LocalData.ChannelsArray(), LocalData.ChannelsName());
                    if (retvalue == 0)
                    {
                     //   retvalue = clCard.GetRecordWithoutDecrypto(item.pan, out data, out msg);
                        sw.WriteLine(string.Format("{0};{1};{2}", item.maskedpan, "OK", item.fleetpwd));
                    }
                    else
                    {
                        ret = false;
                        sw.WriteLine(string.Format("{0};Card was not processing.Return code:{1} {2};{3}", item.maskedpan, retvalue, msg, item.fleetpwd));
                    }
                }

                sw.Close();

                CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                clAction.AddAction(ActionType.Upload, string.Format("Upload file {0}", File), out msg);

                CMail clMail = new CMail(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                STMail mail = new STMail();
                mail.linkkey = null;
                mail.to = stUser.email; ;
                mail.pan = null;
                mail.fleetpwd = null;
                mail.dtcreate = DateTime.Now.ToString("yyyyMMddHHmmss");
                mail.dtmistsent = null;
                mail.tamplate = "MailToUserReportMassUpload.txt";
                mail.attachment = fullpath;

                clMail.Insert(mail, out msg);

                       SMTPNotice smtp = new SMTPNotice(LocalData.SmtpHost(), LocalData.SmtpPort(), LocalData.SmtpUseSSL(),
                           LocalData.SmtpUserName(), LocalData.SmtpPassword(), LocalData.SmtpFrom(), LocalData.CSDbUsers(),
                              LocalData.LogPath(), LocalData.GetTemplatePath(), LocalData.Images());
                          smtp.SendNotice(out msg);

            }
            catch (Exception ex) { msg = ex.Message; ret = false; }
            return ret;
        }

         
    }
}