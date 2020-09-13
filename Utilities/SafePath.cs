﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Whydoisuck.Utilities
{
    class SafePath
    {
        public static char DirectorySeparator => Path.DirectorySeparatorChar;

        public static string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }
    }
}
