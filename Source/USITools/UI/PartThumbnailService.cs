using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace USITools
{
    public class PartThumbnailService
    {
        private readonly Dictionary<string, string> _fileNameCache
            = new Dictionary<string, string>();

        private string FindThumbnailPath(string partName)
        {
            var gamedataPath = Path.GetFullPath(KSPUtil.ApplicationRootPath);
            var files = Directory.GetFiles(gamedataPath, partName + "_icon*.png", SearchOption.AllDirectories);
            return files.Where(f => f.Contains("@thumbs")).FirstOrDefault();
        }

        public Texture2D GetThumbnail(AvailablePart part)
        {
            return GetThumbnail(part.partPrefab);
        }

        public Texture2D GetThumbnail(Part part)
        {
            if (!_fileNameCache.ContainsKey(part.name))
            {
                var filePath = FindThumbnailPath(part.name);
                if (string.IsNullOrEmpty(filePath))
                {
                    return null;
                }
                _fileNameCache.Add(part.name, filePath);
            }
            return LoadTextureFromFile(_fileNameCache[part.name]);
        }

        private Texture2D LoadTextureFromFile(string filePath)
        {
            var texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(filePath));
            return texture;
        }
    }
}
