using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoImporter
{
    public class PhotoImporter
    {
        public static void Main(string[] args)
        {
            if (args.Length == 3 && args[0].Equals("-i"))
            {
                string sourceFolder = args[1];
                string targetFolder = args[2];
                Import(sourceFolder, targetFolder);
            }
        }

        public static void Import(string sourceFolder, string targetFolder)
        {
            string[] allfiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories);
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                DateTime? timeTaken = null;
                try { 
                    var tfile = TagLib.File.Create(file);
                    string title = tfile.Tag.Title;
                    var tag = tfile.Tag as TagLib.Image.CombinedImageTag;
                    timeTaken = tag.DateTime;
                }
                catch (Exception)
                {
                    timeTaken = info.CreationTime;
                }
                string destFolderName;
                if (timeTaken.HasValue)
                {
                    destFolderName = String.Format("{0:yyyy.MM.dd}", timeTaken.Value);
                }
                else
                {
                    destFolderName = "_DATE_UNSPECIFIED";
                }
                string destFolderPath = targetFolder + "\\" + destFolderName;
                string targetFileName = destFolderPath + "\\" + info.Name;

                System.IO.Directory.CreateDirectory(destFolderPath);
                bool exec = true;
                if (System.IO.File.Exists(targetFileName))
                {
                    FileInfo existFile = new FileInfo(targetFileName);
                    if (info.Length != existFile.Length)
                    {
                        int counter = 0;
                        do
                        {
                            counter++;
                            if(counter>5)
                            {
                                exec = false;
                                break;
                            }
                        } while (System.IO.File.Exists(targetFileName + counter));
                        targetFileName = targetFileName + counter;
                    }
                    else exec = false;
                }
                if (exec)
                {
                    System.IO.File.Move(info.FullName, targetFileName);
                    Console.WriteLine("Copy: " + timeTaken + "   " + info.FullName+ "   " + targetFileName);
                }
                else
                {
                    Console.WriteLine("Skip: " + timeTaken + "   " + info.FullName + "   " + targetFileName);
                }

            }
        }
    }
}
