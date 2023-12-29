using System.Collections.Generic;
using System.Linq;

namespace Backend.Fx.AspNet.ErrorHandling;

public class ErrorShape
{
    public Dictionary<string,string[]> Errors { get; set; }

    public string[] GenericError
    {
        get
        {
            Errors.TryGetValue("_error", out var genericError);
            return genericError;
        }
    }

    public bool HasOnlyGenericError()
    {
        return Errors.Count == 1 && Errors.Keys.Single() == "_error";
    }
}