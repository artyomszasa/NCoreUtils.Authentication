using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using NCoreUtils.Data;
using NCoreUtils.OAuth2.Data;

namespace NCoreUtils.OAuth2.WebService
{
    class ClientApplicationBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var httpContext = bindingContext.ActionContext.HttpContext;
            var resolver = httpContext.RequestServices.GetRequiredService<ClientApplicationBinderService>();
            var clientApplication = await resolver.ResolveClientApplication(httpContext.RequestAborted);
            if (null == clientApplication)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Unable to resolve client application.");
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(clientApplication);
            }
        }
    }
}