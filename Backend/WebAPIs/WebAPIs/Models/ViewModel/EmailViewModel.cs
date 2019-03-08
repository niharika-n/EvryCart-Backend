using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Models
{
    public class EmailViewModel
    {
        public List<MailUser> ToEmailList { get; set; } = new List<MailUser>();

        public string Subject { get; set; }

        public string Body { get; set; }

        public List<MailUser> ToCclist { get; set; } = new List<MailUser>();

        public List<MailUser> ToBccList { get; set; } = new List<MailUser>();
    }

    public class MailUser
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
