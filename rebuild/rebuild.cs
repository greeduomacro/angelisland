/***************************************************************************
 *                               CREDITS
 *                         -------------------
 *                         : (C) 2004-2009 Luke Tomasello (AKA Adam Ant)
 *                         :   and the Angel Island Software Team
 *                         :   luke@tomasello.com
 *                         :   Official Documentation:
 *                         :   www.game-master.net, wiki.game-master.net
 *                         :   Official Source Code (SVN Repository):
 *                         :   http://game-master.net:8050/svn/angelisland
 *                         : 
 *                         : (C) May 1, 2002 The RunUO Software Team
 *                         :   info@runuo.com
 *
 *   Give credit where credit is due!
 *   Even though this is 'free software', you are encouraged to give
 *    credit to the individuals that spent many hundreds of hours
 *    developing this software.
 *   Many of the ideas you will find in this Angel Island version of 
 *   Ultima Online are unique and one-of-a-kind in the gaming industry! 
 *
 ***************************************************************************/

/***************************************************************************
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 ***************************************************************************/


/* Rebuild/Rebuild.cs
 * CHANGELOG:
 *  3/10/07, Adam
 *      Replace old hardcoded email list with new distribution list: devnotify@game-master.net
 *	11/13/06, Kit
 *		Initial Creation
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;
using Server.SMTP;

namespace rebuild
{
    class rebuild
    {
         // To add reporting emailing, fill in EmailServer and Emails:
        // Example:
        //  private const string Emails = "first@email.here;second@email.here;third@email.here";
        private static string Emails = "devnotify@game-master.net";
        //wait two minutes for server to possibly fail compileing and give error code
        private static TimeSpan ServerCompileWait = TimeSpan.FromMinutes(2.0); 

        private static void SendEmail(string body, bool testcenter)
        {
            Console.Write("Sending email...");

            try
            {
                MailMessage message = new MailMessage();
                if(testcenter)
                    message.Subject = "Automated RunUO Rebuild/Merge Report Test Center";
                else
                    message.Subject = "Automated RunUO Rebuild/Merge Report Production";

                message.From = new MailAddress(SmtpDirect.FromEmailAddress);
                message.To.Add(SmtpDirect.ClassicList(Emails));
                message.Body = "Automated RunUO Rebuild/Merge Report" + body;
           
                bool result = new SmtpDirect().SendEmail(message);
                Console.WriteLine("done: {0}", result.ToString());
            }
            catch
            {
                Console.WriteLine("failed");
            }
        }

        static void Main(string[] args)
        {
            string directory;
            string output = "";
            bool testcenter = false;

            DateTime LastMessage = DateTime.Now;
            if (args.Length < 2)
            {
                Console.WriteLine("Usage is rebuild processid, test center true/false");
                return;
            }

// 15 second delay while we attach a debugger
//Thread.Sleep(1000 * 15);
Debugger.Break();
        
            //grab process id of server.exe that was passed to us
            try
            {
                int serverid = Convert.ToInt32(args[0]);
                testcenter = Convert.ToBoolean(args[1]);
                Process p = Process.GetProcessById(serverid);

                //if active get server.exe directory and wait for it to terminate
                if (p != null && p.ProcessName.ToLower().StartsWith("server"))
                {
                    directory = p.StartInfo.WorkingDirectory;

                    while (!p.HasExited)
                    {
                        p.Refresh(); //refresh process info
                        if (DateTime.Now >= LastMessage)
                        {
                            Console.WriteLine("Waiting on {0} to exit", p.ProcessName);
                            LastMessage = DateTime.Now + TimeSpan.FromSeconds(5.0);
                        }

                        Thread.Sleep(1000);
                    }
                    Console.WriteLine("Server.exe exited");
                    Console.WriteLine("Starting cvs process");

                    //server exe closed out
                    //start up cvs process with checkout command line
                    Process cvs = new Process();
                    cvs.StartInfo.FileName = "cvs.exe";
                    cvs.StartInfo.UseShellExecute = false;
                    cvs.StartInfo.RedirectStandardOutput = true;
                    cvs.StartInfo.Arguments = "co .";

                    if (cvs.Start())
                    {
                        //wait for cvs to finish before doing anything else
                        Console.WriteLine("cvs started succesfully");
                        output = cvs.StandardOutput.ReadToEnd();
                        cvs.WaitForExit();
                        Console.WriteLine("cvs exited with code {0}", cvs.ExitCode);
                    }
                    else
                    {
                        output = "Cvs Failed to start.";
                        Console.WriteLine("cvs failed to start!!!");
                    }

                    //startup our server again
                    Process server = new Process();
                    server.StartInfo.WorkingDirectory = directory;
                    server.StartInfo.FileName = "Server.exe";
                    server.StartInfo.Arguments = "-quiet";
                    if (server.Start())
                    {
                      
                        bool WaitingOnCompile = true;
                        DateTime compilewait = DateTime.Now + ServerCompileWait;
                        while (!server.HasExited && WaitingOnCompile)
                        {
                            server.Refresh(); //refresh process info
                            if (DateTime.Now >= compilewait) //wait two minutes for server compile time if it hasnt exited
                            {
                                WaitingOnCompile = false;
                            }
                        }
                        if (server.HasExited) //we got some server error
                        {
                            output = output + "Server started but exited with code(script compile failed most likely) : " + server.ExitCode.ToString();
                        }
                        else
                        {
                            output = output + "Server process started okay.";
                            Console.WriteLine("Server.exe started okay");
                        }
                        
                    }
                    else
                    {
                        output = output + "Server process failed to start!!!";
                        Console.WriteLine("Server.exe failed to start");
                        return;
                    }
                }
                else
                {
                    output = output + "Unable to find server process, no work done";
                    Console.WriteLine("Unable to attached to server.exe process, aborting");
                    Console.ReadLine();
                    return;
                }
                SendEmail(output, testcenter);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                output = output + e.Message + e.StackTrace;
                SendEmail(output, testcenter);
                Console.WriteLine("Failed to get server.exe process");
            }
        

        }
    }
}
