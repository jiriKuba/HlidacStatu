﻿using HlidacStatu.Lib.Data;
using HlidacStatu.Util;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static HlidacStatu.Web.Models.ApiV1Models;

namespace HlidacStatu.Web.Controllers
{
    public partial class GenericAuthController : AsyncController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public GenericAuthController()
        {
        }

        public GenericAuthController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        [OutputCache(VaryByParam = "*", Duration = 60 * 60 * 48)]
        [ChildActionOnly]
        public ActionResult CachedAction_Child_48H(object model, string nameOfView, params string[] parameters)
        {
            ViewBag.NameOfView = nameOfView;
            ViewBag.Parameters = parameters;
            return View(nameOfView, model);
        }

        [OutputCache(VaryByParam = "*", Duration = 60 * 60 * 12)]
        [ChildActionOnly]
        public ActionResult CachedAction_Child_12H(object model, string nameOfView, params string[] parameters)
        {
            ViewBag.NameOfView = nameOfView;
            ViewBag.Parameters = parameters;
            return View(nameOfView, model);
        }

        [OutputCache(VaryByParam = "*", Duration = 60 * 60 * 1)]
        [ChildActionOnly]
        public ActionResult CachedAction_Child_1H(object model, string nameOfView, params string[] parameters)
        {
            ViewBag.NameOfView = nameOfView;
            ViewBag.Parameters = parameters;
            return View(nameOfView, model);
        }

        [ChildActionOnly]
        public ActionResult Action_Child(object model, string nameOfView, params string[] parameters)
        {
            ViewBag.NameOfView = nameOfView;
            ViewBag.Parameters = parameters;
            return View(nameOfView, model);
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


    }
}