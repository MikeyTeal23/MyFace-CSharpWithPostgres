using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor;

namespace MyFace.Helpers
{
    /// <summary>
    /// Access to the WindsorContainer to resolve dependencies via the HttpContext
    /// </summary>
    /// <remarks>
    /// In current MVC, ActionFilters are cached aggressively and are effectively singletons,
    /// hence we cannot inject dependencies into them. Instead they need to resolve their
    /// dependencies at runtime and so need access to the IWindsorContainer.
    ///
    /// We use HttpContext.Items since it is per-request and we have access to it both in the
    /// overridden WindsorControllerFactory, where we can store the IWindsorContainer, via the
    /// RequestContext, and in our ActionFilter via the FilterContext.
    /// </remarks>
    public static class WindsorContextHelper
    {
        private static readonly string WindsorContainerItemKey = "WindsorContainer";

        public static void SaveContainerInContext(this RequestContext requestContext, IWindsorContainer container)
        {
            requestContext.HttpContext.Items[WindsorContainerItemKey] = container;
        }

        public static IWindsorContainer WindsorContainer(this HttpContextBase context)
        {
            var container = context.Items[WindsorContainerItemKey] as IWindsorContainer;
            if (container == null)
            {
                throw new InvalidOperationException(
                    $"IWindsorContainer not found in HttpContext.Items[{WindsorContainerItemKey}]");
            }

            return container;
        }

        /// <summary>
        /// Returns a component instance from a Windsor container instance saved in HttpContext
        /// </summary>
        /// <remarks>
        /// Prefer dependency injection where possible. This is intended to be used in cases where
        /// cannot inject dependencies, e.g. long-lived (not per request) action filters or
        /// singletons.
        /// </remarks>
        public static T Resolve<T>(this ControllerContext context)
        {
            return WindsorContainer(context.HttpContext).Resolve<T>();
        }
    }
}
