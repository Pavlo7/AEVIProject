using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using AEVIDomain;

namespace AEVIWeb.Models
{
    public class TransactModelsViewParam
    {
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date begin")]
        public DateTime BeginDate { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date end")]
        public DateTime EndDate { get; set; }

        [Display(Name = "PAN")]
        public string MaskPan { get; set; }

        [Display(Name = "POS")]
        public string MaskPos { get; set; }
    }

    public class TransactModels
    {
        [Display(Name = "LTime")]
        public string LocalTime { get; set; }

        [Display(Name = "PAN")]
        public string Pan { get; set; }

        [Display(Name = "POS")]
        public string Pos { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Amount")]
        public string Amount { get; set; }

        [Required]
        [Display(Name = "Currency")]
        public string Currency { get; set; }

        [Required]
        [Display(Name = "Product")]
        public string Product { get; set; }

        [Display(Name = "Quantity ")]
        public string Quantity { get; set; }
    }

    public class TransactModelsRepository
    {
        public static TransactModelsRepository Instance = new TransactModelsRepository();

        public List<TransactModels> GetListTransact(STTransactVP param)
        {
            List<TransactModels> ret = new List<TransactModels>();
            TransactModels item;

            List<STTransact> data = new List<STTransact>();
            string msg;

            CTransact clTransact = new CTransact(LocalData.UserId(), LocalData.CSDbTransacts1(), LocalData.CSDbTransacts2(),
                LocalData.LogPath());

            try
            {
                int retvalue = clTransact.GetData(param, out data, out msg);

                foreach (STTransact row in data)
                {
                    item = new TransactModels();
                   
                    item.Amount = row.amount;
                    item.Country = row.country;
                    item.Currency = row.currency;
                    item.LocalTime = row.ltime.ToString("yyyy-MM-dd HH:mm:ss");
                    item.Pan = row.pan;
                    item.Pos = row.pos;
                    item.Product = row.product;
                    item.Quantity = row.quantity;
                    
                    ret.Add(item);
                }

            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public List<TransactModels> GetListTransact(int pageSize, int pageNum, STTransactVP param)
        {
            List<TransactModels> ret = new List<TransactModels>();
            List<TransactModels> data = new List<TransactModels>();
            string msg = null;

            try
            {
                data = GetListTransact(param);
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

        public STTransactVP GetParam(TransactModelsViewParam prm)
        {
            STTransactVP ret = new STTransactVP();
            string msg;

            try
            {
                ret.strdata = null;
                ret.dtbegin = prm.BeginDate;
                ret.strdata += string.Format("<DateBegin={0}>", ret.dtbegin.ToString("yyyy-MM-dd HH:mm:ss"));
                ret.dtend = prm.EndDate;
                ret.strdata += string.Format("<DateEnd={0}>", ret.dtend.ToString("yyyy-MM-dd HH:mm:ss"));

                if (prm.MaskPan != null)
                {
                    ret.maskedpan = prm.MaskPan;
                    ret.strdata += string.Format("<MaskPAN={0}>", ret.maskedpan);
                }
                if (prm.MaskPos != null)
                {
                    ret.maskedpos = prm.MaskPos;
                    ret.strdata += string.Format("<MaskPOS={0}>", ret.maskedpos);
                }
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public TransactModelsViewParam SetParam(STTransactVP param)
        {
            TransactModelsViewParam ret = new TransactModelsViewParam();
            string msg;

            try
            {
                ret.MaskPan = param.maskedpan;
                ret.MaskPos = param.maskedpos;
                ret.BeginDate = param.dtbegin;
                ret.EndDate = param.dtend;
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
    }
}