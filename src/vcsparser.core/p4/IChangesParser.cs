﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core.p4
{
    public interface IChangesParser
    {
        List<int> Parse(Stream ms);
    }
}