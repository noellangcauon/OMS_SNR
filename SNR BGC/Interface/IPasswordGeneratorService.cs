using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SNR_BGC.Interface
{
    public interface IPasswordGeneratorService
    {
        string GeneratePassword(int length = 12);
    }
}
