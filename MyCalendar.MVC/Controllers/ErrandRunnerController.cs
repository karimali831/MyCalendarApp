﻿using MyCalendar.Controllers;
using MyCalendar.Enums;
using MyCalendar.Helpers;
using MyCalendar.Service;
using MyCalendar.Website.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyCalendar.Website.Controllers
{
    public class ErrandRunnerController : UserMvcController
    {
        public ErrandRunnerController(IUserService userService, IFeatureRoleService featureRoleService) : base(userService, featureRoleService)
        {

        }
        public ActionResult NewOrder()
        {
            return View();
        }
    }
}