﻿namespace PharmaceuticalChain.API
{
    public class EthereumSettings
    {
        public EthereumSettings()
        {
            // Empty
        }
        public string EthereumAccount { get; set; }
        public string EthereumPassword { get; set; }

        public string Abi { get; set; }
        public string ContractAddress { get; set; }

    }
}