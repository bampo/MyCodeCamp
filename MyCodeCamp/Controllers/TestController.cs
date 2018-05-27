using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Rest;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Controllers
{
    [Route("api/Test")]
    public class TestController : Controller
    {
        private readonly ActionDescriptor _desc;

        public TestController()
        {
            ;
        }
        [HttpGet("Get")]
        public object  Get()
        {
            return "test string";
        }

        [HttpGet("GetObject")]
        public object GetObject([FromHeader] string authorization)
        {

            return "GetObject result";
        }
    
    }

}
