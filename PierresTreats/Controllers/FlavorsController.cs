using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PierresTreats.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using System;


namespace PierresTreats.Controllers
{
  public class FlavorsController : Controller
  {
    private readonly PierresTreatsContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public FlavorsController(UserManager<ApplicationUser> UserManager, PierresTreatsContext db)
    {
      _userManager = UserManager;
      _db = db;
    }
  

    public ActionResult Index()
    {
      var userFlavors = _db.Flavors.OrderByDescending(flavor => flavor.Name).ToList();
      return View(userFlavors);
    }
    
    public ActionResult Create()
    {
      return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> Create(Flavor flavor, int CategoryId)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      flavor.User = currentUser;
      _db.Flavors.Add(flavor);
      _db.SaveChanges();
      
      return RedirectToAction("Index");
    }

    public ActionResult Details(int id)
    {
      var thisFlavor = _db.Flavors
        .Include(flavor => flavor.JoinPierresTreats)
        .ThenInclude(join => join.Treat)
        .FirstOrDefault(flavor => flavor.FlavorId == id);
      return View(thisFlavor);
    }

    public ActionResult Edit(int id)
    {
      var thisFlavor = _db.Flavors.FirstOrDefault(flavor => flavor.FlavorId == id);
      return View(thisFlavor);
    }

    [Authorize]
    [HttpPost]
    public ActionResult Edit(Flavor flavor, int CategoryId)
    {
      _db.Entry(flavor).State = EntityState.Modified;
      _db.SaveChanges();
      return RedirectToAction("Index");
    }


    public async Task<ActionResult> AddTreat(int id)
    {
      var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var currentUser = await _userManager.FindByIdAsync(userId);
      var thisFlavor = _db.Flavors
      .Include(flavor => flavor.JoinPierresTreats)
      .FirstOrDefault(flavor => flavor.FlavorId == id);
      
      ViewBag.TreatId = new SelectList(_db.Treats,"TreatId", "Name");
      return View(thisFlavor);
    }    

    [Authorize]
    [HttpPost]
    public ActionResult AddTreat(Flavor flavor, int TreatId)
    {
      bool duplicate = _db.PierresTreats.Any(join => join.TreatId == TreatId && join.FlavorId == flavor.FlavorId);

      if (TreatId != 0)
      {      
        if (duplicate)
        {
          ViewBag.SuccessMessage = "This Treat has already been added";
          ViewBag.TreatId = new SelectList(_db.Treats,"TreatId", "Name");
          return View();
        }
        else
        {
          ViewBag.SuccessMessage = "Not Duplicate";
          _db.PierresTreats.Add(new FlavorTreat() { TreatId = TreatId, FlavorId = flavor.FlavorId });
          _db.SaveChanges();
        }
      }
      return RedirectToAction("Index");
    }    

    public ActionResult Delete(int id)
    {
      var thisFlavor = _db.Flavors.FirstOrDefault(flavor => flavor.FlavorId == id);
      return View(thisFlavor);
    }

    [Authorize]
    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      var thisFlavor = _db.Flavors.FirstOrDefault(flavor => flavor.FlavorId == id);
      _db.Flavors.Remove(thisFlavor);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

   

    [Authorize]
    [HttpPost]
    public ActionResult DeleteTreat(int joinId)
    {
      var joinEntry = _db.PierresTreats.FirstOrDefault(entry => entry.FlavorTreatId == joinId);
      _db.PierresTreats.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }
  }
}