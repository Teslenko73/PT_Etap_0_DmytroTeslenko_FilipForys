using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppBall
{
    internal class BallRepository : IBallApi
    {
        public Ball CreateBall()
        {
            return new Ball { X = 100, Y = 100 };
        }
    }
}
