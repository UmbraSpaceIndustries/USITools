using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace USITools
{
    public class TextureService
    {
        private readonly Dictionary<string, string> _fileNameCache
            = new Dictionary<string, string>();

        public Texture2D GetTexture(string filePath, int width, int height)
        {
            if (_fileNameCache.ContainsKey(filePath))
            {
                return LoadTexture(_fileNameCache[filePath], width, height);
            }
            var fullPath = Path.Combine(KSPUtil.ApplicationRootPath, filePath);
            if (!File.Exists(fullPath))
            {
                throw new Exception($"File not found at '{fullPath}'");
            }
            _fileNameCache.Add(filePath, fullPath);
            return LoadTexture(fullPath, width, height);
        }

        protected Texture2D LoadTexture(string filePath, int width, int height)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception($"File not found at '{filePath}'");
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.LoadImage(File.ReadAllBytes(filePath));

            return texture;
        }
    }
}
