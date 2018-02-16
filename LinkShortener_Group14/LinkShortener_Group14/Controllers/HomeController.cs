﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LinkShortener.Models.Database;
using System.ServiceModel.Web;

namespace LinkShortener_Group14.Controllers
{
    public class HomeController : Controller
    {
        
        [HttpPost]
        public ActionResult Index(string URL) 
        {
            LinkDatabase dataBase = LinkDatabase.getInstance(); //Create DB instance
            string key = dataBase.saveLongURL(URL); //Saves long URL to DB and returns ID key

            string valueShort = "35.193.100.24/home/about?id=" + key; //Appends key returned from DB to shortened URL
            ViewBag.MyMessage = valueShort; //Places shortened URL in view bag to be dislayed in view


            return View();
        }

        public ActionResult Index()
        {
            

            return View();
        }


        public ActionResult About()
        {
            LinkDatabase database = LinkDatabase.getInstance(); //Creates DB instance 

            string resultLong = database.getLongUrl(Request.QueryString["id"]); //Returns long URL based on key

            return new RedirectResult(resultLong); //Redirects user to intended page
        }

        ///[HttpGet]
        //[Route("Home/SaveCompanyReview/{id?}")]
        [WebGet(UriTemplate = "/SaveCompanyReview/{id}")]
        public ActionResult SaveCompanyReview(string id)
        {
            LinkDatabase database = LinkDatabase.getInstance(); //Creates DB instance

            database.saveReview(id);

            ViewBag.Json = "success";

            return View();
        }

        public ActionResult GetCompanyReview()
        {

            return View();
        }
    }
}