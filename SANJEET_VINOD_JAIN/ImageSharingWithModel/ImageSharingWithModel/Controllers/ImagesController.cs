﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageSharingWithModel.DAL;
using ImageSharingWithModel.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithModel.Controllers;

using SysIOFile = File;

public class ImagesController : BaseController
{
    private readonly ApplicationDbContext db;

    private readonly IWebHostEnvironment hostingEnvironment;

    private readonly ILogger<ImagesController> logger;

    // Dependency injection
    public ImagesController(ApplicationDbContext db, IWebHostEnvironment environment, ILogger<ImagesController> logger)
    {
        this.db = db;
        hostingEnvironment = environment;
        this.logger = logger;
    }

    protected void mkDirectories()
    {
        var dataDir = Path.Combine(hostingEnvironment.WebRootPath,
            "data", "images");
        if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
    }

    protected string imageDataFile(int id)
    {
        return Path.Combine(
            hostingEnvironment.WebRootPath,
            "data", "images", "img-" + id + ".jpg");
    }

    public static string imageContextPath(int id)
    {
        return "data/images/img-" + id + ".jpg";
    }


    //TODO-DONE
    [HttpGet]
    public ActionResult Upload()
    {
        CheckAda();
        var Username = GetLoggedInUser();
        if (Username == null) return ForceLogin();

        ViewBag.Message = "";
        var imageView = new ImageView();
        imageView.Tags = new SelectList(db.Tags, "Id", "Name", 1);
        return View(imageView);
    }

    //TODO-DONE
    [HttpPost]
    public async Task<IActionResult> Upload(ImageView imageView)
    {
        CheckAda();

        var Username = GetLoggedInUser();
        if (Username == null) return ForceLogin();

        await TryUpdateModelAsync(imageView);

        if (!ModelState.IsValid)
        {
            ViewBag.Message = "Please correct the errors in the form!";
            ViewBag.ImageErrorMessage = "No image file specified!";
            ViewBag.ImageNotUploaded = true;
            if (ModelState["DateTaken"]?.Errors.Count > 0)
            {
                ModelState["DateTaken"].Errors.Clear();
                ModelState.AddModelError("DateTaken", "Please Enter Valid Date");
            }

            imageView.Tags = new SelectList(db.Tags, "Id", "Name", 1);
            return View(imageView);
        }

        if (imageView.ImageFile == null || imageView.ImageFile.Length <= 0)
        {
            ViewBag.ImageErrorMessage = "No image file specified!";
            ViewBag.ImageNotUploaded = true;
            imageView.Tags = new SelectList(db.Tags, "Id", "Name", 1);
            return View(imageView);
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.Username.Equals(Username));
        if (user == null)
        {
            ViewBag.Message = "No such Username registered!";
            imageView.Tags = new SelectList(db.Tags, "Id", "Name", 1);
            return View(imageView);
        }


        // TODO-DONE save image metadata in the database
        var selectedTag = await db.Tags.SingleOrDefaultAsync(x => x.Id.Equals(imageView.TagId));

        var metadataImage = new Image
        {
            Caption = imageView.Caption,
            Description = imageView.Description,
            DateTaken = imageView.DateTaken,
            UserId = user.Id,
            TagId = selectedTag.Id,
            User = user,
            Tag = selectedTag
        };
        await db.Images.AddAsync(metadataImage);
        await db.SaveChangesAsync();
        // end TODO

        mkDirectories();

        // TODO-DONE save image file on disk
        using (var DestinationStream = SysIOFile.Create(imageDataFile(metadataImage.Id)))
        {
            await imageView.ImageFile.CopyToAsync(DestinationStream);
        }

        // end TODO

        return RedirectToAction("Details", new { metadataImage.Id });
    }

    [HttpGet]
    public IActionResult Query()
    {
        CheckAda();
        if (GetLoggedInUser() == null) return ForceLogin();

        ViewBag.Message = "";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Details(int Id)
    {
        CheckAda();
        if (GetLoggedInUser() == null) return ForceLogin();

        var image = await db.Images.FindAsync(Id);
        if (image == null) return RedirectToAction("Error", "Home", new { ErrId = "Details:" + Id });

        var imageView = new ImageView();
        imageView.Id = image.Id;
        imageView.Caption = image.Caption;
        imageView.Description = image.Description;
        imageView.DateTaken = image.DateTaken;
        /*
         * Eager loading of related entities
         */
        var imageEntry = db.Entry(image);
        await imageEntry.Reference(i => i.Tag).LoadAsync();
        await imageEntry.Reference(i => i.User).LoadAsync();
        imageView.TagName = image.Tag.Name;
        imageView.Username = image.User.Username;
        return View(imageView);
    }

    //TODO-DONE
    [HttpGet]
    public async Task<IActionResult> Edit(int Id)
    {
        CheckAda();
        if (GetLoggedInUser() == null) return ForceLogin();

        var image = await db.Images.FindAsync(Id);
        if (image == null) return RedirectToAction("Error", "Home", new { ErrId = "EditNotFound" });

        var Username = GetLoggedInUser();
        await db.Entry(image).Reference(im => im.User).LoadAsync(); // Eager load of user
        if (!image.User.Username.Equals(Username))
            return RedirectToAction("Error", "Home", new { ErrId = "EditNotAuth" });

        ViewBag.Message = "";

        var imageView = new ImageView();
        imageView.Tags = new SelectList(db.Tags, "Id", "Name", image.TagId);
        imageView.Id = image.Id;
        imageView.TagId = image.TagId;
        imageView.Caption = image.Caption;
        imageView.Description = image.Description;
        imageView.DateTaken = image.DateTaken;

        return View("Edit", imageView);
    }

    //TODO-DONE
    [HttpPost]
    public async Task<IActionResult> DoEdit(int Id, ImageView imageView)
    {
        CheckAda();
        var Username = GetLoggedInUser();
        if (Username == null) return ForceLogin();

        if (!ModelState.IsValid)
        {
            ViewBag.Message = "Please correct the errors on the page";
            imageView.Id = Id;
            imageView.Tags = new SelectList(db.Tags, "Id", "Name", imageView.TagId);
            if (ModelState["DateTaken"]?.Errors.Count > 0)
            {
                ModelState["DateTaken"].Errors.Clear();
                ModelState.AddModelError("DateTaken", "Please Enter Valid Date");
            }

            return View("Edit", imageView);
        }

        logger.LogDebug("Saving changes to image " + Id);
        var image = await db.Images.FindAsync(Id);
        if (image == null) return RedirectToAction("Error", "Home", new { ErrId = "EditNotFound" });

        await db.Entry(image).Reference(im => im.User).LoadAsync(); // Explicit load of user
        if (!image.User.Username.Equals(Username))
            return RedirectToAction("Error", "Home", new { ErrId = "EditNotAuth" });

        image.TagId = imageView.TagId;
        image.Caption = imageView.Caption;
        image.Description = imageView.Description;
        image.DateTaken = imageView.DateTaken;
        db.Entry(image).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return RedirectToAction("Details", new { Id });
    }

    //TODO-DONE
    [HttpGet]
    public async Task<IActionResult> Delete(int Id)
    {
        CheckAda();
        var Username = GetLoggedInUser();
        if (Username == null) return ForceLogin();

        var image = await db.Images.FindAsync(Id);
        if (image == null) return RedirectToAction("Error", "Home", new { ErrId = "Delete" });

        await db.Entry(image).Reference(im => im.User).LoadAsync(); // Explicit load of user
        if (!image.User.Username.Equals(Username))
            return RedirectToAction("Error", "Home", new { ErrId = "DeleteNotAuth" });

        var imageView = new ImageView();
        imageView.Id = image.Id;
        imageView.Caption = image.Caption;
        imageView.Description = image.Description;
        imageView.DateTaken = image.DateTaken;
        /*
         * Eager loading of related entities
         */
        await db.Entry(image).Reference(i => i.Tag).LoadAsync();
        imageView.TagName = image.Tag.Name;
        imageView.Username = image.User.Username;
        return View(imageView);
    }

    //TODO-DONE
    [HttpPost]
    public async Task<IActionResult> DoDelete(int Id)
    {
        CheckAda();
        var Username = GetLoggedInUser();
        if (Username == null) return ForceLogin();

        var image = await db.Images.FindAsync(Id);
        if (image == null) return RedirectToAction("Error", "Home", new { ErrId = "DeleteNotFound" });

        await db.Entry(image).Reference(im => im.User).LoadAsync(); // Explicit load of user
        if (!image.User.Username.Equals(Username))
            return RedirectToAction("Error", "Home", new { ErrId = "DeleteNotAuth" });

        //db.Entry(imageEntity).State = EntityState.Deleted;
        db.Images.Remove(image);
        await db.SaveChangesAsync();

        SysIOFile.Delete(imageDataFile(image.Id));

        return RedirectToAction("Index", "Home", new { Username = Username });
    }

    //TODO-DONE
    [HttpGet]
    public IActionResult ListAll()
    {
        CheckAda();
        var Username = GetLoggedInUser();
        if (Username == null) return ForceLogin();

        IEnumerable<Image> images = db.Images.Include(im => im.User).Include(im => im.Tag);
        ViewBag.Username = Username;
        return View(images);
    }

    //TODO-DONE
    [HttpGet]
    public IActionResult ListByUser()
    {
        CheckAda();
        if (GetLoggedInUser() == null) return ForceLogin();

        // TODO-DONE Return form for selecting a user from a drop-down list
        var userView = new ListByUserView();
        userView.Users = new SelectList(db.Users, "Id", "Username", 1);

        // TODO-DONE
        return View(userView);
    }

    //TODO-DONE
    [HttpPost]
    public async Task<IActionResult> DoListByUser(ListByUserView userView)
    {
        CheckAda();
        var Username = GetLoggedInUser();
        if (Username == null) return ForceLogin();

        // TODO-DONE list all images uploaded by the user in userView (see List By Tag)
        var user = await db.Users.FindAsync(userView.Id);
        if (user == null) return RedirectToAction("Error", "Home", new { ErrId = "ListByUser" });
        ViewBag.Username = Username;
        /*
         * Eager loading of related entities
         */
        var images = db.Entry(user).Collection(t => t.Images).Query().Include(im => im.User).Include(t => t.Tag)
            .ToList();
        // TODO-DONE
        return View("ListAll", user.Images);
    }

    //TODO-DONE
    [HttpGet]
    public IActionResult ListByTag()
    {
        CheckAda();
        if (GetLoggedInUser() == null) return ForceLogin();

        var tagView = new ListByTagViewModel();
        tagView.Tags = new SelectList(db.Tags, "Id", "Name", 1);
        return View(tagView);
    }

    //TODO-DONE
    [HttpPost]
    public async Task<IActionResult> DoListByTag(ListByTagViewModel tagView)
    {
        CheckAda();
        var Username = GetLoggedInUser();
        if (Username == null) return ForceLogin();

        var tag = await db.Tags.FindAsync(tagView.Id);
        if (tag == null) return RedirectToAction("Error", "Home", new { ErrId = "ListByTag" });

        ViewBag.Username = Username;
        /*
         * Eager loading of related entities
         */
        var images = db.Entry(tag).Collection(t => t.Images).Query().Include(im => im.User).ToList();
        return View("ListAll", tag.Images);
    }
}