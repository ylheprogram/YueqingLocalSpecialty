﻿using Global;
using SRV.ProductionService;
using SRV.ServiceInterface;
using SRV.ViewModel;
using System;
using System.Web;
using System.Web.Mvc;
using UI.Filters;
using UI.Helper;

namespace UI.Controllers
{
    [ModelValidationFilter]
    public class EmailController : Controller
    {
        private readonly IContactService contactService;

        public EmailController() => contactService = new ContactService();

        [NeedLoginFilter]
        public ActionResult Index()
        {
            EmailModel email = contactService.GetEmail(CookieHelper.GetCurrentUserId());
            return View(email);
        }

        [HttpPost]
        [NeedLoginFilter]
        public ActionResult Index(EmailModel email)
        {
            if (email.VerificationCode != Session[Key.Captcha]?.ToString())
            {
                ModelState.AddModelError(nameof(email.VerificationCode), "* 验证码错误");
                return View();
            }
            else { contactService.BindEmail(CookieHelper.GetCurrentUserId(), email.EmailAddress); }

            return RedirectToAction("Index");
        }

        public JsonResult Send()
        {
            string emailAddress = Request.Form[Key.EmailAddress];
            string emailEvent;
            string userNickname;

            // 判断邮件发送场景
            if (Request.UrlReferrer.Segments[1].ToLower().Contains("email"))
            {
                if (contactService.ValidateEmailExists(emailAddress))
                { return Json(new { email = "activation" }); }

                emailEvent = "绑定邮箱";
                userNickname = HttpUtility.UrlDecode(ViewData[Key.Nickname].ToString());
            }
            else if (Request.UrlReferrer.Segments[2].ToLower() == "verify")
            {
                if (!contactService.ValidateEmailExists(emailAddress))
                { return Json(new { email = "none" }); }

                emailEvent = "找回密码";
                userNickname = contactService.GetEmailUserNickname(emailAddress);
            }
            else { throw new Exception("Unanticipated exception."); }

            string captcha = new Random().Next(999999).ToString().PadLeft(6, '0');
            string emailContent = "<div style=\"padding:1rem\">" +
                                      $"<p>{userNickname}，你好：</p>" +
                                      $"<p>你正在请求{emailEvent}，</p>" +
                                      $"<p>邮箱验证码为：<b>{captcha}</b></p>" +
                                      "<p>20分钟内有效。</p>" +
                                      "<p>如非本人操作，请忽略此邮件。</p>" +
                                  "</div>";

            Utility.SendEmail(emailAddress, $"一起帮{emailEvent}验证", emailContent);
            Session.Add(Key.Captcha, captcha);
            return Json(new { email = "succeed" });
        }
    }
}