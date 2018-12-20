using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPG.Other
{
    public class Counter
    {
        private int _currentCounter = 0;
        public int Peek() { return _currentCounter; }
        public int Get() { return _currentCounter++; }
    }
}
