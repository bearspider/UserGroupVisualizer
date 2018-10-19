using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UserGroupVisualizer
{
    public class Helper
    {
        public Helper()
        {
            Username = "";
            UserDomain = "";
        }
        public String Username { get; set; }
        public String UserDomain { get; set; }
    }
}
