using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PhotoSauce.MagicScaler;

namespace PhotobookWebAPI.Wrappers
{
    public class FileSystem : IFileSystem
    {
        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public virtual DirectoryInfo DirectoryCreate(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public virtual void SmallFileCreate(string fromPath, string toPath, ProcessImageSettings settings)
        {
            using (var outStream = new FileStream(toPath, FileMode.Create))
            {
                MagicImageProcessor.ProcessImage(fromPath, outStream, settings);
            }
        }

        public virtual void FileCreate(string path, byte[] bytes)
        {
            using (var imageFile = new FileStream(path, FileMode.Create))
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }
        }

        public virtual void DirectoryDelete(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }

        public virtual void FileDelete(string path)
        {
            File.Delete(path);
        }
    }

    public interface IFileSystem
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        DirectoryInfo DirectoryCreate(string path);
        void FileCreate(string path, byte[] bytes);
        void SmallFileCreate(string fromPath, string toPath, ProcessImageSettings settings);
        void DirectoryDelete(string path, bool recursive);
        void FileDelete(string path);
    }
}
