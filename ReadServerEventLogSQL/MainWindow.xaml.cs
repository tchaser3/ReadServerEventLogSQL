using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NewEventLogDLL;
using EmployeeDateEntryDLL;
using DateSearchDLL;
using NewEmployeeDLL;

namespace ReadServerEventLogSQL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        SendEmailClass TheSendEmailClass = new SendEmailClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        EmployeeDateEntryClass TheEmployeeDateEntryClass = new EmployeeDateEntryClass();
        DateSearchClass TheDateSearchClass = new DateSearchClass();
        EmployeeClass TheEmployeeClass = new EmployeeClass();

        FindExtraServerEventLogInformationDataSet aFindExtraServerEventLogInformationDataSet;
        FindExtraServerEventLogInformationDataSet TheFindExtraServerEventLogInformationDataSet;
        FindExtraServerEventLogInformationDataSetTableAdapters.FindExtraServerEventLogInformationTableAdapter aFindExtraServerEventLogInformationTableAdapter;
        FindServerEventLogForReportsVerificationDataSet TheFindServerEventLogForReportsVerificationDataSet = new FindServerEventLogForReportsVerificationDataSet();
        FindEmployeeByLastNameDataSet TheFindEmployeeByLastNameDataSet = new FindEmployeeByLastNameDataSet();

        public static int gintEmployeeID;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void UpdateServerEventLogReports()
        {
            DateTime datStartDate = DateTime.Now;
            DateTime datEndDate = DateTime.Now;
            int intCounter;
            int intNumberOfRecords;
            DateTime datTransactionDate;
            string strLogonName;
            string strItemAccessed;
            string strEventNotes;
            bool blnFatalError = false;
            int intRecordsReturned;

            try
            {
                datStartDate = TheDateSearchClass.RemoveTime(datStartDate);
                datStartDate = TheDateSearchClass.SubtractingDays(datStartDate, 1);
                datEndDate = TheDateSearchClass.AddingDays(datEndDate, 1);
                datEndDate = TheDateSearchClass.RemoveTime(datEndDate);                

                intNumberOfRecords = TheFindExtraServerEventLogInformationDataSet.FindExtraServerEventLogInformation.Rows.Count;

                if (intNumberOfRecords > 0)
                {
                    for (intCounter = 0; intCounter < intNumberOfRecords; intCounter++)
                    {
                        datTransactionDate = TheFindExtraServerEventLogInformationDataSet.FindExtraServerEventLogInformation[intCounter].TransactionDate;
                        strLogonName = "Just Beginging";
                        strItemAccessed = "Date Goes Here";
                        strEventNotes = TheFindExtraServerEventLogInformationDataSet.FindExtraServerEventLogInformation[intCounter].EventNotes;

                        char[] delims = new[] { '\n', '\t', '\r' };
                        string[] strNewItems = strEventNotes.Split(delims, StringSplitOptions.RemoveEmptyEntries);

                        strLogonName = strNewItems[5];

                        if (strNewItems.Length < 16)
                        {
                            strItemAccessed = strNewItems[strNewItems.Length - 1];
                        }
                        else
                        {
                            strItemAccessed = strNewItems[16];
                        }


                        datTransactionDate = TheDateSearchClass.RemoveTime(datTransactionDate);
                        datTransactionDate = TheDateSearchClass.RemoveTime(datTransactionDate);

                        TheFindServerEventLogForReportsVerificationDataSet = TheEventLogClass.FindServerEventLogForReportsVerification(datTransactionDate, strLogonName, strItemAccessed);

                        intRecordsReturned = TheFindServerEventLogForReportsVerificationDataSet.FindServerEventLogForReportsVerification.Rows.Count;

                        if (intRecordsReturned < 1)
                        {
                            blnFatalError = TheEventLogClass.InsertServerEventLogForReports(datTransactionDate, strLogonName, strItemAccessed);

                            if (blnFatalError == true)
                                throw new Exception();
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                TheSendEmailClass.SendEventLog("Read Server Event Log SQL // Main Window // Update Server Event Log Reports " + Ex.ToString());

                TheMessagesClass.ErrorMessage(Ex.ToString());

                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Read Server Event Log SQL // Main Window // Update Server Event Log Reports " + Ex.Message);
            }
        }
        public FindExtraServerEventLogInformationDataSet FindExtraServerEventLogInformation()
        {
            try
            {
                aFindExtraServerEventLogInformationDataSet = new FindExtraServerEventLogInformationDataSet();
                aFindExtraServerEventLogInformationTableAdapter = new FindExtraServerEventLogInformationDataSetTableAdapters.FindExtraServerEventLogInformationTableAdapter();
                aFindExtraServerEventLogInformationTableAdapter.Fill(aFindExtraServerEventLogInformationDataSet.FindExtraServerEventLogInformation);
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Read Server Event Log SQL // Main Window // Find Extra Server Event Log Information " + Ex.Message);

                TheSendEmailClass.SendEventLog("Read Server Event Log SQL // Main Window // Find Extra Server Event Log Information " + Ex.ToString());

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            return aFindExtraServerEventLogInformationDataSet;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClass.CloseTheProgram();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoginUser();

            TheFindExtraServerEventLogInformationDataSet = FindExtraServerEventLogInformation();

            dgrResults.ItemsSource = TheFindExtraServerEventLogInformationDataSet.FindExtraServerEventLogInformation;
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            UpdateServerEventLogReports();

            PleaseWait.Close();
        }
        private void LoginUser()
        {
            //setting up the variables
            string strComputerName;
            string strUserName;
            string strFirstName;
            string strLastName;
            int intNumberOfRecords;
            int intCounter;
            string strTempFirstName;

            strComputerName = System.Environment.MachineName;
            strUserName = System.Environment.UserName;

            try
            {
                if (strUserName.Contains("2"))
                {
                    strUserName = strUserName.Substring(0, strUserName.Length - 1);
                }

                strLastName = strUserName.Substring(1).ToUpper();
                strFirstName = strUserName.Substring(0, 1).ToUpper();

                TheFindEmployeeByLastNameDataSet = TheEmployeeClass.FindEmployeesByLastNameKeyWord(strLastName);

                intNumberOfRecords = TheFindEmployeeByLastNameDataSet.FindEmployeeByLastName.Rows.Count;

                if (intNumberOfRecords == 1)
                {
                    gintEmployeeID = TheFindEmployeeByLastNameDataSet.FindEmployeeByLastName[0].EmployeeID;
                }
                else if (intNumberOfRecords > 1)
                {
                    for (intCounter = 0; intCounter < intNumberOfRecords; intCounter++)
                    {
                        strTempFirstName = TheFindEmployeeByLastNameDataSet.FindEmployeeByLastName[intCounter].FirstName.Substring(0, 1).ToUpper();

                        if (strTempFirstName == strFirstName)
                        {
                            gintEmployeeID = TheFindEmployeeByLastNameDataSet.FindEmployeeByLastName[intCounter].EmployeeID;
                        }
                    }
                }

                TheEmployeeDateEntryClass.InsertIntoEmployeeDateEntry(gintEmployeeID, strUserName + " " + strComputerName + " Read Remote Server Log");
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Read Server Event Log SQL // Main Window // Login User " + Ex.ToString());

                TheSendEmailClass.SendEventLog("Read Server Event Log SQL // Login User " + Ex.ToString());

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }


        }
    }
}
