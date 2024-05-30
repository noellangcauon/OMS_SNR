using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SNR_BGC.Models;
using System.Net.Http;
using System.Net;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using SNR_BGC.Utilities;

namespace SNR_BGC.Controllers
{
    public class UserController : Controller
    {
        private readonly UserClass _userInfoConn;
        public UserController(UserClass userinfo)
        {
            _userInfoConn = userinfo;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddUser()
        {
            return View();
        }
        public IActionResult CreateUser()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateNewUser(UserModelDTO userform)
        {

            if (userform.accessType == "Native")
            {
                if (userform.fullname == null || userform.username == null || userform.password == null || userform.role == "0")
                {
                    TempData["Error"] = "Failed";
                    return View();
                }
                else
                {
                    var userstbl = new UsersTable();
                    var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;

                    userstbl.accessType = userform.accessType;
                    userstbl.userFullname = userform.fullname;
                    userstbl.username = userform.username + "@snrshopping.com";
                    userstbl.password = EncryptorDecryptor.Encrypt(userform.password);
                    userstbl.userRole = userform.role;
                    userstbl.withOmsAccess = userform.oms;
                    userstbl.withRunnerAccess = userform.runnerapp;
                    userstbl.withPickerAccess = userform.pickerapp;
                    userstbl.withBoxerAccess = userform.boxerapp;
                    userstbl.OmsSubModule = userform.oms ? "L,S" : null;
                    userstbl.userStatus = "Active";
                    userstbl.lastEditDate = DateTime.Now;
                    userstbl.lastEditUser = claims.Claims.ToList()[0].Value;
                    userstbl.newUser = true;

                    _userInfoConn.Add(userstbl);
                    _userInfoConn.SaveChanges();
                    return RedirectToAction("CreateUser", "User");

                }
            }
            else
            {
                if (userform.fullname == null || userform.username == null || userform.role == "0")
                {
                    TempData["Error"] = "Failed";
                    return View();
                }
                else
                {
                    var userstbl = new UsersTable();
                    var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;

                    userstbl.accessType = userform.accessType;
                    userstbl.userFullname = userform.fullname;
                    userstbl.username = userform.username + "@snrshopping.com";
                    userstbl.userRole = userform.role;
                    userstbl.withOmsAccess = userform.oms;
                    userstbl.OmsSubModule = userform.oms ? "L,S" : null;
                    userstbl.withRunnerAccess = userform.runnerapp;
                    userstbl.withPickerAccess = userform.pickerapp;
                    userstbl.withBoxerAccess = userform.boxerapp;
                    userstbl.OmsSubModule = userform.oms ? "L,S" : null;
                    userstbl.userStatus = "Active";
                    userstbl.lastEditDate = DateTime.Now;
                    userstbl.lastEditUser = claims.Claims.ToList()[0].Value;
                    userstbl.newUser = false;

                    _userInfoConn.Add(userstbl);
                    _userInfoConn.SaveChanges();
                    return RedirectToAction("CreateUser", "User");


                }
            }

        }


        [HttpPost]

        public JsonResult InsertUser([FromBody] UserInfoClass uinfo)
        {
            var uname = string.Empty;
            var results = String.Empty;

            if (uinfo.userId == 0)
            {
                var counts = (from row in _userInfoConn.userAccessTable where row.username == uinfo.username && row.userModule == uinfo.userModule && row.userSubModule == uinfo.userSubModule select row).ToList();
                if (counts.Count() != 0)
                {

                    results = " is Already Exist!";

                }
                else
                {
                    _userInfoConn.Add(uinfo);
                    _userInfoConn.SaveChanges();
                    results = "Sucessfully Save!";
                }
            }
            else
            {
                var userChanges = (from c in _userInfoConn.userAccessTable where c.userId == uinfo.userId select c).First();

                userChanges.username = uinfo.username;
                userChanges.userFullname = uinfo.userFullname;
                userChanges.userRole = uinfo.userRole;
                userChanges.userModule = uinfo.userModule;
                userChanges.userSubModule = uinfo.userSubModule;
                userChanges.userAccess = uinfo.userAccess;
                userChanges.userStatus = uinfo.userStatus;
                userChanges.lastEditDate = DateTime.Now;
                userChanges.lastEditUser = uinfo.lastEditUser;

                _userInfoConn.SaveChanges();
                results = "Sucessfully Updated!";
            }
            _userInfoConn.Dispose();
            return Json(new { responseText = "Success", uname = uinfo.userFullname, set = results });


        }

        [HttpPost]
        public List<UserInfoClass> FetchUser([FromQuery] string filter, [FromQuery] string category)
        {

            var result = new List<UserInfoClass>();

            if (!String.IsNullOrEmpty(filter))
            {
                switch (category)
                {
                    case "u_username":
                        result = _userInfoConn.userAccessTable.Where(u => u.username.Contains(filter)).ToList();
                        break;
                    case "u_fullname":
                        result = _userInfoConn.userAccessTable.Where(u => u.userFullname.Contains(filter)).ToList();
                        break;
                    case "u_role":
                        result = _userInfoConn.userAccessTable.Where(u => u.userRole.Contains(filter)).ToList();
                        break;
                    case "u_module":
                        result = _userInfoConn.userAccessTable.Where(u => u.userModule.Contains(filter)).ToList();
                        break;
                    case "u_submodule":
                        result = _userInfoConn.userAccessTable.Where(u => u.userSubModule.Contains(filter)).ToList();
                        break;
                    case "u_status":
                        result = _userInfoConn.userAccessTable.Where(u => u.userStatus.Contains(filter)).ToList();
                        break;

                }

            }
            else
            {
                result = _userInfoConn.userAccessTable.ToList();
            }

            _userInfoConn.Dispose();
            return result;
        }
        [HttpPost]
        public JsonResult DeleteUser([FromBody] UserInfoClass uinfo)
        {

            var delobj = _userInfoConn.userAccessTable.Where(p => p.userId == uinfo.userId).SingleOrDefault();
            _userInfoConn.userAccessTable.Remove(delobj);
            _userInfoConn.SaveChanges();

            return Json(new { responseText = "Success", uname = uinfo.userFullname, set = "Successfully Deleted!" });
        }
        [HttpPost]
        public JsonResult FetchAD([FromQuery] string uname)
        {

            List<UserInfoClass> userInfos = new List<UserInfoClass>();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, "snrshopping.com"))
                {
                    UserPrincipal account = new UserPrincipal(context);
                    account.SamAccountName = '*' + uname + '*';

                    using (var searcher = new PrincipalSearcher(account))
                    {

                        foreach (var result in searcher.FindAll())
                        {
                            var info = new UserInfoClass();
                            DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;

                            info.username = (string)de.Properties["samAccountName"].Value;
                            info.userFullname = (string)de.Properties["givenName"].Value + " " + de.Properties["sn"].Value;
                            userInfos.Add(info);
                        }

                    }

                }
            }
            catch
            {
                Console.Write("Error occurred.");
            }

            _userInfoConn.Dispose();
            return Json(new { set = userInfos });

        }


        public JsonResult GetUsers()
        {

            var result = new List<UsersTable>();

            result = _userInfoConn.usersTable.Where(u => u.userStatus == "Active").ToList();

            _userInfoConn.Dispose();
            return Json(new { set = result });
        }
        public JsonResult GetUsersInactive()
        {

            var result = new List<UsersTable>();

            result = _userInfoConn.usersTable.Where(u => u.userStatus == "Inactive").ToList();

            _userInfoConn.Dispose();
            return Json(new { set = result });
        }

        public JsonResult DeactivateUser(int id)
        {

            var result = new UsersTable();

            result = _userInfoConn.usersTable.Where(u => u.userId == id).FirstOrDefault();
            result.userStatus = "Inactive";

            _userInfoConn.Update(result);
            _userInfoConn.SaveChanges();



            var grid = new List<UsersTable>();

            grid = _userInfoConn.usersTable.Where(u => u.userStatus == "Inactive").ToList();

            return Json(new { set = grid });
        }
        public JsonResult ActivateUser(int id)
        {

            var result = new UsersTable();

            result = _userInfoConn.usersTable.Where(u => u.userId == id).FirstOrDefault();
            result.userStatus = "Active";

            _userInfoConn.Update(result);
            _userInfoConn.SaveChanges();



            var grid = new List<UsersTable>();

            grid = _userInfoConn.usersTable.Where(u => u.userStatus == "Active").ToList();

            return Json(new { set = grid });
        }

        public JsonResult NewUser(UserModelDTO userform)
        {
            try
            {
                var checkUser = new UsersTable();
                checkUser = _userInfoConn.usersTable.Where(u => u.username == userform.username + "@snrshopping.com").FirstOrDefault();

                if (checkUser == null)
                {
                    var userstbl = new UsersTable();
                    var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;

                    userstbl.accessType = userform.accessType;
                    userstbl.userFullname = userform.fullname;
                    userstbl.username = userform.username + "@snrshopping.com";
                    userstbl.password = userform.accessType == "AD" ? null : EncryptorDecryptor.Encrypt(userform.password);
                    userstbl.userRole = userform.role;
                    userstbl.withOmsAccess = userform.oms;
                    userstbl.withRunnerAccess = userform.runnerapp;
                    userstbl.withPickerAccess = userform.pickerapp;
                    userstbl.withBoxerAccess = userform.boxerapp;
                    userstbl.OmsSubModule = userform.oms ? "L,S" : null;
                    userstbl.userStatus = "Active";
                    userstbl.lastEditDate = DateTime.Now;
                    userstbl.lastEditUser = claims.Claims.ToList()[0].Value;
                    userstbl.newUser = true;


                    _userInfoConn.Add(userstbl);
                    _userInfoConn.SaveChanges();


                    return Json(new { set = userstbl });
                }
                else
                {
                    return Json(new { set = "The username is already exist!" });
                }

            }
            catch (Exception ex)
            {

                return Json(new { set = ex });
            }

        }

        public JsonResult EditUser(UserModelDTO userform)
        {
            try
            {

                var userstbl = new UsersTable();
                var claims = (System.Security.Claims.ClaimsIdentity)User.Identity;

                userstbl.userId = userform.userId;
                userstbl.accessType = userform.accessType;
                userstbl.userFullname = userform.fullname;
                userstbl.username = userform.username + "@snrshopping.com";
                userstbl.password = userform.password == null ? null : EncryptorDecryptor.Encrypt(userform.password);
                userstbl.userRole = userform.role;
                userstbl.withOmsAccess = userform.oms;
                userstbl.withRunnerAccess = userform.runnerapp;
                userstbl.withPickerAccess = userform.pickerapp;
                userstbl.withBoxerAccess = userform.boxerapp;
                userstbl.OmsSubModule = userform.oms ? "L,S" : null;
                userstbl.userStatus = "Active";
                userstbl.lastEditDate = DateTime.Now;
                userstbl.lastEditUser = claims.Claims.ToList()[0].Value;

                _userInfoConn.Update(userstbl);
                _userInfoConn.SaveChanges();


                return Json(new { set = userstbl });


            }
            catch (Exception ex)
            {

                return Json(new { set = ex });
            }

        }
        public JsonResult EditForm(int id)
        {
            try
            {
                var result = new UsersTable();

                result = _userInfoConn.usersTable.Where(u => u.userId == id).FirstOrDefault();
                if(result.password != null)
                {
                    result.password = EncryptorDecryptor.Decrypt(result.password);
                }
                
                _userInfoConn.Dispose();
                return Json(new { set = result });

            }
            catch (Exception ex)
            {

                return Json(new { set = ex });
            }

        }
    }

}
