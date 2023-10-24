using DoAn2VADT.Database.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace DoAn2VADT.ViewModel
{
    public class ReasonView
    {
        public string Id { get; set; }
        public string Reason { set; get; }
        public string RefuseReason { set; get; }
    }
}