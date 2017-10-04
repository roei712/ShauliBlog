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
    /// <summary>
    ///  ד
    /// </summary>
    public class PostController : Controller
    {
        private BlogDBContext db = new BlogDBContext();        


        // GET: Post
        public ActionResult Index(string SearchTitle, string SearchAuthor)
        {
            // TODO: unremark useraccounts

            ViewBag.TotalPosts = db.Post.Count();
            ViewBag.TotalComments = db.Comment.Count();                                    
            // ViewBag.TotalAccounts = db.userAccounts.Count();
            ViewBag.TotalFans = db.Fan.Count();
            
            List<Post> posts;

            String query = "select * from posts where {0}";
            string select = "";
            string where = "";

            if (!String.IsNullOrEmpty(SearchTitle))
            {
                select += "PostTitle,";
                where += "PostTitle like '%" + SearchTitle + "%'";
            }
            if (!String.IsNullOrEmpty(SearchAuthor))// should insert to here
            {
                select += "PostAuthor ,";

                if (!String.IsNullOrEmpty(where))
                {
                    where += "and ";
                }
                where += "PostAuthor like '%" + SearchAuthor + "%'";
            }



            if (where == "")
            {
                query = query.Substring(0, query.Length - 10);// empty query
            }
            query = String.Format(query, where);
            posts = (List<Post>)db.Post.SqlQuery(query).ToList();
            return View(posts.ToList());
        }

        // GET: Post
        //public ActionResult Search()
        //{
        //    return View(db.Post.ToList());
        //}


        //
        // GET: /Post/Details/5

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Post.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        //
        // GET: /Post/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Post/Create

        [HttpPost]
        public ActionResult Create([Bind(Include = "PostID,PostTitle,PostAuthor,PostAuthorWebsite,PostDate,PostText,PostPicturePath,PostVideoPath")] Post post)
        {
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
                            
                            var originalDirectory = new DirectoryInfo(string.Format("{0}{1}", Server.MapPath(@"\"), targetFolder));

                            string pathString = originalDirectory.ToString(); // System.IO.Path.Combine(originalDirectory.ToString(), "imagepath");

                            var fileName1 = Path.GetFileName(file.FileName);

                            bool isExists = System.IO.Directory.Exists(pathString);

                            if (!isExists)
                                System.IO.Directory.CreateDirectory(pathString);

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
                    post.PostDate = DateTime.Now;

                    db.Post.Add(post);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View(post);
        }

        //
        // GET: /Post/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post post = db.Post.Find(id);
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        //
        // POST: /Post/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PostID,PostTitle,PostAuthor,PostAuthorWebsite,PostDate,PostText,PostPicturePath,PostVideoPath")] Post post)
        {
            if (ModelState.IsValid)
            {
                db.Entry(post).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(post);
        }



        //
        // GET: /Post/Delete/5

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Post singer = db.Post.Find(id);
            if (singer == null)
            {
                return HttpNotFound();
            }
            return View(singer);
        }

        //
        // POST: /Post/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            bool isAdmin = (Boolean)Session["isAmdin"];
            if (!isAdmin)
            {
                return RedirectToAction("Index");
            }
            Post singer = db.Post.Find(id);
            db.Post.Remove(singer);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Statistics()
        {
            var query = from i in db.Post
                        group i by i.PostAuthor into g
                        select new { PostAuthor = g.Key, c = g.Count() };
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
            if (fileType == "image/jpg" || fileType == "image/png")
                return true;

            return false;
        }

    }
}