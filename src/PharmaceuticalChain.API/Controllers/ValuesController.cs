﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaceuticalChain.API.Services.Interfaces;

namespace PharmaceuticalChain.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IEthereumService ethereumService;
        public ValuesController(IEthereumService _ethereumService)
        {
            ethereumService = _ethereumService;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            ethereumService.GetContract();
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet]
        [Route("contract/get")]
        public async Task<string> GetValue()
        {
            return await ethereumService.Get();
        }

        //[HttpGet]
        //[Route("exeContract/{name}/{contractMethod}/{value}")]
        //public async Task<string> ExecuteContract([FromRoute] string name, [FromRoute] string contractMethod, [FromRoute] int value)
        //{
        //    string contractAddress = await service.TryGetContractAddress(name);
        //    var contract = await service.GetContract(name);
        //    if (contract == null) throw new System.Exception("Contact not present in storage");
        //    var method = contract.GetFunction(contractMethod);
        //    try
        //    {
        //        // var result = await method.CallAsync<int>(value);
        //        var result = await method.SendTransactionAsync(service.AccountAddress, value);
        //        return result.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        return "error";
        //    }
        //}
    }
}
