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


        // GET: Post
        public ActionResult Index()
        {
            return View(db.Post.ToList());
        }

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
            Post singer = db.Post.Find(id);
            db.Post.Remove(singer);
            db.SaveChanges();
            return RedirectToAction("Index");
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