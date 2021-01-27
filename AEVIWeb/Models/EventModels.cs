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
    public class EventModelsViewParam
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
    }

    public class EventModels
    {
        [Display(Name = "LTime")]
        public string LocalTime { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Msg")]
        public string Msg { get; set; }

        [Display(Name = "ProcCode")]
        public string ProcCode { get; set; }

        [Display(Name = "PAN")]
        public string Pan { get; set; }
                
        [Display(Name = "IFSFCode")]
        public string IfsfCode { get; set; }

        [Display(Name = "UTDCode")]
        public string UTDCode { get; set; }

        [Display(Name = "Text")]
        public string Text { get; set; }
    }

    public class EventModelsRepository
    {
        public static EventModelsRepository Instance = new EventModelsRepository();

        public List<EventModels> GetListEvent(STEventVP param)
        {
            List<EventModels> ret = new List<EventModels>();
            EventModels item;

            List<STEvent> data = new List<STEvent>();
            string msg;

            CEvent clEvent = new CEvent(LocalData.UserId(), LocalData.CSDbCards1(), LocalData.CSDbCards2(),
                LocalData.LogPath());

            try
            {
                int retvalue = clEvent.GetData(param, out data, out msg);

                foreach (STEvent row in data)
                {
                    item = new EventModels();

                    item.IfsfCode = row.ifsfcode;
                    item.Country = row.country;
                    item.Pan = row.maskedpan;
                    item.LocalTime = row.ltime.ToString("yyyy-MM-dd HH:mm:ss");
                    item.ProcCode = row.proccode;
                    item.Text = row.text;
                    item.UTDCode = row.utdcode;
                    item.Msg = row.msg;

                    ret.Add(item);
                }

            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public List<EventModels> GetListEvent(int pageSize, int pageNum, STEventVP param)
        {
            List<EventModels> ret = new List<EventModels>();
            List<EventModels> data = new List<EventModels>();
            string msg = null;

            try
            {
                data = GetListEvent(param);
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

        public STEventVP GetParam(EventModelsViewParam prm)
        {
            STEventVP ret = new STEventVP();
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
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
        public EventModelsViewParam SetParam(STEventVP param)
        {
            EventModelsViewParam ret = new EventModelsViewParam();
            string msg;

            try
            {
                ret.MaskPan = param.maskedpan;
                ret.BeginDate = param.dtbegin;
                ret.EndDate = param.dtend;
            }
            catch (Exception ex) { msg = ex.Message; }
            return ret;
        }
    }
}