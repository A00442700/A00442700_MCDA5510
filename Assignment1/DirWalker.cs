using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

namespace Assignment1
{
    class Dirwalker
    {
        public int validRowS = 0;
        public int skippedRows = 0;
        List<Model> model = new List<Model>();   
        List<Exceptions> exe = new List<Exceptions>();    
        String[] csvHeader = {"First Name","Last Name","Street Number","Street","City","Province","Postal Code","Country","Phone Number","email Address"};
        Exceptions cs = new Exceptions(); 
        Model m;
        //SimpleCSVParser csv = new SimpleCSVParser();

        public bool compareArrays(String[] a,String[] b)
        {
            if(a.Length != b.Length)
            {
                return false;
            }
            else
            {
                for (int i=0;i<a.Length;i++)
                {
                    if(a[i] != b[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void append(String OutputFile,string logFile)
        {
            try
            {
                using (StreamWriter w = File.CreateText(OutputFile))
                {
                    foreach(var item in csvHeader)
                    {
                        w.Write(item + ",\t");
                    }
                    w.WriteLine(" ");
                    
                    model.ForEach(x => w.WriteLine($"{x.firstName} ,\t {x.lastName} ,\t {x.streetNumber} ,\t {x.street} ,\t {x.city} ,\t {x.province} ,\t {x.country} ,\t {x.PostalCode} ,\t {x.phoneNumber} ,\t {x.email}"));
                    w.WriteLine ("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    w.WriteLine($"Valid Rows: {validRowS}");
                    w.WriteLine($"Skipped Rows : {skippedRows}");
                }
                using (StreamWriter w = File.CreateText(logFile))
                {   
                    exe.ForEach(x => w.WriteLine($"{x.message}"));    
                }
               
            }
            catch(Exception E)
            {
                Console.WriteLine(E.StackTrace);
                
            }
            
        }
        public void parse(String fileName)
        {
            try 
            { 
                using (TextFieldParser parser = new TextFieldParser(fileName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    string[] headers = parser.ReadLine().Split(',');
                    bool val = compareArrays(headers,csvHeader);
                    
                    if (val)
                    {
                        while (!parser.EndOfData)
                        {
                            //Process row
                            string[] fields = parser.ReadFields();
                            bool emptyRow = false;
                            foreach (String field in fields)
                            {
                                if(String.IsNullOrEmpty(field))
                                {
                                    emptyRow = true;
                                }
                            }
                            if(emptyRow)
                            {
                                skippedRows++;
                            }
                            else
                            {
                                validRowS++;
                                try{
                                m = new Model() {firstName = fields[0], lastName = fields[1], streetNumber = fields[2], street = fields[3], city = fields[4],
                                    province = fields[5], PostalCode = fields[6], country = fields[7], phoneNumber = fields[8], email = fields[9]};
                                }
                                catch(Exception)
                                {
                                    Console.WriteLine("got some exception....... ");
                                    Console.WriteLine($"{m.firstName} ,\t {m.lastName} ,\t {m.streetNumber.Trim()} ,\t {m.street} ,\t {m.city} ,\t {m.province} ,\t {m.country} ,\t {m.PostalCode} ,\t {m.phoneNumber.Trim()} ,\t {m.email}");
                                }
                            }
                            model.Add(m);
                        }
                    }
                    else
                    {
                        Console.WriteLine("File: " + fileName + " did not match the headers" );
                    }
                }
                
            }catch(IOException ioe){
                cs .message = ioe.StackTrace;
                exe.Add(cs);
            }
        }
        
        public void  walk(String path,String outputFile,String logFile)
        {
            try
            {
                string[] list = Directory.GetDirectories(path);

                if (list == null) return;

                foreach (string dirpath in list)
                {
                    if (Directory.Exists(dirpath))
                    {
                        walk(dirpath,outputFile,logFile);
                    }
                }
                string[] fileList = Directory.GetFiles(path,"*.csv");
                
                foreach (string filepath in fileList)
                {       
                    //Console.WriteLine(filepath);
                    try{
                    parse(filepath);  
                    }
                    catch
                    {
                    Console.WriteLine ("Some exeption" +filepath);
                    }
                }
                append(outputFile,logFile);
            }
            catch (FileNotFoundException)
            {
                
                cs .message = "The file  cannot be found.";
                exe.Add(cs);
            }
            catch (DirectoryNotFoundException)
            {
                cs .message = "The file or directory cannot be found.";
                exe.Add(cs);
                
            }
            catch (DriveNotFoundException)
            {
                cs.message = "The drive specified in 'path is invalid.";
                exe.Add(cs);
                
            }
            catch (PathTooLongException)
            {
                cs.message = "'path' exceeds the maximu supported path length";
                exe.Add(cs);
                
            }
            catch (UnauthorizedAccessException)
            {
                cs.message = "You do not have permission to create this file";
                exe.Add(cs);
                
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 32)
            {
                cs.message = " there is a sharing violation.";
                exe.Add(cs);
                
            }
            catch (IOException e) when ((e.HResult & 0x0000FFFF) == 80)
            {
                
                cs.message = "File already exists";
                exe.Add(cs);
            }
            catch (IOException e)
            {
                cs.message = "An exception occured: Error Code:" +$"{e.HResult & 0x0000FFFF}";
                exe.Add(cs);
                
            }
            catch(Exception e)
            {
                cs.message =e.StackTrace;
                exe.Add(cs);
            }
        }
        
        public static void Main(String[] args)
        {
            Dirwalker fw = new Dirwalker();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Enter path to read Directories");
            String path = Console.ReadLine(); 
            
            //Creating Output and Log Directories
            String currentPath = Directory.GetCurrentDirectory();
            String outputPath = currentPath+@"\Output";
            String logsPath = currentPath+@"\logs";
            
            string[] dirs = {outputPath,logsPath};
            
            // If directories does not exist, create it
            foreach (String dir in dirs)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }           
            
            String OutputFile = outputPath+@"\Output.txt";
            String logFile = logsPath+@"\log.txt";
            
            fw.walk(path,OutputFile,logFile);
            
            using (StreamWriter w = File.AppendText(logFile))
            {   
                fw.exe.ForEach(x => w.WriteLine($"{x.message}"));    
            }     

            using (StreamWriter w = File.AppendText(OutputFile))
            {
                w.WriteLine($"Execution Time: {watch.Elapsed} ");
            }
             
            
            watch.Stop();  
        }
    }
}

        
            
        
    

