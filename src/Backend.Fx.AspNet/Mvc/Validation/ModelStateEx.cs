using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Backend.Fx.AspNet.Mvc.Validation;

public static class ModelStateEx
{
    public static Errors ToErrorsDictionary(this ModelStateDictionary modelState)
    {
        var dictionary = new Dictionary<string, string[]>();

        foreach (KeyValuePair<string, ModelStateEntry> keyValuePair in modelState)
        {
            dictionary.Add(keyValuePair.Key, keyValuePair.Value.Errors.Select(err => err.ErrorMessage).ToArray());
        }

        return new Errors(dictionary);
    }

    public static void Add(this ModelStateDictionary modelState, Errors errors)
    {
        foreach ((string key, string[] value) in errors)
        {
            foreach (string errorMessage in value)
            {
                modelState.AddModelError(key, errorMessage);
            }
        }
    }
}
