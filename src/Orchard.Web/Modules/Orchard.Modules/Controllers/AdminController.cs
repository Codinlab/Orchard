﻿using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Modules.ViewModels;
using Orchard.Mvc.AntiForgery;
using Orchard.Mvc.Results;
using Orchard.UI.Notify;

namespace Orchard.Modules.Controllers {
    public class AdminController : Controller {
        private readonly IModuleService _moduleService;

        public AdminController(IOrchardServices services, IModuleService moduleService) {
            Services = services;
            _moduleService = moduleService;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Not allowed to manage modules")))
                return new HttpUnauthorizedResult();

            var modules = _moduleService.GetInstalledModules().ToList();
            return View(new ModulesIndexViewModel {Modules = modules});
        }

        public ActionResult Edit(string id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageModules, T("Not allowed to edit module")))
                return new HttpUnauthorizedResult();

            var module = _moduleService.GetModuleByName(id);

            if (module == null)
                return new NotFoundResult();

            return View(new ModuleEditViewModel {
                                               Name = module.DisplayName
                                           });
        }

        public ActionResult Features() {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            var features = _moduleService.GetAvailableFeatures().ToList();
            return View(new FeaturesViewModel {Features = features});
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Enable(string id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return new NotFoundResult();

            _moduleService.EnableFeatures(new [] {id});

            return RedirectToAction("Features");
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Disable(string id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageFeatures, T("Not allowed to manage features")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrEmpty(id))
                return new NotFoundResult();

            _moduleService.DisableFeatures(new[] { id });

            return RedirectToAction("Features");
        }

        private class FormValueRequiredAttribute : ActionMethodSelectorAttribute {
            private readonly string _submitButtonName;

            public FormValueRequiredAttribute(string submitButtonName) {
                _submitButtonName = submitButtonName;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
                var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
                return !string.IsNullOrEmpty(value);
            }
        }
    }
}