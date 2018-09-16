using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AssemblyTest
{
    public enum TestEnum
    {
        Input,
        Processing,
        [Browsable(false)]
        SomeMoreProcessing,
        Output,
    }
}
