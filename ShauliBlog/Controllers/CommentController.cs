﻿using ShauliBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShauliBlog.Controllers
{
    public class CommentController : Controller
    {
        BlogDBContext db = new BlogDBContext();
        //
        // GET: /Comment/

        public ActionResult Index()
        {
            return View();
        }

        public void Delete(long id = 0)
        {
            Comment Comment = db.Comment.Find(id);
            if (Comment != null)
            {
                db.Comment.Remove(Comment);
                db.SaveChanges();
            }
            /////// return RedirectToAction("Index", "Post");
        }

        //public ActionResult Create([Bind(Include = "CommentID,PostID,CommentTitle,CommentAuthor,CommentAuthorWebsite,CommentText,CommentPost")] Comment comment)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Comment.Add(comment);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(comment);
        //}

        public ActionResult Create(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.CommentDate = DateTime.Now;

                db.Comment.Add(comment);
                db.SaveChanges();
                //return RedirectToAction("Index");
            }

            //return View(comment);

            return Json(new
            {
                PostId = comment.PostID,
                //UserId: userId,
                CommentAuthor = comment.CommentAuthor,
                CommentText = comment.CommentText,

                Text = comment.CommentText,                
                //User = comment.User != null ? comment.User.Username : comment.Name,
                Date = comment.CommentDate.ToString(),
                id = comment.CommentID
            });
        }

        //[HttpPost]
        //public ActionResult AddComment(NewComment newComment)
        //{
        //    Comment comment = new Comment();

        //    comment.CommentPost = this.db.Post.Find(newComment.PostId);

        //    int userId;

        //    if (int.TryParse(newComment.UserId, out userId))
        //    {
        //        //comment.CommentAuthor = this.db.user.Find(userId);
        //    }
        //    else
        //    {
        //        comment.CommentTitle = newComment.Name;
        //    }

        //    comment.CommentText = newComment.Text;
        //    //comment.Date = DateTime.Now;

        //    this.db.Comment.Add(comment);
        //    this.db.SaveChanges();

        //    return Json(new
        //    {
        //        Text = comment.CommentText,
        //        PostId = comment.CommentPost.PostID,
        //        //User = comment.User != null ? comment.User.Username : comment.Name,
        //        //Date = comment.Date.ToString(),
        //        id = comment.CommentID
        //    });
        //}
    }
}