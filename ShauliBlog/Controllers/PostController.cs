﻿using ShauliBlog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ShauliBlog.Controllers
{
   
    public class PostController : Controller
    {
        private BlogDBContext db = new BlogDBContext();

        public ActionResult Index(string SearchTitle, string SearchAuthor)
        {
            // Redirects unlogged user to the login page            
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // Set site statistics properties
                ViewBag.TotalPosts = db.Post.Count();
                ViewBag.TotalComments = db.Comment.Count();
                ViewBag.TotalAccounts = db.Account.Count();
                ViewBag.TotalMovies = db.Movie.Count();

                if ((SearchAuthor != null && SearchAuthor != string.Empty) ||
                    (SearchTitle != null && SearchTitle != string.Empty))
                {
                    return View(db.Post.Where(p => p.Account.UserName.Contains(SearchAuthor) &&
                    p.PostTitle.Contains(SearchTitle)).ToList());
                }
                else
                {
                    return View(db.Post.ToList());
                }                
            }
        }
        
        //
        // GET: /Post/Details/5

        public ActionResult Details(int? id)
        {
            // Redirects unlogged user to the login page            
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // returns bad request message if id is null
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                // finds the post by the given id
                Post post = db.Post.Find(id);
                if (post == null)
                {
                    return HttpNotFound();
                }
                return View(post);
            }
        }

        //
        // GET: /Post/Create
        public ActionResult Create()
        {
            // Redirects unlogged user to the login page            
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // Fills the genreitems to be used in the client side
                ViewBag.GenreItems = new SelectList(db.Genre, "GenreId", "GenreName");
                return View();
            }
        }

        //
        // POST: /Post/Create

        [HttpPost]
        public ActionResult Create(Post post)
        {
            // Saves the post to the db
            if (ModelState.IsValid)
            {
                string AttachmentPath = string.Empty;
                bool isSavedSuccessfully = true;
                string fName = "";
                try
                {
                    foreach (string fileName in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[fileName];

                        //Save file content goes here
                        fName = file.FileName;
                        if (file != null && file.ContentLength > 0)
                        {
                            string targetFolder = "videos";

                            if (fileName == "image")
                            {
                                targetFolder = "images";
                                post.PostPicturePath = fName;
                            }
                            else
                            {
                                post.PostVideoPath = fName;
                            }

                            //choose a directory to save in : images or videos
                            var originalDirectory = new DirectoryInfo(string.Format("{0}{1}", Server.MapPath(@"\"), targetFolder));

                            string pathString = originalDirectory.ToString();

                            // save the name of a file
                            var fileName1 = Path.GetFileName(file.FileName);

                            bool isExists = Directory.Exists(pathString);

                            // create the directory if not exists
                            if (!isExists)
                            {
                                Directory.CreateDirectory(pathString);
                            }

                            // save the file in a relative path
                            AttachmentPath = string.Format("{0}\\{1}", pathString, file.FileName);
                            file.SaveAs(AttachmentPath);

                        }
                    }

                }
                catch (Exception ex)
                {
                    isSavedSuccessfully = false;
                }


                if (isSavedSuccessfully)
                {
                    post.Account = db.Account.FirstOrDefault(a => a.UserId == post.AccountId);

                    post.PostDate = DateTime.Now;

                    // save the post to DB
                    db.Post.Add(post);
                    db.SaveChanges();

                    // Run the Apriori algorithm on the data include the new post added to the db
                    var controller = DependencyResolver.Current.GetService<AprioriAlgorithmController>();
                    controller.newDataAddedToDb();

                    return RedirectToAction("Index");
                }
            }

            return View(post);
        }

        //
        // GET: /Post/Edit/5
        public ActionResult Edit(int? id)
        {
            // Redirects unlogged user to the login page            
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.GenreItems = new SelectList(db.Genre, "GenreId", "GenreName");

                // returns bad request message if id is null
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                // finds the post by the given id
                Post post = db.Post.Find(id);
                if (post == null)
                {
                    return HttpNotFound();
                }
                return View(post);
            }
        }

        //
        // POST: /Post/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Post post)
        {
            if (ModelState.IsValid)
            {
                post.PostDate = DateTime.Now;
                // sets state to modified
                db.Entry(post).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }

        public ActionResult Delete(long postId = 0)
        {
            bool isAdmin = (Boolean)((ShauliBlog.Models.Account)Session["user"]).IsAdmin;
            if (!isAdmin)
            {
                return RedirectToAction("Index");
            }
            Post post = db.Post.Find(postId);
            if (post != null)
            {
                db.Post.Remove(post);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
   
        }
      
        public bool CheckEntityExist(long postId = 0)
        {
            bool isExist = false;

            if (postId != null)
            {
                // finds the post by the given id
                Post post = db.Post.Find(postId);

                if (post != null)
                {
                    isExist = true;
                }
            }

            return isExist;
        }

        //
        // POST: /Post/Delete/5

        public ActionResult Statistics()
        {
            // group by the users posts
            var query = from i in db.Post
                        group i by i.Account.UserName into g
                        select new { UserName = g.Key, c = g.Count() };
           
            return View(query.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool IsImage(string fileType)
        {
            // check the image type
            if (fileType == "image/jpg" || fileType == "image/png")
                return true;

            return false;
        }

    }
}