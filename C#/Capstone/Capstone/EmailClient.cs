using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace Capstone
{
    // Author: Peter
    // Used to send confirmation emails
    public class EmailClient
    {
        public SmtpClient Client { get; }
        private string Sender = "CallousKitchen@gmail.com";
        private string Password = "tisVN6iDy2r6kh";

        public EmailClient()
        {
            Client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(Sender, Password),
                EnableSsl = true
            };

        }

        public void SendConfirmEmail(string Reciever,Guid guid)
        {
            string url = @"https://callousfrontend.azurewebsites.net"; // change for production
            url += @"/Email/Confirm?Key=" + guid;

            MailMessage message = new MailMessage(Sender, Reciever);
            message.Subject = "Callous Kitchen Account Confirmation";
            message.IsBodyHtml = true;
            message.Body = "<h1>You need to confirm your account.</h1><br><a href='" + url
                + "'>Confirm Account</a>";
            Client.Send(message);
        }


    }

}