using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.common
{
    public class Result
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public string? TraceId { get; set; }

     
    }
}