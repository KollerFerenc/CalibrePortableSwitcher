using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrePortableSwitcher
{
    public enum TargetBinary
    {
        Toggle = 1,
        Binary32 = 32,
        Binary64 = 64,
    }

    public enum ErrorCode
    {
        NoError = 0,
        InvalidPath = -1,
        CalibreNotFound = -2,
        CannotToggle = -3,
        UnauthorizedAccess = -100,
        CannotLaunchCalibrePortale = -200,
        FatalUnknownError = -999,
    }
}
