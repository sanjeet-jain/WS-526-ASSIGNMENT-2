﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using ImageSharingWithModel.DAL;
using ImageSharingWithModel.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithModel.Controllers
{
    public class AccountController : BaseController
    {
        private ApplicationDbContext db;

        private ILogger<AccountController> logger;

        // Dependency injection of DB context (see Startup)
        public AccountController(ApplicationDbContext db,
                                 ILogger<AccountController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        //TODO-DONE
        [HttpGet]
        public ActionResult Register()
        {
            CheckAda();

            ViewBag.Username = GetLoggedInUser();
 
            ViewBag.Message = "";

            return View();
        }

        //TODO-DONE
        [HttpPost]
        public async Task<IActionResult> Register(UserView info)
        {
            CheckAda();

            if (ModelState.IsValid)
            {
                User User = await db.Users.SingleOrDefaultAsync(u => u.Username.Equals(info.Username));
                if (User == null)
                {
                    // Save to database
                    User = new User(info.Username, info.IsADA());
                    db.Users.Add(User);
                logger.LogDebug("Successfully registered: " + info.Username);
                }
                else
                {
                    //TODO-DONE Check if user exists, if it does handle issue gracefully
                    logger.LogDebug("Existing User Detected: " + info.Username);
                    ViewBag.Message = "User Already Exists! Please Login";
                    return View(info);
                    //since user exists dont change anything
                    //User.ADA = info.IsADA();
                    //db.Entry(User).State = EntityState.Modified;
                }


                await db.SaveChangesAsync();
                SaveCookie("ADA", User.ADA.ToString());

                return RedirectToAction("Index", "Home", new { Username = info.Username });
            }
            else
            {
                ViewBag.Message = "Input validation errors!";
                return View(info);
            }

        }

        //TODO
        [HttpGet]
        public ActionResult Login()
        {
            CheckAda();
            ViewBag.Message = "";
            return View();
        }

        //TODO
        [HttpPost]
        public async Task<IActionResult> Login(UserView info)
        {
            CheckAda();
            User User = await db.Users.SingleOrDefaultAsync(u => u.Username.Equals(info.Username));
            if (User != null)
            {
                SaveCookies(info.Username, User.ADA);
                logger.LogDebug(info.Username + " has logged in.");
                return RedirectToAction("Index", "Home", new { Username = info.Username });
            }
            else
            {
                ViewBag.Message = "No such user registered!";
                logger.LogDebug(info.Username + " is not registered.");
                return View("Login");
            }
        }

        protected void SaveCookies (String username, bool ADA) {
            SaveCookie("Username", username);
            SaveCookie("ADA", ADA.ToString());
        }

        protected void SaveCookie(String key, String value)
        {
            // TODO-DONE save the value in a cookie field key DONE
            var options = new CookieOptions() { IsEssential = true, Secure = true, SameSite = SameSiteMode.None, Expires = DateTime.Now.AddMonths(3) };
            Response.Cookies.Append(key, value, options);

        }

    }
}
