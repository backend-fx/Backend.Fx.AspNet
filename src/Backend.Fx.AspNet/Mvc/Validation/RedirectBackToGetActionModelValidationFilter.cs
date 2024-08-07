﻿using Backend.Fx.Exceptions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Backend.Fx.AspNet.Mvc.Validation;

[PublicAPI]
public class RedirectBackToGetActionModelValidationFilter : ModelValidationFilter
{
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public RedirectBackToGetActionModelValidationFilter(IModelMetadataProvider modelMetadataProvider)
    {
        _modelMetadataProvider = modelMetadataProvider;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid && AcceptsHtml(context) && context.RouteData.Values.ContainsKey("action"))
        {
            var errors = context.ModelState.ToErrorsDictionary();
            LogErrors(context, context.Controller.ToString() ?? "UnknownController", errors);

            // return the same view, using the posted model again
            var viewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
            BeforeRedirect(viewData);
            context.Result = new ViewResult
            {
                ViewName = context.RouteData.Values["action"]!.ToString(),
                ViewData = viewData
            };
        }
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is ClientException cex && AcceptsHtml(context) &&
            context.RouteData.Values.ContainsKey("action"))
        {
            LogErrors(context, context.Controller.ToString() ?? "UnknownController", cex.Errors);
            context.ModelState.Add(cex.Errors);

            // return the same view, using the posted model again
            var viewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
            BeforeRedirect(viewData);
            context.Result = new ViewResult
            {
                ViewName = context.RouteData.Values["action"]!.ToString(),
                ViewData = viewData
            };
            context.ExceptionHandled = true;
        }
    }

    protected virtual void BeforeRedirect(ViewDataDictionary viewData)
    { }
}
