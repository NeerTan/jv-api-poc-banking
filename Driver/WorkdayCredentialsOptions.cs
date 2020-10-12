using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Driver
{
    public class WorkdayCredentialsOptions
    {
        public const string Section = "WorkdayCredentials";

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
