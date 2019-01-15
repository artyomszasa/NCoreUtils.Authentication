using System;
using Microsoft.AspNetCore.Mvc;

namespace NCoreUtils.OAuth2.WebService
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class ClientApplicationAttribute : ModelBinderAttribute
    {
        public ClientApplicationAttribute() : base(typeof(ClientApplicationBinder)) { }
    }
}