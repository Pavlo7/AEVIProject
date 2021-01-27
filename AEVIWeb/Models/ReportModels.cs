using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Web.Mvc;
using AEVIDomain;

namespace AEVIWeb.Models
{
    public class ReportParamModels
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
    }

    public class ReportModels
    {
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date/time")]
        public DateTime DT { get; set; }

        [Required]
        [Display(Name = "Action name")]
        public string ActionName { get; set; }

        [Required]
        [Display(Name = "Conmment")]
        public string Comment { get; set; }

        [Required]
        [Display(Name = "User")]
        public string User { get; set; }
    }

    public class ReportRepository
    {
        public static ReportRepository Instance = new ReportRepository();

        public List<ReportModels> GetReport(ReportParamModels param)
        {
            List<ReportModels> ret = new List<ReportModels>();
            ReportModels item;

            List<STAction> data = new List<STAction>();
            
            try
            {
                CAction clAction = new CAction(LocalData.UserId(), LocalData.CSDbUsers(), LocalData.LogPath());
                data = clAction.GetReport(param.BeginDate, param.EndDate);

                foreach (STAction row in data)
                {
                    item = new ReportModels();
                    item.ActionName = row.actionname;
                    item.Comment = row.value;
                    item.DT = row.dt;
                    item.User = row.username;
                    ret.Add(item);
                }

            }
            catch (Exception ex) { }
            return ret;
        }
    }
}