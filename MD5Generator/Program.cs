using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;

namespace MD5Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Run(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        private static void Run(IEnumerable<string> args)
        {
            args = from arg in args
                   select arg.Trim() into arg
                   where !string.IsNullOrEmpty(arg)
                   select arg;

            if (!args.Any())
                throw new Exception("Missing file path.");

            var fileList = GetFileList(args);

            string fileName = args.ToList()[1];
            using (FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    {
                        var csv = new CsvWriter(sw);

                        foreach (var e in from path in fileList
                            select new
                            {
                                Path = Path.GetFullPath(path),
                                MD5 = BitConverter.ToString(Md5Hash(path))
                                    .ToLowerInvariant()
                                    .Replace("-", string.Empty),
                                SHA1 = BitConverter.ToString(Sha1Hash(path))
                                    .ToLowerInvariant()
                                    .Replace("-", string.Empty)
                            })
                        {
                            Console.WriteLine(e.MD5 + " " + e.SHA1 + " " + e.Path);

                            csv.WriteRecord(e);

                        }
                    }
                }
            }
        }

        private static IEnumerable<string> GetFileList(IEnumerable<string> args)
        {
            try
            {
                var sourceCsvFile = args.First();

                string text = File.ReadAllText(sourceCsvFile);

                return text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Source file missing");
            }
        }

        private static byte[] Md5Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                                                     FileAccess.Read,
                                                     FileShare.Read,
                                                     4096,
                                                     FileOptions.SequentialScan))
            {
                return MD5.Create().ComputeHash(stream);
            }
        }

        static byte[] Sha1Hash(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open,
                                                     FileAccess.Read,
                                                     FileShare.Read,
                                                     4096,
                                                     FileOptions.SequentialScan))
            {
                return SHA1.Create().ComputeHash(stream);
            }
        }
    }
}