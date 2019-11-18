﻿using PharmaceuticalChain.API.Controllers.Models.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaceuticalChain.API.Services.Interfaces
{
    public interface ISupplyChainService
    {
        BatchSupplyChainQueryData GetBatchSupplyChain(Guid batchId);
    }
}
