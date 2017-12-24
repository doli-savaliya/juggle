namespace juggle.Models
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;

    public partial class jugglecontext : DbContext
    {

        public jugglecontext()
            : base("name=jugglecontext")
        {
        }

        public virtual DbSet<tbl_appointment> tbl_appointment { get; set; }
        public virtual DbSet<tbl_attribute_data> tbl_attribute_data { get; set; }
        public virtual DbSet<tbl_client> tbl_client { get; set; }
        public virtual DbSet<tbl_employee_info> tbl_employee_info { get; set; }
        public virtual DbSet<tbl_role> tbl_role { get; set; }
        //public virtual DbSet<tbl_schedule> tbl_schedule { get; set; }
        public virtual DbSet<tbl_status> tbl_status { get; set; }
        public virtual DbSet<tbl_time_interval> tbl_time_interval { get; set; }
        public virtual DbSet<tbl_transpotation_list> tbl_transpotation_list { get; set; }
        public virtual DbSet<tbl_user> tbl_user { get; set; }
        public virtual DbSet<tbl_user_invitations> tbl_user_invitations { get; set; }
        public virtual DbSet<tbl_worktype> tbl_worktype { get; set; }
        // public virtual DbSet<tbl_worktype_category> tbl_worktype_category { get; set; }
        public virtual DbSet<tblAppointment_Days> tblAppointment_Days { get; set; }
        public virtual DbSet<tbl_employee_availability> tbl_employee_availability { get; set; }
        public virtual DbSet<tbl_timezone> tbl_timezone { get; set; }
        public virtual DbSet<servicetypetime> servicetypetimes { get; set; }
        public virtual DbSet<tbl_appointmentEveryday_hopper> tbl_appointmentEveryday_hopper { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tbl_appointment>()
                .Property(e => e.time_range_start)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_appointment>()
                .Property(e => e.time_range_end)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_client>()
                .Property(e => e.client_code)
                .HasPrecision(18, 0);

            modelBuilder.Entity<tbl_employee_info>()
                .Property(e => e.emp_code)
                .HasPrecision(18, 0);

            modelBuilder.Entity<tbl_user>()
                .Property(e => e.username)
                .IsUnicode(false);

            modelBuilder.Entity<tbl_employee_availability>()
                  .Property(e => e.time_range_start)
                  .IsUnicode(false);

            modelBuilder.Entity<tbl_employee_availability>()
                .Property(e => e.time_range_end)
                .IsUnicode(false);
        }

        /// <summary>
        /// Sends the mail.
        /// </summary>
        /// <param name="toAddress">To address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="mailBody">The mail body.</param>
        public void SendMail(string toAddress, string subject, string mailBody, [Optional] string password, [Optional] string useraName)
        {
            SmtpClient smtp = new SmtpClient();
            MailMessage mail = new MailMessage();
           // MailAddress Sender = new MailAddress(Environment.GetEnvironmentVariable("UserId"));
             MailAddress Sender = new MailAddress(ConfigurationManager.AppSettings["UserId"]);
            mail.To.Add(toAddress);
            mail.From = Sender;
            mail.Subject = subject;
            mail.Body = mailBody;
            mail.IsBodyHtml = true;

             smtp.Host = ConfigurationManager.AppSettings["smtpServer"];
            //smtp.Host = Environment.GetEnvironmentVariable("SmtpServer");
            //smtp.Port = Convert.ToInt32(Environment.GetEnvironmentVariable("SmtpPort"));
           smtp.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);

            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["UserId"], ConfigurationManager.AppSettings["password"]);
         //   smtp.Credentials = new System.Net.NetworkCredential(Environment.GetEnvironmentVariable("UserId"), Environment.GetEnvironmentVariable("Password"));
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
        public string connectionString()
        {

            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["jugglecontext"].ConnectionString;
            return connectionString;
        }
        public string Encrypt(string str)
        {
            string EncrptKey = "2013;[pnuLIT)WebCodeExpert";
            byte[] byKey = { };
            byte[] IV = { 18, 52, 86, 120, 144, 171, 205, 239 };
            byKey = System.Text.Encoding.UTF8.GetBytes(EncrptKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(str);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }
        public string Decrypt(string str)
        {
            str = str.Replace(" ", "+");
            string DecryptKey = "2013;[pnuLIT)WebCodeExpert";
            byte[] byKey = { };
            byte[] IV = { 18, 52, 86, 120, 144, 171, 205, 239 };
            byte[] inputByteArray = new byte[str.Length];

            byKey = System.Text.Encoding.UTF8.GetBytes(DecryptKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Convert.FromBase64String(str.Replace(" ", "+"));
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            return encoding.GetString(ms.ToArray());
        }
        public string GeneratePassword(int PwdLength)
        {
            string _allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            Random rndNum = new Random();
            char[] chars = new char[PwdLength];
            int strLength = _allowedChars.Length;
            for (int i = 0; i <= PwdLength - 1; i++)
            {
                chars[i] = _allowedChars[Convert.ToInt32(Math.Floor((_allowedChars.Length) * rndNum.NextDouble()))];
            }
            return new string(chars);
        }
        public string redirectUrl()
        {
            string Url = "http://apps.juggle.guru";
            return Url;
        }
    }
}
