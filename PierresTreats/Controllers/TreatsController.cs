using Microsoft.AspNetCore.Mvc;
using PierresTreats.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using System.Web;
using System.Net;


namespace PierresTreats.Controllers
{
  public class TreatsController : Controller
  {
    private readonly PierresTreatsContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TreatsController(UserManager<ApplicationUser> UserManager, PierresTreatsContext db)
    {
      _userManager = UserManager;
      _db = db;
    }

    public ActionResult Index()
    {
      List<Treat> model = _db.Treats.OrderBy(treat => treat.Name).ToList();
      return View(model);
    }

    public ActionResult Create()
    {
      ViewBag.FlavorId = new SelectList(_db.Flavors, "FlavorId", "Name");
      return View();
    }

    [Authorize]
    [HttpPost]
    public ActionResult Create(Treat treat, int FlavorId)
    {
      bool duplicateTreat = _db.Treats.Any(theTreat => theTreat.Name == treat.Name);

      if (treat.Name != null)
      {
        if (duplicateTreat)
        {
          ViewBag.SuccessMessage = "This Treat already exists";
          ViewBag.FlavorId = new SelectList(_db.Flavors, "FlavorId", "Name");
          // Return view since using ViewBag cannot use RedirectToAction
          return View();
        }
        else  // new treat
        {
          ViewBag.SuccessMessage = "Not Duplicate";
          _db.Treats.Add(treat);
          _db.SaveChanges();
        }
        if (FlavorId != 0)
        {
          _db.PierresTreats.Add(new FlavorTreat() { FlavorId = FlavorId, TreatId = treat.TreatId});
          _db.SaveChanges();
        }
      }
      return RedirectToAction("Index");
    }

    public ActionResult Details(int id)
    {
      var thisTreat = _db.Treats
        .Include(treat => treat.JoinEntities)
        .ThenInclude(join => join.Flavor)
        .FirstOrDefault(treat => treat.TreatId == id);
      ViewBag.UsersTreats = _db.PierresTreats.Where(entry => entry.TreatId == id).ToList();
      return View(thisTreat);
    }

    public ActionResult Edit(int id)
    {
      var thisTreat = _db.Treats.FirstOrDefault(treat => treat.TreatId == id);
      return View(thisTreat);
    }

    [Authorize]
    [HttpPost]
    public ActionResult Edit(Treat treat, int FlavorId)
    {
      _db.Entry(treat).State = EntityState.Modified;
      _db.SaveChanges();
      bool duplicate = _db.PierresTreats.Any(join => join.FlavorId == FlavorId && join.TreatId == treat.TreatId);
      if (FlavorId !=0 && !duplicate)
      {
        _db.PierresTreats.Add(new FlavorTreat() { FlavorId = FlavorId, TreatId = treat.TreatId});
      }
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Delete(int id)
    {
      var thisTreat = _db.Treats.FirstOrDefault(treat => treat.TreatId == id);
      return View(thisTreat);
    }

    [Authorize]
    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisTreat = _db.Treats.FirstOrDefault(treat => treat.TreatId == id);
      _db.Treats.Remove(thisTreat);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    [Authorize]
    [HttpPost]
    public ActionResult DeleteFlavor(int joinId)
    {
      ViewBag.PageTitle = "Remove this Treat from the Flavor";
      var joinEntry = _db.PierresTreats.FirstOrDefault(entry => entry.FlavorTreatId == joinId);
      
      _db.PierresTreats.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
  }
}