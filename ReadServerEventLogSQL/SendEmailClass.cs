/* Title:           Send Email Class
 * Date:            7-12-17
 * Author:          Terry Holmes */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using NewEventLogDLL;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;


namespace ReadServerEventLogSQL
{
    class SendEmailClass
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
               
        public bool SendEmail(string mailTo, string subject, string message)
        {
            try
            {

                MailMessage mailMessage = new MailMessage("bluejayerpreports@bluejaycommunications.com", mailTo, subject, message);
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.SubjectEncoding = Encoding.UTF8;

                SmtpClient smtpClient = new SmtpClient("192.168.0.240", 25);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = false;
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Read Servere Event Log SQL // Send Email Class // Send Mail " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());

                return false;
            }


        }
       
        public void SendEventLog(string strLogEntry)
        {
            string strComputerName;
            string strUser;
            string strMessage;
            bool blnFatalError = false;
            string strHeader = "Event Log Entry";
            string strEmailAddress = "bjc-it@bluejaycommunications.com";

            try
            {
                strComputerName = System.Environment.MachineName;
                strUser = System.Environment.UserName;

                strMessage ="<h1>" + strUser + " " + strComputerName + " " + strLogEntry + "</h1>";

                blnFatalError = !(SendEmail(strEmailAddress, strHeader, strMessage));

                if (blnFatalError == true)
                    throw new Exception();
            }
            catch(Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Read Server Event Log SQL // Send Email Class // Send Event Log " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

        }
    }
}
