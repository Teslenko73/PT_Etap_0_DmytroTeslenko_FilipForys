using System;
using System.Collections.Generic;
using System.Text;

namespace PR_Ball
{
    internal class Data : Api
    {
        public Ball GetBall() => new Ball { X = 100, Y = 100 };
    }
}
