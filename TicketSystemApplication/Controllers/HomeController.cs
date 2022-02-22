using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;

namespace TicketSystemApplication.Controllers
{
    public class HomeController : Controller
    {
        protected string _url;
        protected int _width, _height;
        protected int _thumbWidth, _thumbHeight;
        protected Bitmap _bmp;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index1()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CheckWebURL(string url)
        {
            int weburlId = 0;
            try
            {
                TicketSystemEntities ts = new TicketSystemEntities();
                tbWebUrl webURL = ts.tbWebUrls.FirstOrDefault(x => x.WebURL == url);
                if(webURL != null)
                {
                    weburlId = webURL.Id;
                }
                return Json(new
                {
                    id = weburlId,
                    msg = webURL
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult CheckEmailExist(string email)
        {
            int result = 0;
            try
            {
                TicketSystemEntities ts = new TicketSystemEntities();
                tbglUser tbEmail = ts.tbglUsers.FirstOrDefault(x => x.Email == email);
                if (tbEmail != null)
                {
                    string OTP = GenerateRandomOTP();
                    result = tbEmail.Id;
                    tbEmail.OTP = OTP;
                    ts.SaveChanges();
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        sendMail22(tbEmail.Email, "OTP for Authentication", "", CreateMailBodyforEndUserOTP(tbEmail.Email, OTP));
                    }).Start();
                }
                return Json(new
                {
                    msg = result
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult CheckValidOTPAndSubmit(string email, string OTP, string urlId, string title, string description, string keydatapoints, string keywords, string metadescription)
        {
            try
            {
                string result = string.Empty;
                int Id = Convert.ToInt32(urlId);
                TicketSystemEntities ts = new TicketSystemEntities();
                tbglUser tbEmail = ts.tbglUsers.FirstOrDefault(x => x.Email == email && x.OTP == OTP);
                if(tbEmail!= null)
                {
                    int userid = tbEmail.Id;
                    tbWebUrl webURL = ts.tbWebUrls.FirstOrDefault(x => x.Id == Id);
                    int newTicketNo = 0;
                    try
                    {
                        newTicketNo = ts.tbglTickets.OrderByDescending(x => x.Id).FirstOrDefault().Id;
                    }
                    catch { }
                    newTicketNo = newTicketNo + 1;
                    if (tbEmail != null && webURL != null)
                    {
                        tbglTicket ticket = new tbglTicket();
                        ticket.Createdby = userid;
                        ticket.CreatedDate = DateTime.Now;
                        ticket.Description = webURL.Description;
                        ticket.KeyDataPoints = webURL.KeyDatapoints;
                        ticket.Modifiedby = 0;
                        ticket.Status = "Pending";
                        ticket.WebURL = webURL.WebURL;
                        ticket.Keywords = keywords;
                        ticket.MetaDescription = metadescription;
                        string ticketNo = "T-" + newTicketNo.ToString().PadLeft(5, '0');
                        ticket.TicketID = ticketNo;
                        ts.tbglTickets.Add(ticket);
                        ts.SaveChanges();
                        result = ticketNo;
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            sendMailtoProjectUsers(tbEmail.Email, "Ticket Submitted", CreateMailBodyforTicketSubmit(tbEmail.Email, ticketNo), tbEmail.ProjectID);
                        }).Start();

                        return Json(new
                        {
                            msg = "Ticket is submitted successfully. Your Ticket Number is: <span style='font-size:20px; font-weight:bold;'>" + result + "</span>.<br/>Our team will contact you shortly."
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            msg = 0
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        msg = 0
                    });
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult ResendOTP(string emailId)
        {
            int result = 0;
            try
            {
                int Id = Convert.ToInt32(emailId);
                TicketSystemEntities ts = new TicketSystemEntities();
                tbglUser tbEmail = ts.tbglUsers.FirstOrDefault(x => x.Id == Id);
                if (tbEmail != null)
                {
                    result = tbEmail.Id;
                    string OTP = tbEmail.OTP;
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        sendMail22(tbEmail.Email, "OTP for Authentication", "", CreateMailBodyforEndUserOTP(tbEmail.Email, OTP));
                    }).Start();
                    return Json(new
                    {
                        msg = result
                    });
                }
                else
                {
                    return Json(new
                    {
                        msg = result
                    });
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult SubmitTicket(string emailId, string urlId, string title, string description, string keydatapoints, string keywords, string metadescription)
        {
            string result = "";
            try
            {
                int userid = Convert.ToInt32(emailId);
                int urlid = Convert.ToInt32(urlId);
                TicketSystemEntities ts = new TicketSystemEntities();
                tbglUser tbEmail = ts.tbglUsers.FirstOrDefault(x => x.Id == userid);
                tbWebUrl tbUrl = ts.tbWebUrls.FirstOrDefault(x => x.Id == urlid);
                int newTicketNo = 0;
                try
                {
                    newTicketNo = ts.tbglTickets.OrderByDescending(x => x.Id).FirstOrDefault().Id;
                }
                catch { }
                newTicketNo = newTicketNo + 1;
                if (tbEmail != null && tbUrl != null)
                {
                    tbglTicket ticket = new tbglTicket();
                    ticket.Createdby = userid;
                    ticket.CreatedDate = DateTime.Now;
                    ticket.Description = tbUrl.Description;
                    ticket.KeyDataPoints = tbUrl.KeyDatapoints;
                    ticket.Modifiedby = 0;
                    ticket.Status = "Pending";
                    ticket.WebURL = tbUrl.WebURL;
                    ticket.Keywords = keywords;
                    ticket.MetaDescription = metadescription;
                    string ticketNo = "T-" + newTicketNo.ToString().PadLeft(5, '0');
                    ticket.TicketID = ticketNo;
                    ts.tbglTickets.Add(ticket);
                    ts.SaveChanges();
                    result = ticketNo;
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        sendMailtoProjectUsers(tbEmail.Email, "Ticket Submitted", CreateMailBodyforTicketSubmit(tbEmail.Email, ticketNo), tbEmail.ProjectID);
                    }).Start();

                    return Json(new
                    {
                        msg = "Ticket is submitted successfully. Your Ticket Number is: <span style='font-size:20px; font-weight:bold;'>" + result +"</span>.<br/>Our team will contact you shortly."
                    });
                }
                else
                {
                    return Json(new
                    {
                        msg = "Something Worng. Please try again."
                    });
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult TrackTicket(string ticket)
        {
            string status = string.Empty;
            try
            {
                TicketSystemEntities ts = new TicketSystemEntities();
                List<tbglTicket_H> tckt = ts.tbglTicket_H.Where(x => x.TicketID == ticket).OrderByDescending(x => x.CreatedDate).ToList();
                return Json(new
                {
                    msg = tckt
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateRandomOTP()
        {
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();
            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

            for (int i = 0; i < 6; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }
        public static bool sendMail22(string ToAddress, string subject, string userId, string body)
        {
            bool Message = false;
            var fromAddress = new MailAddress("noreply@shooratech.com", "WaysAheadGlobal Support");
            var toAddress = new MailAddress(ToAddress);
            const string fromPassword = "Shoora@99";
            ServicePointManager.ServerCertificateValidationCallback =
        delegate (
            object s,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors
        ) {
            return true;
        };
            var smtp = new SmtpClient
            {
                Host = "mail.shooratech.com",
                Port = 25,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
                Message = true;
            }
            return Message;
        }
        public static bool sendMailtoProjectUsers(string ToAddress, string subject, string body, string projectId)
        {
            bool Message = false;
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            ServicePointManager.ServerCertificateValidationCallback =
        delegate (
            object s,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors
        ) {
            return true;
        };
            message.From = new MailAddress("noreply@shooratech.com", "WaysAheadGlobal Support");
            message.To.Add(ToAddress);
            TicketSystemEntities ts = new TicketSystemEntities();
            List<tbglUser> ccUsers = ts.tbglUsers.Where(x => x.ProjectID == projectId && x.EmailType == "cc" ).ToList();
            foreach(tbglUser usr in ccUsers)
            {
                if (usr.Email != ToAddress)
                {
                    message.CC.Add(usr.Email);
                }
            }
            List<tbglUser> bccUsers = ts.tbglUsers.Where(x => x.ProjectID == projectId && x.EmailType == "bcc").ToList();
            foreach (tbglUser usr in bccUsers)
            {
                if (usr.Email != ToAddress)
                {
                    message.Bcc.Add(usr.Email);
                }
            }
            message.Subject = subject;
            message.IsBodyHtml = true; 
            message.Body = body;
            smtp.Port = 25;
            smtp.Host = "mail.shooratech.com"; //for gmail host  
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("noreply@shooratech.com", "Shoora@99");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);
            Message = true;
            return Message;
        }
        public string CreateMailBodyforEndUserOTP(string email, string verificationCode)
        {
            try
            {
                StringBuilder strhtml = new StringBuilder();
                string AppUrl = "";// Request.Url.Authority;// System.Configuration.ConfigurationManager.AppSettings["AppURL"].ToString();
                string imageDirectoty = AppUrl + "/Images/";
                #region Mailbody
                strhtml.Append("<div class=\"wrapper\" style=\"width:660px; margin:0 auto; color:#000; background:#197aa7; font:12px/1.8em Tahoma, Geneva, sans-serif;\">");
                strhtml.Append("<div class=\"container\" style=\"padding:30px;\">");
                strhtml.Append("<div class=\"header\" style=\"width:600px; height:6px; background:url(" + imageDirectoty + "header-bg.png) 0 0 no-repeat; box-shadow:0 13px 8px 0 rgba(0,0,0,0.07); -webkit-box-shadow:0 13px 8px 0 rgba(0,0,0,0.07); -moz-box-shadow:0 13px 8px 0 rgba(0,0,0,0.07); -ms-box-shadow:0 13px 8px 0 rgba(0,0,0,0.07);\"></div>");
                strhtml.Append("<div class=\"content\" style=\"background:#fff; padding:30px; box-shadow:2px 24px 18px -8px rgba(0, 0, 0, 0.1); -webkit-box-shadow:2px 24px 18px -8px rgba(0, 0, 0, 0.1); -moz-box-shadow:2px 24px 18px -8px rgba(0, 0, 0, 0.1); -ms-box-shadow:2px 24px 18px -8px rgba(0, 0, 0, 0.1)\">");
                strhtml.Append("<div class=\"heading\" style=\"background:url(" + imageDirectoty + "heading-bg.png) 0 0 repeat; !overflow:hidden;\">");
                strhtml.Append("<h2 style=\"font-size:20px; font-family:Tahoma, Geneva, sans-serif; float:left; background:#fff; padding:0 20px 0 0; margin:0;\"></h2>");
                strhtml.Append("<div style=\"clear:both;\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("<div class=\"separator\" style=\"margin:10px 0; height:2px\"></div>");
                strhtml.Append("<div class=\"separator\" style=\"margin:11px 0; height:2px\"></div>");
                strhtml.Append("<p>Dear User,</p>");
                strhtml.Append("<div class=\"d_row\" style=\"margin:0 0 0px 0\">");
                strhtml.Append("<div style=\"clear:both;\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("<div style=\"width:100%; float:left;\">");
                strhtml.Append("<div style=\"clear:both;\"></div>");
                strhtml.Append("<p style=\"float:left;\">As part of the security feature on WaysAheadGlobal tool, system generated OTP (One Time Password) has been shared with you. <br/>Please use Validation Code: <b>" + verificationCode + "</b> " + " to confirm that you are authorized to use email address " + email + ". </p>");
                //strhtml.Append("<p style=\"float:left;margin-left:20px;\">If you did not request for verification of this email address, kindly ignore it. Please note that at times, the situation isn't a phising attempt, but either a misunderstanding of how to use our service.</p>");
                //strhtml.Append("<p style=\"float:left;margin-left:20px;\">If you are still concerned, request you to please forward this notification to support@waysaheadglobal.com and state in the email that you did not request for this particular verification.</p>");
                strhtml.Append("<div class=\"separator\" style=\"margin:5px 0; height:2px\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("<div class=\"separator\" style=\"margin:5px 0;\">Thanks,</div>");
                strhtml.Append("<div class=\"separator\" >The WaysAheadGlobal Team</div></br></br>");
                strhtml.Append("</div>");
                strhtml.Append("</div>");
                strhtml.Append("<div style=\"clear:both;\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("</div>");
                strhtml.Append("<div class=\"footer\" style=\"width:600px; height:12px; !margin:-4px 0 0 0;\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("</div>");
                #endregion
                return strhtml.ToString();
            }
            catch { return string.Empty; }

        }
        public string CreateMailBodyforTicketSubmit(string email, string ticketNo)
        {
            try
            {
                StringBuilder strhtml = new StringBuilder();
                string AppUrl = "";// Request.Url.Authority;// System.Configuration.ConfigurationManager.AppSettings["AppURL"].ToString();
                string imageDirectoty = AppUrl + "/Images/";
                #region Mailbody
                strhtml.Append("<div class=\"wrapper\" style=\"width:660px; margin:0 auto; color:#000; background:#197aa7; font:12px/1.8em Tahoma, Geneva, sans-serif;\">");
                strhtml.Append("<div class=\"container\" style=\"padding:30px;\">");
                strhtml.Append("<div class=\"header\" style=\"width:600px; height:6px; background:url(" + imageDirectoty + "header-bg.png) 0 0 no-repeat; box-shadow:0 13px 8px 0 rgba(0,0,0,0.07); -webkit-box-shadow:0 13px 8px 0 rgba(0,0,0,0.07); -moz-box-shadow:0 13px 8px 0 rgba(0,0,0,0.07); -ms-box-shadow:0 13px 8px 0 rgba(0,0,0,0.07);\"></div>");
                strhtml.Append("<div class=\"content\" style=\"background:#fff; padding:30px; box-shadow:2px 24px 18px -8px rgba(0, 0, 0, 0.1); -webkit-box-shadow:2px 24px 18px -8px rgba(0, 0, 0, 0.1); -moz-box-shadow:2px 24px 18px -8px rgba(0, 0, 0, 0.1); -ms-box-shadow:2px 24px 18px -8px rgba(0, 0, 0, 0.1)\">");
                strhtml.Append("<div class=\"heading\" style=\"background:url(" + imageDirectoty + "heading-bg.png) 0 0 repeat; !overflow:hidden;\">");
                strhtml.Append("<h2 style=\"font-size:20px; font-family:Tahoma, Geneva, sans-serif; float:left; background:#fff; padding:0 20px 0 0; margin:0;\"></h2>");
                strhtml.Append("<div style=\"clear:both;\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("<div class=\"separator\" style=\"margin:10px 0; height:2px\"></div>");
                strhtml.Append("<div class=\"separator\" style=\"margin:11px 0; height:2px\"></div>");
                strhtml.Append("<p>Dear User,</p>");
                strhtml.Append("<div class=\"d_row\" style=\"margin:0 0 0px 0\">");
                strhtml.Append("<div style=\"clear:both;\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("<div style=\"width:100%; float:left;\">");
                strhtml.Append("<div style=\"clear:both;\"></div>");
                strhtml.Append("<p style=\"float:left;\">Ticket is submitted and ticket no is: <b>" + ticketNo + "</b>.<br/>Our team will contact you shortly.</p>");
                //strhtml.Append("<p style=\"float:left;margin-left:20px;\">If you did not request for verification of this email address, kindly ignore it. Please note that at times, the situation isn't a phising attempt, but either a misunderstanding of how to use our service.</p>");
                //strhtml.Append("<p style=\"float:left;margin-left:20px;\">If you are still concerned, request you to please forward this notification to support@waysaheadglobal.com and state in the email that you did not request for this particular verification.</p>");
                strhtml.Append("<div class=\"separator\" style=\"margin:5px 0; height:2px\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("<div class=\"separator\" style=\"margin:5px 0;\">Thanks,</div>");
                strhtml.Append("<div class=\"separator\" >The WaysAheadGlobal Team</div></br></br>");
                strhtml.Append("</div>");
                strhtml.Append("</div>");
                strhtml.Append("<div style=\"clear:both;\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("</div>");
                strhtml.Append("<div class=\"footer\" style=\"width:600px; height:12px; !margin:-4px 0 0 0;\"></div>");
                strhtml.Append("</div>");
                strhtml.Append("</div>");
                #endregion
                return strhtml.ToString();
            }
            catch { return string.Empty; }

        }
        public Bitmap GetThumbnaill(string url, int width, int height, int thumbWidth, int thumbHeight)
        {
            return GetThumbnail();
        }
        protected Bitmap GetThumbnail()
        {
            // WebBrowser is an ActiveX control that must be run in a
            // single-threaded apartment so create a thread to create the
            // control and generate the thumbnail
            Thread thread = new Thread(new ThreadStart(GetThumbnailWorker));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return _bmp.GetThumbnailImage(_thumbWidth, _thumbHeight,
              null, IntPtr.Zero) as Bitmap;
        }
        protected void GetThumbnailWorker()
        {
            using (WebBrowser browser = new WebBrowser())
            {
                browser.ClientSize = new Size(_width, _height);
                browser.ScrollBarsEnabled = false;
                browser.ScriptErrorsSuppressed = true;
                browser.Navigate(_url);

                // Wait for control to load page
                while (browser.ReadyState != WebBrowserReadyState.Complete)
                    Application.DoEvents();

                // Render browser content to bitmap
                _bmp = new Bitmap(_width, _height);
                browser.DrawToBitmap(_bmp, new Rectangle(0, 0,
                  _width, _height));
            }
        }

    }
}