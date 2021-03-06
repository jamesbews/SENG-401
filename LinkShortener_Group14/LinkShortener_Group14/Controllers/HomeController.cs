﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LinkShortener.Models.Database;
using System.ServiceModel.Web;
using System.IO;
using System.Net;
using System.Net.Sockets;
using LinkShortener_Group14.Models;


namespace LinkShortener_Group14.Controllers
{
    public class HomeController : Controller
    {
        
        [HttpPost]
        public ActionResult Index(string URL) 
        {
            LinkDatabase dataBase = LinkDatabase.getInstance(); //Create DB instance
            string key = dataBase.saveLongURL(URL); //Saves long URL to DB and returns ID key

           

            string valueShort = Globals.getIp() + "/home/about?id=" + key; //Appends key returned from DB to shortened URL
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


        [HttpPost]
        public ActionResult SaveCompanyReview(int? id)
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();
            
            LinkDatabase database = LinkDatabase.getInstance(); //Creates DB instance

            ViewBag.Json = database.saveReview(json);

            return View();
        }

        [WebGet(UriTemplate = "/GetCompanyReview/{id}")]
        public ActionResult GetCompanyReview(string id)
        {

            LinkDatabase database = LinkDatabase.getInstance();

           ViewBag.Json = database.getReview(id);

            return Json(database.getReview(id));
        }
    }
}