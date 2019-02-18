using MetroDemo.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroDemo.Pages
{
    public interface BaseInterface
    {
        Chord chord { set; get; }
        UserInfo userInfo { set; get; }
        void InitThis();
       
    }
}
