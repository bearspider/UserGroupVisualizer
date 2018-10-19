using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserGroupVisualizer
{
    public class ADGroup
    {
        public ADGroup()
        {
            Groupname = "";
            Subgroups = new ObservableCollection<ADGroup>();
        }
        public String Groupname { get; set; }
        public ObservableCollection<ADGroup> Subgroups { get; set; }
    }
}
