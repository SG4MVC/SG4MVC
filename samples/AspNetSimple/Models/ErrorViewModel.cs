using System;

namespace AspNetSimple.Models;

public class ErrorViewModel
{
    public Int32 StatusCode { get; set; }

    public String RequestId { get; set; }

    public Boolean ShowRequestId => !String.IsNullOrEmpty(RequestId);
}
