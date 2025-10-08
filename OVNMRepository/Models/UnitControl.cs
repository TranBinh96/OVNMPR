using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVNMRepository.Models
{
    public class UnitControl
    {
        public ComputerLine computerLine;
        public PasVideo pasVideo;
        public Users users;

        public UnitControl()
        {
            computerLine = new ComputerLine();
            users = new Users();
            pasVideo = new PasVideo();  
        }
    }
}
